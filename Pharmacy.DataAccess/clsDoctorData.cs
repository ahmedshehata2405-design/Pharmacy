using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DataAccess
{
    public class clsDoctorData
    {
        public static bool GetDoctorInfoByID(int doctorID, ref string name, ref string specialization, ref string phone)
        {
            bool isFound = false;
            string query = @"SELECT * FROM Doctors WHERE DoctorID = @DoctorID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DoctorID", doctorID);

                try
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        isFound = true;
                        name = reader["Name"].ToString();
                        specialization = reader["Specialization"] == DBNull.Value ? "" : reader["Specialization"].ToString();
                        phone = reader["Phone"] == DBNull.Value ? "" : reader["Phone"].ToString();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetDoctorInfoByID: " + ex.Message);
                }
            }

            return isFound;
        }

        public static int AddNewDoctor(string name, string specialization, string phone)
        {
            int newID = -1;
            string query = @"INSERT INTO Doctors (Name, Specialization, Phone)
                              VALUES (@Name, @Specialization, @Phone);
                              SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Specialization", string.IsNullOrEmpty(specialization) ? (object)DBNull.Value : specialization);
                command.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                        newID = Convert.ToInt32(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in AddNewDoctor: " + ex.Message);
                }
            }

            return newID;
        }

        public static bool UpdateDoctor(int doctorID, string name, string specialization, string phone)
        {
            int rowsAffected = 0;
            string query = @"UPDATE Doctors
                              SET Name = @Name, Specialization = @Specialization, Phone = @Phone
                              WHERE DoctorID = @DoctorID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DoctorID", doctorID);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Specialization", string.IsNullOrEmpty(specialization) ? (object)DBNull.Value : specialization);
                command.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in UpdateDoctor: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static bool DeleteDoctor(int doctorID)
        {
            int rowsAffected = 0;
            string query = @"DELETE FROM Doctors WHERE DoctorID = @DoctorID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DoctorID", doctorID);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in DeleteDoctor: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static DataTable GetAllDoctors()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT DoctorID, Name, Specialization, Phone FROM Doctors";

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
                    Console.WriteLine("Error in GetAllDoctors: " + ex.Message);
                }
            }

            return dt;
        }

        public static DataTable GetDoctorsForComboBox()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT DoctorID, Name FROM Doctors ORDER BY Name";

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
                    Console.WriteLine("Error in GetDoctorsForComboBox: " + ex.Message);
                }
            }

            return dt;
        }
    }
}
