using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiServer.Model;
using ApiServer.Model.views;
using VDS.AspNetCore.Mvc.Authorization;
using ApiServer.Core.Authorization;

namespace ApiServer.Controllers
{
    [AppAuthorize(VdsPermissions.ViewProject)]
    [Produces("application/json")]
    [Route("api/ProjectSettings/[action]")]
    public class ProjectSettingsController : Controller
    {
        private readonly VdsContext _context;

        public ProjectSettingsController(VdsContext context)
        {
            _context = context;
        }

        // GET: api/ProjectSettings
        [HttpGet]
        public IEnumerable<ProjectSetting> GetProjectSettings()
        {
            return _context.ProjectSettings;
        }

        // GET: api/ProjectSettings/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProjectSetting([FromRoute] Guid id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var projectSetting = await _context.ProjectSettings.Include(x=>x.Project).SingleOrDefaultAsync(m => m.Project.Id == id);

            if (projectSetting == null)
            {
                return NotFound();
            }

            return Ok(new ProjectSettingModel.ProjectSetting() { Id = projectSetting.Id, TaggTimeValue = projectSetting.TaggTimeValue, QuantityCheckLevel = projectSetting.QuantityCheckLevel });
        }

        // PUT: api/ProjectSettings/5
        [HttpPut("{id}")]
        [AppAuthorize(VdsPermissions.AddProject)]
        public async Task<IActionResult> PutProjectSetting([FromRoute] Guid id, [FromBody] ProjectSettingModel.ProjectSettingForUpdate projectSetting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var project = _context.Projects.SingleOrDefault(x => x.Id == id);
            if (project == null) return Content("Project not found");

            var originObject = _context.ProjectSettings.Include(x=>x.Project).SingleOrDefault(x => x.Project == project);
            if(originObject == null)
            {
                return Content("Project setting not found!");
            }

            originObject.QuantityCheckLevel = projectSetting.QuantityCheckLevel;
            originObject.TaggTimeValue = projectSetting.TaggTimeValue;

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                return Content(ex.Message);
            }
        }

        // POST: api/ProjectSettings
        [HttpPost]
        [AppAuthorize(VdsPermissions.AddProject)]
        public async Task<IActionResult> PostProjectSetting([FromBody] ProjectSettingModel.ProjectSettingForAdd projectSetting)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var project = _context.Projects.SingleOrDefault(x => x.Id == projectSetting.ProjectId);
            if (project == null) return Content("Project not found!");

            var newProjectSetting = new ProjectSetting()
            {
                QuantityCheckLevel = projectSetting.QuantityCheckLevel,
                TaggTimeValue = projectSetting.TaggTimeValue,
                Project = project
            };

            _context.ProjectSettings.Add(newProjectSetting);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProjectSetting", new { id = newProjectSetting.Id }, projectSetting);
        }

        private bool ProjectSettingExists(int id)
        {
            return _context.ProjectSettings.Any(e => e.Id == id);
        }
    }
}