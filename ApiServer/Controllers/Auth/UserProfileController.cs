using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ApiServer.Core.Authorization;
using ApiServer.Model;
using ApiServer.Model.views;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using VDS.AspNetCore.Mvc.Authorization;

namespace ApiServer.Controllers.Auth
{
    [AppAuthorize]
    [Produces("application/json")]
    [Route("api/UserProfile")]
    public class UserProfileController : Controller
    {
        private readonly VdsContext _context;
        private readonly IUserService _userService;
        public UserProfileController(VdsContext vdsContext, IUserService userService)
        {
            _context = vdsContext;
            _userService = userService;
        }

        [HttpGet]
        public IActionResult GetProfile() {
            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _userService.GetCurrentUser(identity);
            if (currentUser == null) return Content("User not found");

            var result = new UserProfileModel.UserProfile()
            {
                Email = currentUser.Email,
                Id = currentUser.Id,
                Name = currentUser.Name,
                Surname = currentUser.Name,
                Username = currentUser.UserName
            };

            return Ok(result);
        }

        [HttpGet("{email}")]
        [ActionName("withemail")]
        public IActionResult WithEmail([FromRoute] string email)
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == email);
            if (user != null)
                return Ok(user);
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{username}")]
        [ActionName("withusername")]
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
    }
}