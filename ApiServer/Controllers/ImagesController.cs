﻿using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiServer.Model;
using VDS.AspNetCore.Mvc.Authorization;
using ApiServer.Core.Authorization;
using System.Net.Http;
using ApiServer.Controllers.Auth;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Security.Claims;
using static ApiServer.Model.views.ImageModel;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.SignalR;
using ApiServer.Hubs;
using ApiServer.Core.SignalR;
using ApiServer.Core.Queues;
using VDS.Security;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
namespace ApiServer.Controllers
{
    [Produces("application/json")]
    [Route("api/Images/[action]")]
    //[AppAuthorize(VdsPermissions.ViewImage)]
    public class ImagesController : Controller
    {
        private readonly VdsContext _context;

        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IHubContext<VdsHub> _hubContext;
        private readonly IUserService _userService;

        private readonly IImageQueueService _imageQueueService;
        //private IHubContext<VdsHub> _hubContext;

        public ImagesController(VdsContext context, IHostingEnvironment hostingEnvironment, IHubContext<VdsHub> hubContext, IUserService userService, IImageQueueService queueService)
        {
            _context = context;
            _hubContext = hubContext;
            _hostingEnvironment = hostingEnvironment;
            _userService = userService;
            _imageQueueService = queueService;
        }


        public User GetCurrentUser()
        {
            var identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;
            foreach (var claim in claims)
            {
                if (claim.Type == "id" && !string.IsNullOrEmpty(claim.Value))
                {
                    var userId = long.Parse(claim.Value);
                    return _context.Users.SingleOrDefault(x => x.Id == userId);
                }
            }

            return null;
        }

        public async Task<List<VDS.Security.Role>> GetCurrentRole(long UserId)
        {
            var userRoles = _context.UserRoles.Where(x => x.UserId == UserId);
            var result = new List<VDS.Security.Role>();
            foreach (var r in userRoles)
            {
                result.Add(await _context.Roles.SingleOrDefaultAsync(x => x.Id == r.RoleId));
            }
            return result;
        }

