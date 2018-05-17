﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ApiServer.Core.Authorization;
using ApiServer.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VDS.AspNetCore.Mvc.Authorization;
using System.Drawing;


namespace ApiServer.Controllers
{
    [AppAuthorize(VdsPermissions.AddProject)]
    [Produces("application/json")]
    [Route("api/ImportProject")]
    public class ImportProjectController : Controller
    {
        private readonly VdsContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUserService _userService;
        private readonly string[] AllowedFileExtensions = new string[] { "rar", "RAR", "ZIP", "zip", "jpg", "JPG", "png", "PNG", "bmp", "BMP", "txt", "TXT" };
        private readonly string[] imageExtentions = new string[] { "jpg", "JPG", "png", "PNG", "bmp", "BMP", "txt", "TXT" };
        private Folder folder;

        public ImportProjectController(VdsContext vdsContext, IHostingEnvironment hostingEnvironment, IUserService userService)
        {
            _context = vdsContext;
            _hostingEnvironment = hostingEnvironment;
            _userService = userService;
        }

        [HttpPost("{id}"), DisableRequestSizeLimit]
        public async Task<IActionResult> Upload([FromRoute] Guid id)
        {
            var project = new Project();
            var currentUser = _userService.GetCurrentUser();
            var currentRole = _userService.GetCurrentRole(currentUser.Id);

            if (currentRole.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
            {
                project = await _context.ProjectUsers
                .Include(x => x.Project)
                .Include(u => u.User)
                .Where(a => a.Project.Id == id).Select(b => b.Project).FirstOrDefaultAsync();
            }
            else
            {
                project = await _context.ProjectUsers
                .Include(x => x.Project)
                .Include(u => u.User)
                .Where(a => a.User.Id == currentUser.Id && a.Project.Id == id).Select(b => b.Project).FirstOrDefaultAsync();
            }

            if (project == null)
                return Content("Project not found");

            try
            {
                var files = Request.Form.Files;
                foreach (var file in files)
                {
                    if (file.Length <= 0)
                    {
                        return Content("There are no files to upload !");
                    }

                    string fileExtension = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"').Split('.')[1];

                    if (!AllowedFileExtensions.Contains(fileExtension))
                    {
                        return Content("File not allowed !");
                    }

                    folder = MakeFolder(project.Name, file.FileName);

                    using (var stream = new FileStream(folder.TempPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    if (ExtractFile(folder.FinalPath))
                    {
                        try
                        {
                            await ProccessFiles(folder.FinalPath, project);

                            folder.Delete(folder.TempPath);
                        }
                        catch (Exception ex)
                        {
                            folder.CleanAll();
                            return Content(ex.ToString());
                        }
                    }
                    else
                    {
                        return Content("Cant not import file!");
                    }

                }

                return Ok();
            }
            catch (Exception err)
            {
                return Content(err.ToString());
            }
        }

        private bool ExtractFile(string folderPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(folderPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string[] filenames = entry.Name.Split('.');
                    if (!AllowedFileExtensions.Contains(filenames.Last()))
                    {
                        return false;
                    }
                    else
                    {
                        entry.ExtractToFile(Path.Combine(folderPath, entry.Name));
                    }
                }
            }
            return true;
        }

        private async Task ProccessFiles(string folderPath, Project project)
        {
            var textFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories).Where(x => Path.GetExtension(x).ToLower().Equals("txt"));
            var imageFiles = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories).Where(x => imageExtentions.Contains(Path.GetExtension(x)));

            var images = new List<Model.Image>();
            var tags = new List<Tag>();

            foreach (var imagePath in imageFiles)
            {
                var name = Path.GetFileName(imagePath);
                var textFile = textFiles.SingleOrDefault(x => Path.GetFileName(x).Equals(name));
                if (textFile != null)
                {
                    try
                    {
                        using (StreamReader file = new StreamReader(textFile))
                        {
                            string line;
                            var image = new Model.Image()
                            {
                                Project = project,
                                Ignored = false,
                                Path = folder.PathToStoreDb,
                                TagHasClass = 0,
                                TagNotHasClass = 0,
                                TotalClass = 0,
                                TaggedDate = DateTime.Now
                            };

                            _context.Images.Add(image);
                            images.Add(image);

                            var count = 0;
                            while ((line = await file.ReadLineAsync()) != null)
                            {
                                count++;
                                var data = line.Split(';');
                                if (data.Count() < 5)
                                {
                                    continue;
                                }
                                try
                                {
                                    await AddDataToContext(count ,data, imagePath, image, tags);
                                }
                                catch (Exception ex)
                                {
                                    throw ex;
                                }
                            }
                            image.Tags = tags;
                            
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }

            try
            {
                UpdateProject(project, images);

                await _context.SaveChangesAsync();

            }catch(Exception ex)
            {
                throw ex;
            }
        }

        private async Task AddDataToContext(int tagIndex, string[] data, string imagePath, Model.Image image, ICollection<Tag> tags)
        {
            var classId = data[0].ToUpper().Equals("NULL")? 0: int.Parse(data[0]);
            var centerX = double.Parse(data[1]);
            var centerY = double.Parse(data[2]);
            var width = double.Parse(data[3]);
            var height = double.Parse(data[4]);

            using(var img = new Bitmap(imagePath))
            {
                var imgWidth = img.Width;
                var imgHeight = img.Height;
                centerX *= imgHeight;
                centerY *= imgWidth;
                width *= imgWidth;
                height *= imgHeight;

                var firstPoint = new Point()
                {
                    Left = centerX - width / 2,
                    Top = centerY - height / 2
                };

                var lastPoint = new Point()
                {
                    Left = width + firstPoint.Left,
                    Top = height + firstPoint.Top
                };

                var newTag = new Tag()
                {
                    Left = firstPoint.Top * imgHeight,
                    Top = firstPoint.Left * imgWidth,
                    Index = tagIndex,
                    height = lastPoint.Top * imgHeight,
                    Width = lastPoint.Left * imgWidth,
                    Image = image,
                    TaggedDate = DateTime.Now
                };

                if (classId != 0)
                {
                    image.TagNotHasClass += 1;
                }
                else
                {
                    var klass = await _context.Classes.SingleOrDefaultAsync(x => x.Id == classId);
                    newTag.Image.TagHasClass += 1;
                    newTag.Class = klass ?? throw new ArgumentNullException("Class not found!");
                    newTag.ClassId = klass.Id;
                }

                _context.Tags.Add(newTag);
                tags.Add(newTag);
            }

        }

        private void UpdateProject(Project project, List<Model.Image> images)
        {
            try
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
            catch(Exception ex)
            {
                throw ex;
            }
        }

        private Folder MakeFolder(string project, string fileName)
        {
            string tempFolderName = "temp";
            string folderName = DateTime.UtcNow.ToString("dd-MM-yyy", CultureInfo.InvariantCulture);
            string webRootPath = _hostingEnvironment.WebRootPath;
            string projectFolder = project + "\\" + folderName;
            string newPath = Path.Combine(webRootPath, projectFolder);
            string pathToDatabase = "\\" + projectFolder + "\\";

            if (!Directory.Exists(webRootPath + "\\" + tempFolderName))
            {
                Directory.CreateDirectory(webRootPath + "\\" + tempFolderName);
            }

            if (!Directory.Exists(webRootPath + "\\" + tempFolderName))
            {
                Directory.CreateDirectory(webRootPath + "\\" + tempFolderName);
            }

            string tempPath = Path.Combine(webRootPath + "\\" + tempFolderName, fileName);

            return new Folder() { FinalPath = newPath, TempPath = tempPath, PathToStoreDb = pathToDatabase };
        }

        private class Folder
        {
            public string FinalPath { get; set; }
            public string TempPath { get; set; }
            public string PathToStoreDb { get; set; }
            
            public void MoveFiles(string source, string destination)
            {
                if (!Directory.Exists(destination))
                {
                    Directory.CreateDirectory(destination);
                }

                if (Directory.Exists(source))
                {
                    string[] files = Directory.GetFiles(source);

                    foreach (string s in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        var fileName = Path.GetFileName(s);
                        var destFile = Path.Combine(destination, fileName);
                        System.IO.File.Copy(s, destFile, true);
                    }
                }
                else
                {
                    throw new Exception("Cannot import file! {code: 101}");
                }

                Delete(source);

            }

            public void Delete(string source)
            {
                try
                {
                    System.IO.File.Delete(source);
                }
                catch (IOException)
                {
                    return;
                }
            }

            public void CleanAll()
            {
                try
                {
                    System.IO.File.Delete(FinalPath);
                    System.IO.File.Delete(TempPath);

                }
                catch (IOException)
                {
                    return;
                }
            }
        }

        private class Point
        {
            public double Left { get; set; }
            public double Top { get; set; }
        }
    }
}