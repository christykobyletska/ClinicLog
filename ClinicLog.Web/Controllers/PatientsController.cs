using ClinicLog.Web.Data;
using ClinicLog.Web.Models;
using ClinicLog.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClinicLog.Web.Controllers
{
    [Authorize(Roles = Roles.AdminAndDoctor)]
    public class PatientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PatientsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Patients
        public async Task<IActionResult> Index()
        {
            var patients = new List<User>();

            if (User.IsInRole(Roles.Doctor))
            {
                var user = await _userManager.GetUserAsync(User);
                patients.AddRange(_context.PatientRequests
                    .Where(p => p.DoctorID == user.Id)
                    .Select(r => r.Patient.ToUserViewModel()));
            }
            else if (User.IsInRole(Roles.Admin))
            {
                foreach (var appUser in _userManager.Users)
                {
                    var isDoctor = await _userManager.IsInRoleAsync(appUser, Roles.Doctor);
                    var isAdmin = await _userManager.IsInRoleAsync(appUser, Roles.Admin);

                    if (!isDoctor && !isAdmin)
                    {
                        patients.Add(appUser.ToUserViewModel());
                    }
                }
            }

            return View(patients);
        }

        // GET: Patients/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient.ToUserViewModel());
        }

        // GET: Patients/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Patients/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,PhoneNumber,Password")] User patient)
        {
            if (ModelState.IsValid)
            {
                patient.Id = Guid.NewGuid().ToString().ToLowerInvariant();

                var identityUser = patient.ToUserModel();

                await _userManager.CreateAsync(identityUser, patient.Password);
                await _userManager.AddToRoleAsync(identityUser, Roles.Doctor);

                return RedirectToAction(nameof(Index));
            }
            return View(patient);
        }

        // GET: Patients/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Users.FindAsync(id);
            if (patient == null)
            {
                return NotFound();
            }
            return View(patient.ToUserViewModel());
        }

        // POST: Patients/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("FullName,Email,PhoneNumber")] User patient)
        {
            patient.Id = id;

            if (ModelState.IsValid)
            {
                try
                {
                    var user = _context.Users.First(u => u.Id == id);

                    user.FullName = patient.FullName;
                    user.Email = patient.Email;
                    user.PhoneNumber = patient.PhoneNumber;

                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PatientExists(patient.Id))
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
            return View(patient);
        }

        // GET: Patients/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var patient = await _context.Users.FirstOrDefaultAsync(m => m.Id == id);
            if (patient == null)
            {
                return NotFound();
            }

            return View(patient.ToUserViewModel());
        }

        // POST: Patients/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var patient = await _context.Users.FindAsync(id);
            _context.Users.Remove(patient);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PatientExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
