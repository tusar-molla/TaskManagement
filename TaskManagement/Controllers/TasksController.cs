using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Data;
using TaskManagement.Models;

namespace TaskManagement.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public TasksController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(bool? isCompleted, string sortOrder)
        {
            var userId = _userManager.GetUserId(User);
            var tasks = _context.TaskItems.Where(t => t.UserId == userId);

            if (isCompleted.HasValue)   
            {
                tasks = tasks.Where(t => t.IsCompleted == isCompleted.Value);
            }

            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            tasks = sortOrder == "date_desc" ? tasks.OrderByDescending(t => t.DueDate) : tasks.OrderBy(t => t.DueDate);

            return View(await tasks.ToListAsync());
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,DueDate")] TaskItem taskItem)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User is not authenticated. Please log in.");
                return View(taskItem);
            }

            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                taskItem.UserId = userId;
                taskItem.IsCompleted = false;
                _context.Add(taskItem);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"Task created: Id={taskItem.Id}, UserId={taskItem.UserId}, Title={taskItem.Title}");
                return RedirectToAction(nameof(Index));
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            foreach (var error in errors)
            {
                ModelState.AddModelError("", error);
                System.Diagnostics.Debug.WriteLine($"Validation error: {error}");
            }
            return View(taskItem);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId) || taskItem.UserId != userId)
            {
                return Unauthorized();
            }
            return View(taskItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DueDate,IsCompleted")] TaskItem taskItem)
        {
            if (id != taskItem.Id) return NotFound();
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                ModelState.AddModelError("", "User is not authenticated. Please log in.");
                return View(taskItem);
            }

            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingTask = await _context.TaskItems.FindAsync(id);
                    if (existingTask == null)
                    {
                        return NotFound();
                    }
                    if (existingTask.UserId != userId)
                    {
                        return Unauthorized();
                    }

                    existingTask.Title = taskItem.Title;
                    existingTask.Description = taskItem.Description;
                    existingTask.DueDate = taskItem.DueDate;
                    existingTask.IsCompleted = taskItem.IsCompleted;

                    _context.Update(existingTask);
                    await _context.SaveChangesAsync();                 
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TaskItems.Any(e => e.Id == taskItem.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            foreach (var error in errors)
            {
                ModelState.AddModelError("", error);
                System.Diagnostics.Debug.WriteLine($"Validation error: {error}");
            }
            return View(taskItem);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId) || taskItem.UserId != userId)
            {
                return Unauthorized();
            }
            return View(taskItem);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId) || taskItem.UserId != userId)
            {
                return Unauthorized();
            }
            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null)
            {
                return NotFound();
            }
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId) || taskItem.UserId != userId)
            {
                return Unauthorized();
            }
            taskItem.IsCompleted = !taskItem.IsCompleted;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}