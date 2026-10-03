using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Business;
using Pharmacy.Web.Helpers;

namespace Pharmacy.Web.Controllers
{
    [Authorize(Roles = "Admin,Pharmacist")]
    public class PurchasesController : Controller
    {
        public IActionResult Index()
        {
            DataTable purchases = clsPurchase.GetAllPurchases();
            return View(purchases);
        }

        public IActionResult Create()
        {
            _FillDropDowns();
            return View(new clsPurchase());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(clsPurchase purchase)
        {
            // Drop any blank rows the user didn't fill in (JS lets them add/remove rows freely)
            purchase.Lines = purchase.Lines?
                .Where(l => l.MedicineID > 0 && l.Quantity > 0)
                .ToList();

            if (purchase.Lines == null || purchase.Lines.Count == 0)
            {
                ModelState.AddModelError("", "أضف صنف واحد على الأقل بكمية صحيحة.");
                _FillDropDowns();
                return View(purchase);
            }

            if (!purchase.Save())
            {
                ModelState.AddModelError("", purchase.LastError ?? "حدث خطأ أثناء حفظ فاتورة الشراء.");
                _FillDropDowns();
                return View(purchase);
            }

            TempData["SuccessMessage"] = "تم حفظ فاتورة الشراء، وتحديث المخزون تلقائيًا.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            DataTable details = clsPurchase.GetPurchaseDetails(id);
            ViewBag.PurchaseID = id;
            return View(details);
        }

        private void _FillDropDowns()
        {
            ViewBag.Suppliers = clsSelectListHelper.FromDataTable(clsSupplier.GetAllSuppliers(), "SupplierID", "Name");
            ViewBag.Branches = clsSelectListHelper.FromDataTable(clsBranch.GetBranchesForComboBox(), "BranchID", "BranchName");
            ViewBag.Medicines = clsMedicine.GetMedicinesForComboBox(); // used raw in JS to build row dropdowns
        }
    }
}
