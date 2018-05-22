using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using VDS.AspNetCore.Mvc.Authorization;
using ApiServer.Model.views;
using ApiServer.Model;
using ApiServer.Core.Authorization;
using System.Security.Claims;

namespace ApiServer.Controllers
{
    [AppAuthorize]
    [Produces("application/json")]
    [Route("api/Dashboard/[action]")]
    public class DashboardController : Controller
    {
        private readonly VdsContext _context;
        private readonly IUserService _userService;
        public DashboardController(VdsContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }
        [HttpGet("{id}")]
        public IActionResult GetDataProject([FromRoute] Guid id)
        {
            var project = _context.Projects.SingleOrDefault(x => x.Id == id);

            if (project == null) return Content("Project not found");

            var imgs = _context.Images.Include(x => x.Project).Include(x=>x.UsersTagged).Include(x=>x.UsersQc).Include(x => x.Tags).Where(x => x.Project == project);

            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _userService.GetCurrentUser(identity);
            var _currentRoles = _userService.GetCurrentRole(currentUser.Id);
            var projectUsers = new List<DashboardModel.UserProject>();

            if (_currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper()) || x.NormalizedRoleName.Equals("PROJECTMANAGER")))
            {
                var pus = _context.ProjectUsers.Where(x => x.ProjectId == id).Include(x => x.User);
                if (pus.Count() > 0)
                {
                    foreach (var pu in pus)
                    {
                        var totalTags = _context.Tags.Include(x => x.UsersTagged).Where(x => x.UsersTagged.Any(a=>a.User == pu.User)).Count();
                        var totalQcs = _context.QuantityChecks.Include(x => x.UsersQc).Where(x => x.UsersQc.Any(a=>a.User == pu.User)).Count();

                        var newPu = new DashboardModel.UserProject()
                        {
                            Email = pu.User.Email,
                            UserId = pu.UserId,
                            UserName = pu.User.UserName,
                            RoleNames = _userService.GetCurrentRole(pu.User.Id).Select(x=>x.RoleName).ToArray(),
                            TotalTags = totalTags,
                            TotalQcs = totalQcs,
                            TaggedTime = 0
                        };

                        var userTaggedTimes = _context.userTaggedTimes.Where(x => x.User == pu.User);
                        if(userTaggedTimes.Count() > 0)
                        {
                            foreach(var d in userTaggedTimes)
                            {
                                newPu.TaggedTime += d.TaggedTime;
                            }
                            
                        }
                        

                        projectUsers.Add(newPu);
                    }
                    
                }

            }


            var result = new DashboardModel.ProjectAnalist()
            {
                ImagesHadQc = project.TotalImgQC,
                TotalImages = project.TotalImg,
                ImagesTagged = project.TotalImg - project.TotalImgNotTagged,
                TotalTags = 0,
                TotalTagsHaveClass = 0,
                UserProjects = projectUsers
            };

            double taggedTimeOnSecond = 0;
            if (imgs.Count() > 0)
            {
                foreach (var img in imgs)
                {
                    taggedTimeOnSecond += img.TagTime;
                    result.TotalTags += img.Tags.Count();
                    result.TotalTagsHaveClass += img.TagHasClass;
                }
            }

            if (taggedTimeOnSecond > 0)
            {
                var ts = TimeSpan.FromMinutes(taggedTimeOnSecond);
                result.TotalTaggedTime = String.Format("{0} hours {1:00} minutes {2:00}", ts.Hours, ts.Minutes, ts.Seconds);
            }
            else
            {
                result.TotalTaggedTime = "0 minutes";
            }

            return Ok(result);
        }
    }
}