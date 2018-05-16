using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiServer.Core.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VDS.AspNetCore.Mvc.Authorization;
using ApiServer.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using ApiServer.Hubs;
using ApiServer.Core.Merge;

namespace ApiServer.Controllers
{
    [AppAuthorize(VdsPermissions.EditProject)]
    [Produces("application/json")]
    [Route("api/Merge/[action]")]
    public class MergeController : Controller
    {
        private readonly VdsContext _context;
        
        private IHubContext<VdsHub> _hubContext;

        private IMergeService _mergeService;

        public MergeController(VdsContext context, IHubContext<VdsHub> hubContext, IMergeService mergeService)
        {
            _context = context;
            _hubContext = hubContext;
            _mergeService = mergeService;
        }

        [HttpPost]
        public async Task MergeProjcet([FromBody] MergeModel.Merge mergeData)
        {
            await _mergeService.Execute(mergeData);
        }
    }
}