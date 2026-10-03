using System.Data;
using Pharmacy.DataAccess;

namespace Pharmacy.Business
{
    public class clsSupplier
    {
        public enMode Mode = enMode.AddNew;

        public int SupplierID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }

        public clsSupplier()
        {
            this.SupplierID = -1;
            this.Name = "";
            this.Phone = "";
            this.Email = "";
            this.Address = "";

            Mode = enMode.AddNew;
        }

        private clsSupplier(int supplierID, string name, string phone, string email, string address)
        {
            this.SupplierID = supplierID;
            this.Name = name;
            this.Phone = phone;
            this.Email = email;
            this.Address = address;

            Mode = enMode.Update;
        }

        public static clsSupplier Find(int supplierID)
        {
            string name = "", phone = "", email = "", address = "";

            bool isFound = clsSupplierData.GetSupplierInfoByID(supplierID, ref name, ref phone, ref email, ref address);

            if (isFound)
                return new clsSupplier(supplierID, name, phone, email, address);

            return null;
        }

        private bool _AddNewSupplier()
        {
            this.SupplierID = clsSupplierData.AddNewSupplier(this.Name, this.Phone, this.Email, this.Address);
            return (this.SupplierID != -1);
        }

        private bool _UpdateSupplier()
        {
            return clsSupplierData.UpdateSupplier(this.SupplierID, this.Name, this.Phone, this.Email, this.Address);
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewSupplier())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateSupplier();
            }

            return false;
        }

        public bool Delete()
        {
            return clsSupplierData.DeleteSupplier(this.SupplierID);
        }

        public static DataTable GetAllSuppliers()
        {
            return clsSupplierData.GetAllSuppliers();
        }
    }
}
