using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Business;

namespace Pharmacy.Web.Controllers
{
    [Authorize(Roles = "Admin,Pharmacist")]
    public class CategoriesController : Controller
    {
        public IActionResult Index()
        {
            DataTable categories = clsCategory.GetAllCategories();
            return View(categories);
        }

        public IActionResult Create()
        {
            return View(new clsCategory());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(clsCategory category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
                ModelState.AddModelError("CategoryName", "اسم التصنيف مطلوب.");

            if (!ModelState.IsValid)
                return View(category);

            category.CategoryName = category.CategoryName.Trim();

            if (!category.Save())
            {
                ModelState.AddModelError("", "تعذر إضافة التصنيف، ربما الاسم موجود بالفعل.");
                return View(category);
            }

            TempData["SuccessMessage"] = "تم إضافة التصنيف بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            clsCategory category = clsCategory.Find(id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, clsCategory category)
        {
            if (id != category.CategoryID)
                return NotFound();

            if (string.IsNullOrWhiteSpace(category.CategoryName))
                ModelState.AddModelError("CategoryName", "اسم التصنيف مطلوب.");

            if (!ModelState.IsValid)
                return View(category);

            category.CategoryName = category.CategoryName.Trim();
            category.Mode = enMode.Update;

            if (!category.Save())
            {
                ModelState.AddModelError("", "تعذر تعديل التصنيف، ربما الاسم موجود بالفعل.");
                return View(category);
            }

            TempData["SuccessMessage"] = "تم تعديل التصنيف بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            clsCategory category = clsCategory.Find(id);
            if (category == null)
                return NotFound();

            if (category.Delete())
                TempData["SuccessMessage"] = "تم حذف التصنيف بنجاح.";
            else
                TempData["ErrorMessage"] = "لا يمكن حذف هذا التصنيف لأن هناك أدوية مرتبطة به.";

            return RedirectToAction(nameof(Index));
        }
    }
}