        // GET: api/Images
        [HttpGet("{id}/{start}/{stop}")]
        [ActionName("GetImage")]
        public async Task<IActionResult> GetImages([FromRoute] Guid id, [FromRoute] int start, [FromRoute] int stop)
        {
            var _currentUser = GetCurrentUser();
            var _currentRoles = await GetCurrentRole(_currentUser.Id);
            var results = new List<ImageForView2>();
            if (_currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
            {
                var imgs = _context.Images.Include(x => x.Project).Where(a => a.Project.Id == id).Include(x=>x.QuantityCheck).Include(b => b.UsersQc).Include(c => c.UsersTagged).Skip(start).Take(stop);
                foreach (var img in imgs)
                {
                    var imgForView = ImgForView(img);
                    imgForView.UserUsing = await _imageQueueService.GetUserUsing(id, img.Id);
                    results.Add(imgForView);
                }
            }
            else
            {
                var userInProject = _context.ProjectUsers.Include(x => x.User).Include(a => a.Project).Any(p => p.Project.Id == id && p.User.Id == _currentUser.Id);

                if (userInProject)
                {
                    var imgs = _context.Images.Include(x => x.Project).Where(a => a.Project.Id == id).Include(x => x.QuantityCheck).Include(b => b.UsersQc).Include(c => c.UsersTagged).Skip(start).Take(stop);
                    foreach (var img in imgs)
                    {
                        var imgForView = ImgForView(img);
                        imgForView.UserUsing = await _imageQueueService.GetUserUsing(id, img.Id);

                        results.Add(imgForView);
                    }
                }
            }
            return Ok(results);
        }
        private ImageForView2 ImgForView(Model.Image image)
        {
            var newImg = new ImageForView2()
            {
                Classes = image.Classes,
                Id = image.Id,
                Ignored = image.Ignored,
                Path = image.Path.Replace('\\', '/'),
                QcDate = image.QcDate,
                TaggedDate = image.TaggedDate,
                TagHasClass = image.TagHasClass,
                TagNotHasClass = image.TagNotHasClass,
                TagTime = Utilities.ConvertTime(TimeSpan.FromMinutes(image.TagTime)),
                TotalClass = image.TotalClass,
                QcStatus = new List<Model.views.QuantityCheckModel.Qc>(),
                UsersTagged = new List<string>(),
                UsersQc = new List<string>()
            };

            var usersTagged = _context.UserTags.Where(x => x.ImageId == image.Id).ToList();
            if(usersTagged.Count() > 0)
            {
                foreach(var userTagged in usersTagged)
                {
                    var user = _context.Users.SingleOrDefault(x => x.Id == userTagged.UserId);
                    if(user != null)
                    {
                        if (!newImg.UsersTagged.Contains(user.UserName))
                        {
                            newImg.UsersTagged.Add(user.UserName);
                        }
                    }
                }
                
            }

            if(image.QuantityCheck != null)
            {
                var qcUsers = _context.UserQuantityChecks.Where(x => x.QuantityCheckId == image.QuantityCheck.Id);
                if (qcUsers.Count() > 0)
                {
                    foreach (var qcUser in qcUsers)
                    {
                        var user = _context.Users.SingleOrDefault(x => x.Id == qcUser.UserId);
                        if (user != null)
                        {
                            newImg.UsersQc.Add(user.UserName);
                        }
                    }
                }

            }

            if (string.IsNullOrEmpty(image.QcStatus)) return newImg;

            var status = image.QcStatus.Split(';');
            for (var i = 0; i < status.Count(); i++)
            {
                if (string.IsNullOrEmpty(status[i])) continue;
                var lv = status[i].Split(':').First();
                var val = status[i].Split(':').Last().Trim().Equals("passed") ? true : false;
                newImg.QcStatus.Add(new Model.views.QuantityCheckModel.Qc()
                {
                    Level = lv,
                    value = val
                });
            }

            return newImg;
        }

        [HttpGet("{id}")]
        [ActionName("GetImageListId")]
        public async Task<IActionResult> GetImageListId([FromRoute] Guid id)
        {
            var project = await _context.Projects.SingleOrDefaultAsync(x => x.Id == id);
            if (project == null) { return Content("Project not found !"); }

            var imgList = _context.Images.Where(x => x.Project == project).Select(x => x.Id);
            if (imgList.Any())
            {
                return Content("Unknow Error !");
            }
            return Ok(imgList);
        }
        [HttpGet("{id}")]
        [ActionName("GetTotal")]
        public async Task<IActionResult> GetTotal([FromRoute] Guid id)
        {
            var _currentUser = GetCurrentUser();
            var _currentRoles = await GetCurrentRole(_currentUser.Id);
            var results = 0;
            if (_currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
            {
                results = _context.Images.Include(x => x.Project).Where(a => a.Project.Id == id).Include(b => b.UsersQc).Include(c => c.UsersTagged).Count();
            }
            else
            {
                var userInProject = _context.ProjectUsers.Include(x => x.User).Include(a => a.Project).Any(p => p.Project.Id == id && p.User.Id == _currentUser.Id);

                if (userInProject)
                {
                    results = _context.Images.Include(x => x.Project).Where(a => a.Project.Id == id).Include(b => b.UsersQc).Include(c => c.UsersTagged).Count();
                }
            }
            return Ok(results);
        }

        [HttpGet("{id}/{projId}")]
        [ActionName("GetImageBinary")]
        public async Task<HttpResponseMessage> GetImageBinary([FromRoute] Guid id, [FromRoute] Guid projId)
        {
            HttpResponseMessage response = null;
            var _currentUser = GetCurrentUser();
            var _currentRoles = await GetCurrentRole(_currentUser.Id);
            var results = new List<ImageForView>();
            string imgPath = string.Empty;

            if (_currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
            {

                imgPath = await _context.Images.Where(x => x.Id == id).Select(a => a.Path).FirstOrDefaultAsync();
            }
            else
            {
                var userInProject = _context.ProjectUsers.Include(x => x.User).Include(a => a.Project).Any(p => p.Project.Id == id && p.User.Id == _currentUser.Id);

                if (userInProject)
                {
                    imgPath = await _context.Images.Include(x => x.Project).Where(a => a.Project.Id == projId && a.Id == id).Select(a => a.Path).FirstOrDefaultAsync();
                }
                else
                {
                    return new HttpResponseMessage(HttpStatusCode.Gone);
                }
            }
            try
            {
                var fStream = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
                response = new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StreamContent(fStream)
                };
                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
                {
                    FileName = Path.GetFileName(fStream.Name)
                };
                string mimeType = Path.GetExtension(imgPath).Split('.')[1];
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/" + mimeType);
            }
            catch (Exception ex)
            {
                return new HttpResponseMessage(HttpStatusCode.Gone);
            }


            return response;
            //response.Headers.Add("content-type", "application/octet-stream");
        }
        // GET: api/Images/5
        [HttpGet("{userId}/{projId}/{imgId}")]
        [ActionName("GetNextImage")]
        public IActionResult GetNextImage([FromRoute] long userId, [FromRoute] Guid projId, [FromRoute] Guid imgId)
        {
            var project = _context.Projects.SingleOrDefaultAsync(x => x.Id == projId);
            if (project == null) return Content("project not found!");

            var image = _imageQueueService.GetImage(projId, imgId, userId);
            //var image = await ImageQueues.GetImage(projId, imgId, userId);

            if (image == null || image.Id.ToString().Equals("00000000-0000-0000-0000-000000000000"))
            {
                return Content("Image is using or not exsit!");
            }

            return Ok(image);

        }

        [HttpGet("{userId}/{projId}/{imgId}")]
        [ActionName("GetImageById")]
        public IActionResult GetImageById([FromRoute] long userId, [FromRoute] Guid projId, [FromRoute] Guid imgId)
        {
            var project = _context.Projects.SingleOrDefault(x => x.Id == projId);
            if (project == null) return Content("project not found");
;            var image = _imageQueueService.GetImageById(projId, imgId, userId);

            //ImageQueues.Append(userId, project.Id, _context, _hubContext);

            //var image = await ImageQueues.GetImageById(projId, imgId, userId);

            if (image == null || image.Id.ToString().Equals("00000000-0000-0000-0000-000000000000"))
            {
                return Content("Image is using or not exsit!");
            }

            return Ok(image);

        }

        [HttpDelete("{userId}/{projId}/{imgId}")]
        [ActionName("ReleaseImage")]
        public async Task<IActionResult> ReleaseImage([FromRoute] long userId, [FromRoute] Guid projId, [FromRoute] Guid imgId)
        {
            if (!ModelState.IsValid)
            {
                return NotFound();
            }

            //if (await ImageQueues.ReleaseImage(projId, imgId))
            if(await _imageQueueService.ReleaseImage(projId, imgId))
            {

                return Ok("Ok");
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost("{projId}/{imgId}")]
        [ActionName("Ping")]
        public IActionResult Ping([FromRoute] Guid projId, [FromRoute] Guid imgId)
        {
            //ImageQueues.SetTimePing(projId, imgId, DateTime.Now);
            _imageQueueService.SetTimePing(projId,imgId,DateTime.Now);
            return Ok();
        }
        // PUT: api/Images/5
        [HttpPut("{id}")]
        [ActionName("UpdateTaggedTime")]
        public async Task<IActionResult> UpdateTaggedTime([FromRoute] Guid id, [FromBody] double taggedTime)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _userService.GetCurrentUser(identity);

            var image = _context.Images.Include(x=>x.UserTaggedTimes).SingleOrDefault(x => x.Id == id);

            if (image == null) return Content("Image not found!");
            
            if(image.UserTaggedTimes.Count() <= 0)
            {
                var newUserTaggedTime = new UserTaggedTime()
                {
                    ImageId = image.Id,
                    UserId = currentUser.Id,
                    TaggedTime = taggedTime,
                    Image = image,
                };
                _context.userTaggedTimes.Add(newUserTaggedTime);
            }
            else
            {
                var currentUserTaggedTime = image.UserTaggedTimes.SingleOrDefault(x => x.UserId == currentUser.Id);
                if(currentUserTaggedTime == null)
                {
                    var newUserTaggedTime = new UserTaggedTime()
                    {
                        ImageId = image.Id,
                        UserId = currentUser.Id,
                        TaggedTime = taggedTime,
                        Image = image,
                    };
                    _context.userTaggedTimes.Add(newUserTaggedTime);
                }
                else
                {
                    currentUserTaggedTime.TaggedTime = taggedTime;
                }
            }

            double totalTaggedTime = 0;
            foreach(var userTaggedTime in image.UserTaggedTimes)
            {
                totalTaggedTime += userTaggedTime.TaggedTime;
            }
            image.TagTime = totalTaggedTime;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Content(ex.Message);
            }
        }

        // POST: api/Images
        [HttpPost]
        public async Task<IActionResult> AddImage([FromBody] Model.Image image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetImage", new { id = image.Id }, image);
        }

        // DELETE: api/Images/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserLogin = GetCurrentUser();

            var currentRole = await GetCurrentRole(currentUserLogin.Id);

            Project project = new Project();

            var ids = id.Split('_');
            for (var i = 0; i < ids.Length; i++)
            {
                var img = await _context.Images.Include(x => x.Project).Include(x => x.Tags).Include(x=>x.QuantityCheck).SingleOrDefaultAsync(m => m.Id == Guid.Parse(ids[i]));
                if (img == null)
                {
                    return Ok("error#Image not found");
                }
                else
                {
                    project = img.Project;
                    try
                    {
                        foreach (var tag in img.Tags)
                        {
                            var t = await _context.Tags.SingleOrDefaultAsync(x => x.Id == tag.Id);
                            await _context.SaveChangesAsync();
                        }
                        _context.Tags.RemoveRange(img.Tags);

                        if(img.QuantityCheck != null)
                            _context.QuantityChecks.Remove(img.QuantityCheck);

                        _context.Images.Remove(img);

                        await _context.SaveChangesAsync();
                        await _imageQueueService.DeleteImage(project.Id, img.Id);
                        DeleteFile(img.Path);
                    }
                    catch (Exception ex)
                    {
                        return Content("Cant delete image !" + ex.Message);
                    }
                }
            }
            try
            {
                await UpdateProjectInfo(project.Id);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }


            return Ok();
        }

        private async Task UpdateProjectInfo(Guid projectId)
        {
            var project = await _context.Projects.SingleOrDefaultAsync(x => x.Id == projectId);

            if (project == null) return;

            var images = _context.Images.Where(x => x.Project == project);
            project.TotalImg = images == null ? 0 : images.Count();
            var imgsNotClassed = project.TotalImg;
            var imgsNotTagged = project.TotalImg;
            var imgsNotQc = project.TotalImg;
            var imgsQc = 0;
            if (images != null && images.Count() > 0)
            {
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
            }

            project.TotalImgNotClassed = imgsNotClassed;
            project.TotalImgNotTagged = imgsNotTagged;
            project.TotalImgNotQC = imgsNotQc;
            project.TotalImgQC = imgsQc;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void DeleteFile(string path)
        {
            try
            {
                string webRootPath = _hostingEnvironment.WebRootPath;
                string finalPath = webRootPath + path;
                System.IO.File.Delete(finalPath);
            }
            catch(Exception)
            {
                return;
            }
            
        }

        private bool ImageExists(Guid id)
        {
            return _context.Images.Any(e => e.Id == id);
        }
    }
}