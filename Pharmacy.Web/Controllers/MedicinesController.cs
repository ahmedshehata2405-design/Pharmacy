using System.Collections.Generic;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Pharmacy.Business;
using Pharmacy.Web.Helpers;

namespace Pharmacy.Web.Controllers
{
    [Authorize] // any logged-in user can view; specific actions below tighten this further
    public class MedicinesController : Controller
    {
        // GET: /Medicines
        public IActionResult Index()
        {
            DataTable medicines = clsMedicine.GetAllMedicines();
            return View(medicines);
        }

        // GET: /Medicines/Create
        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Create()
        {
            ViewBag.Categories = _GetCategoriesSelectList();
            return View(new clsMedicine());
        }

        // POST: /Medicines/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Create(clsMedicine medicine)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _GetCategoriesSelectList();
                return View(medicine);
            }

            bool isSaved = medicine.Save();

            if (!isSaved)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء إضافة الدواء، حاول مرة أخرى.");
                ViewBag.Categories = _GetCategoriesSelectList();
                return View(medicine);
            }

            TempData["SuccessMessage"] = "تم إضافة الدواء بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Medicines/Edit/5
        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Edit(int id)
        {
            clsMedicine medicine = clsMedicine.Find(id);

            if (medicine == null)
                return NotFound();

            ViewBag.Categories = _GetCategoriesSelectList();
            return View(medicine);
        }

        // POST: /Medicines/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Edit(int id, clsMedicine medicine)
        {
            if (id != medicine.MedicineID)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _GetCategoriesSelectList();
                return View(medicine);
            }

            // clsMedicine.Find() sets Mode = Update internally; here we already have
            // an object bound from the form, so make sure it will Update, not insert.
            medicine.Mode = enMode.Update;
            bool isSaved = medicine.Save();

            if (!isSaved)
            {
                ModelState.AddModelError("", "حدث خطأ أثناء تعديل الدواء، حاول مرة أخرى.");
                ViewBag.Categories = _GetCategoriesSelectList();
                return View(medicine);
            }

            TempData["SuccessMessage"] = "تم تعديل بيانات الدواء بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /Medicines/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            clsMedicine medicine = clsMedicine.Find(id);

            if (medicine == null)
                return NotFound();

            if (medicine.Delete())
                TempData["SuccessMessage"] = "تم حذف الدواء بنجاح.";
            else
                TempData["ErrorMessage"] = "لا يمكن حذف هذا الدواء لأنه مرتبط بمخزون أو فواتير أو وصفات.";

            return RedirectToAction(nameof(Index));
        }

        private List<SelectListItem> _GetCategoriesSelectList()
        {
            DataTable categories = clsCategory.GetAllCategories();
            return clsSelectListHelper.FromDataTable(categories, "CategoryID", "CategoryName");
        }
    }
}
