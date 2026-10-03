using System.Data;
using Pharmacy.DataAccess;

namespace Pharmacy.Business
{
    public class clsDashboard
    {
        public int MedicinesCount { get; set; }
        public int LowStockCount { get; set; }
        public int ExpiringSoonCount { get; set; }
        public decimal TodaysSalesTotal { get; set; }
        public DataTable RecentSales { get; set; }

        public static clsDashboard Load()
        {
            return new clsDashboard
            {
                MedicinesCount = clsDashboardData.GetMedicinesCount(),
                LowStockCount = clsDashboardData.GetLowStockCount(10),   // "low stock" threshold: 10 units
                ExpiringSoonCount = clsDashboardData.GetExpiringSoonCount(30), // expiring within 30 days
                TodaysSalesTotal = clsDashboardData.GetTodaysSalesTotal(),
                RecentSales = clsDashboardData.GetRecentSales(5)
            };
        }
    }
}
