using ApiServer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Microsoft.AspNetCore.SignalR;
using ApiServer.Hubs;

namespace ApiServer.Core.MergeProject
{
    public static class Merge
    {
        public static VdsContext _ctx;
        public static string webPathRoot;
        public static IHubContext<VdsHub> _hubContext;

        public static async Task Execute(MergeModel.Merge mergeData)
        {
            var newProject = new Project();
            newProject.Name = mergeData.ProjectName;
            newProject.IsDisabled = false;
            newProject.TotalImg = newProject.TotalImgNotClassed = newProject.TotalImgNotQC = newProject.TotalImgNotTagged = newProject.TotalImgQC = 0;

            _ctx.Projects.Add(newProject);
            var newProjectUsers = new List<ProjectUser>();
            foreach (var user in mergeData.Users)
            {
                var u = _ctx.Users.SingleOrDefault(x => x.UserName == user.UserName);
                var role = _ctx.Roles.SingleOrDefault(x => x.RoleName == user.RoleName);
                if (u != null && role != null)
                {
                    _ctx.ProjectUsers.Add(new ProjectUser()
                    {
                        User = u,
                        UserId = u.Id,
                        Role = role,
                        RoleId = role.Id,
                        Project = newProject,
                        ProjectId = newProject.Id
                    });
                }
            }

            var IndexClassMap = new Dictionary<int, int>();
            var newClasses = new List<Class>();



            //foreach (var projId in mergeData.Projects)
            //{
            //    var originClasses = _ctx.Classes.Include(x => x.Project).Where(x => x.Project.Id == projId);

            //    foreach (var klass in mergeData.Classes)
            //    {
            //        if (klass.Id != 0)
            //        {
            //            var c = originClasses.SingleOrDefault(x => x.Id == klass.Id);
            //            if (c != null)
            //            {
            //                var newClass = new Class()
            //                {
            //                    ClassColor = c.ClassColor,
            //                    Name = c.Name,
            //                    Note = c.Note,
            //                    Project = newProject,
            //                    Description = c.Description
            //                };

            //                _ctx.Classes.Add(newClass);
            //                newClasses.Add(newClass);
            //                IndexClassMap.Add(c.Id, newClass.Id);
            //            }
            //        }
            //    }
            //}


            foreach (var klass in mergeData.Classes)
            {
                if (klass.Id != 0)
                {
                    var newClass = new Class()
                    {
                        ClassColor = klass.ClassColor,
                        Name = klass.Name,
                        Project = newProject,
                        Description = klass.Description
                    };

                    _ctx.Classes.Add(newClass);
                    newClasses.Add(newClass);
                    IndexClassMap.Add(klass.Id, newClass.Id);
                }
            }

            foreach (var data in mergeData.MergeClasses)
            {
                var newClass = new Class()
                {
                    ClassColor = data.NewClass.ClassColor,
                    Name = data.NewClass.Name,
                    Project = newProject,
                };

                _ctx.Classes.Add(newClass);
                newClasses.Add(newClass);

                foreach (var klass in data.OldClasses)
                {
                    IndexClassMap.Add(klass.Id, newClass.Id);
                }
            }

            var images = MergeTag(mergeData, IndexClassMap, newProject, newClasses);

            updateProjectParams(newProject, images);

            try
            {
                await _ctx.SaveChangesAsync();
                await CopyImageToNewLocation(images, newProject);
                await _hubContext.Clients.Client(mergeData.ConnectionId).SendAsync("MergeNotification", new MergeNotificationMessage() { Result = true, Message = "Merge porject success!" });
                }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                await _hubContext.Clients.Client(mergeData.ConnectionId).SendAsync("MergeNotification", new MergeNotificationMessage() { Result = true, Message = "Merge porject success!" });
            }
        }

        public static List<Image> MergeTag(MergeModel.Merge mergeData, Dictionary<int, int> tagMap, Project newProject, List<Class> newClasses)
        {
            var images = new List<Image>();
            var newTags = new List<Tag>();

            foreach (var projectId in mergeData.Projects)
            {
                var imgs = _ctx.Images.Include(x => x.Project).Include(x => x.QuantityCheck).Where(x => x.Project.Id == projectId).ToList();

                foreach (var img in imgs)
                {
                    if (!filter(img, mergeData.FilterOptions))
                    {
                        imgs.Remove(img);
                        continue;
                    }

                    var newImg = new Image()
                    {
                        Ignored = img.Ignored,
                        Path = img.Path,
                        Project = newProject,
                        TagHasClass = 0,
                        TagNotHasClass = 0,
                        TagTime = 0,
                        TotalClass = 0,
                        Tags = new List<Tag>()
                    };

                    _ctx.Images.Add(newImg);

                    var tags = _ctx.Tags.Include(x => x.Class).Where(x => x.Image.Id == img.Id);
                    foreach (var tag in tags)
                    {
                        var newClass = newClasses.SingleOrDefault(x => x.Id == tagMap[tag.Class.Id]);
                        var newTag = new Tag()
                        {
                            Class = newClass,
                            ClassId = newClass.Id,
                            height = tag.height,
                            Image = img,
                            Index = tag.Index,
                            Left = tag.Left,
                            TaggedDate = tag.TaggedDate,
                            Top = tag.Top,
                            Width = tag.Width
                        };

                        _ctx.Tags.Add(newTag);

                        newImg.Tags.Add(newTag);
                    }

                    caculateClassTag(newProject, newImg);

                    images.Add(newImg);
                }

            }

            return images;
        }

