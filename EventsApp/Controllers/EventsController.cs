using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Day11Identity.IdentityData;
using EventsApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json;
using System.Security.Claims;

namespace EventsApp.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventIdentityDbContext _context;

        public EventsController(EventIdentityDbContext context)
        {
            _context = context;
        }

        // GET: Events
        public async Task<IActionResult> Index(int Category)
        {
            var eventIdentityDbContext = _context.Events.ToList();
            if (Category != null && Category != 0)
            {
                eventIdentityDbContext = _context.Events.Where(m => m.CategoryId == Category).ToList();
            }
            ViewBag.Image = _context.Images.ToList();
            return View(eventIdentityDbContext);
        }

        public IActionResult CardInfo(int id)
        {
            ViewBag.EventUserRlation = _context.EventUserRelations.Where(m=>m.EventId==id).ToList();
            ViewBag.Image = _context.Images.ToList();
            Event _event = _context.Events.Find(id);

            ViewBag.Category = _context.Categories.Find(_event.CategoryId).Name;
            return View(_event);
        }
        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var mevent = await _context.Events
                .Include(m => m.Category)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mevent == null)
            {
                return NotFound();
            }

            return View(mevent);
        }

        // GET: Events/Create
        [HttpGet]

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList();
            return View();
        }

        // POST: Events/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,CategoryId,ProviderId,Accepted")] Event mevent, string base64ImageData)
        {
            _context.Add(mevent);

            int NewId = _context.Events.OrderByDescending(m => m.Id).FirstOrDefault().Id;
            if (!string.IsNullOrEmpty(base64ImageData))
            {
                List<string> images = JsonSerializer.Deserialize<List<string>>(base64ImageData);
                mevent.ProviderId = _context.Users.Where(d => d.UserName == User.Identity.Name).ToList().FirstOrDefault().Id;
                UploadBase64(images, NewId);

            }
            await _context.SaveChangesAsync();
            

            return RedirectToAction(nameof(Index));
            ViewBag.Categories = _context.Categories.ToList();
            return View(mevent);
        }

        // GET: Events/Edit/5

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var mevent = await _context.Events.FindAsync(id);
            if (mevent == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", mevent.CategoryId);
            ViewData["ProviderId"] = new SelectList(_context.Users, "Id", "Id", mevent.ProviderId);
            return View(mevent);
        }

        // POST: Events/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]

        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,CategoryId,ProviderId,Accepted")] Event mevent)
        {
            if (id != mevent.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(mevent);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(mevent.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "Id", "Id", mevent.CategoryId);
            ViewData["ProviderId"] = new SelectList(_context.Users, "Id", "Id", mevent.ProviderId);
            return View(mevent);
        }

        // GET: Events/Delete/5

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Events == null)
            {
                return NotFound();
            }

            var mevent = await _context.Events
                .Include(m => m.Category)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (mevent == null)
            {
                return NotFound();
            }

            return View(mevent);
        }
        
        [NonAction]

        [Authorize(Roles = "Admin")]
        public void UploadBase64(List<string> model, int EventId)
        {
            if (model.Count != 0)
            {
                foreach (var item in model)
                {

                    EventImage image = new EventImage() { EventId = EventId, Url = item };

                    _context.Images.Add(image);

                    // Save imageBytes to database or perform other actions}

                }
            }
        }


        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Events == null)
            {
                return Problem("Entity set 'EventIdentityDbContext.Events'  is null.");
            }
            var mevent = await _context.Events.FindAsync(id);
            if (mevent != null)
            {
                _context.Events.Remove(mevent);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles ="Admin, User")]
        public async Task<IActionResult> Participate(int EventId)
        {
            var userIdClaim = User?.Identity?.IsAuthenticated;

            if (userIdClaim==true)
            {
                int userId = _context.Users.Where(item => item.UserName == User.Identity.Name).FirstOrDefault().Id;
                var eventUserRelation = new EventUserRelation()
                {
                    Date = DateTime.Now,
                    EventId = EventId,
                    UserId = userId
                };
                await _context.EventUserRelations.AddAsync(eventUserRelation);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> UnParticipate(int Id)
        {
            var relation = _context.EventUserRelations.Where(m => m.Id == Id).FirstOrDefault();
                _context.EventUserRelations.Remove(relation);
                await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }
        private bool EventExists(int id)
        {
          return (_context.Events?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
