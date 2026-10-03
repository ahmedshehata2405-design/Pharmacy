using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Linq;

namespace Pharmacy.DataAccess
{
    public class clsPrescriptionData
    {
        public static int AddNewPrescription(int customerID, int doctorID)
        {
            int newID = -1;
            string query = @"INSERT INTO Prescriptions (CustomerID, DoctorID, IssueDate)
                              VALUES (@CustomerID, @DoctorID, GETDATE());
                              SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerID", customerID);
                command.Parameters.AddWithValue("@DoctorID", doctorID);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                        newID = Convert.ToInt32(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in AddNewPrescription: " + ex.Message);
                }
            }

            return newID;
        }

        public static bool AddPrescriptionDetail(int prescriptionID, int medicineID, string dosage, int quantity)
        {
            int rowsAffected = 0;
            string query = @"INSERT INTO PrescriptionDetails (PrescriptionID, MedicineID, Dosage, Quantity)
                              VALUES (@PrescriptionID, @MedicineID, @Dosage, @Quantity)";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PrescriptionID", prescriptionID);
                command.Parameters.AddWithValue("@MedicineID", medicineID);
                command.Parameters.AddWithValue("@Dosage", string.IsNullOrEmpty(dosage) ? (object)DBNull.Value : dosage);
                command.Parameters.AddWithValue("@Quantity", quantity);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in AddPrescriptionDetail: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static DataTable GetAllPrescriptions()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT P.PrescriptionID, C.Name AS CustomerName, D.Name AS DoctorName, P.IssueDate
                              FROM Prescriptions P
                              JOIN Customers C ON P.CustomerID = C.CustomerID
                              JOIN Doctors D ON P.DoctorID = D.DoctorID
                              ORDER BY P.IssueDate DESC";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    using SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetAllPrescriptions: " + ex.Message);
                }
            }

            return dt;
        }

        // Used to build the "prescription #12 - Ahmed - 2026-09-20" combo box on the Sales screen
        public static DataTable GetPrescriptionsForComboBox()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT P.PrescriptionID, P.CustomerID,
                                     (CAST(P.PrescriptionID AS NVARCHAR) + N' - ' + C.Name + N' - '
                                      + CONVERT(NVARCHAR, P.IssueDate, 23)) AS DisplayText
                              FROM Prescriptions P
                              JOIN Customers C ON P.CustomerID = C.CustomerID
                              ORDER BY P.IssueDate DESC";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    using SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetPrescriptionsForComboBox: " + ex.Message);
                }
            }

            return dt;
        }

        public static DataTable GetPrescriptionDetails(int prescriptionID)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT PD.PrescriptionDetailID, PD.MedicineID, M.Name AS MedicineName,
                                     PD.Dosage, PD.Quantity
                              FROM PrescriptionDetails PD
                              JOIN Medicines M ON PD.MedicineID = M.MedicineID
                              WHERE PD.PrescriptionID = @PrescriptionID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PrescriptionID", prescriptionID);

                try
                {
                    connection.Open();
                    using SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetPrescriptionDetails: " + ex.Message);
                }
            }

            return dt;
        }

        // Returns the set of MedicineIDs this prescription covers, so Sales can validate
        // that a "requires prescription" medicine being sold is actually listed on it.
        public static List<int> GetMedicineIDsForPrescription(int prescriptionID)
        {
            List<int> ids = new List<int>();
            string query = @"SELECT MedicineID FROM PrescriptionDetails WHERE PrescriptionID = @PrescriptionID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PrescriptionID", prescriptionID);

                try
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                        ids.Add((int)reader["MedicineID"]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetMedicineIDsForPrescription: " + ex.Message);
                }
            }

            return ids;
        }

    }
}
