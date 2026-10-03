using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DataAccess
{
    public class clsSupplierData
    {
        public static bool GetSupplierInfoByID(int supplierID, ref string name, ref string phone,
            ref string email, ref string address)
        {
            bool isFound = false;
            string query = @"SELECT * FROM Suppliers WHERE SupplierID = @SupplierID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SupplierID", supplierID);

                try
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        isFound = true;
                        name = reader["Name"].ToString();
                        phone = reader["Phone"] == DBNull.Value ? "" : reader["Phone"].ToString();
                        email = reader["Email"] == DBNull.Value ? "" : reader["Email"].ToString();
                        address = reader["Address"] == DBNull.Value ? "" : reader["Address"].ToString();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetSupplierInfoByID: " + ex.Message);
                }
            }

            return isFound;
        }

        public static int AddNewSupplier(string name, string phone, string email, string address)
        {
            int newID = -1;
            string query = @"INSERT INTO Suppliers (Name, Phone, Email, Address)
                              VALUES (@Name, @Phone, @Email, @Address);
                              SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                command.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                        newID = Convert.ToInt32(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in AddNewSupplier: " + ex.Message);
                }
            }

            return newID;
        }

        public static bool UpdateSupplier(int supplierID, string name, string phone, string email, string address)
        {
            int rowsAffected = 0;
            string query = @"UPDATE Suppliers
                              SET Name = @Name, Phone = @Phone, Email = @Email, Address = @Address
                              WHERE SupplierID = @SupplierID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SupplierID", supplierID);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);
                command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                command.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in UpdateSupplier: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static bool DeleteSupplier(int supplierID)
        {
            int rowsAffected = 0;
            string query = @"DELETE FROM Suppliers WHERE SupplierID = @SupplierID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SupplierID", supplierID);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in DeleteSupplier: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static DataTable GetAllSuppliers()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT SupplierID, Name, Phone, Email, Address FROM Suppliers";

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
                    Console.WriteLine("Error in GetAllSuppliers: " + ex.Message);
                }
            }

            return dt;
        }
    }
}
