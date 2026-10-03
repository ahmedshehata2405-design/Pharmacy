using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DataAccess
{
    public class clsCategoryData
    {
        public static bool GetCategoryInfoByID(int categoryID, ref string categoryName)
        {
            bool isFound = false;
            string query = @"SELECT CategoryName FROM Categories WHERE CategoryID = @CategoryID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CategoryID", categoryID);

                try
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        isFound = true;
                        categoryName = reader["CategoryName"].ToString();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetCategoryInfoByID: " + ex.Message);
                }
            }

            return isFound;
        }

        // Returns -1 when it fails (for example the name already exists - CategoryName is UNIQUE)
        public static int AddNewCategory(string categoryName)
        {
            int newID = -1;
            string query = @"INSERT INTO Categories (CategoryName) VALUES (@CategoryName);
                              SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CategoryName", categoryName);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                        newID = Convert.ToInt32(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in AddNewCategory: " + ex.Message);
                }
            }

            return newID;
        }

        public static bool UpdateCategory(int categoryID, string categoryName)
        {
            int rowsAffected = 0;
            string query = @"UPDATE Categories SET CategoryName = @CategoryName WHERE CategoryID = @CategoryID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CategoryID", categoryID);
                command.Parameters.AddWithValue("@CategoryName", categoryName);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in UpdateCategory: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        // Fails (returns false) when medicines still use the category - the foreign key blocks it
        public static bool DeleteCategory(int categoryID)
        {
            int rowsAffected = 0;
            string query = @"DELETE FROM Categories WHERE CategoryID = @CategoryID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CategoryID", categoryID);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in DeleteCategory: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static DataTable GetAllCategories()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT C.CategoryID, C.CategoryName,
                                    (SELECT COUNT(*) FROM Medicines M WHERE M.CategoryID = C.CategoryID) AS MedicinesCount
                              FROM Categories C
                              ORDER BY C.CategoryName";

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
                    Console.WriteLine("Error in GetAllCategories: " + ex.Message);
                }
            }

            return dt;
        }
    }
}
