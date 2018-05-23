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

            var imgs = _context.Images.Include(x => x.Project).Include(x => x.UsersTagged).Include(x => x.UsersQc).Include(x => x.Tags).Where(x => x.Project == project);

            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _userService.GetCurrentUser(identity);
            var _currentRoles = _userService.GetCurrentRole(currentUser.Id);
            var projectUsers = new List<DashboardModel.UserProject>();

            var totalProjects = 0;
            var totalImageProjects = 0;

            if (_currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper()) || x.NormalizedRoleName.Equals("PROJECTMANAGER")))
            {
                var pus = _context.ProjectUsers.Where(x => x.ProjectId == id).Include(x => x.User);
                if (pus.Count() > 0)
                {
                    foreach (var pu in pus)
                    {
                        var totalTags = _context.Tags.
                            Include(x => x.Image).
                            Include(x => x.UsersTagged).
                            Where(x => x.UsersTagged.Any(a => a.User == pu.User) && x.Image.Project.Id == project.Id);

                        var totalQcs = _context.QuantityChecks.
                            Include(x => x.Image).
                            Include(x => x.UsersQc).
                            Where(x => x.UsersQc.Any(a => a.User == pu.User) && x.Image.Project.Id == project.Id);

                        var newPu = new DashboardModel.UserProject()
                        {
                            Email = pu.User.Email,
                            UserId = pu.UserId,
                            UserName = pu.User.UserName,
                            RoleNames = _userService.GetCurrentRole(pu.User.Id).Select(x => x.RoleName).Distinct().ToArray(),
                            TotalTags = totalTags.Count(),
                            TotalQcs = totalQcs.Count(),
                            TagsHaveClass = GetTagsHaveClass(totalTags),
                            ImagesHaveTag = GetImagesTagged(totalTags, imgs),
                        };
                        double TaggedTime = 0;
                        var userTaggedTimes = _context.userTaggedTimes.Where(x => x.UserId == pu.User.Id && x.Image.Project.Id == project.Id);
                        if (userTaggedTimes.Count() > 0)
                        {
                            foreach (var d in userTaggedTimes)
                            {
                                TaggedTime += d.TaggedTime;
                            }

                        }

                        newPu.TaggedTime = ConvertTime(TimeSpan.FromMinutes(TaggedTime));

                        projectUsers.Add(newPu);
                    }
                }

                if (!_currentRoles.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
                {
                    var d = _context.ProjectUsers.Include(x=>x.Project).Where(x => x.UserId == currentUser.Id).Distinct();
                    totalProjects = d.Count();
                    totalImageProjects = d.Select(x => x.Project.TotalImg).Sum();
                }
                else
                {
                    var d = _context.ProjectUsers.Distinct();
                    totalProjects = _context.ProjectUsers.Distinct().Count();
                    totalImageProjects = d.Select(x => x.Project.TotalImg).Sum();
                }
                
            }


            var result = new DashboardModel.ProjectAnalist()
            {
                ImagesHadQc = project.TotalImgQC,
                TotalImages = project.TotalImg,
                ImagesTagged = project.TotalImg - project.TotalImgNotTagged,
                TotalTags = 0,
                TotalTagsHaveClass = 0,
                UserProjects = projectUsers,
                CurrentProgress = GetCurrentUserProgress(id, currentUser.Id),
                TotalProjectImages = totalImageProjects,
                TotalProjects = totalProjects
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

        public DashboardModel.UserProject GetCurrentUserProgress(Guid projId, long userId)
        {
            var projectUser = _context.ProjectUsers.Include(x => x.Project).Include(x => x.User).SingleOrDefault(x => x.UserId == userId && x.ProjectId == projId);

            if (projectUser == null) return null;

            var project = projectUser.Project;

            if (project == null) return null;

            var imgs = _context.Images.Include(x => x.Project).Include(x => x.UsersTagged).Include(x => x.UsersQc).Include(x => x.Tags).Where(x => x.Project == project);

            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _userService.GetCurrentUser(identity);
            var _currentRoles = _userService.GetCurrentRole(currentUser.Id);

            var totalTags = _context.Tags.
                            Include(x => x.Image).
                            Include(x => x.UsersTagged).
                            Where(x => x.UsersTagged.Any(a => a.User == projectUser.User) && x.Image.Project.Id == project.Id);

            var totalQcs = _context.QuantityChecks.
                Include(x => x.Image).
                Include(x => x.UsersQc).
                Where(x => x.UsersQc.Any(a => a.User == projectUser.User) && x.Image.Project.Id == project.Id);

            var result = new DashboardModel.UserProject()
            {
                Email = projectUser.User.Email,
                UserId = projectUser.UserId,
                UserName = projectUser.User.UserName,
                RoleNames = _userService.GetCurrentRole(projectUser.User.Id).Select(x => x.RoleName).Distinct().ToArray(),
                TotalTags = totalTags.Count(),
                TotalQcs = totalQcs.Count(),
                TagsHaveClass = GetTagsHaveClass(totalTags),
                ImagesHaveTag = GetImagesTagged(totalTags, imgs),
            };

            double taggedTime = 0;
            var userTaggedTimes = _context.userTaggedTimes.Where(x => x.UserId == projectUser.User.Id && x.Image.Project.Id == project.Id);
            if (userTaggedTimes.Count() > 0)
            {
                foreach (var d in userTaggedTimes)
                {
                    taggedTime += d.TaggedTime;
                }

            }
            result.TaggedTime = ConvertTime(TimeSpan.FromMinutes(taggedTime));

            return result;
        }

        private string ConvertTime(TimeSpan timeSpan)
        {
            var result = "";

            if (timeSpan.Hours > 0)
            {
                result += timeSpan.Hours + " hours " + timeSpan.Minutes + " minutes ";
                if (timeSpan.Seconds > 0)
                {
                    result += timeSpan.Seconds + " seconds";
                }
            }
            else
            {
                if (timeSpan.Minutes > 0)
                {
                    result += timeSpan.Minutes + " minutes ";
                    if (timeSpan.Seconds > 0)
                    {
                        result += timeSpan.Seconds + " seconds";
                    }
                }
                else
                {
                    result += timeSpan.Seconds + " seconds";
                }
            }
            return result;
        }

        private int GetTagsHaveClass(IQueryable<Tag> tags)
        {
            var result = 0;

            foreach (var tag in tags)
            {
                if (tag.ClassId == null)
                {
                    continue;
                }

                result++;
            }

            return result;
        }

        private int GetImagesTagged(IQueryable<Tag> tags, IQueryable<Image> images)
        {
            var totalImgs = new List<Image>();
            foreach (var tag in tags)
            {
                totalImgs.AddRange(images.Where(x => x.Tags.Any(t => t.Id == tag.Id) && !totalImgs.Contains(x)));
            }

            return totalImgs.Count();
        }

    }
}