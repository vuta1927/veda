using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ApiServer.Core.Authorization;
using ApiServer.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VDS.AspNetCore.Mvc.Authorization;

namespace ApiServer.Controllers
{
    [AppAuthorize(VdsPermissions.AddProject)]
    [Produces("application/json")]
    [Route("api/ImportProject")]
    public class ImportProjectController : Controller
    {
        private readonly VdsContext _context;
        private readonly IHostingEnvironment _hostingEnvironment;
        public ImportProjectController(VdsContext vdsContext, IHostingEnvironment hostingEnvironment)
        {
            _context = vdsContext;
            _hostingEnvironment = hostingEnvironment;
        }
        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Upload()
        {
            string[] AllowedFileExtensions = new string[] { "rar", "RAR", "ZIP", "zip", "7z", "7Z" };
            try
            {
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
                    string fileExtension = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"').Split('.')[1];

                    if (!AllowedFileExtensions.Contains(fileExtension.ToLower()))
                    {
                        return Content("File not allowed !");
                    }


                }

                return Ok("OK");
            }
            catch (Exception err)
            {
                return Content("error@" + err);
            }
        }
    }
}