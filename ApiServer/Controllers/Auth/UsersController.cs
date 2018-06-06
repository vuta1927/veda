using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VDS.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ApiServer.Core.Authorization;
using ApiServer.Model;
using System.Security.Claims;
using VDS.Security;
using Microsoft.AspNetCore.Identity;
using ApiServer.Core.Email;

namespace ApiServer.Controllers.Auth
{

    [AppAuthorize(VdsPermissions.ViewUser)]
    [Produces("application/json")]
    [Route("api/Users/[action]")]
    public class UsersController : Controller
    {
        private readonly VdsContext _context;
        private readonly IUserService _userService;
        private readonly IEmailHelper _emailHelper;
        public UsersController(VdsContext context, IUserService userService, IEmailHelper emailHelper)
        {
            _context = context;
            _userService = userService;
            _emailHelper = emailHelper;
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

        public async Task<List<VDS.Security.Role>> GetCurrentRole(long UserId)
        {
            var userRoles = _context.UserRoles.Where(x => x.UserId == UserId);
            var result = new List<VDS.Security.Role>();
            foreach (var r in userRoles)
            {
                result.Add(await _context.Roles.SingleOrDefaultAsync(x => x.Id == r.RoleId));
            }
            return result;
        }
        // GET: api/Users
        [HttpGet("{skip}/{take}")]
        [ActionName("GetUser")]
        public IEnumerable<User> GetUser([FromRoute] int skip, [FromRoute] int take)
        {
            return _context.Users.Skip(skip).Take(take);
        }
        [HttpGet]
        [ActionName("GetUserList")]
        public IEnumerable<User> GetUserList()
        {
            return _context.Users.Where(x => x.UserName != "admin");
        }

        [HttpGet("{id}")]
        [ActionName("getUserForCreateOrEdit")]
        public async Task<IActionResult> getUserForCreateOrEdit([FromRoute] long id)
        {
            var result = new ApiServer.Model.views.UserModel.UserForCreateOrEdit()
            {
                AssignedRoleCount = 0,
                isEditMode = false,
                Roles = new List<Model.views.UserModel.UserRole>()
            };
            var roles = _context.Roles.Where(x => x.NormalizedRoleName != "TEACHER" && x.NormalizedRoleName != "QUANTITYCHECK");
            if (id > 0)
            {
                var user = await _context.Users.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Id == id);
                if (user == null) return Content("user not found!");

                foreach (var role in roles)
                {
                    var isAssigned = user.Roles.Any(x => x.Id == role.Id);
                    result.Roles.Add(new Model.views.UserModel.UserRole
                    {
                        IsAssigned = isAssigned,
                        RoleDisplayName = role.RoleName,
                        RoleName = role.RoleName,
                        RoleId = role.Id
                    });
                    if (isAssigned)
                    {
                        result.AssignedRoleCount += 1;
                    }
                }
                result.User = new Model.views.UserModel.UserEdit()
                {
                    EmailAddress = user.Email,
                    Id = user.Id,
                    IsActive = user.IsActive,
                    Name = user.Name,
                    Password = user.PasswordHash,
                    ShouldChangePasswordOnNextLogin = false,
                    Surname = user.Surname,
                    Username = user.UserName
                };
                result.isEditMode = true;
            }
            else
            {
                foreach (var role in roles)
                {
                    result.Roles.Add(new Model.views.UserModel.UserRole
                    {
                        IsAssigned = false,
                        RoleDisplayName = role.RoleName,
                        RoleName = role.RoleName,
                        RoleId = role.Id
                    });
                }

                result.User = new Model.views.UserModel.UserEdit();
            }

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
                return Content("");
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
                return Content("");
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
        [ActionName("AddOrUpdateUser")]
        public async Task<IActionResult> AddOrUpdateUser([FromBody] Model.views.UserModel.CreateOrUpdateUser userData)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            User user = null;
            var passHasher = new PasswordHasher<User>();
            string password = "";
            if (userData.User.Id <= 0)
            {
                user = new User()
                {
                    Email = userData.User.EmailAddress,
                    EmailConfirmed = userData.SendActivationEmail,
                    IsActive = userData.User.IsActive,
                    Name = userData.User.Name,
                    Surname = userData.User.Surname,
                    UserName = userData.User.Username,
                    NormalizedEmail = userData.User.EmailAddress.ToUpper(),
                    NormalizedUserName = userData.User.Username.ToUpper(),
                    Roles = new List<UserRole>()
                };
                if (!string.IsNullOrEmpty(userData.User.Password))
                {
                    user.PasswordHash = passHasher.HashPassword(user, userData.User.Password);
                }

                if (userData.RandomPassword)
                {
                    password = Utilities.GeneratePassword();
                    user.PasswordHash = passHasher.HashPassword(user, password);
                }

                _context.Users.Add(user);
            }
            else
            {
                user = _context.Users.SingleOrDefault(x => x.Id == userData.User.Id);
                password = user.PasswordHash;

                if (user == null) return Content("User not found!");
                user.Id = userData.User.Id;
                user.Email = userData.User.EmailAddress;
                user.EmailConfirmed = userData.SendActivationEmail;
                user.IsActive = userData.User.IsActive;
                user.Name = userData.User.Name;
                user.Surname = userData.User.Surname;
                user.UserName = userData.User.Username;
                user.NormalizedEmail = userData.User.EmailAddress.ToUpper();
                user.NormalizedUserName = userData.User.Username.ToUpper();
                if (!string.IsNullOrEmpty(userData.User.Password))
                {
                    user.PasswordHash = passHasher.HashPassword(user, userData.User.Password);
                }

                user.Roles = new List<UserRole>();
            }
            var identity = (ClaimsIdentity)User.Identity;
            var currentUser = _userService.GetCurrentUser(identity);

            foreach (var rolename in userData.AssignedRoleNames)
            {
                var role = _context.Roles.SingleOrDefault(x => x.NormalizedRoleName.Equals(rolename.ToUpper()));
                if (role != null)
                {
                    var userRole =
                        _context.UserRoles.SingleOrDefault(x => x.RoleId == role.Id && x.UserId == userData.User.Id);
                    if (userRole == null)
                    {
                        user.Roles.Add(new UserRole()
                        {
                            CreationTime = DateTime.Now,
                            CreatorUserId = currentUser.Id,
                            RoleId = role.Id,
                            UserId = user.Id,
                        });
                    }
                }
            }

            foreach (var rolename in userData.UnAssignedRoleNames)
            {
                var role = _context.Roles.SingleOrDefault(x => x.NormalizedRoleName.Equals(rolename.ToUpper()));
                if (role == null) continue;
                var userRole = _context.UserRoles.SingleOrDefault(x => x.RoleId == role.Id && x.UserId == userData.User.Id);
                if (userRole != null)
                {
                    _context.UserRoles.Remove(userRole);
                }
            }

            try
            {
                await _context.SaveChangesAsync();
                if (userData.User.Id <= 0)
                {
                    if (userData.RandomPassword)
                    {
                        var body = @"<p>You Veda Account info:</p><br /> <p>- username: <b>" + user.UserName + "</b></p> <br /><p>- password: <b>" + password + "</b><p>";
                        _emailHelper.Send(user.Email, "VEDA - Account Info", body);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(userData.User.Password))
                    {
                        var passwordCompair = passHasher.VerifyHashedPassword(user, password, userData.User.Password);

                        if (passwordCompair != PasswordVerificationResult.Success)
                        {
                            var body = "<p>Your password have been changed, you new password is: <b>" + userData.User.Password + "</b></p>";
                            _emailHelper.Send(user.Email, "VEDA - Account update", body);
                        }
                    }

                }
                return Ok();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }


        }

        // DELETE: api/Users/5
        [HttpDelete("{ids}")]
        [ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUser([FromRoute] string ids)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIds = ids.Split(';');

            foreach (var id in userIds)
            {
                var user = await _context.Users.SingleOrDefaultAsync(m => m.Id == long.Parse(id));
                if (user == null)
                {
                    continue;
                }

                _context.Users.Remove(user);
            }


            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }


            return Ok();
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}