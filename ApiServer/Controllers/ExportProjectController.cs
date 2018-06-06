using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ApiServer.Model;
using VDS.AspNetCore.Mvc.Authorization;
using ApiServer.Core.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Threading;
using ApiServer.Core.Email;
using Hangfire;
using VDS.AspNetCore.Mvc.Controllers;

namespace ApiServer.Controllers
{
    [AppAuthorize]
    [Produces("application/json")]
    [Route("api/ExportProject/[action]")]
    public class ExportProjectController : AppController
    {
        private readonly VdsContext _context;
        private IHostingEnvironment _hostingEnvironment;
        private readonly IUserService _userService;
        private IEmailHelper _emailHelper;

        public ExportProjectController(VdsContext vdsContext, IHostingEnvironment hostingEnvironment, IUserService userService, IEmailHelper emailHelper)
        {
            _context = vdsContext;
            _hostingEnvironment = hostingEnvironment;
            _userService = userService;
            _emailHelper = emailHelper;
        }

        [HttpPost]
        [ActionName("Export")]
        public IActionResult Export([FromBody] ExportModel.Export exportData)
        {
            var project = _context.Projects.SingleOrDefault(x => x.Id == exportData.ProjectId);
            if (project == null)
            {
                return Content("Project not found!");
            }

            var imgs = _context.Images.Include(x => x.Project).Include(x => x.Tags).
                Include(x => x.QuantityCheck).
                Where(x =>
                    x.Project == project);

            var images = new List<Image>();
            if (exportData.QcOptions.Any())
            {
                foreach (var image in imgs)
                {
                    if (Filter(image, exportData.QcOptions))
                    {
                        images.Add(image);
                    }
                }
            }
            else
            {
                images = imgs.ToList();
            }


            var imageTagMap = new Dictionary<Image, List<ExportModel.Tag>>();

            foreach (var image in images)
            {
                var tags = _context.Tags.Include(x => x.Image).Where(x => x.Image.Id == image.Id);

                foreach (var tag in tags)
                {
                    var t = _context.Tags.Include(x => x.Class).SingleOrDefault(x => x == tag);
                    if (t.Class == null || !exportData.Classes.Contains(t.Class.Name)) continue;

                    var newTag = new ExportModel.Tag()
                    {
                        ClassId = t.Class.Id,
                        Class = t.Class.Name,
                        CenterX = (t.Left + t.Width) / 2,
                        CenterY = (t.Top + t.Height) / 2,
                        Width = t.Width,
                        Height = t.Height,
                        Image = image
                    };

                    if (imageTagMap.ContainsKey(image))
                    {
                        imageTagMap[image].Add(newTag);
                    }
                    else
                    {
                        var ts = new List<ExportModel.Tag>
                        {
                            newTag
                        };
                        imageTagMap.Add(image, ts);
                    }
                }
            }

            try
            {
                var identity = (ClaimsIdentity)User.Identity;
                var currentUser = _userService.GetCurrentUser(identity);
                Thread thread = new Thread(() => CreateExportFile(imageTagMap, project, currentUser));
                thread.Start();
                return Ok("OK");
                //return new FileContentResult(CreateExportFile(imageTagMap), "application/zip") { FileDownloadName = $"{project.Name}.zip" };

            }
            catch (Exception ex)
            {
                return Content(ex.Message, ex.ToString());
            }
        }

        private bool Filter(Image image, IEnumerable<ExportModel.QcOption> qcOptions)
        {
            var qc = _context.QuantityChecks.SingleOrDefault(x => x.ImageId == image.Id);
            if (qcOptions.Any() && qc != null)
            {
                foreach (var qcOpt in qcOptions)
                {
                    if (qcOpt.Index == 1)
                    {
                        if (qc.Value1 != null && qc.Value1 == qcOpt.Value)
                        {
                            return true;
                        }
                    }
                    if (qcOpt.Index == 2)
                    {
                        if (qc.Value2 != null && qc.Value2 == qcOpt.Value)
                        {
                            return true;
                        }
                    }
                    if (qcOpt.Index == 3)
                    {
                        if (qc.Value3 != null && qc.Value3 == qcOpt.Value)
                        {
                            return true;
                        }
                    }
                    if (qcOpt.Index == 4)
                    {
                        if (qc.Value4 != null && qc.Value4 == qcOpt.Value)
                        {
                            return true;
                        }
                    }
                    if (qcOpt.Index == 5)
                    {
                        if (qc.Value5 != null && qc.Value5 == qcOpt.Value)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void CreateExportFile(Dictionary<Image, List<ExportModel.Tag>> imageTagMap, Project project, VDS.Security.User currentUser)
        {
            var downloadFolder = "download";
            var webRootPath = _hostingEnvironment.WebRootPath;
            var downloadPath = Path.Combine(webRootPath + "/" + downloadFolder);
            if (!Directory.Exists(webRootPath + "/" + downloadFolder))
            {
                Directory.CreateDirectory(webRootPath + "/" + downloadFolder);
            }
            
            var name = Guid.NewGuid().ToString();
            var pathForDownload = downloadFolder + "/" + name + ".zip";
            var fileZipPath = downloadPath + "/" + name + ".zip";


            using (var archive = ZipFile.Open(fileZipPath, ZipArchiveMode.Create))
            {
                foreach (var map in imageTagMap)
                {
                    archive.CreateEntryFromFile(_hostingEnvironment.WebRootPath + "/" + map.Key.Path, Path.GetFileName(map.Key.Path)); //create and add image file to zip

                    var textEntry = archive.CreateEntry(Path.GetFileName(map.Key.Path).Split('.')[0] + ".txt"); //create an empty file txt in zip

                    using (var entryStream = textEntry.Open()) //write data to file txt
                    using (var sw = new StreamWriter(entryStream, System.Text.Encoding.UTF8))
                    {
                        foreach (var tag in map.Value)
                        {
                            sw.NewLine = Environment.NewLine;
                            var data = tag.ClassId + ";" + tag.CenterX + ";" + tag.CenterY + ";" + tag.Width + ";" + tag.Height;
                            sw.WriteLine(data);
                        }
                    }
                }
            }
            BackgroundJob.Schedule(() => System.IO.File.Delete(fileZipPath), TimeSpan.FromMinutes(5));
            _emailHelper.Send(currentUser.Email, "Export Project " + project.Name, "<p><strong>Link download:</strong> http://vds.thehegeo.com:52719/" + pathForDownload + " </p><br><p><strong>ote: This link will be availabe for 24 hours.<strong></p>");
            
        }
        
    }
}