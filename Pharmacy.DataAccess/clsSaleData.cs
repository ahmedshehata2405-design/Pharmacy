using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DataAccess
{
    public class clsSaleData
    {
        public static DataTable GetAllSales()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT S.SaleID, B.BranchName, ISNULL(C.Name, N'عميل نقدي') AS CustomerName,
                                     U.Username AS CashierName, S.SaleDate, S.TotalAmount
                              FROM Sales S
                              JOIN Branches B ON S.BranchID = B.BranchID
                              LEFT JOIN Customers C ON S.CustomerID = C.CustomerID
                              JOIN Users U ON S.UserID = U.UserID
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
                    Console.WriteLine("Error in GetAllSales: " + ex.Message);
                }
            }

            return dt;
        }

        public static DataTable GetSaleDetails(int saleID)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT SD.SaleDetailID, M.Name AS MedicineName, SD.Quantity, SD.UnitPrice,
                                     (SD.Quantity * SD.UnitPrice) AS LineTotal
                              FROM SaleDetails SD
                              JOIN Medicines M ON SD.MedicineID = M.MedicineID
                              WHERE SD.SaleID = @SaleID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SaleID", saleID);

                try
                {
                    connection.Open();
                    using SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetSaleDetails: " + ex.Message);
                }
            }

            return dt;
        }

        /// <summary>
        /// Creates a sale by calling dbo.usp_CreateSale. All rules (prescription, stock, expiry, price) and the
        /// transaction live in the stored procedure. Returns false with errorMessage set when a rule is violated;
        /// throws on unexpected database errors (the procedure has already rolled everything back).
        /// </summary>
        public static bool CreateSale(int branchID, int? customerID, int? prescriptionID, int userID,
            DataTable lines, ref int saleID, ref string errorMessage)
        {
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (SqlCommand command = new SqlCommand("dbo.usp_CreateSale", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@BranchID", branchID);
                command.Parameters.AddWithValue("@CustomerID", (object)customerID ?? DBNull.Value);
                command.Parameters.AddWithValue("@PrescriptionID", (object)prescriptionID ?? DBNull.Value);
                command.Parameters.AddWithValue("@UserID", userID);

                command.Parameters.Add(new SqlParameter("@Lines", SqlDbType.Structured)
                {
                    TypeName = "dbo.SaleLineType",
                    Value = lines
                });

                SqlParameter saleIdParam = new SqlParameter("@SaleID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SqlParameter errorParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 400) { Direction = ParameterDirection.Output };
                command.Parameters.Add(saleIdParam);
                command.Parameters.Add(errorParam);

                connection.Open();
                command.ExecuteNonQuery();

                errorMessage = errorParam.Value == DBNull.Value ? null : (string)errorParam.Value;

                if (saleIdParam.Value == DBNull.Value)
                    return false;

                saleID = (int)saleIdParam.Value;
                return true;
            }
        }
    
        // Invoice header (branch, customer, cashier, date, total) for the printable invoice page
        public static DataTable GetSaleHeader(int saleID)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT S.SaleID, B.BranchName, B.Address AS BranchAddress, B.Phone AS BranchPhone,
                                     ISNULL(C.Name, N'عميل نقدي') AS CustomerName, S.PrescriptionID,
                                     U.Username AS CashierName, S.SaleDate, S.TotalAmount
                              FROM Sales S
                              JOIN Branches B ON S.BranchID = B.BranchID
                              LEFT JOIN Customers C ON S.CustomerID = C.CustomerID
                              JOIN Users U ON S.UserID = U.UserID
                              WHERE S.SaleID = @SaleID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SaleID", saleID);

                try
                {
                    connection.Open();
                    using SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetSaleHeader: " + ex.Message);
                }
            }

            return dt;
        }

    }
}
