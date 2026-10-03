using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace Pharmacy.DataAccess
{
    public class clsInventoryData
    {
        public static bool GetInventoryInfoByID(int inventoryID, ref int branchID, ref int medicineID,
            ref int quantity, ref decimal sellingPrice, ref DateTime? expiryDate)
        {
            bool isFound = false;
            string query = @"SELECT * FROM Inventory WHERE InventoryID = @InventoryID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@InventoryID", inventoryID);

                try
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        isFound = true;
                        branchID = (int)reader["BranchID"];
                        medicineID = (int)reader["MedicineID"];
                        quantity = (int)reader["Quantity"];
                        sellingPrice = (decimal)reader["SellingPrice"];
                        expiryDate = reader["ExpiryDate"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["ExpiryDate"];
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetInventoryInfoByID: " + ex.Message);
                }
            }

            return isFound;
        }

        public static int AddNewInventory(int branchID, int medicineID, int quantity, decimal sellingPrice, DateTime? expiryDate)
        {
            int newID = -1;
            string query = @"INSERT INTO Inventory (BranchID, MedicineID, Quantity, SellingPrice, ExpiryDate)
                              VALUES (@BranchID, @MedicineID, @Quantity, @SellingPrice, @ExpiryDate);
                              SELECT SCOPE_IDENTITY();";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BranchID", branchID);
                command.Parameters.AddWithValue("@MedicineID", medicineID);
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.Parameters.AddWithValue("@SellingPrice", sellingPrice);
                command.Parameters.AddWithValue("@ExpiryDate", expiryDate.HasValue ? (object)expiryDate.Value : DBNull.Value);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                        newID = Convert.ToInt32(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in AddNewInventory: " + ex.Message);
                }
            }

            return newID;
        }

        public static bool UpdateInventory(int inventoryID, int branchID, int medicineID, int quantity,
            decimal sellingPrice, DateTime? expiryDate)
        {
            int rowsAffected = 0;
            string query = @"UPDATE Inventory
                              SET BranchID = @BranchID, MedicineID = @MedicineID, Quantity = @Quantity,
                                  SellingPrice = @SellingPrice, ExpiryDate = @ExpiryDate
                              WHERE InventoryID = @InventoryID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@InventoryID", inventoryID);
                command.Parameters.AddWithValue("@BranchID", branchID);
                command.Parameters.AddWithValue("@MedicineID", medicineID);
                command.Parameters.AddWithValue("@Quantity", quantity);
                command.Parameters.AddWithValue("@SellingPrice", sellingPrice);
                command.Parameters.AddWithValue("@ExpiryDate", expiryDate.HasValue ? (object)expiryDate.Value : DBNull.Value);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in UpdateInventory: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        public static bool DeleteInventory(int inventoryID)
        {
            int rowsAffected = 0;
            string query = @"DELETE FROM Inventory WHERE InventoryID = @InventoryID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@InventoryID", inventoryID);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in DeleteInventory: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        // Adds (or subtracts, with a negative value) from the current quantity.
        // Used by Purchases (add stock) and Sales (subtract stock) so both go through one place.
        public static bool AdjustQuantity(int inventoryID, int quantityChange)
        {
            int rowsAffected = 0;
            string query = @"UPDATE Inventory
                              SET Quantity = Quantity + @QuantityChange
                              WHERE InventoryID = @InventoryID AND Quantity + @QuantityChange >= 0";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@InventoryID", inventoryID);
                command.Parameters.AddWithValue("@QuantityChange", quantityChange);

                try
                {
                    connection.Open();
                    rowsAffected = command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in AdjustQuantity: " + ex.Message);
                }
            }

            return rowsAffected > 0;
        }

        // Finds the Inventory row for this branch+medicine combo, or -1 if it doesn't exist yet.
        // Used by Purchases so buying a medicine that's already stocked adds to the same row.
        public static int GetInventoryIDByBranchAndMedicine(int branchID, int medicineID)
        {
            int inventoryID = -1;
            string query = @"SELECT InventoryID FROM Inventory WHERE BranchID = @BranchID AND MedicineID = @MedicineID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BranchID", branchID);
                command.Parameters.AddWithValue("@MedicineID", medicineID);

                try
                {
                    connection.Open();
                    object result = command.ExecuteScalar();
                    if (result != null)
                        inventoryID = Convert.ToInt32(result);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetInventoryIDByBranchAndMedicine: " + ex.Message);
                }
            }

            return inventoryID;
        }

        // Used by Sales to check available stock and pull the current selling price before finalizing a line
        public static bool GetStockInfo(int branchID, int medicineID, ref int inventoryID, ref int quantity, ref decimal sellingPrice)
        {
            bool isFound = false;
            string query = @"SELECT InventoryID, Quantity, SellingPrice FROM Inventory
                              WHERE BranchID = @BranchID AND MedicineID = @MedicineID";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BranchID", branchID);
                command.Parameters.AddWithValue("@MedicineID", medicineID);

                try
                {
                    connection.Open();
                    using SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        isFound = true;
                        inventoryID = (int)reader["InventoryID"];
                        quantity = (int)reader["Quantity"];
                        sellingPrice = (decimal)reader["SellingPrice"];
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetStockInfo: " + ex.Message);
                }
            }

            return isFound;
        }

        // ===== Transaction-aware overloads (used by clsSale.Save() and clsPurchase.Save() so every
        // insert/update for one invoice commits or rolls back together) =====

        // Joined view for the Index screen: shows branch name and medicine name instead of raw IDs
        public static DataTable GetAllInventoryWithDetails()
        {
            DataTable dt = new DataTable();
            string query = @"SELECT I.InventoryID, I.BranchID, B.BranchName, I.MedicineID, M.Name AS MedicineName,
                                     I.Quantity, I.SellingPrice, I.ExpiryDate
                              FROM Inventory I
                              JOIN Branches B ON I.BranchID = B.BranchID
                              JOIN Medicines M ON I.MedicineID = M.MedicineID
                              ORDER BY B.BranchName, M.Name";

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
                    Console.WriteLine("Error in GetAllInventoryWithDetails: " + ex.Message);
                }
            }

            return dt;
        }

        // Medicines expiring within the given number of days (for alerts on the dashboard)
        public static DataTable GetExpiringSoon(int daysAhead)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT I.InventoryID, B.BranchName, M.Name AS MedicineName, I.Quantity, I.ExpiryDate
                              FROM Inventory I
                              JOIN Branches B ON I.BranchID = B.BranchID
                              JOIN Medicines M ON I.MedicineID = M.MedicineID
                              WHERE I.ExpiryDate IS NOT NULL
                                AND I.ExpiryDate <= DATEADD(DAY, @DaysAhead, GETDATE())
                              ORDER BY I.ExpiryDate";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@DaysAhead", daysAhead);

                try
                {
                    connection.Open();
                    using SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetExpiringSoon: " + ex.Message);
                }
            }

            return dt;
        }
    
        // Sellable stock of ONE branch (in stock and not expired) - feeds the medicine list on the Sales screen
        public static DataTable GetBranchStock(int branchID)
        {
            DataTable dt = new DataTable();
            string query = @"SELECT I.MedicineID, M.Name, M.Unit, M.RequiresPrescription, I.Quantity, I.SellingPrice, I.ExpiryDate
                              FROM Inventory I
                              JOIN Medicines M ON I.MedicineID = M.MedicineID
                              WHERE I.BranchID = @BranchID
                                AND I.Quantity > 0
                                AND (I.ExpiryDate IS NULL OR I.ExpiryDate >= CAST(GETDATE() AS DATE))
                              ORDER BY M.Name";

            using (SqlConnection connection = new SqlConnection(clsDataAccessSettings.ConnectionString))
            {
                using SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@BranchID", branchID);

                try
                {
                    connection.Open();
                    using SqlDataAdapter adapter = new SqlDataAdapter(command);
                    adapter.Fill(dt);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in GetBranchStock: " + ex.Message);
                }
            }

            return dt;
        }

    }
}
