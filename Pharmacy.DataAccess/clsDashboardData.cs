using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DataAccess
{
    // Small read-only queries for the dashboard/home page. Kept separate from the
    // per-entity DataAccess classes since these aren't tied to one table.
    public class clsDashboardData
    {
        public static int GetMedicinesCount()
        {
            return _GetScalarInt(@"SELECT COUNT(*) FROM Medicines");
        }

        public static int GetLowStockCount(int threshold)
        {
            return _GetScalarInt($"SELECT COUNT(*) FROM Inventory WHERE Quantity <= {threshold}");
        }

        public static int GetExpiringSoonCount(int withinDays)
        {
            return _GetScalarInt($@"SELECT COUNT(*) FROM Inventory
                                     WHERE ExpiryDate IS NOT NULL AND ExpiryDate <= DATEADD(DAY, {withinDays}, GETDATE())");
        }

        public static decimal GetTodaysSalesTotal()
        {
            string query = @"SELECT ISNULL(SUM(TotalAmount), 0) FROM Sales WHERE CAST(SaleDate AS DATE) = CAST(GETDATE() AS DATE)";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToDecimal(result) : 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetTodaysSalesTotal: " + ex.Message);
                    return 0;
                }
            }
        }

        public static DataTable GetRecentSales(int count)
        {
            DataTable dt = new DataTable();
            string query = $@"SELECT TOP {count} S.SaleID, B.BranchName, ISNULL(C.Name, N'عميل نقدي') AS CustomerName,
                                      S.SaleDate, S.TotalAmount
                               FROM Sales S
                               JOIN Branches B ON S.BranchID = B.BranchID
                               LEFT JOIN Customers C ON S.CustomerID = C.CustomerID
                               ORDER BY S.SaleDate DESC";

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
                    Console.WriteLine("Error in GetRecentSales: " + ex.Message);
                }
            }

            return dt;
        }

        private static int _GetScalarInt(string query)
        {
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    return result != null ? Convert.ToInt32(result) : 0;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error running dashboard query: " + ex.Message);
                    return 0;
                }
            }
        }
    }
}
