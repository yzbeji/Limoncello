using Limoncello.Data;
using Limoncello.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.HttpSys;
using Microsoft.Build.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Linq;
using System.Threading.Tasks;

namespace Limoncello.Controllers
{
    public class UpdateTaskDisplayInfoRequest
    {
        public int taskId { get; set; }
        public int oldIndex { get; set; }
        public int newIndex { get; set; }
        public int oldColumnId { get; set; }
        public int newColumnId { get; set; }
    }

    public class UpdateTaskColumnDisplayInfoRequest
    {
        public int columnId { get; set; }
        public int oldIndex { get; set; }
        public int newIndex { get; set; }
    }
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

        [Authorize(Roles = "User,Admin")]
        public IActionResult Show(int? id)
        {
            if (!isAdmin(_userManager.GetUserId(User)) && !isMember(id))
            {
                return RedirectToAction("Index");
            }
            
            var project = db.Projects
                                .Include(p => p.TaskColumns.OrderBy(tc => tc.Index))
                                    .ThenInclude(tc => tc.ProjectTasks.OrderBy(pt => pt.Index))
                                        .ThenInclude(c => c.Comments)
                                            .ThenInclude(u => u.User)
                                .Include(p => p.TaskColumns.OrderBy(tc => tc.Index))
                                    .ThenInclude(tc => tc.ProjectTasks.OrderBy(pt => pt.Index))
                                        .ThenInclude(u => u.UserTasks)
                                            .ThenInclude(u => u.User)
                                .Where(p => p.Id == id)
                                .FirstOrDefault();

            ViewBag.UserId = _userManager.GetUserId(User);
            ViewBag.IsOrganizerOrAdmin = isOrganizerOrAdmin(id, false);
            ViewBag.IsMember = isMember(id, false);
            ViewBag.IsAdmin = isAdmin(ViewBag.UserId);

            return View(project);
        }
        
