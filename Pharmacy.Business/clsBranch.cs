using System.Data;
using Pharmacy.DataAccess;

namespace Pharmacy.Business
{
    public class clsBranch
    {
        public enMode Mode = enMode.AddNew;

        public int BranchID { get; set; }
        public string BranchName { get; set; }
        public string Address { get; set; }
        public string Phone { get; set; }

        public clsBranch()
        {
            this.BranchID = -1;
            this.BranchName = "";
            this.Address = "";
            this.Phone = "";

            Mode = enMode.AddNew;
        }

        private clsBranch(int branchID, string branchName, string address, string phone)
        {
            this.BranchID = branchID;
            this.BranchName = branchName;
            this.Address = address;
            this.Phone = phone;

            Mode = enMode.Update;
        }

        public static clsBranch Find(int branchID)
        {
            string branchName = "", address = "", phone = "";
            bool isFound = clsBranchData.GetBranchInfoByID(branchID, ref branchName, ref address, ref phone);

            if (isFound)
                return new clsBranch(branchID, branchName, address, phone);

            return null;
        }

        private bool _AddNewBranch()
        {
            this.BranchID = clsBranchData.AddNewBranch(this.BranchName, this.Address, this.Phone);
            return (this.BranchID != -1);
        }

        private bool _UpdateBranch()
        {
            return clsBranchData.UpdateBranch(this.BranchID, this.BranchName, this.Address, this.Phone);
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewBranch())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateBranch();
            }

            return false;
        }

        public bool Delete()
        {
            return clsBranchData.DeleteBranch(this.BranchID);
        }

        public static DataTable GetAllBranches()
        {
            return clsBranchData.GetAllBranches();
        }

        public static DataTable GetBranchesForComboBox()
        {
            return clsBranchData.GetBranchesForComboBox();
        }
    }
}
