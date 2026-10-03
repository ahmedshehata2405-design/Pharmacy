using System.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Business;

namespace Pharmacy.Web.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        public IActionResult Index()
        {
            DataTable customers = clsCustomer.GetAllCustomers();
            return View(customers);
        }

        public IActionResult Create()
        {
            return View(new clsCustomer());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(clsCustomer customer)
        {
            if (!ModelState.IsValid)
                return View(customer);

            if (!customer.Save())
            {
                ModelState.AddModelError("", "حدث خطأ أثناء إضافة العميل.");
                return View(customer);
            }

            TempData["SuccessMessage"] = "تم إضافة العميل بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        // Used by the "+ عميل جديد" popup on the Sales and Prescriptions screens (no page reload)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult QuickCreate(string name, string phone)
        {
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest(new { message = "اسم العميل مطلوب." });

            clsCustomer customer = new clsCustomer
            {
                Name = name.Trim(),
                Phone = string.IsNullOrWhiteSpace(phone) ? "" : phone.Trim()
            };

            if (!customer.Save())
                return StatusCode(500, new { message = "حدث خطأ أثناء إضافة العميل." });

            return Json(new { id = customer.CustomerID, name = customer.Name });
        }

        public IActionResult Edit(int id)
        {
            clsCustomer customer = clsCustomer.Find(id);
            if (customer == null)
                return NotFound();

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, clsCustomer customer)
        {
            if (id != customer.CustomerID)
                return NotFound();

            if (!ModelState.IsValid)
                return View(customer);

            customer.Mode = enMode.Update;

            if (!customer.Save())
            {
                ModelState.AddModelError("", "حدث خطأ أثناء تعديل بيانات العميل.");
                return View(customer);
            }

            TempData["SuccessMessage"] = "تم تعديل بيانات العميل بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            clsCustomer customer = clsCustomer.Find(id);
            if (customer == null)
                return NotFound();

            if (customer.Delete())
                TempData["SuccessMessage"] = "تم حذف العميل بنجاح.";
            else
                TempData["ErrorMessage"] = "لا يمكن حذف هذا العميل لأنه مرتبط بوصفات أو فواتير.";

            return RedirectToAction(nameof(Index));
        }
    }
}
