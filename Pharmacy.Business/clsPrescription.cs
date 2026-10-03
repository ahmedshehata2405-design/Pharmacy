using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Pharmacy.DataAccess;

namespace Pharmacy.Business
{
    public class clsPrescription
    {
        public int PrescriptionID { get; set; }
        public int CustomerID { get; set; }
        public int DoctorID { get; set; }
        public List<clsPrescriptionLine> Lines { get; set; }

        public clsPrescription()
        {
            this.PrescriptionID = -1;
            this.CustomerID = -1;
            this.DoctorID = -1;
            this.Lines = new List<clsPrescriptionLine>();
        }

        public bool Save()
        {
            if (Lines == null || Lines.Count == 0)
                return false;

            this.PrescriptionID = clsPrescriptionData.AddNewPrescription(this.CustomerID, this.DoctorID);

            if (this.PrescriptionID == -1)
                return false;

            foreach (clsPrescriptionLine line in Lines)
                clsPrescriptionData.AddPrescriptionDetail(this.PrescriptionID, line.MedicineID, line.Dosage, line.Quantity);

            return true;
        }

        public static DataTable GetAllPrescriptions()
        {
            return clsPrescriptionData.GetAllPrescriptions();
        }

        public static DataTable GetPrescriptionsForComboBox()
        {
            return clsPrescriptionData.GetPrescriptionsForComboBox();
        }

        public static DataTable GetPrescriptionDetails(int prescriptionID)
        {
            return clsPrescriptionData.GetPrescriptionDetails(prescriptionID);
        }

        public static List<int> GetMedicineIDsForPrescription(int prescriptionID)
        {
            return clsPrescriptionData.GetMedicineIDsForPrescription(prescriptionID);
        }

    }
}
