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

namespace ApiServer.Controllers
{
    [Produces("application/json")]
    [Route("api/ExportProject")]
    public class ExportProjectController : Controller
    {
        private readonly VdsContext _context;
        private IHostingEnvironment _hostingEnvironment;
        private readonly IUserService _userService;

        public ExportProjectController(VdsContext vdsContext, IHostingEnvironment hostingEnvironment, IUserService userService)
        {
            _context = vdsContext;
            _hostingEnvironment = hostingEnvironment;
            _userService = userService;
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> Export([FromRoute] Guid id, [FromBody] ExportModel.Exoprt exportData)
        {
            var project = _context.Projects.SingleOrDefault(x => x.Id == id);
            if (project == null)
            {
                return Content("Project not found!");
            }

            var images = _context.Images.Include(x => x.Project).Include(x => x.Tags).
                Include(x => x.QuantityCheck).
                Where(x =>
                    x.Project == project &&
                    Filter(x, exportData.FilterOptions)
                );

            var imageTagMap = new Dictionary<Image, List<ExportModel.Tag>>();
            var tagsResult = new List<ExportModel.Tag>();

            foreach (var image in images)
            {
                var tags = _context.Tags.Include(x => x.Image).Where(x => x.Image.Id == image.Id);

                foreach (var tag in tags)
                {
                    var t = await _context.Tags.Include(x => x.Class).SingleOrDefaultAsync(x => x == tag);
                    if (exportData.Classes.Contains(t.Class.Name))
                    {
                        var newTag = new ExportModel.Tag()
                        {
                            ClassId = t.Class.Id,
                            Class = t.Class.Name,
                            CenterX = (t.Left + t.Width) / 2,
                            CenterY = (t.Top + t.height) / 2,
                            Width = t.Width,
                            Height = t.height,
                            Image = image
                        };

                        if (imageTagMap.ContainsKey(image))
                        {
                            imageTagMap[image].Add(newTag);
                        }
                        else
                        {
                            var ts = new List<ExportModel.Tag>();
                            ts.Add(newTag);
                            imageTagMap.Add(image, ts);
                        }
                    }
                }
            }

            try
            {

                return new FileContentResult(CreateExportFile(imageTagMap), "application/zip") { FileDownloadName = $"{project.Name}.zip" };

            }
            catch (Exception ex)
            {
                return Content(ex.Message, ex.ToString());
            }
        }

        private bool Filter(Image image, ExportModel.FilterOptions filterOptions)
        {
            var qcOpts = filterOptions.QcOptions;
            if (qcOpts.Count > 0 && image.QuantityCheck != null)
            {
                foreach (var qcOpt in qcOpts)
                {
                    if (qcOpt.Index == 1)
                    {
                        if (image.QuantityCheck.Value1 != null && image.QuantityCheck.Value1 != qcOpt.Value)
                        {
                            return false;
                        }
                    }
                    if (qcOpt.Index == 2)
                    {
                        if (image.QuantityCheck.Value2 != null && image.QuantityCheck.Value2 != qcOpt.Value)
                        {
                            return false;
                        }
                    }
                    if (qcOpt.Index == 3)
                    {
                        if (image.QuantityCheck.Value3 != null && image.QuantityCheck.Value3 != qcOpt.Value)
                        {
                            return false;
                        }
                    }
                    if (qcOpt.Index == 4)
                    {
                        if (image.QuantityCheck.Value4 != null && image.QuantityCheck.Value4 != qcOpt.Value)
                        {
                            return false;
                        }
                    }
                    if (qcOpt.Index == 5)
                    {
                        if (image.QuantityCheck.Value5 != null && image.QuantityCheck.Value5 != qcOpt.Value)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        private byte[] CreateExportFile(Dictionary<Image, List<ExportModel.Tag>> imageTagMap)
        {
            using (var ms = new MemoryStream())
            {
                using (var archive = new ZipArchive(ms, ZipArchiveMode.Create, true))
                {
                    foreach (var map in imageTagMap)
                    {

                        archive.CreateEntryFromFile(_hostingEnvironment.WebRootPath + "\\" + map.Key.Path, Path.GetFileName(map.Key.Path)); //create and add image file to zip

                        var textEntry = archive.CreateEntry(Path.GetFileName(map.Key.Path).Split('.')[0] + ".txt"); //create an empty file txt in zip

                        using (var entryStream = textEntry.Open()) //write data to file txt
                        using (var sw = new StreamWriter(entryStream))
                        {
                            foreach (var tag in map.Value)
                            {
                                var data = tag.Class + ";" + tag.CenterX + ";" + tag.CenterY + ";" + tag.Width + ";" + tag.Height;
                                sw.WriteLine(data);
                            }
                        }

                    }
                }
                //ms.Position = 0;
                //return File(ms.ToArray(), "application/zip");
                return ms.ToArray();
            }
        }
    }
}