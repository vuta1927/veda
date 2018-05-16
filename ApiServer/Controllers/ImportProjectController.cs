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
using Microsoft.EntityFrameworkCore;
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
        private readonly IUserService _userService;
        private readonly string[] AllowedFileExtensions = new string[] { "rar", "RAR", "ZIP", "zip", "jpg", "JPG", "png", "PNG", "bmp", "BMP", "txt", "TXT" };
        private readonly string[] imageExtentions = new string[] { "jpg", "JPG", "png", "PNG", "bmp", "BMP", "txt", "TXT" };

        public ImportProjectController(VdsContext vdsContext, IHostingEnvironment hostingEnvironment, IUserService userService)
        {
            _context = vdsContext;
            _hostingEnvironment = hostingEnvironment;
            _userService = userService;
        }

        [HttpPost("{id}"), DisableRequestSizeLimit]
        public async Task<IActionResult> Upload([FromRoute] Guid id)
        {
            var project = new Project();
            var currentUser = _userService.GetCurrentUser();
            var currentRole = _userService.GetCurrentRole(currentUser.Id);

            if (currentRole.Any(x => x.NormalizedRoleName.Equals(VdsPermissions.Administrator.ToUpper())))
            {
                project = await _context.ProjectUsers
                .Include(x => x.Project)
                .Include(u => u.User)
                .Where(a => a.Project.Id == id).Select(b => b.Project).FirstOrDefaultAsync();
            }
            else
            {
                project = await _context.ProjectUsers
                .Include(x => x.Project)
                .Include(u => u.User)
                .Where(a => a.User.Id == currentUser.Id && a.Project.Id == id).Select(b => b.Project).FirstOrDefaultAsync();
            }

            if (project == null)
                return Content("Project not found");

            try
            {
                var files = Request.Form.Files;
                foreach (var file in files)
                {
                    if (file.Length <= 0)
                    {
                        return Content("There are no files to upload !");
                    }
                    string tempFolderName = "temp";

                    string folderName = DateTime.UtcNow.ToString("dd-MM-yyy", CultureInfo.InvariantCulture);
                    string webRootPath = _hostingEnvironment.WebRootPath;
                    string projectFolder = project.Name + "\\" + folderName;
                    string newPath = Path.Combine(webRootPath, projectFolder);
                    string pathToDatabase = "\\" + projectFolder + "\\";

                    string fileExtension = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"').Split('.')[1];

                    if (!AllowedFileExtensions.Contains(fileExtension))
                    {
                        return Content("File not allowed !");
                    }

                    if (!Directory.Exists(webRootPath + "\\" + tempFolderName))
                    {
                        Directory.CreateDirectory(webRootPath + "\\" + tempFolderName);
                    }

                    string tempPath = Path.Combine(webRootPath + "\\" + tempFolderName, file.FileName);

                    using (var stream = new FileStream(tempPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    ProccessArchive(tempPath);

                }

                return Ok("OK");
            }
            catch (Exception err)
            {
                return Content("error@" + err);
            }
        }

        private void ProccessArchive(string filePath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(filePath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string[] filenames = entry.Name.Split('.');
                    if (!AllowedFileExtensions.Contains(filenames.Last()))
                    {
                        return;
                    }
                    else
                    {
                        entry.ExtractToFile(Path.Combine(filePath, name + '.' + filenames.Last()));
                    }
                }
            }
        }

        private void ProccessTextFile()
    }
}