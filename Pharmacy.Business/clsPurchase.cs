using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Pharmacy.DataAccess;

namespace Pharmacy.Business
{
    public class clsPurchase
    {
        public int PurchaseID { get; set; }
        public int SupplierID { get; set; }
        public int BranchID { get; set; }
        public List<clsPurchaseLine> Lines { get; set; }

        public decimal TotalAmount => Lines?.Sum(l => l.LineTotal) ?? 0;

        public clsPurchase()
        {
            this.PurchaseID = -1;
            this.SupplierID = -1;
            this.BranchID = -1;
            this.Lines = new List<clsPurchaseLine>();
        }

        public string LastError { get; private set; }

        /// <summary>
        /// Saves the purchase through the dbo.usp_CreatePurchase stored procedure: the invoice, its lines and the
        /// stock update (adding to existing stock or creating the branch's stock row) run in ONE transaction.
        /// </summary>
        public bool Save()
        {
            LastError = null;

            if (Lines == null || Lines.Count == 0)
            {
                LastError = "لا يوجد أصناف في فاتورة الشراء.";
                return false;
            }

            DataTable lines = new DataTable();
            lines.Columns.Add("MedicineID", typeof(int));
            lines.Columns.Add("Quantity", typeof(int));
            lines.Columns.Add("UnitPrice", typeof(decimal));

            foreach (clsPurchaseLine line in Lines)
                lines.Rows.Add(line.MedicineID, line.Quantity, line.UnitPrice);

            try
            {
                int purchaseID = -1;
                string errorMessage = null;

                bool saved = clsPurchaseData.CreatePurchase(SupplierID, BranchID, lines, ref purchaseID, ref errorMessage);

                if (!saved)
                {
                    LastError = errorMessage ?? "حدث خطأ أثناء حفظ فاتورة الشراء.";
                    return false;
                }

                this.PurchaseID = purchaseID;
                return true;
            }
            catch (Exception ex)
            {
                // Unexpected database error: the procedure already rolled everything back.
                Console.WriteLine("Error in clsPurchase.Save: " + ex.Message);
                LastError = "حدث خطأ أثناء حفظ فاتورة الشراء، تم التراجع عن كل العملية.";
                return false;
            }
        }

        public static DataTable GetAllPurchases()
        {
            return clsPurchaseData.GetAllPurchases();
        }

        public static DataTable GetPurchaseDetails(int purchaseID)
        {
            return clsPurchaseData.GetPurchaseDetails(purchaseID);
        }
    }
}
