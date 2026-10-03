using System.Data;
using Pharmacy.DataAccess;

namespace Pharmacy.Business
{
    public class clsCustomer
    {
        public enMode Mode = enMode.AddNew;

        public int CustomerID { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }

        public clsCustomer()
        {
            this.CustomerID = -1;
            this.Name = "";
            this.Phone = "";

            Mode = enMode.AddNew;
        }

        private clsCustomer(int customerID, string name, string phone)
        {
            this.CustomerID = customerID;
            this.Name = name;
            this.Phone = phone;

            Mode = enMode.Update;
        }

        public static clsCustomer Find(int customerID)
        {
            string name = "", phone = "";
            bool isFound = clsCustomerData.GetCustomerInfoByID(customerID, ref name, ref phone);

            if (isFound)
                return new clsCustomer(customerID, name, phone);

            return null;
        }

        private bool _AddNewCustomer()
        {
            this.CustomerID = clsCustomerData.AddNewCustomer(this.Name, this.Phone);
            return (this.CustomerID != -1);
        }

        private bool _UpdateCustomer()
        {
            return clsCustomerData.UpdateCustomer(this.CustomerID, this.Name, this.Phone);
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewCustomer())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateCustomer();
            }

            return false;
        }

        public bool Delete()
        {
            return clsCustomerData.DeleteCustomer(this.CustomerID);
        }

        public static DataTable GetAllCustomers()
        {
            return clsCustomerData.GetAllCustomers();
        }

        public static DataTable GetCustomersForComboBox()
        {
            return clsCustomerData.GetCustomersForComboBox();
        }
    }
}