        [Authorize(Roles = "User,Admin")]
        public IActionResult Settings(int? id)
        {
            if (!isOrganizerOrAdmin(id))
            {
                return RedirectToAction("Show", new { id = id });
            }

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

            return View(project);
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

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult SaveEditedComment([FromForm]int id, [FromForm]string? content)
        {
            var comment = db.Comments.FirstOrDefault(c => c.Id == id);
            var projectId = db.ProjectTasks
                   .Include(pt => pt.TaskColumn)
                   .ThenInclude(tc => tc.Project)
                   .Where(pt => pt.Id == comment.ProjectTaskId)
                   .Select(pt => pt.TaskColumn.Project.Id)
                   .FirstOrDefault();

            var userId = _userManager.GetUserId(User);
            if (comment.UserId != userId)
            {
                return RedirectToAction("Show", new { id = projectId });
            }

            comment.Content = content;
            db.SaveChanges();
            TempData["ShowModal"] = true;
            TempData["TaskId"] = comment.ProjectTaskId;
            return RedirectToAction("Show", new { id = projectId });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult EditProject(Project reqProject)
        {
            if (!isOrganizerOrAdmin(reqProject.Id))
            {
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
                return RedirectToAction("Settings", new { id = reqProject.Id });
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult AddMemberToProject(int projectId, string userEmail)
        {
            if (!isOrganizerOrAdmin(projectId))
            {
                return RedirectToAction("Settings", new { id = projectId });
            } 

            var project = db.Projects.Include(p => p.UserProjects).FirstOrDefault(p => p.Id == projectId);

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

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult RemoveMember(int projectId, string userId)
        {
            if (!isOrganizerOrAdmin(projectId))
            {
                return RedirectToAction("Settings", new { id = projectId });
            }

            var userProject = db.UserProjects
                                .Where(up => up.ProjectId == projectId && up.UserId == userId)
                                .FirstOrDefault();

            if (userProject == null)
            {
                TempData["message"] = "Something went wrong!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Settings", new { id = projectId });
            }

            if (userId == _userManager.GetUserId(User) && !isAdmin(userId))
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

        [Authorize(Roles = "User,Admin")]
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

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult Delete(int projectId)
        {
            if (!isOrganizerOrAdmin(projectId))
            {
                return RedirectToAction("Settings", new { id = projectId });
            }

            var project = db.Projects.Include(p => p.UserProjects).FirstOrDefault(p => p.Id == projectId);

            if (project == null)
            {
                TempData["message"] = "Project not found";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            db.Projects.Remove(project);
            db.SaveChanges();

            TempData["message"] = "Project deleted successfully";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult MakeOrganizer(int projectId, string userId)
        {
            if (!isOrganizerOrAdmin(projectId))
            {
                return RedirectToAction("Settings", new { id = projectId });
            }

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

            var action = isAdmin(_userManager.GetUserId(User)) ? "Settings" : "Show";
            return RedirectToAction(action, new { id = projectId });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult AddComment(Comment comment)
        {
            comment.UserId = _userManager.GetUserId(User);
            comment.CreatedDate = DateTime.Now;
            var projectId = db.ProjectTasks
                       .Include(pt => pt.TaskColumn)
                       .ThenInclude(tc => tc.Project)
                       .Where(pt => pt.Id == comment.ProjectTaskId)
                       .Select(pt => pt.TaskColumn.Project.Id) 
                       .FirstOrDefault();

            if (!isAdmin(_userManager.GetUserId(User)) && !isMember(projectId))
            {
                return RedirectToAction("Index");
            }

            db.Comments.Add(comment);
            db.SaveChanges();

            TempData["ShowModal"] = true;
            TempData["TaskId"] = comment.ProjectTaskId;
            return RedirectToAction("Show", new { id = projectId });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult RemoveComment([FromForm] int commentId) 
        {
            var comment = db.Comments.FirstOrDefault(c => c.Id == commentId);
            var projectId = db.ProjectTasks
                       .Include(pt => pt.TaskColumn)
                       .ThenInclude(tc => tc.Project)
                       .Where(pt => pt.Id == comment.ProjectTaskId)
                       .Select(pt => pt.TaskColumn.Project.Id)
                       .FirstOrDefault();

            var userId = _userManager.GetUserId(User);
            if (!isAdmin(userId) && (userId != comment.UserId || !isMember(projectId)))
            {
                TempData["message"] = "Not allowed!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = projectId });
            }

            if (comment != null)
            {
                db.Comments.Remove(comment);
                db.SaveChanges();

                TempData["ShowModal"] = true;
                TempData["TaskId"] = comment.ProjectTaskId;
                return RedirectToAction("Show", new { id = projectId });
            }
            else
            {
                return BadRequest();
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult AddTaskColumn([FromForm] TaskColumn reqTaskColumn)
        {
            if (!isOrganizerOrAdmin(reqTaskColumn.ProjectId))
            {
                return RedirectToAction("Show", new { id = reqTaskColumn.ProjectId });
            }

            if (ModelState.IsValid)
            {
                reqTaskColumn.Index = db.TaskColumns.Where(tc => tc.ProjectId == reqTaskColumn.ProjectId).Count();

                db.TaskColumns.Add(reqTaskColumn);
                db.SaveChanges();

                TempData["message"] = "Column added successfully!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Show", new { id = reqTaskColumn.ProjectId });
            }
            else
            {
                TempData["message"] = "You must name the column!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = reqTaskColumn.ProjectId });
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult EditTaskColumn([FromForm] TaskColumn reqTaskColumn)
        {
            if (!isOrganizerOrAdmin(reqTaskColumn.ProjectId))
            {
                return RedirectToAction("Show", new { id = reqTaskColumn.ProjectId });
            }

            var taskCol = db.TaskColumns.Find(reqTaskColumn.Id);
            var userId = _userManager.GetUserId(User);
            var project = db.Projects.Find(taskCol.ProjectId);

            if (ModelState.IsValid)
            {
                taskCol.Name = reqTaskColumn.Name;
                db.SaveChanges();

                TempData["message"] = "Column edited successfully!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Show", new { id = taskCol.ProjectId });
            }
            else
            {
                TempData["message"] = "You must name the column!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = taskCol.ProjectId });
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult DeleteTaskColumn(int columnId)
        {
            var taskCol = db.TaskColumns.Find(columnId);
            var project = db.Projects.Find(taskCol.ProjectId);
            if (!isOrganizerOrAdmin(project.Id))
            {
                return RedirectToAction("Show", new { id = project.Id });
            }

            // make sure to keep indexes dense
            foreach (var tc in db.TaskColumns.Where(tc => tc.Index > taskCol.Index && tc.ProjectId == taskCol.ProjectId))
            {
                tc.Index -= 1;
            }

            db.TaskColumns.Remove(taskCol);
            db.SaveChanges();

            TempData["message"] = "Column added successfully!";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Show", new { id = taskCol.ProjectId });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult UpdateTaskColumnDisplayInfo([FromBody] UpdateTaskColumnDisplayInfoRequest req)
        {
            var column = db.TaskColumns.Find(req.columnId);
            if (!isAdmin(_userManager.GetUserId(User)) && !isMember(column.ProjectId))
            {
                return Json(new { success = false, message = "You are not part of the board!" });
            }

            if (column == null)
            {
                return Json(new { success = false, message = "Task column not found" });
            }

            if (req.oldIndex < req.newIndex)
            {
                foreach (var tc in db.TaskColumns.Where(tc => tc.ProjectId == column.ProjectId && tc.Index > req.oldIndex && tc.Index <= req.newIndex))
                {
                    tc.Index -= 1;
                }

                column.Index = req.newIndex;
                db.SaveChanges();
            }
            else
            {
                foreach (var tc in db.TaskColumns.Where(tc => tc.ProjectId == column.ProjectId && tc.Index >= req.newIndex && tc.Index < req.oldIndex))
                {
                    tc.Index += 1;
                }

                column.Index = req.newIndex;
                db.SaveChanges();
            }

            return Json(new { success = true, message = "Column moved successfully!" });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult AddTask([FromForm] ProjectTask reqTask)
        {
            var projectId = db.TaskColumns
                                .Include(tc => tc.Project)
                                .Where(tc => tc.Id == reqTask.TaskColumnId)
                                .Select(tc => tc.Project.Id)
                                .FirstOrDefault();

            if (!isOrganizerOrAdmin(projectId))
            {
                return RedirectToAction("Index");
            }

            if (reqTask.StartDate.HasValue && reqTask.DueDate.HasValue && reqTask.StartDate.Value > reqTask.DueDate.Value)
            {
                ModelState.AddModelError("DueDate", "Due Date must be after Start Date.");
            }

            if (ModelState.IsValid)
            {
                reqTask.Status = Models.TaskStatus.NotStarted;
                reqTask.Index = db.ProjectTasks.Where(pt => pt.TaskColumnId == reqTask.TaskColumnId).Count();

                db.ProjectTasks.Add(reqTask);
                db.SaveChanges();

                TempData["message"] = "Task added successfully!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Show", new { id = projectId });
            }
            else
            {
                var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["message"] = string.Join(" ", errorMessages);
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = projectId });
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult EditTask([FromForm] ProjectTask reqTask)
        {
            var projectId = db.TaskColumns
                                .Include(tc => tc.Project)
                                .Where(tc => tc.Id == reqTask.TaskColumnId)
                                .Select(tc => tc.Project.Id)
                                .FirstOrDefault();

            if (!isOrganizerOrAdmin(projectId))
            {
                return RedirectToAction("Index");
            }

            if (reqTask.StartDate.HasValue && reqTask.DueDate.HasValue && reqTask.StartDate.Value > reqTask.DueDate.Value)
            {
                ModelState.AddModelError("DueDate", "Due Date must be after Start Date.");
            }

            if (ModelState.IsValid)
            {
                var task = db.ProjectTasks.Find(reqTask.Id);
                task.Description = reqTask.Description;
                task.Title = reqTask.Title;
                task.StartDate = reqTask.StartDate;
                task.DueDate = reqTask.DueDate;
                task.Content = reqTask.Content;
                db.SaveChanges();

                TempData["message"] = "Task added successfully!";
                TempData["messageType"] = "alert-success";
                return RedirectToAction("Show", new { id = projectId });
            }
            else
            {
                var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["message"] = string.Join(" ", errorMessages);
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = projectId });
            }
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult EditTaskStatus([FromForm] ProjectTask reqTask)
        {

            var projectId = db.TaskColumns
                                .Include(tc => tc.Project)
                                .Where(tc => tc.Id == reqTask.TaskColumnId)
                                .Select(tc => tc.Project.Id)
                                .FirstOrDefault();
            if (!isAdmin(_userManager.GetUserId(User)) && !isMember(projectId))
            {
                return Json(new { success = false, message = "You do not have permission to update this task" });
            }

            var task = db.ProjectTasks
                            .Include(t => t.TaskColumn)
                            .ThenInclude(tc => tc.Project)
                            .ThenInclude(p => p.UserProjects)
                            .Where(t => t.Id == reqTask.Id)
                            .FirstOrDefault();
            
            if (task == null)
            {
                return Json(new { success = false, message = "Task not found" });
            }

            task.Status = reqTask.Status;
            db.SaveChanges();

            return Json(new { success = true, message = "Task status updated!" });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult AddUserToTask(int taskId, string email)
        {
            var projectId = db.ProjectTasks
                .Include(t => t.TaskColumn)
                .Where(tc => tc.Id == taskId)
                .Select(pt => pt.TaskColumn.ProjectId)
                .FirstOrDefault();

            if (!isOrganizerOrAdmin(projectId))
            {
                return RedirectToAction("Show", new { id = projectId });
            }

            var user = _userManager.FindByEmailAsync(email).Result;
            if (user == null)
            {
                TempData["message"] = "User does not exist!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Show", new { id = projectId });
            }
            string userId = user.Id;

            var userTask = new UserTask
            {
                UserId = userId,
                TaskId = taskId
            };

            db.UserTasks.Add(userTask);
            db.SaveChanges();

            TempData["message"] = "User added to task";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Show", new { id = projectId });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult RemoveUserFromTask(int taskId, string userId)
        {
            var projectId = db.ProjectTasks
                .Include(t => t.TaskColumn)
                .Where(tc => tc.Id == taskId)
                .Select(pt => pt.TaskColumn.ProjectId)
                .FirstOrDefault();

            if (!isOrganizerOrAdmin(projectId))
            {
                return RedirectToAction("Show", new { id = projectId });
            }

            var userTask = db.UserTasks
                            .Where(ut => ut.TaskId == taskId && ut.UserId == userId)
                            .FirstOrDefault();
            if (userTask == null)
            {
                TempData["message"] = "Something went wrong!";
                TempData["messageType"] = "alert-danger";
                return RedirectToAction("Index");
            }

            db.UserTasks.Remove(userTask);
            db.SaveChanges();

            TempData["message"] = "User removed successfully!";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Show", new { id = projectId });
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult DeleteTask(int taskId)
        {
            var projectId = db.ProjectTasks
                                .Include(t => t.TaskColumn)
                                .ThenInclude(tc => tc.Project)
                                .Where(t => t.Id == taskId)
                                .Select(t => t.TaskColumn.Project.Id)
                                .FirstOrDefault();
            if (!isOrganizerOrAdmin(projectId))
            {
                return RedirectToAction("Index");
            }

            // keep indexes dense
            var task = db.ProjectTasks.Find(taskId);
            foreach (var pt in db.ProjectTasks.Where(pt => pt.Index > task.Index && pt.TaskColumnId == task.TaskColumnId))
            {
                pt.Index -= 1;
            }

            db.ProjectTasks.Remove(task);
            db.SaveChanges();

            TempData["message"] = "Task deleted successfully!";
            TempData["messageType"] = "alert-success";
            return RedirectToAction("Show", new { id = projectId});
        }

        [Authorize(Roles = "User,Admin")]
        [HttpPost]
        public IActionResult UpdateTaskDisplayInfo([FromBody] UpdateTaskDisplayInfoRequest req)
        {
            var task = db.ProjectTasks.Find(req.taskId);
            var projectId = db.ProjectTasks
                                .Include(pt => pt.TaskColumn)
                                .Where(pt => pt.Id == req.taskId)
                                .Select(pt => pt.TaskColumn.ProjectId)
                                .FirstOrDefault();

            if (!isAdmin(_userManager.GetUserId(User)) && !isMember(projectId))
            {
                return Json(new { success = false, message = "You are not part of the board!" });
            }

            if (task == null)
            {
                return Json(new { success = false, message = "Task not found" });
            }

            if (req.newColumnId == req.oldColumnId)
            {
                if (req.newIndex < req.oldIndex)
                {
                    foreach (var pt in db.ProjectTasks.Where(pt => pt.TaskColumnId == task.TaskColumnId && pt.Index >= req.newIndex && pt.Index < req.oldIndex))
                    {
                        pt.Index += 1;
                    }
                }
                else
                {
                    foreach(var pt in db.ProjectTasks.Where(pt => pt.TaskColumnId == task.TaskColumnId && pt.Index <= req.newIndex && pt.Index > req.oldIndex))
                    {
                        pt.Index -= 1;
                    }
                }
                
                task.Index = req.newIndex;
                db.SaveChanges();
            }
            else
            {
                foreach (var pt in db.ProjectTasks.Where(pt => pt.TaskColumnId == req.oldColumnId && pt.Index > req.oldIndex))
                {
                    pt.Index -= 1;
                }
                foreach (var pt in db.ProjectTasks.Where(pt => pt.TaskColumnId == req.newColumnId && pt.Index >= req.newIndex))
                {
                    pt.Index += 1;
                }

                task.TaskColumnId = req.newColumnId;
                task.Index = req.newIndex;
                db.SaveChanges();
            }

            return Json(new { success = true, message = "Task moved successfully" });
        }

        private bool isAdmin(string? userId)
        {
            if (userId == null)
            {
                return false;
            }

            var user = _userManager.FindByIdAsync(userId).Result;
            return _userManager.IsInRoleAsync(user, "Admin").Result;
        }

        private bool isOrganizerOrAdmin(int? projectId, bool showMessage = true)
        {
            if (projectId == null)
            {
                return false;
            }

            var project = db.Projects.Find(projectId);
            var userId = _userManager.GetUserId(User);
            if (project == null || userId == null)
            {
                return false;
            }
            var isOrganizer = userId == project.OrganizerId || isAdmin(userId);

            if (isOrganizer == false && showMessage == true)
            {
                TempData["message"] = "You are not the organizer of this project";
                TempData["messageType"] = "alert-danger";
            }

            return isOrganizer;
        }

        private bool isMember(int? projectId, bool showMessage = true)
        {
            if (projectId == null)
            {
                return false;
            }

            var projectUsers = db.UserProjects.Where(up => up.ProjectId == projectId).Select(up => up.UserId).ToList();
            var userId = _userManager.GetUserId(User);
            var isMember = projectUsers.Contains(userId);

            if (!isMember && showMessage == true)
            {
                TempData["message"] = "You are not part of the board!";
                TempData["messageType"] = "alert-danger";
            }

            return isMember;
        }
    }
}
