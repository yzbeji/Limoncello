using Limoncello.Data;
using Limoncello.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Limoncello.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            ViewBag.Users = db.ApplicationUsers.ToList();
            return View();
        }

        public async Task<ActionResult> Show(string id)
        {
            ApplicationUser? user = db.ApplicationUsers
                                        .Include(u => u.UserProjects)
                                        .ThenInclude(up => up.Project)
                                        .Where(u => u.Id == id)
                                        .FirstOrDefault();
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.AllRoles = GetAllRoles();
            ViewBag.Role = await _userManager.GetRolesAsync(user);
            return View(user);
        }

        [HttpPost]
        public async Task<ActionResult> Edit([FromForm] ApplicationUser reqUser, [FromForm] string reqRole)
        {
            if (reqUser.Id == _userManager.GetUserId(User))
            {
                TempData["message"] = "You can't edit yourself!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = reqUser.Id });
            }

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(reqUser.Id);
                if (user == null)
                {
                    return NotFound();
                }

                user.Email = reqUser.Email;
                user.FirstName = reqUser.FirstName;
                user.LastName = reqUser.LastName;
                user.PhoneNumber = reqUser.PhoneNumber;

                var roles = db.Roles.ToList();

                foreach (var role in roles)
                {
                    // Scoatem userul din rolurile anterioare
                    await _userManager.RemoveFromRoleAsync(user, role.Name);
                }
                // Adaugam noul rol selectat
                var roleName = await _roleManager.FindByIdAsync(reqRole);
                await _userManager.AddToRoleAsync(user, roleName.ToString());

                var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Show", new { id = user.Id });
                }
                else
                {
                    TempData["message"] = "Something went wrong :(";
                    TempData["messageType"] = "alert-danger";
                    return RedirectToAction("Show", new { id = user.Id });
                }
            }

            return RedirectToAction("Show", new { id = reqUser.Id });
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            if (id == _userManager.GetUserId(User))
            {
                TempData["message"] = "You can't delete yourself!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = id });
            }

            var user = db.Users
                         .Include("Comments")
                         .Where(u => u.Id == id)
                         .First();

            if (user.Comments.Count > 0)
            {
                foreach (var comment in user.Comments)
                {
                    db.Comments.Remove(comment);
                }
            }

            foreach (var project in db.Projects.Where(p => p.OrganizerId == id))
            {
                db.Projects.Remove(project);
            }

            db.ApplicationUsers.Remove(user);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();

            var roles = from role in db.Roles
                        select role;

            foreach (var role in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = role.Id.ToString(),
                    Text = role.Name.ToString()
                });
            }
            return selectList;
        }
    }
}
