using System.Data;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pharmacy.Business;
using Pharmacy.Web.Helpers;

namespace Pharmacy.Web.Controllers
{
    [Authorize]
    public class SalesController : Controller
    {
        public IActionResult Index()
        {
            DataTable sales = clsSale.GetAllSales();
            return View(sales);
        }

        public IActionResult Create()
        {
            clsSale sale = new clsSale();

            // Start on the logged-in user's own branch (they can still switch to another one)
            if (int.TryParse(User.FindFirst("BranchID")?.Value, out int userBranchID))
                sale.BranchID = userBranchID;

            _FillDropDowns();
            return View(sale);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(clsSale sale)
        {
            sale.Lines = sale.Lines?
                .Where(l => l.MedicineID > 0 && l.Quantity > 0)
                .ToList();

            if (sale.BranchID <= 0)
            {
                ModelState.AddModelError("", "اختر الفرع أولًا.");
                _FillDropDowns();
                return View(sale);
            }

            if (sale.Lines == null || sale.Lines.Count == 0)
            {
                ModelState.AddModelError("", "أضف صنف واحد على الأقل بكمية صحيحة.");
                _FillDropDowns();
                return View(sale);
            }

            // The cashier/pharmacist making the sale, from the logged-in user's claims - never trust a posted UserID
            int userID = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            sale.UserID = userID;

            if (!sale.Save())
            {
                // clsSale.Save() fills LastError with a specific reason
                // (missing/invalid prescription, insufficient stock, etc.)
                ModelState.AddModelError("", sale.LastError ?? "حدث خطأ أثناء حفظ الفاتورة.");
                _FillDropDowns();
                return View(sale);
            }

            TempData["SuccessMessage"] = "تمت عملية البيع بنجاح، وتم تحديث المخزون.";
            return RedirectToAction(nameof(Details), new { id = sale.SaleID });
        }

        public IActionResult Details(int id)
        {
            DataTable header = clsSale.GetSaleHeader(id);
            if (header.Rows.Count == 0)
                return NotFound();

            ViewBag.Header = header.Rows[0];
            ViewBag.SaleID = id;

            DataTable details = clsSale.GetSaleDetails(id);
            return View(details);
        }

        // ===== JSON endpoints used by the Sales screen =====

        // Medicines that can actually be sold from ONE branch right now (in stock, not expired), with price and quantity
        [HttpGet]
        public IActionResult BranchStock(int branchId)
        {
            DataTable stock = clsInventory.GetBranchStock(branchId);

            var items = stock.Rows.Cast<DataRow>().Select(r => new
            {
                medicineId = (int)r["MedicineID"],
                name = r["Name"].ToString(),
                unit = r["Unit"] == DBNull.Value ? "" : r["Unit"].ToString(),
                requiresPrescription = (bool)r["RequiresPrescription"],
                quantity = (int)r["Quantity"],
                price = (decimal)r["SellingPrice"]
            }).ToList();

            return Json(items);
        }

        // All prescriptions (with their customer) - lets the screen refresh the list after a new one is added
        [HttpGet]
        public IActionResult PrescriptionsList()
        {
            DataTable prescriptions = clsPrescription.GetPrescriptionsForComboBox();

            var items = prescriptions.Rows.Cast<DataRow>().Select(r => new
            {
                id = (int)r["PrescriptionID"],
                customerId = (int)r["CustomerID"],
                text = r["DisplayText"].ToString()
            }).ToList();

            return Json(items);
        }

        private void _FillDropDowns()
        {
            ViewBag.Branches = clsSelectListHelper.FromDataTable(clsBranch.GetBranchesForComboBox(), "BranchID", "BranchName");
            ViewBag.Customers = clsSelectListHelper.FromDataTable(clsCustomer.GetCustomersForComboBox(), "CustomerID", "Name");
            ViewBag.Prescriptions = clsSelectListHelper.FromDataTable(clsPrescription.GetPrescriptionsForComboBox(), "PrescriptionID", "DisplayText");
        }
    }
}
