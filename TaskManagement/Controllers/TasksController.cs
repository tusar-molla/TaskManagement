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
        public async Task<IActionResult> Create([Bind("Title,Description,DueDate,UserId")] TaskItem taskItem)
        {
            if (ModelState.IsValid)
            {
                taskItem.UserId = _userManager.GetUserId(User);
                taskItem.IsCompleted = false;
                _context.Add(taskItem);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(taskItem);
        }
        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null || taskItem.UserId != _userManager.GetUserId(User)) return NotFound();
            return View(taskItem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,DueDate,IsCompleted")] TaskItem taskItem)
        {
            if (id != taskItem.Id) return NotFound();
            if (taskItem.UserId != _userManager.GetUserId(User)) return Unauthorized();
            if (ModelState.IsValid)
            {
                try
                {
                    taskItem.UserId = _userManager.GetUserId(User);
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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var taskItem = await _context.TaskItems.FindAsync(id);
            if (taskItem == null || taskItem.UserId != _userManager.GetUserId(User)) return NotFound();
            return View(taskItem);
        }

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