using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Pharmacy.DataAccess;

namespace Pharmacy.Business
{
    public class clsInventory
    {
        public enMode Mode = enMode.AddNew;

        public int InventoryID { get; set; }
        public int BranchID { get; set; }
        public int MedicineID { get; set; }
        public int Quantity { get; set; }
        public decimal SellingPrice { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public clsInventory()
        {
            this.InventoryID = -1;
            this.BranchID = -1;
            this.MedicineID = -1;
            this.Quantity = 0;
            this.SellingPrice = 0;
            this.ExpiryDate = null;

            Mode = enMode.AddNew;
        }

        private clsInventory(int inventoryID, int branchID, int medicineID, int quantity,
            decimal sellingPrice, DateTime? expiryDate)
        {
            this.InventoryID = inventoryID;
            this.BranchID = branchID;
            this.MedicineID = medicineID;
            this.Quantity = quantity;
            this.SellingPrice = sellingPrice;
            this.ExpiryDate = expiryDate;

            Mode = enMode.Update;
        }

        public static clsInventory Find(int inventoryID)
        {
            int branchID = 0, medicineID = 0, quantity = 0;
            decimal sellingPrice = 0;
            DateTime? expiryDate = null;

            bool isFound = clsInventoryData.GetInventoryInfoByID(inventoryID, ref branchID, ref medicineID,
                ref quantity, ref sellingPrice, ref expiryDate);

            if (isFound)
                return new clsInventory(inventoryID, branchID, medicineID, quantity, sellingPrice, expiryDate);

            return null;
        }

        private bool _AddNewInventory()
        {
            this.InventoryID = clsInventoryData.AddNewInventory(this.BranchID, this.MedicineID,
                this.Quantity, this.SellingPrice, this.ExpiryDate);

            return (this.InventoryID != -1);
        }

        private bool _UpdateInventory()
        {
            return clsInventoryData.UpdateInventory(this.InventoryID, this.BranchID, this.MedicineID,
                this.Quantity, this.SellingPrice, this.ExpiryDate);
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewInventory())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateInventory();
            }

            return false;
        }

        public bool Delete()
        {
            return clsInventoryData.DeleteInventory(this.InventoryID);
        }

        /// <summary>
        /// Adds stock (positive) or removes stock (negative). Returns false if it would
        /// take quantity below zero (the DataAccess query blocks that at the DB level too).
        /// </summary>
        public static bool AdjustQuantity(int inventoryID, int quantityChange)
        {
            return clsInventoryData.AdjustQuantity(inventoryID, quantityChange);
        }

        public static bool GetStockInfo(int branchID, int medicineID, ref int inventoryID, ref int quantity, ref decimal sellingPrice)
        {
            return clsInventoryData.GetStockInfo(branchID, medicineID, ref inventoryID, ref quantity, ref sellingPrice);
        }

        public static DataTable GetAllInventoryWithDetails()
        {
            return clsInventoryData.GetAllInventoryWithDetails();
        }

        public static DataTable GetExpiringSoon(int daysAhead = 30)
        {
            return clsInventoryData.GetExpiringSoon(daysAhead);
        }
    
        public static DataTable GetBranchStock(int branchID)
        {
            return clsInventoryData.GetBranchStock(branchID);
        }

    }
}
