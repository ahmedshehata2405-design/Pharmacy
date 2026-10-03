using System.Data;
using Microsoft.Data.SqlClient;
using Pharmacy.DataAccess;

namespace Pharmacy.Business
{
    public class clsMedicine
    {
        public enMode Mode = enMode.AddNew;

        public int MedicineID { get; set; }
        public string Name { get; set; }
        public int CategoryID { get; set; }
        public string Description { get; set; }
        public string Unit { get; set; }
        public bool RequiresPrescription { get; set; }

        public clsMedicine()
        {
            this.MedicineID = -1;
            this.Name = "";
            this.CategoryID = -1;
            this.Description = "";
            this.Unit = "";
            this.RequiresPrescription = false;

            Mode = enMode.AddNew;
        }

        private clsMedicine(int medicineID, string name, int categoryID, string description,
            string unit, bool requiresPrescription)
        {
            this.MedicineID = medicineID;
            this.Name = name;
            this.CategoryID = categoryID;
            this.Description = description;
            this.Unit = unit;
            this.RequiresPrescription = requiresPrescription;

            Mode = enMode.Update;
        }

        public static clsMedicine Find(int medicineID)
        {
            string name = "", description = "", unit = "";
            int categoryID = 0;
            bool requiresPrescription = false;

            bool isFound = clsMedicineData.GetMedicineInfoByID(medicineID, ref name, ref categoryID,
                ref description, ref unit, ref requiresPrescription);

            if (isFound)
                return new clsMedicine(medicineID, name, categoryID, description, unit, requiresPrescription);

            return null;
        }

        private bool _AddNewMedicine()
        {
            this.MedicineID = clsMedicineData.AddNewMedicine(this.Name, this.CategoryID,
                this.Description, this.Unit, this.RequiresPrescription);

            return (this.MedicineID != -1);
        }

        private bool _UpdateMedicine()
        {
            return clsMedicineData.UpdateMedicine(this.MedicineID, this.Name, this.CategoryID,
                this.Description, this.Unit, this.RequiresPrescription);
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewMedicine())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateMedicine();
            }

            return false;
        }

        public bool Delete()
        {
            return clsMedicineData.DeleteMedicine(this.MedicineID);
        }

        public static bool IsMedicineExist(int medicineID)
        {
            return clsMedicineData.IsMedicineExist(medicineID);
        }

        public static bool IsPrescriptionRequired(int medicineID)
        {
            return clsMedicineData.IsPrescriptionRequired(medicineID);
        }

        public static DataTable GetAllMedicines()
        {
            return clsMedicineData.GetAllMedicines();
        }

        public static DataTable GetMedicinesForComboBox()
        {
            return clsMedicineData.GetMedicinesForComboBox();
        }
    }
}
