using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Business;
using Pharmacy.Web.Helpers;

namespace Pharmacy.Web.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        public IActionResult Index()
        {
            DataTable inventory = clsInventory.GetAllInventoryWithDetails();
            ViewBag.ExpiringSoon = clsInventory.GetExpiringSoon(30);
            return View(inventory);
        }

        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Create()
        {
            _FillDropDowns();
            return View(new clsInventory());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Create(clsInventory inventory)
        {
            if (!ModelState.IsValid)
            {
                _FillDropDowns();
                return View(inventory);
            }

            if (!inventory.Save())
            {
                ModelState.AddModelError("", "حدث خطأ أثناء إضافة السجل للمخزون.");
                _FillDropDowns();
                return View(inventory);
            }

            TempData["SuccessMessage"] = "تم إضافة الدواء للمخزون بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Edit(int id)
        {
            clsInventory inventory = clsInventory.Find(id);
            if (inventory == null)
                return NotFound();

            _FillDropDowns();
            return View(inventory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Edit(int id, clsInventory inventory)
        {
            if (id != inventory.InventoryID)
                return NotFound();

            if (!ModelState.IsValid)
            {
                _FillDropDowns();
                return View(inventory);
            }

            inventory.Mode = enMode.Update;

            if (!inventory.Save())
            {
                ModelState.AddModelError("", "حدث خطأ أثناء تعديل بيانات المخزون.");
                _FillDropDowns();
                return View(inventory);
            }

            TempData["SuccessMessage"] = "تم تعديل بيانات المخزون بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            clsInventory inventory = clsInventory.Find(id);
            if (inventory == null)
                return NotFound();

            if (inventory.Delete())
                TempData["SuccessMessage"] = "تم حذف السجل من المخزون بنجاح.";
            else
                TempData["ErrorMessage"] = "تعذر حذف السجل من المخزون.";

            return RedirectToAction(nameof(Index));
        }

        private void _FillDropDowns()
        {
            ViewBag.Branches = clsSelectListHelper.FromDataTable(clsBranch.GetBranchesForComboBox(), "BranchID", "BranchName");
            ViewBag.Medicines = clsSelectListHelper.FromDataTable(clsMedicine.GetMedicinesForComboBox(), "MedicineID", "Name");
        }
    }
}
