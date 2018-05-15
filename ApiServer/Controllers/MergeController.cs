using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiServer.Core.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VDS.AspNetCore.Mvc.Authorization;
using ApiServer.Model;
using ApiServer.Core.MergeProject;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using ApiServer.Hubs;

namespace ApiServer.Controllers
{
    [AppAuthorize(VdsPermissions.EditProject)]
    [Produces("application/json")]
    [Route("api/Merge/[action]")]
    public class MergeController : Controller
    {
        private readonly VdsContext _context;

        private readonly IHostingEnvironment _hostingEnvironment;
        private IHubContext<VdsHub> _hubContext;

        public MergeController(VdsContext context, IHostingEnvironment hostingEnvironment, IHubContext<VdsHub> hubContext)
        {
            _context = context;
            Merge._ctx = context;
            _hubContext = hubContext;
            _hostingEnvironment = hostingEnvironment;
        }

        [HttpPost]
        public IActionResult MergeProjcet([FromBody] MergeModel.Merge mergeData)
        {
            Merge.webPathRoot = _hostingEnvironment.WebRootPath;
            Merge._hubContext = _hubContext;
            Merge.Execute(mergeData);

            return Ok();
        }
    }
}