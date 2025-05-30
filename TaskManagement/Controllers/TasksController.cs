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

        // GET: Tasks
        public async Task<IActionResult> Index(bool? isCompleted, string sortOrder)
        {
            var userId = _userManager.GetUserId(User);
            var tasks = _context.TaskItems.Where(t => t.UserId == userId);

            // Filter by completion status
            if (isCompleted.HasValue)
            {
                tasks = tasks.Where(t => t.IsCompleted == isCompleted.Value);
            }

            // Sort by due date
            ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
            tasks = sortOrder == "date_desc" ? tasks.OrderByDescending(t => t.DueDate) : tasks.OrderBy(t => t.DueDate);

            return View(await tasks.ToListAsync());
        }

        // GET: Tasks/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Tasks/Create
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

            // Remove UserId from ModelState validation
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                taskItem.UserId = userId;
                taskItem.IsCompleted = false;
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Log validation errors for debugging
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
            foreach (var error in errors)
            {
                ModelState.AddModelError("", error);
            }
            return View(taskItem);
        }

        // GET: Tasks/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null || taskItem.UserId != _userManager.GetUserId(User)) return NotFound();
            return View(taskItem);
        }

        // POST: Tasks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DueDate,IsCompleted")] TaskItem taskItem)
        {
            if (id != taskItem.Id) return NotFound();
            var userId = _userManager.GetUserId(User);
            if (taskItem.UserId != userId) return Unauthorized();

            // Remove UserId from ModelState validation
            ModelState.Remove("UserId");

            if (ModelState.IsValid)
            {
                try
                {
                    taskItem.UserId = userId;
                    _context.Update(taskItem);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.TaskItems.Any(e => e.Id == taskItem.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(taskItem);
        }

        // GET: Tasks/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null || taskItem.UserId != _userManager.GetUserId(User)) return NotFound();
            return View(taskItem);
        }

        // POST: Tasks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null || taskItem.UserId != _userManager.GetUserId(User)) return NotFound();
            _context.TaskItems.Remove(taskItem);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // POST: Tasks/ToggleComplete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null || taskItem.UserId != _userManager.GetUserId(User)) return NotFound();
            taskItem.IsCompleted = !taskItem.IsCompleted;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}