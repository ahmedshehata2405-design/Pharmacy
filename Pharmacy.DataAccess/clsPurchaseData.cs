using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Data.SqlClient.Server;

namespace Pharmacy.DataAccess
{
    public class clsPurchaseData
    {
        public static DataTable GetAllPurchases()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT P.PurchaseID, S.Name AS SupplierName, B.BranchName, P.PurchaseDate, P.TotalAmount
                              FROM Purchases P
                              JOIN Suppliers S ON P.SupplierID = S.SupplierID
                              JOIN Branches B ON P.BranchID = B.BranchID
                              ORDER BY P.PurchaseDate DESC";

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
                    Console.WriteLine("Error in GetAllPurchases: " + ex.Message);
                }
            }

            return dt;
        }

        public static DataTable GetPurchaseDetails(int purchaseID)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT PD.PurchaseDetailID, M.Name AS MedicineName, PD.Quantity, PD.UnitPrice,
                                     (PD.Quantity * PD.UnitPrice) AS LineTotal
                              FROM PurchaseDetails PD
                              JOIN Medicines M ON PD.MedicineID = M.MedicineID
                              WHERE PD.PurchaseID = @PurchaseID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@PurchaseID", purchaseID);

                try
                {
                    connection.Open();
                    using SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetPurchaseDetails: " + ex.Message);
                }
            }

            return dt;
        }

        /// <summary>
        /// Creates a purchase by calling dbo.usp_CreatePurchase (invoice + lines + stock update in one transaction).
        /// Returns false with errorMessage set on a rule violation; throws on unexpected database errors.
        /// </summary>
        public static bool CreatePurchase(int supplierID, int branchID, DataTable lines,
            ref int purchaseID, ref string errorMessage)
        {
            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            using (SqlCommand command = new SqlCommand("dbo.usp_CreatePurchase", connection))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("@SupplierID", supplierID);
                command.Parameters.AddWithValue("@BranchID", branchID);

                command.Parameters.Add(new SqlParameter("@Lines", SqlDbType.Structured)
                {
                    TypeName = "dbo.PurchaseLineType",
                    Value = _ToRecords(lines)
                });

                SqlParameter purchaseIdParam = new SqlParameter("@PurchaseID", SqlDbType.Int) { Direction = ParameterDirection.Output };
                SqlParameter errorParam = new SqlParameter("@ErrorMessage", SqlDbType.NVarChar, 400) { Direction = ParameterDirection.Output };
                command.Parameters.Add(purchaseIdParam);
                command.Parameters.Add(errorParam);

                connection.Open();
                command.ExecuteNonQuery();

                errorMessage = errorParam.Value == DBNull.Value ? null : (string)errorParam.Value;

                if (purchaseIdParam.Value == DBNull.Value)
                    return false;

                purchaseID = (int)purchaseIdParam.Value;
                return true;
            }
        }

        // Sends the lines with explicit column metadata. A plain DataTable would let the driver guess the
        // decimal precision/scale, which can round prices; DECIMAL(10,2) here matches dbo.PurchaseLineType.
        private static IEnumerable<SqlDataRecord> _ToRecords(DataTable lines)
        {
            SqlMetaData[] metadata =
            {
                new SqlMetaData("MedicineID", SqlDbType.Int),
                new SqlMetaData("Quantity", SqlDbType.Int),
                new SqlMetaData("UnitPrice", SqlDbType.Decimal, 10, 2)
            };

            foreach (DataRow row in lines.Rows)
            {
                SqlDataRecord record = new SqlDataRecord(metadata);
                record.SetInt32(0, (int)row["MedicineID"]);
                record.SetInt32(1, (int)row["Quantity"]);
                record.SetDecimal(2, Math.Round((decimal)row["UnitPrice"], 2));
                yield return record;
            }
        }
    }
}
