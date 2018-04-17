using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiServer.Model;
using static ApiServer.Model.views.ProjectUserModel;
using System.Security.Claims;
using VDS.AspNetCore.Mvc.Authorization;
using ApiServer.Core.Authorization;

namespace ApiServer.Controllers
{
    [Produces("application/json")]
    [Route("api/ProjectUsers/[action]")]
    [AppAuthorize(VdsPermissions.ViewProject)]
    public class ProjectUsersController : Controller
    {
        private readonly VdsContext _context;

        public ProjectUsersController(VdsContext context)
        {
            _context = context;
        }

        public VDS.Security.User GetCurrentUser()
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

        // GET: api/ProjectUsers
        [HttpGet("{id}/{start}/{stop}")]
        [ActionName("GetProjectUsers")]
        public IActionResult GetProjectUsers([FromRoute] Guid id, [FromRoute] int start, [FromRoute] int stop)
        {
            var projUsers = _context.ProjectUsers.Include(c=>c.Project).Where(x => x.Project.Id == id).Include(a=>a.Role).Include(b=>b.User).Skip(start).Take(stop);

            var results = new List<ProjectUserForView>();

            foreach(var p in projUsers)
            {
                if (p.Role.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper()))
                {
                    continue;
                }
                else
                {
                    results.Add(new ProjectUserForView()
                    {
                        Id = p.Id,
                        UserName = p.User.UserName,
                        RoleName = p.Role.RoleName,
                    });
                }
            }
            return Ok(results);
        }

        // GET: api/ProjectUsers/5
        [HttpGet("{id}")]
        [ActionName("GetProjectUser")]
        public async Task<IActionResult> GetProjectUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var projectUser = await _context.ProjectUsers.Include(x=>x.Role).Include(a=>a.User).SingleOrDefaultAsync(m => m.Id == id);

            if (projectUser == null)
            {
                return Content("Not found!");
            }

            return Ok(projectUser);
        }

        // PUT: api/ProjectUsers/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ProjectUserForView projectUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != projectUser.Id)
            {
                return BadRequest();
            }

            var originProjectUser = await _context.ProjectUsers.SingleOrDefaultAsync(x => x.Id == id);
            if(originProjectUser == null)
            {
                return Content("Not esxit in database!");
            }

            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == projectUser.UserName);
            if(user == null)
            {
                return Content("User not found!");
            }

            var role = await _context.Roles.SingleOrDefaultAsync(x => x.RoleName == projectUser.RoleName);
            if(role == null)
            {
                return Content("Role not found!");
            }
            var RoleInUserRole = await _context.UserRoles.SingleOrDefaultAsync(x => x.RoleId == role.Id);

            if (RoleInUserRole == null)
            {
                return Content("Role not found!");
            }

            var currentUser = GetCurrentUser();
            try
            {
                originProjectUser.User = user;
                originProjectUser.Role = role;
                RoleInUserRole.RoleId = role.Id;

                await _context.SaveChangesAsync();


            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProjectUserExists(id))
                {
                    return Content("Not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/ProjectUsers
        [HttpPost]
        public async Task<IActionResult> AddProjectUser([FromBody] ProjectUserForAdd projectUser)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var project = await _context.Projects.SingleOrDefaultAsync(x => x.Id == projectUser.Id);
            if (project == null)
            {
                return Content("Not esxit in database!");
            }

            var user = await _context.Users.SingleOrDefaultAsync(x => x.UserName == projectUser.UserName);
            if (user == null)
            {
                return Content("User not found!");
            }

            var role = await _context.Roles.SingleOrDefaultAsync(x => x.RoleName == projectUser.RoleName);
            if (role == null)
            {
                return Content("Role not found!");
            }
            var RoleForUser = await _context.UserRoles.SingleOrDefaultAsync(x => x.RoleId == role.Id);

            if (RoleForUser == null)
            {
                return Content("Cant found role!");
            }

            var currentUser = GetCurrentUser();
            var isRoleEsxitForUser = _context.UserRoles.Any(x => x.RoleId == role.Id);

            try
            {
                var newProjectUser = new ProjectUser() { User = user, Project = project, Role = role };

                _context.ProjectUsers.Add(newProjectUser);

                _context.UserRoles.Add(new VDS.Security.UserRole() { CreationTime = DateTime.Now, CreatorUserId = currentUser.Id, UserId = user.Id, RoleId = role.Id });
                
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetProjectUser", new { id = newProjectUser.Id }, newProjectUser);
            }
            catch (Exception er)
            {
                return Content(er.Message);
            }
        }

        // DELETE: api/ProjectUsers/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProjectUser([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var projectUser = await _context.ProjectUsers.Include(x=>x.Role).SingleOrDefaultAsync(m => m.Id == id);
            if (projectUser == null)
            {
                return Content("Not Found !");
            }
            var role = await _context.Roles.SingleOrDefaultAsync(x => x.Id == projectUser.Role.Id);
            if (role == null)
            {
                return Content("Role not found!");
            }
            var RoleForUser = await _context.UserRoles.SingleOrDefaultAsync(x => x.RoleId == role.Id);

            if (RoleForUser == null)
            {
                return Content("Cant found role!");
            }

            var currentUser = GetCurrentUser();
            try
            {
                _context.ProjectUsers.Remove(projectUser);

                _context.UserRoles.Remove(RoleForUser);

                await _context.SaveChangesAsync();

                return Ok(projectUser);

            }catch(Exception ex)
            {
                return Content(ex.Message);
            }
        }

        private bool ProjectUserExists(int id)
        {
            return _context.ProjectUsers.Any(e => e.Id == id);
        }
    }
}