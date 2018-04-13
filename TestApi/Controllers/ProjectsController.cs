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
namespace ApiServer.Controllers
{
    [Produces("application/json")]
    [Route("api/Projects/[action]")]
    [AppAuthorize(VdsPermissions.ViewProject)]
    public class ProjectsController : Controller
    {
        private readonly VdsContext _context;

        public ProjectsController(VdsContext context)
        {
            _context = context;
        }

        public VDS.Security.User GetCurrentUser()
        {
            var project = new List<ProjectModel.ProjectForView>();
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

        public async Task<VDS.Security.Role> GetCurrentRole(long UserId)
        {
            var userRole = await _context.UserRoles.SingleOrDefaultAsync(x => x.UserId == UserId);
            return await _context.Roles.SingleOrDefaultAsync(x => x.Id == userRole.RoleId);
        }
        // GET: api/Projects
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var results = new List<ProjectModel.ProjectForView>();

            var currentUserLogin = GetCurrentUser();

            var currentRole = await GetCurrentRole(currentUserLogin.Id);

            var projs = new List<Project>();

            if (currentRole.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper()))
            {
                projs = _context.ProjectUsers
                .Include(p => p.User)
                .Include(a => a.Project)
                .Select(b => b.Project).ToList();
            }
            else
            {
                projs = _context.ProjectUsers
                .Include(p => p.User)
                .Where(x => x.User.Id == currentUserLogin.Id)
                .Include(a => a.Project)
                .Select(b => b.Project).ToList().
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
        public async Task<IActionResult> GetProject([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var project = await _context.Projects.SingleOrDefaultAsync(m => m.Id == id && !m.IsDisabled);

            if (project == null)
            {
                return NotFound();
            }

            return Ok(project);
        }

        // PUT: api/Projects/5
        [HttpPut]
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
        public async Task<IActionResult> Create([FromBody] ProjectModel.ProjectForCreate project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
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

                _context.ProjectUsers.Add(new ProjectUser()
                {
                    User = GetCurrentUser(),
                    Project = newProject
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
        public async Task<IActionResult> DeleteProject([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var currentUserLogin = GetCurrentUser();

            var currentRole = await GetCurrentRole(currentUserLogin.Id);

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
                if (!currentRole.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper()))
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