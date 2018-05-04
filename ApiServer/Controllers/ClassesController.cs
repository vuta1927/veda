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
    [Route("api/Classes/[action]")]
    [AppAuthorize]
    public class ClassesController : Controller
    {
        private readonly VdsContext _context;

        public ClassesController(VdsContext context)
        {
            _context = context;
        }

        // GET: api/Classes
        [HttpGet("{id}")]
        [ActionName("GetClasses")]
        public IActionResult GetClasses([FromRoute] Guid id)
        {
            var classes = _context.Classes.Include(x => x.Project).Where(p => p.Project.Id == id).Include(x=>x.Tags);
            var results = new List<ClassModel.ClassForView>();
            if(classes.Count() > 0)
            {
                foreach (var c in classes)
                {
                    var newClass = new ClassModel.ClassForView()
                    {
                        Id = c.Id,
                        Code = c.Code,
                        Description = c.Description,
                        Name = c.Name,
                        TotalTag = c.Tags.Count,
                        ClassColor = c.ClassColor
                };
                    results.Add(newClass);
                }
            }
            
            return Ok(results);
        }

        [HttpGet("{id}")]
        [ActionName("GetTotal")]
        public IActionResult GetTotal([FromRoute] Guid id)
        {
            var result = 0;
            result = _context.Classes.Include(x => x.Project).Where(p => p.Project.Id == id).Include(x => x.Tags).Count();
            return Ok(result);
        }
        // GET: api/Classes/5
        [HttpGet("{id}")]
        [ActionName("GetClassById")]
        public async Task<IActionResult> GetClassById([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var @class = await _context.Classes.SingleOrDefaultAsync(m => m.Id == id);

            if (@class == null)
            {
                return Content("Class not found !");
            }

            return Ok(new ClassModel.ClassForView() { Id = @class.Id, Code = @class.Code, Name = @class.Name, Description = @class.Description });
        }

        [HttpGet("{id}/{name}")]
        [ActionName("GetClassByName")]
        public async Task<IActionResult> GetClassByName([FromRoute] Guid id, [FromRoute] string name)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var @class = await _context.Classes.Include(x => x.Project).SingleOrDefaultAsync(x => x.Project.Id == id && x.Name == name);
            if (@class != null)
            {
                return Ok(@class);
            }
            else
            {
                return Ok();
            }

        }
        [HttpGet("{id}/{code}")]
        [ActionName("GetCodeOfClass")]
        public async Task<IActionResult> GetCodeOfClass([FromRoute] Guid id, [FromRoute] string code)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var @class = await _context.Classes.Include(x => x.Project).SingleOrDefaultAsync(x => x.Project.Id == id && x.Code == code);
            if (@class != null)
            {
                return Ok(@class);
            }
            else
            {
                return Ok();
            }

        }

        // PUT: api/Classes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] ClassModel.ClassForUpdate @class)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != @class.Id)
            {
                return BadRequest();
            }

            var originClass = await _context.Classes.SingleOrDefaultAsync(x => x.Id == id);

            if (originClass == null)
            {
                return Content("Class not found !");
            }

            originClass.Name = @class.Name;
            originClass.Description = @class.Description;
            originClass.Code = @class.Code;
            originClass.ClassColor = @class.ClassColor;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClassExists(id))
                {
                    return Content("Class not Found !");
                }
                else
                {
                    return Content("Unkown Error !");
                }
            }

            return Ok(originClass);
        }

        // POST: api/Classes
        [HttpPost]
        public async Task<IActionResult> AddClass([FromBody] ClassModel.ClassForAdd @class)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var project = await _context.Projects.SingleOrDefaultAsync(x => x.Id == @class.ProjectId);

            if (project == null) return Content("Project not found !");

            var newClass = new Class()
            {
                Code = @class.Code,
                Description = @class.Description,
                Name = @class.Name,
                ClassColor = @class.ClassColor,
                Project = project
            };
            _context.Classes.Add(newClass);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception err)
            {
                return Content(err.Message);
            }

            return CreatedAtAction("GetClassById", new { id = newClass.Id }, newClass);
        }

        // DELETE: api/Classes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClass([FromRoute] string id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var ids = id.Split('_');
            for (var j = 0; j < ids.Length; j++)
            {
                var @class = await _context.Classes.Include(x => x.Tags).SingleOrDefaultAsync(m => m.Id == int.Parse(ids[j]));
                if (@class == null)
                {
                    return Ok("Class not found");
                }
                _context.Classes.Remove(@class);
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }

        private bool ClassExists(int id)
        {
            return _context.Classes.Any(e => e.Id == id);
        }
    }
}