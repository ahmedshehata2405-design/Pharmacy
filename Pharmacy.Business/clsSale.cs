using System;
using System.Collections.Generic;
using System.Data;
using Pharmacy.DataAccess;

namespace Pharmacy.Business
{
    public class clsSale
    {
        public int SaleID { get; set; }
        public int BranchID { get; set; }
        public int? CustomerID { get; set; }
        public int? PrescriptionID { get; set; }
        public int UserID { get; set; }
        public List<clsSaleLine> Lines { get; set; }

        /// <summary>
        /// Set by Save() when it returns false, so the controller can show the person why.
        /// </summary>
        public string LastError { get; private set; }

        public clsSale()
        {
            this.SaleID = -1;
            this.BranchID = -1;
            this.CustomerID = null;
            this.PrescriptionID = null;
            this.UserID = -1;
            this.Lines = new List<clsSaleLine>();
        }

        /// <summary>
        /// Saves the sale through the dbo.usp_CreateSale stored procedure, which checks the prescription rule,
        /// stock, expiry date and price, saves the invoice and deducts the stock in ONE transaction.
        /// </summary>
        public bool Save()
        {
            LastError = null;

            if (Lines == null || Lines.Count == 0)
            {
                LastError = "لا يوجد أصناف في الفاتورة.";
                return false;
            }

            DataTable lines = new DataTable();
            lines.Columns.Add("MedicineID", typeof(int));
            lines.Columns.Add("Quantity", typeof(int));

            foreach (clsSaleLine line in Lines)
                lines.Rows.Add(line.MedicineID, line.Quantity);

            try
            {
                int saleID = -1;
                string errorMessage = null;

                bool saved = clsSaleData.CreateSale(BranchID, CustomerID, PrescriptionID, UserID,
                    lines, ref saleID, ref errorMessage);

                if (!saved)
                {
                    LastError = errorMessage ?? "حدث خطأ أثناء حفظ الفاتورة.";
                    return false;
                }

                this.SaleID = saleID;
                return true;
            }
            catch (Exception ex)
            {
                // Unexpected database error: the procedure already rolled everything back.
                Console.WriteLine("Error in clsSale.Save: " + ex.Message);
                LastError = "حدث خطأ أثناء حفظ الفاتورة، تم التراجع عن كل العملية.";
                return false;
            }
        }

        public static DataTable GetAllSales()
        {
            return clsSaleData.GetAllSales();
        }

        public static DataTable GetSaleDetails(int saleID)
        {
            return clsSaleData.GetSaleDetails(saleID);
        }
    
        public static DataTable GetSaleHeader(int saleID)
        {
            return clsSaleData.GetSaleHeader(saleID);
        }

    }
}
