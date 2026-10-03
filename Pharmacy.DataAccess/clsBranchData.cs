using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DataAccess
{
    public class clsBranchData
    {
        public static bool GetBranchInfoByID(int branchID, ref string branchName, ref string address, ref string phone)
        {
            bool isFound = false;
            string query = @"SELECT * FROM Branches WHERE BranchID = @BranchID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BranchID", branchID);

                try
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        isFound = true;
                        branchName = reader["BranchName"].ToString();
                        address = reader["Address"] == DBNull.Value ? "" : reader["Address"].ToString();
                        phone = reader["Phone"] == DBNull.Value ? "" : reader["Phone"].ToString();
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetBranchInfoByID: " + ex.Message);
                }
            }

            return isFound;
        }

        public static int AddNewBranch(string branchName, string address, string phone)
        {
            int newID = -1;
            string query = @"INSERT INTO Branches (BranchName, Address, Phone)
                              VALUES (@BranchName, @Address, @Phone);
                              SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BranchName", branchName);
                command.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address);
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
                    Console.WriteLine("Error in AddNewBranch: " + ex.Message);
                }
            }

            return newID;
        }

        public static bool UpdateBranch(int branchID, string branchName, string address, string phone)
        {
            int rowsAffected = 0;
            string query = @"UPDATE Branches SET BranchName = @BranchName, Address = @Address, Phone = @Phone
                              WHERE BranchID = @BranchID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BranchID", branchID);
                command.Parameters.AddWithValue("@BranchName", branchName);
                command.Parameters.AddWithValue("@Address", string.IsNullOrEmpty(address) ? (object)DBNull.Value : address);
                command.Parameters.AddWithValue("@Phone", string.IsNullOrEmpty(phone) ? (object)DBNull.Value : phone);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in UpdateBranch: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static bool DeleteBranch(int branchID)
        {
            int rowsAffected = 0;
            string query = @"DELETE FROM Branches WHERE BranchID = @BranchID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BranchID", branchID);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in DeleteBranch: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static DataTable GetAllBranches()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT BranchID, BranchName, Address, Phone FROM Branches";

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
                    Console.WriteLine("Error in GetAllBranches: " + ex.Message);
                }
            }

            return dt;
        }

        public static DataTable GetBranchesForComboBox()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT BranchID, BranchName FROM Branches ORDER BY BranchName";

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
                    Console.WriteLine("Error in GetBranchesForComboBox: " + ex.Message);
                }
            }

            return dt;
        }
    }
}
