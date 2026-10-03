using System.Data;
using Pharmacy.DataAccess;

namespace Pharmacy.Business
{
    public class clsDoctor
    {
        public enMode Mode = enMode.AddNew;

        public int DoctorID { get; set; }
        public string Name { get; set; }
        public string Specialization { get; set; }
        public string Phone { get; set; }

        public clsDoctor()
        {
            this.DoctorID = -1;
            this.Name = "";
            this.Specialization = "";
            this.Phone = "";

            Mode = enMode.AddNew;
        }

        private clsDoctor(int doctorID, string name, string specialization, string phone)
        {
            this.DoctorID = doctorID;
            this.Name = name;
            this.Specialization = specialization;
            this.Phone = phone;

            Mode = enMode.Update;
        }

        public static clsDoctor Find(int doctorID)
        {
            string name = "", specialization = "", phone = "";
            bool isFound = clsDoctorData.GetDoctorInfoByID(doctorID, ref name, ref specialization, ref phone);

            if (isFound)
                return new clsDoctor(doctorID, name, specialization, phone);

            return null;
        }

        private bool _AddNewDoctor()
        {
            this.DoctorID = clsDoctorData.AddNewDoctor(this.Name, this.Specialization, this.Phone);
            return (this.DoctorID != -1);
        }

        private bool _UpdateDoctor()
        {
            return clsDoctorData.UpdateDoctor(this.DoctorID, this.Name, this.Specialization, this.Phone);
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewDoctor())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateDoctor();
            }

            return false;
        }

        public bool Delete()
        {
            return clsDoctorData.DeleteDoctor(this.DoctorID);
        }

        public static DataTable GetAllDoctors()
        {
            return clsDoctorData.GetAllDoctors();
        }

        public static DataTable GetDoctorsForComboBox()
        {
            return clsDoctorData.GetDoctorsForComboBox();
        }
    }
}