        public static async Task CopyImageToNewLocation(List<Image> images, Project newProject)
        {
            string new_folderName = DateTime.UtcNow.ToString("dd-MM-yyy", CultureInfo.InvariantCulture);
            string new_projectFolder = newProject.Name + "\\" + new_folderName;
            string webRootPath = webPathRoot;
            string newPath = Path.Combine(webRootPath, new_projectFolder);
            string new_pathToDatabase = "\\" + new_projectFolder + "\\";

            if (!Directory.Exists(newPath))
            {
                Directory.CreateDirectory(newPath);
            }

            foreach (var img in images)
            {
                var sourceFile = webRootPath + img.Path;
                if (File.Exists(sourceFile))
                {
                    var filename = Path.GetFileName(sourceFile).Split('.');
                    img.Path = new_pathToDatabase + "\\" + img.Id + "." + filename.Last();
                    File.Copy(sourceFile, newPath + "\\" + img.Id + "." + filename.Last(), true);
                }
            }

            await _ctx.SaveChangesAsync();
        }

        public static bool filter(Image image, MergeModel.FilterOptions filterOptions)
        {
            var qcOpts = filterOptions.QcOptions;
            if (qcOpts.Count > 0 && image.QuantityCheck != null)
            {
                foreach (var qcOpt in qcOpts)
                {
                    if (qcOpt.Index == 1)
                    {
                        if (image.QuantityCheck.Value1 != null && image.QuantityCheck.Value1 != qcOpt.Value)
                        {
                            return false;
                        }
                    }
                    if (qcOpt.Index == 2)
                    {
                        if (image.QuantityCheck.Value2 != null && image.QuantityCheck.Value2 != qcOpt.Value)
                        {
                            return false;
                        }
                    }
                    if (qcOpt.Index == 3)
                    {
                        if (image.QuantityCheck.Value3 != null && image.QuantityCheck.Value3 != qcOpt.Value)
                        {
                            return false;
                        }
                    }
                    if (qcOpt.Index == 4)
                    {
                        if (image.QuantityCheck.Value4 != null && image.QuantityCheck.Value4 != qcOpt.Value)
                        {
                            return false;
                        }
                    }
                    if (qcOpt.Index == 5)
                    {
                        if (image.QuantityCheck.Value5 != null && image.QuantityCheck.Value5 != qcOpt.Value)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private static void caculateClassTag(Project project, Image image)
        {
            var tagCount = image.Tags.Count();
            if (tagCount <= 0)
            {
                return;
            }
            var tagHaveClass = 0;
            foreach (var t in image.Tags)
            {
                if (t.Class != null)
                {
                    tagHaveClass += 1;

                    if (string.IsNullOrEmpty(image.Classes))
                    {
                        image.Classes = t.Class.Name;
                        image.TotalClass = 1;
                    }
                    else
                    {
                        var classes = image.Classes.Split(';');
                        if (!classes.Contains(t.Class.Name))
                        {
                            image.Classes += ";" + t.Class.Name;
                        }

                        image.TotalClass = classes.Count();
                    }
                }
            }
            image.TagHasClass = tagHaveClass;
            image.TagNotHasClass = tagCount - tagHaveClass;
            image.TaggedDate = DateTime.Now;

            //try
            //{
            //    await _ctx.SaveChangesAsync();
            //}
            //catch (Exception)
            //{
            //    return;
            //}
        }

        private static void updateProjectParams(Project project, List<Image> images)
        {
            project.TotalImg = images.Count();
            var imgsNotClassed = project.TotalImg;
            var imgsNotTagged = project.TotalImg;
            var imgsNotQc = project.TotalImg;
            var imgsQc = 0;

            foreach (var img in images)
            {
                if (img.TagHasClass > 0)
                {
                    imgsNotClassed -= 1;
                }
                if (img.TagHasClass != 0 || img.TagNotHasClass != 0)
                {
                    imgsNotTagged -= 1;
                }
            }

            project.TotalImgNotClassed = imgsNotClassed;
            project.TotalImgNotTagged = imgsNotTagged;
            project.TotalImgNotQC = imgsNotQc;
            project.TotalImgQC = imgsQc;
        }
    }
}
