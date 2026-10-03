using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DataAccess
{
    public class clsCustomerData
    {
        public static bool GetCustomerInfoByID(int customerID, ref string name, ref string phone)
        {
            bool isFound = false;
            string query = @"SELECT * FROM Customers WHERE CustomerID = @CustomerID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerID", customerID);

                try
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        isFound = true;
                        name = reader["Name"].ToString();
                        phone = reader["Phone"] == DBNull.Value ? "" : reader["Phone"].ToString();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetCustomerInfoByID: " + ex.Message);
                }
            }

            return isFound;
        }

        public static int AddNewCustomer(string name, string phone)
        {
            int newID = -1;
            string query = @"INSERT INTO Customers (Name, Phone) VALUES (@Name, @Phone);
                              SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Name", name);
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
                    Console.WriteLine("Error in AddNewCustomer: " + ex.Message);
                }
            }

            return newID;
        }

        public static bool UpdateCustomer(int customerID, string name, string phone)
        {
            int rowsAffected = 0;
            string query = @"UPDATE Customers SET Name = @Name, Phone = @Phone WHERE CustomerID = @CustomerID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerID", customerID);
                command.Parameters.AddWithValue("@Name", name);
                command.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in UpdateCustomer: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static bool DeleteCustomer(int customerID)
        {
            int rowsAffected = 0;
            string query = @"DELETE FROM Customers WHERE CustomerID = @CustomerID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CustomerID", customerID);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in DeleteCustomer: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static DataTable GetAllCustomers()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT CustomerID, Name, Phone FROM Customers";

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
                    Console.WriteLine("Error in GetAllCustomers: " + ex.Message);
                }
            }

            return dt;
        }

        public static DataTable GetCustomersForComboBox()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT CustomerID, Name FROM Customers ORDER BY Name";

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
                    Console.WriteLine("Error in GetCustomersForComboBox: " + ex.Message);
                }
            }

            return dt;
        }
    }
}
