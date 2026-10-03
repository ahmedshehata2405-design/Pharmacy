using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DataAccess
{
    public class clsMedicineData
    {
        public static bool GetMedicineInfoByID(int medicineID, ref string name, ref int categoryID,
            ref string description, ref string unit, ref bool requiresPrescription)
        {
            bool isFound = false;
            string query = @"SELECT * FROM Medicines WHERE MedicineID = @MedicineID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MedicineID", medicineID);

                try
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        isFound = true;
                        name = reader["Name"].ToString();
                        categoryID = (int)reader["CategoryID"];
                        description = reader["Description"] == DBNull.Value ? "" : reader["Description"].ToString();
                        unit = reader["Unit"] == DBNull.Value ? "" : reader["Unit"].ToString();
                        requiresPrescription = (bool)reader["RequiresPrescription"];
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetMedicineInfoByID: " + ex.Message);
                }
            }

            return isFound;
        }

        public static int AddNewMedicine(string name, int categoryID, string description,
            string unit, bool requiresPrescription)
        {
            int newMedicineID = -1;
            string query = @"INSERT INTO Medicines (Name, CategoryID, Description, Unit, RequiresPrescription)
                              VALUES (@Name, @CategoryID, @Description, @Unit, @RequiresPrescription);
                              SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@CategoryID", categoryID);
                command.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                command.Parameters.AddWithValue("@Unit", string.IsNullOrEmpty(unit) ? (object)DBNull.Value : unit);
                command.Parameters.AddWithValue("@RequiresPrescription", requiresPrescription);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                        newMedicineID = Convert.ToInt32(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in AddNewMedicine: " + ex.Message);
                }
            }

            return newMedicineID;
        }

        public static bool UpdateMedicine(int medicineID, string name, int categoryID, string description,
            string unit, bool requiresPrescription)
        {
            int rowsAffected = 0;
            string query = @"UPDATE Medicines
                              SET Name = @Name, CategoryID = @CategoryID, Description = @Description,
                                  Unit = @Unit, RequiresPrescription = @RequiresPrescription
                              WHERE MedicineID = @MedicineID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MedicineID", medicineID);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@CategoryID", categoryID);
                command.Parameters.AddWithValue("@Description", string.IsNullOrEmpty(description) ? (object)DBNull.Value : description);
                command.Parameters.AddWithValue("@Unit", string.IsNullOrEmpty(unit) ? (object)DBNull.Value : unit);
                command.Parameters.AddWithValue("@RequiresPrescription", requiresPrescription);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in UpdateMedicine: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static bool DeleteMedicine(int medicineID)
        {
            int rowsAffected = 0;
            string query = @"DELETE FROM Medicines WHERE MedicineID = @MedicineID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MedicineID", medicineID);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in DeleteMedicine: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        // Lightweight check used by Sales to decide whether a PrescriptionID is required for this line
        public static bool IsPrescriptionRequired(int medicineID)
        {
            bool requiresPrescription = false;
            string query = @"SELECT RequiresPrescription FROM Medicines WHERE MedicineID = @MedicineID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MedicineID", medicineID);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                        requiresPrescription = (bool)result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in IsPrescriptionRequired: " + ex.Message);
                }
            }

            return requiresPrescription;
        }

        public static bool IsMedicineExist(int medicineID)
        {
            bool isFound = false;
            string query = @"SELECT MedicineID FROM Medicines WHERE MedicineID = @MedicineID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@MedicineID", medicineID);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    isFound = (result != null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in IsMedicineExist: " + ex.Message);
                }
            }

            return isFound;
        }

        public static DataTable GetAllMedicines()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT MedicineID, Name, CategoryID, Description, Unit, RequiresPrescription
                              FROM Medicines";

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
                    Console.WriteLine("Error in GetAllMedicines: " + ex.Message);
                }
            }

            return dt;
        }

        // Used to fill a search/lookup combo box (e.g. for Sales / Prescriptions screens)
        public static DataTable GetMedicinesForComboBox()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT MedicineID, Name FROM Medicines ORDER BY Name";

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
                    Console.WriteLine("Error in GetMedicinesForComboBox: " + ex.Message);
                }
            }

            return dt;
        }
    }
}
