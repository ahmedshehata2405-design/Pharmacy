using System;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DataAccess
{
    public class clsUserData
    {
        public static bool GetUserInfoByID(int userID, ref string username, ref string passwordHash,
            ref string email, ref int roleID, ref int branchID, ref bool isActive)
        {
            bool isFound = false;
            string query = @"SELECT * FROM Users WHERE UserID = @UserID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserID", userID);

                try
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        isFound = true;
                        username = reader["Username"].ToString();
                        passwordHash = reader["PasswordHash"].ToString();
                        email = reader["Email"] == DBNull.Value ? "" : reader["Email"].ToString();
                        roleID = (int)reader["RoleID"];
                        branchID = (int)reader["BranchID"];
                        isActive = (bool)reader["IsActive"];
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetUserInfoByID: " + ex.Message);
                }
            }

            return isFound;
        }

        public static bool GetUserInfoByUsername(string username, ref int userID, ref string passwordHash,
            ref string email, ref int roleID, ref int branchID, ref bool isActive)
        {
            bool isFound = false;
            string query = @"SELECT * FROM Users WHERE Username = @Username";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                try
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        isFound = true;
                        userID = (int)reader["UserID"];
                        passwordHash = reader["PasswordHash"].ToString();
                        email = reader["Email"] == DBNull.Value ? "" : reader["Email"].ToString();
                        roleID = (int)reader["RoleID"];
                        branchID = (int)reader["BranchID"];
                        isActive = (bool)reader["IsActive"];
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetUserInfoByUsername: " + ex.Message);
                }
            }

            return isFound;
        }

        public static int AddNewUser(string username, string passwordHash, string email, int roleID, int branchID)
        {
            int newUserID = -1;
            string query = @"INSERT INTO Users (Username, PasswordHash, Email, RoleID, BranchID, IsActive, CreatedDate)
                              VALUES (@Username, @PasswordHash, @Email, @RoleID, @BranchID, 1, GETDATE());
                              SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                command.Parameters.AddWithValue("@RoleID", roleID);
                command.Parameters.AddWithValue("@BranchID", branchID);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                        newUserID = Convert.ToInt32(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in AddNewUser: " + ex.Message);
                }
            }

            return newUserID;
        }

        public static bool UpdateUser(int userID, string username, string passwordHash, string email,
            int roleID, int branchID, bool isActive)
        {
            int rowsAffected = 0;
            string query = @"UPDATE Users
                              SET Username = @Username, PasswordHash = @PasswordHash, Email = @Email,
                                  RoleID = @RoleID, BranchID = @BranchID, IsActive = @IsActive
                              WHERE UserID = @UserID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserID", userID);
                command.Parameters.AddWithValue("@Username", username);
                command.Parameters.AddWithValue("@PasswordHash", passwordHash);
                command.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                command.Parameters.AddWithValue("@RoleID", roleID);
                command.Parameters.AddWithValue("@BranchID", branchID);
                command.Parameters.AddWithValue("@IsActive", isActive);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in UpdateUser: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static bool DeleteUser(int userID)
        {
            int rowsAffected = 0;
            string query = @"DELETE FROM Users WHERE UserID = @UserID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@UserID", userID);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in DeleteUser: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static bool IsUsernameExist(string username)
        {
            bool isFound = false;
            string query = @"SELECT UserID FROM Users WHERE Username = @Username";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Username", username);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    isFound = (result != null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in IsUsernameExist: " + ex.Message);
                }
            }

            return isFound;
        }
    }
}
