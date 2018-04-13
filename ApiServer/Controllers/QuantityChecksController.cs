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

namespace ApiServer.Controllers
{
    [Produces("application/json")]
    [Route("api/QuantityChecks")]
    [AppAuthorize(VdsPermissions.ViewQc)]
    public class QuantityChecksController : Controller
    {
        private readonly VdsContext _context;

        public QuantityChecksController(VdsContext context)
        {
            _context = context;
        }

        // GET: api/QuantityChecks
        [HttpGet]
        public IEnumerable<QuantityCheck> GetQuantityChecks()
        {
            return _context.QuantityChecks;
        }

        // GET: api/QuantityChecks/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuantityCheck([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var quantityCheck = await _context.QuantityChecks.SingleOrDefaultAsync(m => m.Id == id);

            if (quantityCheck == null)
            {
                return NotFound();
            }

            return Ok(quantityCheck);
        }

        // PUT: api/QuantityChecks/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutQuantityCheck([FromRoute] int id, [FromBody] QuantityCheck quantityCheck)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != quantityCheck.Id)
            {
                return BadRequest();
            }

            _context.Entry(quantityCheck).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuantityCheckExists(id))
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

        // POST: api/QuantityChecks
        [HttpPost]
        public async Task<IActionResult> PostQuantityCheck([FromBody] QuantityCheck quantityCheck)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.QuantityChecks.Add(quantityCheck);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetQuantityCheck", new { id = quantityCheck.Id }, quantityCheck);
        }

        // DELETE: api/QuantityChecks/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuantityCheck([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var quantityCheck = await _context.QuantityChecks.SingleOrDefaultAsync(m => m.Id == id);
            if (quantityCheck == null)
            {
                return NotFound();
            }

            _context.QuantityChecks.Remove(quantityCheck);
            await _context.SaveChangesAsync();

            return Ok(quantityCheck);
        }

        private bool QuantityCheckExists(int id)
        {
            return _context.QuantityChecks.Any(e => e.Id == id);
        }
    }
}