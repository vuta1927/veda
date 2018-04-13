using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiServer.Models;
using Microsoft.AspNetCore.Authorization;
using VDS.AspNetCore.Mvc.Controllers;

namespace ApiServer.Controllers
{
    [Produces("application/json")]
    [Route("api/QuantityCheckTypes")]
    public class QuantityCheckTypesController : AppController
    {
        private readonly VdsContext _context;

        public QuantityCheckTypesController(VdsContext context)
        {
            _context = context;
        }

        // GET: api/QuantityCheckTypes
        [HttpGet]
        public IEnumerable<QuantityCheckType> GetQuantityCheckTypes()
        {
            return _context.QuantityCheckTypes;
        }

        // GET: api/QuantityCheckTypes/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuantityCheckType([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var quantityCheckType = await _context.QuantityCheckTypes.SingleOrDefaultAsync(m => m.Id == id);

            if (quantityCheckType == null)
            {
                return NotFound();
            }

            return Ok(quantityCheckType);
        }

        // PUT: api/QuantityCheckTypes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuantityCheckType([FromRoute] int id, [FromBody] QuantityCheckType quantityCheckType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != quantityCheckType.Id)
            {
                return BadRequest();
            }

            _context.Entry(quantityCheckType).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuantityCheckTypeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/QuantityCheckTypes
        [HttpPost]
        public async Task<IActionResult> PostQuantityCheckType([FromBody] QuantityCheckType quantityCheckType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.QuantityCheckTypes.Add(quantityCheckType);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuantityCheckType", new { id = quantityCheckType.Id }, quantityCheckType);
        }

        // DELETE: api/QuantityCheckTypes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuantityCheckType([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var quantityCheckType = await _context.QuantityCheckTypes.SingleOrDefaultAsync(m => m.Id == id);
            if (quantityCheckType == null)
            {
                return NotFound();
            }

            _context.QuantityCheckTypes.Remove(quantityCheckType);
            await _context.SaveChangesAsync();

            return Ok(quantityCheckType);
        }

        private bool QuantityCheckTypeExists(int id)
        {
            return _context.QuantityCheckTypes.Any(e => e.Id == id);
        }
    }
}