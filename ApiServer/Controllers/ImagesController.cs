using System;
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
using VDS.Security;

namespace ApiServer.Controllers
{
    [Produces("application/json")]
    [Route("api/Images/[action]")]
    //[AppAuthorize(VdsPermissions.ViewImage)]
    public class ImagesController : Controller
    {
        private readonly VdsContext _context;

        private readonly IHostingEnvironment _hostingEnvironment;

        public ImagesController(VdsContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
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

        public async Task<List<Role>> GetCurrentRole(long UserId)
        {
            var userRoles = _context.UserRoles.Where(x => x.UserId == UserId);
            var result = new List<Role>();
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
            var results = new List<ImageForView>();
            if (_currentRoles.Any(x=>x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
            {
                var imgs = _context.Images.Include(x => x.Project).Where(a => a.Project.Id == id).Include(b => b.UserQc).Include(c => c.UserTagged).Skip(start).Take(stop);
                foreach (var img in imgs)
                {
                    var imgForView = new ImageForView()
                    {
                        Id = img.Id,
                        Path = img.Path.Replace('\\', '/'),
                        UserTagged = img.UserTagged != null ? img.UserTagged.UserName : null,
                        Classes = img.Classes,
                        Ignored = img.Ignored,
                        QcStatus = img.QcStatus,
                        TagHasClass = img.TagHasClass,
                        TagNotHasClass = img.TagNotHasClass,
                        TagTime = img.TagTime,
                        TotalClass = img.TotalClass,
                        UserQc = img.UserQc != null ? img.UserQc.UserName : null,
                        QcDate = img.QcDate,
                        TaggedDate = img.TaggedDate
                    };
                    results.Add(imgForView);
                }
            }
            else
            {
                var userInProject = _context.ProjectUsers.Include(x => x.User).Include(a => a.Project).Any(p => p.Project.Id == id && p.User.Id == _currentUser.Id);

                if (userInProject)
                {
                    var imgs = _context.Images.Include(x => x.Project).Where(a => a.Project.Id == id).Include(b => b.UserQc).Include(c => c.UserTagged);
                    foreach (var img in imgs)
                    {
                        var imgForView = new ImageForView()
                        {
                            Id = img.Id,
                            Path = img.Path.Replace('\\', '/'),
                            UserTagged = img.UserTagged != null ? img.UserTagged.UserName : null,
                            Classes = img.Classes,
                            Ignored = img.Ignored,
                            QcStatus = img.QcStatus,
                            TagHasClass = img.TagHasClass,
                            TagNotHasClass = img.TagNotHasClass,
                            TagTime = img.TagTime,
                            TotalClass = img.TotalClass,
                            UserQc = img.UserQc != null ? img.UserQc.UserName : null,
                            QcDate = img.QcDate,
                            TaggedDate = img.TaggedDate
                        };

                        results.Add(imgForView);
                    }
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

            if (_currentRoles.Any(x=>x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
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
        [HttpGet("{id}")]
        [ActionName("GetImageById")]
        public async Task<IActionResult> GetImageById([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var image = await _context.Images.SingleOrDefaultAsync(m => m.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            return Ok(image);
        }

        // PUT: api/Images/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] Image image)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != image.Id)
            {
                return BadRequest();
            }

            _context.Entry(image).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ImageExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Images
        [HttpPost]
        public async Task<IActionResult> AddImage([FromBody] Image image)
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

            var ids = id.Split('_');
            for (var i = 0; i < ids.Length; i++)
            {
                var img = await _context.Images.SingleOrDefaultAsync(m => m.Id == Guid.Parse(ids[i]));

                if (img == null)
                {
                    return Ok("error#Image not found");
                }
                else
                {
                    _context.Images.Remove(img);
                    await _context.SaveChangesAsync();
                    DeleteFile(img.Path);
                }
            }

            return Ok();
        }

        private void DeleteFile(string path)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            string finalPath = webRootPath + path;
            System.IO.File.Delete(finalPath);
        }

        private bool ImageExists(Guid id)
        {
            return _context.Images.Any(e => e.Id == id);
        }
    }
}