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

namespace ApiServer.Controllers
{
    [AppAuthorize]
    [Produces("application/json")]
    [Route("api/Dashboard/[action]")]
    public class DashboardController : Controller
    {
        private readonly VdsContext _context;
        public DashboardController(VdsContext context)
        {
            _context = context;
        }
        [HttpGet("{id}")]
        public IActionResult GetDataProject([FromRoute] Guid id)
        {
            var project = _context.Projects.SingleOrDefault(x => x.Id == id);

            if (project == null) return Content("Project not found");

            var imgs = _context.Images.Include(x=>x.Project).Where(x => x.Project == project);

            var result = new DashboardModel.ProjectAnalist()
            {
                ImagesHadQc = project.TotalImgQC,
                TotalImages = project.TotalImg,
                ImagesTagged = project.TotalImg - project.TotalImgNotTagged
            };

            double taggedTimeOnSecond = 0;
            if(imgs.Count() > 0)
            {
                foreach(var img in imgs)
                {
                    taggedTimeOnSecond += img.TagTime;
                }
            }

            if(taggedTimeOnSecond > 0)
            {
                var ts = TimeSpan.FromSeconds(taggedTimeOnSecond);
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