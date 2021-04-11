using ClinicLog.Web.Data;
using ClinicLog.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicLog.Web.Controllers
{
    [Authorize]
    public class PatientRequestsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientRequestsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: PatientRequests
        public async Task<IActionResult> Index()
        {
            List<PatientRequest> list;

            if (User.IsInRole(Roles.Admin))
            {
                list = await _context.PatientRequests
                    .Include(r=>r.Doctor)
                    .Include(r=>r.Patient)
                    .ToListAsync();
            }
            else if (User.IsInRole(Roles.Doctor))
            {
                list = await _context.PatientRequests
                    .Where(r => r.Doctor.Id == _userManager.GetUserId(User))
                    .Include(r => r.Patient)
                    .ToListAsync();
            }
            else // Patient
            {
                list = await _context.PatientRequests
                    .Where(r => r.Patient.Id == _userManager.GetUserId(User))
                    .Include(r => r.Doctor)
                    .ToListAsync();
            }

            return View(list);
        }

        // GET: PatientRequests/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientRequests = await _context.PatientRequests
                .Include(r => r.Doctor)
                .Include(r => r.Patient)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (patientRequests == null)
            {
                return NotFound();
            }

            return View(patientRequests);
        }

        // GET: PatientRequests/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PatientRequests/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Subscject,Description")] PatientRequest patientRequests)
        {
            if (ModelState.IsValid)
            {
                patientRequests.Patient = await _userManager.GetUserAsync(User);

                var doctors = await _userManager.GetUsersInRoleAsync(Roles.Doctor);
                var doctorsIds = doctors.Select(d => d.Id).ToList();

                patientRequests.Doctor = _context.Users
                    .Where(u => doctorsIds.Contains(u.Id))
                    .Include(u => u.PatientRequests)
                    .OrderBy(d => d.PatientRequests.Count())
                    .FirstOrDefault();

                _context.Add(patientRequests);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(patientRequests);
        }

        // GET: PatientRequests/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientRequests = await _context.PatientRequests.FindAsync(id);
            if (patientRequests == null)
            {
                return NotFound();
            }
            return View(patientRequests);
        }

        // POST: PatientRequests/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Subscject,Description")] PatientRequest patientRequests)
        {
            if (id != patientRequests.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(patientRequests);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientRequestsExists(patientRequests.Id))
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
            return View(patientRequests);
        }

        // GET: PatientRequests/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patientRequests = await _context.PatientRequests
                .FirstOrDefaultAsync(m => m.Id == id);
            if (patientRequests == null)
            {
                return NotFound();
            }

            return View(patientRequests);
        }

        // POST: PatientRequests/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var patientRequests = await _context.PatientRequests.FindAsync(id);
            _context.PatientRequests.Remove(patientRequests);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientRequestsExists(int id)
        {
            return _context.PatientRequests.Any(e => e.Id == id);
        }
    }
}
