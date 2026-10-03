using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Business;

namespace Pharmacy.Web.Controllers
{
    [Authorize]
    public class SuppliersController : Controller
    {
        public IActionResult Index()
        {
            DataTable suppliers = clsSupplier.GetAllSuppliers();
            return View(suppliers);
        }

        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Create()
        {
            return View(new clsSupplier());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Create(clsSupplier supplier)
        {
            if (!ModelState.IsValid)
                return View(supplier);

            if (!supplier.Save())
            {
                ModelState.AddModelError("", "حدث خطأ أثناء إضافة المورد.");
                return View(supplier);
            }

            TempData["SuccessMessage"] = "تم إضافة المورد بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Edit(int id)
        {
            clsSupplier supplier = clsSupplier.Find(id);
            if (supplier == null)
                return NotFound();

            return View(supplier);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,Pharmacist")]
        public IActionResult Edit(int id, clsSupplier supplier)
        {
            if (id != supplier.SupplierID)
                return NotFound();

            if (!ModelState.IsValid)
                return View(supplier);

            supplier.Mode = enMode.Update;

            if (!supplier.Save())
            {
                ModelState.AddModelError("", "حدث خطأ أثناء تعديل بيانات المورد.");
                return View(supplier);
            }

            TempData["SuccessMessage"] = "تم تعديل بيانات المورد بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            clsSupplier supplier = clsSupplier.Find(id);
            if (supplier == null)
                return NotFound();

            if (supplier.Delete())
                TempData["SuccessMessage"] = "تم حذف المورد بنجاح.";
            else
                TempData["ErrorMessage"] = "لا يمكن حذف هذا المورد لأنه مرتبط بفواتير شراء.";

            return RedirectToAction(nameof(Index));
        }
    }
}
