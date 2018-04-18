﻿using System;
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
    [Route("api/Tags/[action]")]
    [AppAuthorize(VdsPermissions.ViewTag)]
    public class TagsController : Controller
    {
        private readonly VdsContext _context;

        public TagsController(VdsContext context)
        {
            _context = context;
        }

        // GET: api/Tags
        [HttpGet("{id}")]
        [ActionName("GetTags")]
        public IActionResult GetTags([FromRoute] Guid id)
        {
            var tags = _context.Tags.Include(t=>t.Image).Where(x=>x.Image.Id == id).Include("ClassTags.Class").Include(q => q.QuantityCheck);
            var results = new List<TagModel.TagForView>();
            foreach(var tag in tags)
            {
                results.Add(new TagModel.TagForView()
                {
                    Id = tag.Id,
                    Left = tag.Left,
                    Top = tag.Top,
                    ClassId = tag.Classes.Select(x=>x.Id),
                    QuantityCheckId = tag.QuantityCheck.Id,
                    ImageId = tag.Image.Id
                });
            }
            return Ok(results);
        }

        // GET: api/Tags/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTag([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tag = await _context.Tags.Include(t => t.Image).Include("ClassTags.Class").Include(q => q.QuantityCheck).SingleOrDefaultAsync(m => m.Id == id);

            if (tag == null)
            {
                return Content("Tag not found !");
            }

            return Ok(new TagModel.TagForView()
            {
                Id = tag.Id,
                Left = tag.Left,
                Top = tag.Top,
                ClassId = tag.Classes.Select(x => x.Id),
                QuantityCheckId = tag.QuantityCheck.Id,
                ImageId = tag.Image.Id
            });
        }

        // PUT: api/Tags/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] TagModel.TagForAddOrUpdate tag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != tag.Id)
            {
                return BadRequest();
            }

            var originTag = await _context.Tags.SingleOrDefaultAsync(x => x.Id == id);
            if(originTag == null)
            {
                return Content("Tag not found !");
            }

            var image = await _context.Images.SingleOrDefaultAsync(x => x.Id == tag.ImageId);
            if(image == null)
            {
                return Content("Image not found !");
            }

            try
            {
                foreach (var @class in originTag.Classes)
                {
                    originTag.Classes.Remove(@class);
                }

                var classes = new List<Class>();
                if (tag.ClassIds != null && tag.ClassIds.Count() > 0)
                {
                    foreach (var classId in tag.ClassIds)
                    {
                        var @class = await _context.Classes.SingleOrDefaultAsync(x => x.Id == classId);
                        classes.Add(@class);
                    }
                }

                originTag.Classes = classes;
                originTag.Id = tag.Id;
                originTag.Left = tag.Left;
                originTag.Top = tag.Top;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TagExists(id))
                    {
                        return Content("Tag not Found !");
                    }
                    else
                    {
                        return Content("Unknow error !");
                    }
                }

                return Ok(originTag);

            }
            catch(Exception ex)
            {
                return Content("Cant update classes !");
            }
        }

        // POST: api/Tags
        [HttpPost]
        public async Task<IActionResult> AddTag([FromBody] TagModel.TagForAddOrUpdate tag)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var originTag = await _context.Tags.SingleOrDefaultAsync(x => x.Id == tag.Id);
            if (originTag != null)
            {
                return Content("Tag id "+ tag.Id +" is exsit !");
            }

            var image = await _context.Images.SingleOrDefaultAsync(x => x.Id == tag.ImageId);
            if (image == null)
            {
                return Content("Image not found !");
            }


            var classes = new List<Class>();
            if (tag.ClassIds != null && tag.ClassIds.Count() > 0)
            {
                foreach (var classId in tag.ClassIds)
                {
                    var @class = await _context.Classes.SingleOrDefaultAsync(x => x.Id == classId);
                    classes.Add(@class);
                }

            }
            try
            {
                _context.Tags.Add(new Tag()
                {
                    Id = tag.Id,
                    Classes = classes,
                    Image = image,
                    Left = tag.Left,
                    Top = tag.Top
                });
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetTag", new { id = tag.Id }, tag);
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
            
        }

        // DELETE: api/Tags/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTag([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var tag = await _context.Tags.SingleOrDefaultAsync(m => m.Id == id);
            if (tag == null)
            {
                return Content("Tag not found !");
            }

            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();

            return Ok(tag);
        }

        private bool TagExists(int id)
        {
            return _context.Tags.Any(e => e.Id == id);
        }
    }
}