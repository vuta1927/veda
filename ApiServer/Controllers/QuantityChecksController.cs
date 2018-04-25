using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiServer.Model;
using VDS.AspNetCore.Mvc.Authorization;
using ApiServer.Core.Authorization;
using ApiServer.Model.views;

namespace ApiServer.Controllers
{
    [Produces("application/json")]
    [Route("api/QuantityChecks/[action]")]
    [AppAuthorize(VdsPermissions.ViewQc)]
    public class QuantityChecksController : Controller
    {
        private readonly VdsContext _context;

        public QuantityChecksController(VdsContext context)
        {
            _context = context;
        }

        // POST: api/QuantityChecks
        [HttpPost]
        public async Task<IActionResult> AddOrUpdate([FromBody] QuantityCheckModel.QcObjectForAdd data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == data.UserId);
            if(user == null)
            {
                return Content("User not found !");
            }

            var img = await _context.Images.Include(x=>x.Project).FirstOrDefaultAsync(x => x.Id == data.ImageId);
            if(img == null)
            {
                return Content("Tag not found !");
            }

            var project = img.Project;

            var qc = await _context.QuantityChecks.FirstOrDefaultAsync(x => x.Image == img);

            if(qc != null)
            {
                qc.Value1 = data.QcValue;
                qc.UserQc = user;
            }
            else
            {
                qc.Comment = data.QcComment;
                qc.QCDate = DateTime.Now;
                qc.Image = img;
                qc.UserQc = user;
                _context.QuantityChecks.Add(qc);
                qc.Value1 = data.QcValue;

                img.QuantityCheck = qc;
            }
            img.QcStatus = qc.Value1;
            img.QcDate = DateTime.Now;
            img.UserQc = user;

            project.TotalImgQC += 1;
            project.TotalImgNotQC -= 1;
            try
            {
                await _context.SaveChangesAsync();
                return Ok("OK");
            }catch(Exception e)
            {
                return Content(e.ToString());
            }
        }

        private bool QuantityCheckExists(int id)
        {
            return _context.QuantityChecks.Any(e => e.Id == id);
        }
    }
}