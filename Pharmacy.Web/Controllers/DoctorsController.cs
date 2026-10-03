using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Business;

namespace Pharmacy.Web.Controllers
{
    [Authorize]
    public class DoctorsController : Controller
    {
        public IActionResult Index()
        {
            DataTable doctors = clsDoctor.GetAllDoctors();
            return View(doctors);
        }

        public IActionResult Create()
        {
            return View(new clsDoctor());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(clsDoctor doctor)
        {
            if (!ModelState.IsValid)
                return View(doctor);

            if (!doctor.Save())
            {
                ModelState.AddModelError("", "حدث خطأ أثناء إضافة الطبيب.");
                return View(doctor);
            }

            TempData["SuccessMessage"] = "تم إضافة الطبيب بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // Used by the "+ طبيب جديد" popup on the Prescriptions screen (no page reload)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult QuickCreate(string name, string specialization, string phone)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "اسم الطبيب مطلوب." });

            clsDoctor doctor = new clsDoctor
            {
                Name = name.Trim(),
                Specialization = string.IsNullOrWhiteSpace(specialization) ? "" : specialization.Trim(),
                Phone = string.IsNullOrWhiteSpace(phone) ? "" : phone.Trim()
            };

            if (!doctor.Save())
                return StatusCode(500, new { message = "حدث خطأ أثناء إضافة الطبيب." });

            return Json(new { id = doctor.DoctorID, name = doctor.Name });
        }

        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Edit(int id)
        {
            clsDoctor doctor = clsDoctor.Find(id);
            if (doctor == null)
                return NotFound();

            return View(doctor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Edit(int id, clsDoctor doctor)
        {
            if (id != doctor.DoctorID)
                return NotFound();

            if (!ModelState.IsValid)
                return View(doctor);

            doctor.Mode = enMode.Update;

            if (!doctor.Save())
            {
                ModelState.AddModelError("", "حدث خطأ أثناء تعديل بيانات الطبيب.");
                return View(doctor);
            }

            TempData["SuccessMessage"] = "تم تعديل بيانات الطبيب بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            clsDoctor doctor = clsDoctor.Find(id);
            if (doctor == null)
                return NotFound();

            if (doctor.Delete())
                TempData["SuccessMessage"] = "تم حذف الطبيب بنجاح.";
            else
                TempData["ErrorMessage"] = "لا يمكن حذف هذا الطبيب لأنه مرتبط بوصفات طبية.";

            return RedirectToAction(nameof(Index));
        }
    }
}
