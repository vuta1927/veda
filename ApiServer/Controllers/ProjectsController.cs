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
using System.Net.Http.Headers;
using System.IO.Compression;
using VDS.Security;
using ApiServer.Core;
using ApiServer.Core.Queues;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing.Drawing;

namespace ApiServer.Controllers
{
    [Produces("application/json")]
    [Route("api/Projects/[action]")]
    [AppAuthorize(VdsPermissions.ViewProject)]
    public class ProjectsController : Controller
    {
        private readonly VdsContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IUserService _userService;

        public ProjectsController(VdsContext context, IHostingEnvironment hostingEnvironment, IUserService userService)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _userService = userService;
        }

        [HttpPost("{id}"), DisableRequestSizeLimit]
        [ActionName("UploadImage")]
        public async Task<IActionResult> UploadImage([FromRoute] Guid id)
        {
            string[] AllowedFileExtensions = new string[] { "jpg", "png", "bmp", "zip", "rar" };
            try
            {

                var identity = (ClaimsIdentity)User.Identity;
                var currentUser = _userService.GetCurrentUser(identity);
                var _currentRoles = _userService.GetCurrentRole(currentUser.Id);
                var project = new Project();
                if (_currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
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
                {
                    return Content("You have no permission to upload images in this project!");
                }

                var files = Request.Form.Files;
                foreach (var file in files)
                {
                    if (file.Length <= 0)
                    {
                        return Content("There are no files to upload !");
                    }

                    string folderName = DateTime.UtcNow.ToString("dd-MM-yyy", CultureInfo.InvariantCulture);
                    string tempFolderName = "temp";
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    string projectFolder = project.Name + "/" + folderName;

                    var projectPath = webRootPath + "/" + project.Name;
                    var newPath = projectPath + "/" + folderName;

                    if (!Directory.Exists(webRootPath + "/" + project.Name))
                    {
                        Directory.CreateDirectory(projectPath);
                    }

                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }


                    string pathToDatabase = "/" + projectFolder + "/";
                    string fileExtension = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"').Split('.')[1];

                    if (!AllowedFileExtensions.Contains(fileExtension.ToLower()))
                    {
                        return Content("File not allowed !");
                    }
                    
                    if (fileExtension.ToLower().Equals("zip") || fileExtension.ToLower().Equals("rar"))
                    {
                        if (!Directory.Exists(webRootPath + "/" + tempFolderName))
                        {
                            Directory.CreateDirectory(webRootPath + "/" + tempFolderName);
                        }

                        string fileZipName = Guid.NewGuid().ToString() + "." + fileExtension;

                        string tempPath = Path.Combine(webRootPath + "/" + tempFolderName, fileZipName);

                        using (var stream = new FileStream(tempPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        using (ZipArchive archive = ZipFile.OpenRead(tempPath))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                string name = Guid.NewGuid().ToString();
                                string[] filenames = entry.FullName.Split('.');
                                var newPathToDatabase = pathToDatabase + name + "." + filenames.Last();
                                var nPath = Path.Combine(newPath, name + '.' + filenames.Last());
                                if (AllowedFileExtensions.Contains(filenames.Last().ToLower()))
                                {
                                    entry.ExtractToFile(nPath);

                                    using (var img = SixLabors.ImageSharp.Image.Load(nPath))
                                    {
                                        await AddImageToDatabase(name, newPathToDatabase, project, img.Width, img.Height);
                                    }
                                    //using (var entryStram = entry.Open())
                                    //{

                                    //}
                                    //using (var img = new System.Drawing.Bitmap(entryStram))
                                    //{
                                    //    await AddImageToDatabase(name, newPathToDatabase, project, img);
                                    //}
                                }
                            }
                        }
                        System.IO.File.Delete(tempPath);
                    }
                    else
                    {
                        string name = Guid.NewGuid().ToString();
                        string fileName = Guid.NewGuid().ToString() + "." + fileExtension;
                        pathToDatabase += fileName;
                        string fullPath = Path.Combine(newPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }
                        using (var img = SixLabors.ImageSharp.Image.Load(fullPath))
                        {
                            await AddImageToDatabase(name, pathToDatabase, project, img.Width, img.Height);
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

        public async Task AddImageToDatabase(string filename, string filepath, Project proj, double width, double height)
        {
            var newImg = new Model.Image
            {
                Id = Guid.Parse(filename),
                Ignored = false,
                Path = filepath,
                Project = proj,
                Width = width,
                Height = height
            };

            try
            {
                _context.Images.Add(newImg);
                proj.TotalImg += 1;
                proj.TotalImgNotTagged += 1;
                proj.TotalImgNotClassed += 1;
                proj.TotalImgNotQC += 1;
                proj.TotalImgQC = 0;
                await _context.SaveChangesAsync();

                ImageQueues.AddImage(proj.Id, newImg.Id);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // GET: api/Projects
        [HttpGet("{start}/{stop}")]
        [ActionName("GetProjects")]
        public IActionResult GetProjects([FromRoute] int start, [FromRoute]int stop)
        {
            var results = new List<ProjectModel.ProjectForView>();


            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _userService.GetCurrentUser(identity);
            var currentRoles = _userService.GetCurrentRole(currentUser.Id);

            var projs = new List<Project>();

            if (currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
            {
                projs = _context.Projects.Include(x => x.Users).OrderBy(x => x.Name).Skip(start).Take(stop).ToList();
            }
            else
            {
                projs = _context.ProjectUsers
                .Include(p => p.User)
                .Where(x => x.User.Id == currentUser.Id)
                .Include(a => a.Project)
                .Select(b => b.Project).Distinct().ToList().
                Where(x => !x.IsDisabled).Skip(start).Take(stop).ToList();
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
                    if (users != null || users.Count() > 0)
                    {
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

        [HttpGet]
        [ActionName("GetProjectNames")]
        public IActionResult GetProjectNames()
        {
            var project = (from x in _context.Projects select new ProjectModel.ProjectSetting { Name = x.Name, Id = x.Id }).Distinct();
            return Ok(project);
        }

        [HttpGet]
        [ActionName("GetProjectsByUser")]
        public IActionResult GetProjectsByUser()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _userService.GetCurrentUser(identity);
            var currentRoles = _userService.GetCurrentRole(currentUser.Id);

            var result = new List<ProjectModel.ProjectSetting>();

            var projs = new List<Project>();

            if (currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
            {
                projs = _context.Projects.Include(x => x.Users).OrderBy(x => x.Name).ToList();
            }
            else
            {
                projs = _context.ProjectUsers
                .Include(p => p.User)
                .Where(x => x.User.Id == currentUser.Id)
                .Include(a => a.Project)
                .Select(b => b.Project).Distinct().ToList().
                Where(x => !x.IsDisabled).ToList();
            }

            if (projs.Count() <= 0)
            {
                return Ok();
            }
            else
            {
                foreach (var p in projs)
                {
                    result.Add(new ProjectModel.ProjectSetting()
                    {
                        Name = p.Name,
                        Id = p.Id
                    });
                }

                return Ok(result);
            }
        }

        [HttpGet]
        [ActionName("GetTotal")]
        public IActionResult GetTotal()
        {
            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _userService.GetCurrentUser(identity);
            var currentRoles = _userService.GetCurrentRole(currentUser.Id);

            var result = 0;
            if (currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
            {
                result = _context.ProjectUsers
                .Include(p => p.User)
                .Include(a => a.Project)
                .Select(b => b.Project).Distinct().OrderBy(x => x.Name).Count();
            }
            else
            {
                result = _context.ProjectUsers
                .Include(p => p.User)
                .Where(x => x.User.Id == currentUser.Id)
                .Include(a => a.Project)
                .Select(b => b.Project).Distinct().ToList().
                Where(x => !x.IsDisabled).Count();
            }
            return Ok(result);
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

                var identity = (ClaimsIdentity)User.Identity;
                var currentUser = _userService.GetCurrentUser(identity);
                var currentRoles = _userService.GetCurrentRole(currentUser.Id);

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

                var projectSetting = new ProjectSetting()
                {
                    QuantityCheckLevel = Constants.QuantityCheckDefaultLevel,
                    Project = newProject,
                    TaggTimeValue = Constants.TaggTimeDefaultValue
                };

                _context.ProjectSettings.Add(projectSetting);

                await _context.SaveChangesAsync();
                if (currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
                {
                    var newProjectUser = new ProjectUser()
                    {
                        //User = currentUser,
                        UserId = currentUser.Id,
                        Project = newProject,
                        RoleId = currentRoles.SingleOrDefault(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())).Id
                    };
                    _context.ProjectUsers.Add(newProjectUser);
                    await _context.SaveChangesAsync();

                    return CreatedAtAction("GetProject", new { id = newProject.Id }, newProject);
                }

                else
                {
                    var newProjectUser = new ProjectUser()
                    {
                        //User = currentUser,
                        UserId = currentUser.Id,
                        Project = newProject,
                        RoleId = currentRoles.SingleOrDefault(x => x.ProjectRole).Id
                    };
                    _context.ProjectUsers.Add(newProjectUser);
                }

                await _context.SaveChangesAsync();

                return CreatedAtAction("GetProject", new { id = newProject.Id }, newProject);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
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


            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _userService.GetCurrentUser(identity);
            var currentRoles = _userService.GetCurrentRole(currentUser.Id);

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
                if (!currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
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