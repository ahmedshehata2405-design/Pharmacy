using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Business;

namespace Pharmacy.Web.Controllers
{
    // Branch management is an owner-level job, so the whole controller is Admin only
    [Authorize(Roles = "Admin")]
    public class BranchesController : Controller
    {
        public IActionResult Index()
        {
            DataTable branches = clsBranch.GetAllBranches();
            return View(branches);
        }

        public IActionResult Create()
        {
            return View(new clsBranch());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(clsBranch branch)
        {
            if (string.IsNullOrWhiteSpace(branch.BranchName))
                ModelState.AddModelError("BranchName", "اسم الفرع مطلوب.");

            if (!ModelState.IsValid)
                return View(branch);

            branch.BranchName = branch.BranchName.Trim();

            if (!branch.Save())
            {
                ModelState.AddModelError("", "حدث خطأ أثناء إضافة الفرع.");
                return View(branch);
            }

            TempData["SuccessMessage"] = "تم إضافة الفرع بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            clsBranch branch = clsBranch.Find(id);
            if (branch == null)
                return NotFound();

            return View(branch);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, clsBranch branch)
        {
            if (id != branch.BranchID)
                return NotFound();

            if (string.IsNullOrWhiteSpace(branch.BranchName))
                ModelState.AddModelError("BranchName", "اسم الفرع مطلوب.");

            if (!ModelState.IsValid)
                return View(branch);

            branch.BranchName = branch.BranchName.Trim();
            branch.Mode = enMode.Update;

            if (!branch.Save())
            {
                ModelState.AddModelError("", "حدث خطأ أثناء تعديل بيانات الفرع.");
                return View(branch);
            }

            TempData["SuccessMessage"] = "تم تعديل بيانات الفرع بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            clsBranch branch = clsBranch.Find(id);
            if (branch == null)
                return NotFound();

            // The database refuses (foreign keys) when the branch already has users, stock or invoices
            if (branch.Delete())
                TempData["SuccessMessage"] = "تم حذف الفرع بنجاح.";
            else
                TempData["ErrorMessage"] = "لا يمكن حذف هذا الفرع لأنه مرتبط بمستخدمين أو مخزون أو فواتير.";

            return RedirectToAction(nameof(Index));
        }
    }
}
