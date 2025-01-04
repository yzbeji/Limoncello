using Limoncello.Data;
using Limoncello.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Execution;
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

            ViewBag.UserId = userId;

            if (projectUsers.Contains(userId))
            {
                return View(project);
            }
            else
            {
                return RedirectToAction("Index");
            }
        }

        public IActionResult Settings(int? id)
        {
            var project = db.Projects
                            .Include(p => p.UserProjects)
                            .ThenInclude(up => up.User)
                            .FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                TempData["message"] = "Project not found";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            string userId = _userManager.GetUserId(User);
            ViewBag.UserId = userId;

            // display the organizer first
            project.UserProjects = project.UserProjects
                                            .OrderBy(up => up.UserId != project.OrganizerId)
                                            .ToList();

            if (project.OrganizerId != userId)
            {
                TempData["message"] = "You are not the organizer of this project";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = id });
            }
            else
            {
                return View(project);
            }
        }

        [Authorize(Roles = "User,Admin")]
        public IActionResult New(Project project)
        {
            project.OrganizerId = _userManager.GetUserId(User);
            if (ModelState.IsValid)
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

        [HttpPost]
        public IActionResult Edit(Project reqProject)
        {
            var projectOrganizer = db.Projects.Where(p => p.Id == reqProject.Id).Select(p => p.OrganizerId).FirstOrDefault();
            if (_userManager.GetUserId(User) != projectOrganizer)
            {
                TempData["message"] = "You are not the organizer of this project";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = reqProject.Id });
            }

            if (ModelState.IsValid)
            {
                var project = db.Projects.Find(reqProject.Id);
                project.Name = reqProject.Name;
                db.SaveChanges();
                TempData["message"] = "Board name updated";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Settings", new { id = reqProject.Id });
            }
            else
            {
                TempData["message"] = "You must name the board!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Settings", new { id = reqProject.Id});
            }   
        }

        [HttpPost]
        public IActionResult AddMember(int projectId, string userEmail)
        {
            if (userEmail == null)
            {
                TempData["message"] = "Please specify an email!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Settings", new { id = projectId });
            }

            var user = _userManager.FindByEmailAsync(userEmail).Result;

            if (user == null)
            {
                TempData["message"] = "User not found";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Settings", new { id = projectId });
            }

            var project = db.Projects.Include(p => p.UserProjects).FirstOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                TempData["message"] = "Project not found";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            if (project.UserProjects.Any(up => up.UserId == user.Id))
            {
                TempData["message"] = "User is already a member of this project";
                TempData["messageType"] = "alert-warning";
                return RedirectToAction("Settings", new { id = projectId });
            }

            var userProject = new UserProject
            {
                UserId = user.Id,
                ProjectId = project.Id
            };

            db.UserProjects.Add(userProject);
            db.SaveChanges();

            TempData["message"] = "User added to project";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Settings", new { id = projectId });
        }

        [HttpPost]
        public IActionResult RemoveMember(int projectId, string userId)
        {
            var userProject = db.UserProjects
                                .Where(up => up.ProjectId == projectId && up.UserId == userId)
                                .FirstOrDefault();

            if (userProject == null)
            {
                TempData["message"] = "Something went wrong!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Settings", new { id = projectId });
            }

            if (userId == _userManager.GetUserId(User))
            {
                TempData["message"] = "You can't remove yourself!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Settings", new { id = projectId });
            }

            db.UserProjects.Remove(userProject);
            db.SaveChanges();

            TempData["message"] = "User removed successfully!";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Settings", new { id = projectId });
        }

        [HttpPost]
        public IActionResult LeaveBoard(int projectId)
        {
            var userId = _userManager.GetUserId(User);
            var userProject = db.UserProjects
                                .Where(up => up.ProjectId == projectId && up.UserId == userId)
                                .FirstOrDefault();

            var project = db.Projects.Find(projectId);
            if (project.OrganizerId == userId)
            {
                // should not be able to reach this through UI

                TempData["message"] = "You are the organizer of this board!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = projectId });
            }

            db.UserProjects.Remove(userProject);
            db.SaveChanges();

            TempData["message"] = "You left the board!";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "User,Admin")]
        public IActionResult Delete(int id)
        {
            var project = db.Projects.Include(p => p.UserProjects).FirstOrDefault(p => p.Id == id);

            if (project == null)
            {
                TempData["message"] = "Project not found";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            string userId = _userManager.GetUserId(User);

            if (project.OrganizerId != userId)
            {
                TempData["message"] = "You are not the organizer of this project";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = id });
            }

            db.Projects.Remove(project);
            db.SaveChanges();

            TempData["message"] = "Project deleted successfully";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult MakeOrganizer(int projectId, string userId)
        {
            var project = db.Projects.Find(projectId);

            var userProject = db.UserProjects.Where(up => up.ProjectId == projectId && up.UserId == userId);

            if (userProject == null)
            {
                TempData["message"] = "The user is not part of the board!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Settings", new { id = projectId });
            }

            project.OrganizerId = userId;
            db.SaveChanges();

            TempData["message"] = "Organizer changed successfully!";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Show", new { id = projectId });
        }
    }
}
