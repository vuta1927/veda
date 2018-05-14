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

namespace ApiServer.Controllers
{   
    [AppAuthorize(VdsPermissions.EditProject)]
    [Produces("application/json")]
    [Route("api/Merge")]
    public class MergeController : Controller
    {
        private readonly VdsContext _context;

        public MergeController(VdsContext context)
        {
            _context = context;
            Merge._ctx = context;
        }

        [HttpPost]
        public async Task<IActionResult> MergeProjcet([FromBody] MergeModel.Merge mergeData)
        {
            try
            {
                await Merge.Execute(mergeData);
                return Ok();
            }catch(Exception ex)
            {
                return Content("Cant merge there projects: error: " + ex.ToString());
            }
            
        }
    }
}