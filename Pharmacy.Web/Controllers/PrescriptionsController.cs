using System.Data;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Business;
using Pharmacy.Web.Helpers;

namespace Pharmacy.Web.Controllers
{
    [Authorize]
    public class PrescriptionsController : Controller
    {
        public IActionResult Index()
        {
            DataTable prescriptions = clsPrescription.GetAllPrescriptions();
            return View(prescriptions);
        }

        public IActionResult Create()
        {
            _FillDropDowns();
            return View(new clsPrescription());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(clsPrescription prescription)
        {
            prescription.Lines = prescription.Lines?
                .Where(l => l.MedicineID > 0 && l.Quantity > 0)
                .ToList();

            if (prescription.Lines == null || prescription.Lines.Count == 0)
            {
                ModelState.AddModelError("", "أضف دواء واحد على الأقل بكمية صحيحة.");
                _FillDropDowns();
                return View(prescription);
            }

            if (!prescription.Save())
            {
                ModelState.AddModelError("", "حدث خطأ أثناء حفظ الوصفة.");
                _FillDropDowns();
                return View(prescription);
            }

            TempData["SuccessMessage"] = "تم حفظ الوصفة الطبية بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Details(int id)
        {
            DataTable details = clsPrescription.GetPrescriptionDetails(id);
            ViewBag.PrescriptionID = id;
            return View(details);
        }

        private void _FillDropDowns()
        {
            ViewBag.Customers = clsSelectListHelper.FromDataTable(clsCustomer.GetAllCustomers(), "CustomerID", "Name");
            ViewBag.Doctors = clsSelectListHelper.FromDataTable(clsDoctor.GetDoctorsForComboBox(), "DoctorID", "Name");
            ViewBag.Medicines = clsMedicine.GetMedicinesForComboBox(); // used raw in JS to build row dropdowns
        }
    }
}
