using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiServer.Model;
using ApiServer.Model.views;
using VDS.AspNetCore.Mvc.Authorization;
using ApiServer.Core.Authorization;
using System.Security.Claims;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using VDS.Security;
using System.Net.Http.Headers;
using System.IO.Compression;

namespace ApiServer.Controllers
{
    [Produces("application/json")]
    [Route("api/Projects/[action]")]
    [AppAuthorize(VdsPermissions.ViewProject)]
    public class ProjectsController : Controller
    {
        private readonly VdsContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;

        public ProjectsController(VdsContext context, IHostingEnvironment hostingEnvironment)
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
            foreach(var r in userRoles)
            {
                result.Add(await _context.Roles.SingleOrDefaultAsync(x => x.Id == r.RoleId));
            }
            return result;
        }

        [HttpPost("{id}"), DisableRequestSizeLimit]
        [ActionName("UploadImage")]
        public async Task<IActionResult> UploadImage([FromRoute] Guid id)
        {
            string[] AllowedFileExtensions = new string[] { "jpg", "png", "bmp", "zip"};
            try
            {
                var currentUser = GetCurrentUser();

                var project = await _context.ProjectUsers
                    .Include(x => x.Project)
                    .Include(u => u.User)
                    .Where(a => a.User.Id == currentUser.Id && a.Project.Id == id).Select(b => b.Project).FirstOrDefaultAsync();
                if (project == null)
                {
                    return Content("You have no permission to upload images in this project!");
                }

                var files = Request.Form.Files;
                foreach(var file in files)
                {
                    if (file.Length <= 0)
                    {
                        return Content("There are no files to upload !");
                    }

                    string folderName = DateTime.UtcNow.ToString("dd-MM-yyy", CultureInfo.InvariantCulture);
                    string tempFolderName = "temp";
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    string projectFolder = project.Name + "\\" + folderName;
                    string newPath = Path.Combine(webRootPath, projectFolder);
                    string pathToDatabase = "\\" +projectFolder + "\\";
                    string fileExtension = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"').Split('.')[1];

                    if (!AllowedFileExtensions.Contains(fileExtension.ToLower()))
                    {
                        return Content("File not allowed !");
                    }

                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                    string name = Guid.NewGuid().ToString();
                    string fileName = Guid.NewGuid().ToString() + "." + fileExtension;
                    pathToDatabase += fileName;
                    if (fileExtension.ToLower().Equals("zip"))
                    {
                        if (!Directory.Exists(webRootPath + "\\" + tempFolderName))
                        {
                            Directory.CreateDirectory(webRootPath + "\\" + tempFolderName);
                        }

                        string tempPath = Path.Combine(webRootPath + "\\" + tempFolderName, fileName);

                        using (var stream = new FileStream(tempPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        using (ZipArchive archive = ZipFile.OpenRead(tempPath))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                string[] filenames = entry.FullName.Split('.');
                                if (AllowedFileExtensions.Contains(filenames.Last().ToLower()))
                                {
                                    entry.ExtractToFile(Path.Combine(newPath, name + '.' + filenames.Last()));

                                    await StoreImage(name, pathToDatabase, project);
                                }
                            }
                        }
                        System.IO.File.Delete(tempPath);
                    }
                    else
                    {
                        string fullPath = Path.Combine(newPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                            await StoreImage(name, pathToDatabase, project);
                        }
                    }
                }

                return Ok("OK");
            }
            catch (Exception err)
            {
                return Content("error@" + err);
            }
        }

        public async Task StoreImage(string filename, string filepath, Project proj)
        {
            var newImg = new Image
            {
                Id = Guid.Parse(filename),
                Ignored = false,
                Path = filepath,
                Project = proj
            };

            try
            {
                _context.Images.Add(newImg);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

            }
        }

