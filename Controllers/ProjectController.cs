using Limoncello.Data;
using Limoncello.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Limoncello.Controllers
{
    public class ProjectController : Controller
    {
        private readonly ApplicationDbContext db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public ProjectController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager
        )
        {
            db = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        private readonly ILogger<HomeController> _logger;

        // TODO: require authorization
        [Authorize(Roles = "User,Admin")]
        public IActionResult Index()
        {
            string userId = _userManager.GetUserId(User);
            var userProjects = db.UserProjects 
                               .Where(up => up.UserId == userId)
                               .Select(up => up.Project)
                               .ToList();
            ViewBag.IsEmpty = false;
            if (userProjects == null)
            {
                ViewBag.IsEmpty = true;
            }
            else
            {
                ViewBag.Projects = userProjects;
            }
            return View();
        }

        // TODO: require authorization
        public IActionResult Show(int? id)
        {
            // TODO check if the user has access to this project
            string userId = _userManager.GetUserId(User);
            var project = db.Projects.Include(p => p.TaskColumns).ThenInclude(tc => tc.ProjectTasks)
                          .Where(p => p.Id == id)
                          .FirstOrDefault();
            var projectUsers = db.UserProjects
                               .Where(up => project.Id == up.ProjectId)
                               .Select(up => up.UserId)
                               .ToList();

            if (projectUsers.Contains(userId))
            {
                return View(project);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult New(Project project)
        {
            project.OrganizerId = _userManager.GetUserId(User);
            if(ModelState.IsValid)
            {
                db.Projects.Add(project);
                db.SaveChanges();
                UserProject up = new UserProject
                {
                    UserId = project.OrganizerId,
                    ProjectId = project.Id 
                };
                db.UserProjects.Add(up);
                db.SaveChanges();
                TempData["message"] = "Board created succesfully";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Index");   
            }
            else
            {
                TempData["message"] = "Didn't name your board";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }
        }
    }
}
