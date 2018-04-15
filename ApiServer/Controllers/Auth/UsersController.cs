using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VDS.Security;
using ApiServer.Core.Authorization;
using ApiServer.Model;
using System.Security.Claims;

namespace ApiServer.Controllers.Auth
{

    [AppAuthorize(VdsPermissions.ViewUser)]
    [Produces("application/json")]
    [Route("api/Users/[action]")]
    public class UsersController : Controller
    {
        private readonly VdsContext _context;

        public UsersController(VdsContext context)
        {
            _context = context;
        }
        public User GetCurrentUser()
        {
            var identity = (ClaimsIdentity)User.Identity;
            IEnumerable<Claim> claims = identity.Claims;
            foreach (var claim in claims)
            {
                if (claim.Type == "id" && !string.IsNullOrEmpty(claim.Value))
                {
                    var userId = long.Parse(claim.Value);
                    return _context.Users.SingleOrDefault(x => x.Id == userId);
                }
            }

            return null;
        }

        public async Task<Role> GetCurrentRole(long UserId)
        {
            var userRole = await _context.UserRoles.SingleOrDefaultAsync(x => x.UserId == UserId);
            return await _context.Roles.SingleOrDefaultAsync(x => x.Id == userRole.RoleId);
        }
        // GET: api/Users
        [HttpGet]
        public IEnumerable<User> GetUser(int skip, int take)
        {
            return _context.Users.Skip(skip).Take(take);
        }
        
        [HttpGet("{email}")]
        public IActionResult WithEmail([FromRoute] string email)
        {
            var user = _context.Users.SingleOrDefault(u=>u.Email == email);
            if (user != null)
                return Ok(user);
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{username}")]
        public IActionResult WithUserName([FromRoute] string username)
        {
            var user = _context.Users.SingleOrDefault(u => u.UserName == username);
            if (user != null)
                return Ok(user);
            else
            {
                return NotFound();
            }
        }

        // GET: api/Users/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser([FromRoute] long id, [FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != user.Id)
            {
                return BadRequest();
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
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

        // POST: api/Users
        [HttpPost]
        public async Task<IActionResult> PostUser([FromBody] User user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUser", new { id = user.Id }, user);
        }

        // DELETE: api/Users/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser([FromRoute] long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var user = await _context.Users.SingleOrDefaultAsync(m => m.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(user);
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}