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
            if (user == null)
            {
                return Content("User not found !");
            }

            var img = await _context.Images.Include(x => x.Project).Include(x=>x.UsersQc).FirstOrDefaultAsync(x => x.Id == data.ImageId);
            if (img == null)
            {
                return Content("Tag not found !");
            }

            var project = img.Project;

            var qc = await _context.QuantityChecks.Include(x=>x.UsersQc).FirstOrDefaultAsync(x => x.Image == img);
            var qcStatusText = img.QcStatus;

            if (qc != null)
            {
                if (qc.Value1 == null)
                {
                    qc.Value1 = data.QcValue;
                    qc.CommentLevel1 = string.IsNullOrEmpty(data.QcComment) ? null : data.QcComment;
                    if (data.QcValue)
                        qcStatusText += "level 1: passed;";
                    else
                        qcStatusText += "level 1: unpassed;";
                }
                else if (qc.Value2 == null)
                {
                    qc.Value2 = data.QcValue;
                    qc.CommentLevel2 = string.IsNullOrEmpty(data.QcComment) ? null : data.QcComment;
                    if (data.QcValue)
                        qcStatusText += "level 2: passed;";
                    else
                        qcStatusText += "level 2: unpassed;";
                }
                else if (qc.Value3 == null)
                {
                    qc.Value3 = data.QcValue;
                    qc.CommentLevel3 = string.IsNullOrEmpty(data.QcComment) ? null : data.QcComment;
                    if (data.QcValue)
                        qcStatusText += "level 3: passed;";
                    else
                        qcStatusText += "level 3: unpassed;";
                }
                else if (qc.Value4 == data.QcValue)
                {
                    qc.Value4 = data.QcValue;
                    qc.CommentLevel4 = string.IsNullOrEmpty(data.QcComment) ? null : data.QcComment;
                    if (data.QcValue)
                        qcStatusText += "level 4: passed;";
                    else
                        qcStatusText += "level 4: unpassed;";
                }
                else if (qc.Value5 == data.QcValue)
                {
                    qc.Value5 = data.QcValue;
                    qc.CommentLevel5 = string.IsNullOrEmpty(data.QcComment) ? null : data.QcComment;
                    if (data.QcValue)
                        qcStatusText += "level 5: passed;";
                    else
                        qcStatusText += "level 5: unpassed;";
                }
                if (!qc.UsersQc.Any(x => x.UserId == user.Id))
                {
                    var newUserQc = new UserQuantityCheck()
                    {
                        UserId = user.Id,
                        QuantityCheckId = qc.Id,
                        QuantityCheck = qc
                    };
                    _context.UserQuantityChecks.Add(newUserQc);
                    qc.UsersQc.Add(newUserQc);
                }


            }
            else
            {
                qc = new QuantityCheck()
                {
                    QCDate = DateTime.Now,
                    Image = img,
                    Value1 = data.QcValue,
                    CommentLevel1 = string.IsNullOrEmpty(data.QcComment) ? null : data.QcComment,

                };
                var newUserQc = new UserQuantityCheck()
                {
                    UserId = user.Id,
                    QuantityCheckId = qc.Id,
                    QuantityCheck = qc
                };
                _context.UserQuantityChecks.Add(newUserQc);
                qc.UsersQc.Add(newUserQc);


                img.QuantityCheck = qc;
                qcStatusText += data.QcValue ? "level 1: passed;" : "level 1: unpassed;";
                img.QcStatus = qcStatusText;
                project.TotalImgQC += 1;
                project.TotalImgNotQC -= 1;
            }


            img.QcStatus = qcStatusText;
            img.QcDate = DateTime.Now;
            if(!img.UsersQc.Any(x=>x == user))
            {
                img.UsersQc.Add(user);
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok("OK");
            }
            catch (Exception e)
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