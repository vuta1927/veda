using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiServer.Model;
using VDS.AspNetCore.Mvc.Authorization;
using ApiServer.Core.Authorization;
using ApiServer.Model.views;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Numerics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Drawing;
using SixLabors.ImageSharp.PixelFormats;
using VDS.Security;

namespace ApiServer.Controllers
{
    [Produces("application/json")]
    [Route("api/Tags/[action]")]
    [AppAuthorize(VdsPermissions.ViewTag)]
    public class TagsController : Controller
    {
        private readonly VdsContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;
        public TagsController(VdsContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

        // GET: api/Tags
        [HttpGet("{id}")]
        [ActionName("GetTags")]
        public IActionResult GetTags([FromRoute] Guid id)
        {
            var tags = _context.Tags.Include(t => t.Image).Include(x => x.Class).Where(x => x.Image.Id == id).Include(x => x.Class).Include(q => q.QuantityCheck);
            var results = new List<TagModel.TagForView>();
            foreach (var tag in tags)
            {
                var t = new TagModel.TagForView()
                {
                    Id = tag.Id,
                    Left = tag.Left,
                    Index = tag.Index,
                    Top = tag.Top,
                    Width = tag.Width,
                    height = tag.height,
                    ImageId = tag.Image.Id
                };
                if (tag.QuantityCheck != null)
                {
                    t.QuantityCheckId = tag.QuantityCheck.Id;
                }
                if (tag.Class != null)
                    t.ClassId = tag.Class.Id;

                results.Add(t);
            }
            return Ok(results);
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTag([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tag = await _context.Tags.Include(t => t.Image).Include(x => x.Class).Include(q => q.QuantityCheck).SingleOrDefaultAsync(m => m.Id == id);

            if (tag == null)
            {
                return Content("Tag not found !");
            }

            return Ok(new TagModel.TagForView()
            {
                Id = tag.Id,
                Index = tag.Index,
                Left = tag.Left,
                Top = tag.Top,
                Width = tag.Width,
                height = tag.height,
                ClassId = tag.Class.Id,
                QuantityCheckId = tag.QuantityCheck.Id,
                ImageId = tag.Image.Id
            });
        }


        [HttpPost("{projId}/{id}")]
        [ActionName("Update")]
        public async Task<IActionResult> Update([FromRoute] Guid projId, [FromRoute] Guid id, [FromBody] TagModel.DataUpdate data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var image = await _context.Images.Include(x => x.Tags).Include(x => x.UserTaggedTimes).SingleOrDefaultAsync(x => x.Id == id);
            if (image == null)
            {
                return Content("Image not found !");
            }

            var currentUser = await _context.Users.SingleAsync(x => x.Id == data.UserId);
            if (currentUser == null)
            {
                return Content("User not found !");
            }

            image.Ignored = data.Ignored;
            var userTaggedTime = image.UserTaggedTimes.SingleOrDefault(x => x.UserId == currentUser.Id);
            if (userTaggedTime != null)
            {
                userTaggedTime.TaggedTime = data.TaggedTime;
            }
            else
            {
                var newUserTaggedTime = new UserTaggedTime() { Image = image, ImageId = image.Id, TaggedTime = data.TaggedTime, User = currentUser, UserId = currentUser.Id };
                _context.userTaggedTimes.Add(newUserTaggedTime);
            }


            string webRootPath = _hostingEnvironment.WebRootPath;
            var imgPath = webRootPath + image.Path;
            var temp = webRootPath + "\\temp";

            if (!Directory.Exists(temp))
            {
                Directory.CreateDirectory(temp);
            }

            var filename = Path.GetFileName(imgPath);
            temp += "\\" + filename;
            if (data.ExcluseAreas.Count() > 0)
            {
                foreach (var area in data.ExcluseAreas)
                {

                    var points = new SixLabors.Primitives.PointF[100];
                    var t = area.Paths.Count();
                    for (var i = 0; i < area.Paths.Count(); i++)
                    {
                        points[i] = new Vector2(area.Paths[i].X, area.Paths[i].Y);
                    }

                    using (var img = SixLabors.ImageSharp.Image.Load(imgPath))
                    {
                        img.Mutate(x => x.FillPolygon(Rgba32.Black, points));
                        System.IO.File.Delete(imgPath);
                        img.Save(imgPath);
                    };

                    //List<Point> curvePoints = new List<Point>();
                    //foreach (var p in area.Paths)
                    //{
                    //    var x = (int)p.X;
                    //    var y = (int)p.Y;
                    //    curvePoints.Add(new Point(x, y));
                    //}

                    //try
                    //{
                    //    Bitmap bitmap = null;
                    //    using (var fs = new FileStream(imgPath, FileMode.Open))
                    //    {
                    //        var img = System.Drawing.Image.FromStream(fs);
                    //        bitmap = new Bitmap(img);
                    //    }

                    //    using (var graphics = Graphics.FromImage(bitmap))
                    //    {
                    //        graphics.FillPolygon(new SolidBrush(Color.Black), curvePoints.ToArray());
                    //    }
                    //    bitmap.Save(imgPath);
                    //    bitmap.Dispose();
                    //}
                    //catch (Exception ex)
                    //{
                    //    return Content(ex.Message);
                    //}



                }

            }

            foreach (var tag in data.Tags)
            {
                if (tag.Id > 0)
                {
                    var originTag = await _context.Tags.Include(x => x.Class).SingleOrDefaultAsync(x => x.Id == tag.Id);
                    if (originTag == null)
                    {
                        return Content("Tag:" + tag.Id + " not found !");
                    }


                    try
                    {
                        await _context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        return Content(ex.ToString());
                    }

                    var newClass = _context.Classes.SingleOrDefault(x => x.Id == tag.ClassId);
                    if (originTag.Class != null && originTag.Class != newClass)
                    {
                        originTag.Class.Tags.Remove(originTag);

                        await _context.SaveChangesAsync();
                    }

                    originTag.Index = tag.Index;
                    originTag.Left = tag.Left;
                    originTag.Top = tag.Top;
                    originTag.Width = tag.Width;
                    originTag.height = tag.height;
                    originTag.Class = newClass;
                    originTag.TaggedDate = DateTime.Now;
                    if (!_context.UserTags.Any(x => x.UserId == currentUser.Id))
                    {
                        var newUserTag = new UserTag() { UserId = currentUser.Id, TagId = originTag.Id, Tag = originTag, ImageId = image.Id };
                        _context.UserTags.Add(newUserTag);
                        originTag.UsersTagged.Add(newUserTag);
                    }

                    try
                    {
                        await _context.SaveChangesAsync();
                        await ReCaculateClassAndTag(id, currentUser);
                        await UpdateProjectInfo(projId);
                    }
                    catch (Exception ex)
                    {
                        return Content(ex.ToString());
                    }

                }
                else
                {
                    var newTag = new Tag()
                    {
                        Index = tag.Index,
                        Image = image,
                        Left = tag.Left,
                        Top = tag.Top,
                        Width = tag.Width,
                        height = tag.height,
                        TaggedDate = DateTime.Now,
                        UsersTagged = new List<UserTag>()
                    };

                    if (tag.ClassId > 0)
                    {
                        var @class = _context.Classes.FirstOrDefault(x => x.Id == tag.ClassId);
                        newTag.Class = @class;
                    }
                    _context.Tags.Add(newTag);

                    var newUserTag = new UserTag()
                    {
                        UserId = currentUser.Id,
                        TagId = newTag.Id,
                        Tag = newTag,
                        ImageId = image.Id
                    };
                    _context.UserTags.Add(newUserTag);
                    newTag.UsersTagged.Add(newUserTag);

                    try
                    {
                        await _context.SaveChangesAsync();
                        await ReCaculateClassAndTag(id, currentUser);
                        await UpdateProjectInfo(projId);
                    }
                    catch (Exception ex)
                    {
                        return Content(ex.ToString());
                    }
                }

            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            return Ok("OK");

        }
        

        private async Task ReCaculateClassAndTag(Guid imageId, User user)
        {
            var image = await _context.Images.Include(x => x.Tags).ThenInclude(x => x.Class).SingleOrDefaultAsync(x => x.Id == imageId);


            if (image == null) return;



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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return;
            }
        }

        private async Task UpdateProjectInfo(Guid projectId)
        {
            var project = await _context.Projects.SingleOrDefaultAsync(x => x.Id == projectId);

            if (project == null) return;

            var images = _context.Images.Where(x => x.Project == project);

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

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                return;
            }
        }


        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tag = await _context.Tags.Include(x => x.Class).Include(x => x.Image).ThenInclude(x => x.Project).SingleOrDefaultAsync(m => m.Id == id);
            if (tag == null)
            {
                return Content("Tag not found !");
            }
            var img = tag.Image;
            var project = img.Project;

            if (tag.Class != null)
                img.TagHasClass -= 1;
            else
                img.TagNotHasClass -= 1;

            if (img.TagHasClass == 0)
                project.TotalImgNotClassed += 1;

            img.Tags.Remove(tag);

            project.TotalImgNotTagged += 1;

            tag.Class.Tags.Remove(tag);

            _context.Tags.Remove(tag);
            try
            {
                await _context.SaveChangesAsync();
                return Ok(tag);
            }
            catch (Exception e)
            {
                return Content(e.ToString());
            }
        }

        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}