        // GET: api/Projects
        [HttpGet]
        [ActionName("GetProjects")]
        public async Task<IActionResult> GetProjects()
        {
            var results = new List<ProjectModel.ProjectForView>();

            var currentUserLogin = GetCurrentUser();

            var currentRoles = await GetCurrentRole(currentUserLogin.Id);

            var projs = new List<Project>();
            
            if (currentRoles.Any(x=>x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
            {
                projs = _context.ProjectUsers
                .Include(p => p.User)
                .Include(a => a.Project)
                .Select(b => b.Project).Distinct().ToList();
            }
            else
            {
                projs = _context.ProjectUsers
                .Include(p => p.User)
                .Where(x => x.User.Id == currentUserLogin.Id)
                .Include(a => a.Project)
                .Select(b => b.Project).Distinct().ToList().
                Where(x => !x.IsDisabled).ToList();
            }
            
            if (projs.Count() <= 0)
            {
                return Ok(projs);
            }
            else
            {
                foreach (var p in projs)
                {
                    var users = _context.ProjectUsers.Where(x => x.Project.Id == p.Id).Include(u => u.User).Select(u => u.User).ToList();
                    var usernames = string.Empty;

                    for(var i=0; i < users.Count(); i++)
                    {
                        if(i != (users.Count() - 1))
                        {
                            usernames += users[i].UserName + "; ";
                        }
                        else
                        {
                            usernames += users[i].UserName;
                        }
                        
                    }

                    results.Add(new ProjectModel.ProjectForView()
                    {
                        Id = p.Id,
                        Description = p.Description,
                        IsDisabled = p.IsDisabled,
                        Name = p.Name,
                        Note = p.Note,
                        TotalImg = p.TotalImg,
                        TotalImgNotClassed = p.TotalImgNotClassed,
                        TotalImgNotQC = p.TotalImgNotQC,
                        TotalImgNotTagged = p.TotalImgNotTagged,
                        TotalImgQC = p.TotalImgQC,
                        Usernames = usernames
                    });
                }

                return Ok(results);
            }
        }

        [HttpGet("{name}")]
        [ActionName("GetProjectByName")]
        public async Task<IActionResult> GetProjectByName([FromRoute] string name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var project = await _context.Projects.SingleOrDefaultAsync(m => m.Name == name && !m.IsDisabled);

            if (project == null)
            {
                return Ok();
            }

            return Ok(project);
        }

        // GET: api/Projects/5
        [HttpGet("{id}")]
        [ActionName("GetProject")]
        public async Task<IActionResult> GetProject([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var p = await _context.Projects.SingleOrDefaultAsync(m => m.Id == Guid.Parse(id));

            var users = _context.ProjectUsers.Where(x => x.Project.Id == p.Id).Include(u => u.User).Select(u => u.User).ToList();
            var usernames = string.Empty;

            for (var i = 0; i < users.Count(); i++)
            {
                if (i != (users.Count() - 1))
                {
                    usernames += users[i].UserName + "; ";
                }
                else
                {
                    usernames += users[i].UserName;
                }

            }

            var results = new ProjectModel.ProjectForView()
            {
                Id = p.Id,
                Description = p.Description,
                IsDisabled = p.IsDisabled,
                Name = p.Name,
                Note = p.Note,
                TotalImg = p.TotalImg,
                TotalImgNotClassed = p.TotalImgNotClassed,
                TotalImgNotQC = p.TotalImgNotQC,
                TotalImgNotTagged = p.TotalImgNotTagged,
                TotalImgQC = p.TotalImgQC,
                Usernames = usernames
            };
            if (p == null)
            {
                return NotFound();
            }

            return Ok(results);
        }

        // PUT: api/Projects/5
        [HttpPut]
        [ActionName("Update")]
        public async Task<IActionResult> Update([FromBody] ProjectModel.ProjectForUpdate project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var oldProject = _context.Projects.SingleOrDefault(x => x.Id == project.Id);

            if (oldProject == null)
            {
                return NotFound();
            }

            oldProject.Name = project.Name;
            oldProject.Description = project.Description;
            oldProject.Note = project.Note;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectExists(project.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(project);
        }

        // POST: api/Projects
        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> Create([FromBody] ProjectModel.ProjectForCreate project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var currentUserLogin = GetCurrentUser();

                var currentRoles = await GetCurrentRole(currentUserLogin.Id);

                var newProject = new Project
                {
                    Name = project.Name,
                    Description = project.Description,
                    IsDisabled = false,
                    Note = project.Note,
                    TotalImg = 0,
                    TotalImgNotClassed = 0,
                    TotalImgNotQC = 0,
                    TotalImgNotTagged = 0,
                    TotalImgQC = 0
                };
                _context.Projects.Add(newProject);

                await _context.SaveChangesAsync();
                if (currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
                {
                    _context.ProjectUsers.Add(new ProjectUser()
                    {
                        User = GetCurrentUser(),
                        Project = newProject,
                        Role = currentRoles.SingleOrDefault(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper()))
                    });
                    await _context.SaveChangesAsync();

                    return CreatedAtAction("GetProject", new { id = newProject.Id }, newProject);
                }

                _context.ProjectUsers.Add(new ProjectUser()
                {
                    User = GetCurrentUser(),
                    Project = newProject,
                    Role = currentRoles.SingleOrDefault(x => x.ProjectRole)
                });

                await _context.SaveChangesAsync();

                return CreatedAtAction("GetProject", new { id = newProject.Id }, newProject);
            }
            catch (Exception)
            {
                return BadRequest();
            }



        }

        // DELETE: api/Projects/5
        [HttpDelete("{id}")]
        [ActionName("DeleteProject")]
        public async Task<IActionResult> DeleteProject([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var currentUserLogin = GetCurrentUser();

            var currentRoles = await GetCurrentRole(currentUserLogin.Id);

            var ids = id.Split('_');
            var projectUsers = new List<ProjectUser>();
            for (var i = 0; i < ids.Length; i++)
            {
                var projectUser = await _context.ProjectUsers.Include(x => x.Project).SingleOrDefaultAsync(m => m.Project.Id == Guid.Parse(ids[i]));

                if (projectUser == null)
                {
                    return Ok("error#Project not found");
                }
                else
                {
                    var project = await _context.Projects.SingleOrDefaultAsync(x => x.Id == Guid.Parse(ids[i]));

                    project.IsDisabled = true;

                    await _context.SaveChangesAsync();
                }
                projectUsers.Add(projectUser);
            }

            try
            {
                if (!currentRoles.Any(x=>x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
                {
                    _context.ProjectUsers.RemoveRange(projectUsers);
                }

                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception err)
            {
                return Ok("error#There was an error in delete proccess, please try again late!");
            }
        }

        private bool ProjectExists(Guid id)
        {
            return _context.Projects.Any(e => e.Id == id);
        }
    }
}