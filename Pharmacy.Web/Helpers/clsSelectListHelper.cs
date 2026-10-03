using System.Data;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Pharmacy.Web.Helpers
{
    // Builds dropdown items straight from a DataTable's rows.
    //
    // Why not `new SelectList(table.DefaultView, valueField, textField)`? ASP.NET Core's SelectList has to
    // figure out, by reflection, which property on a DataRowView holds "BranchID" etc. It does that by
    // looking at the first row. When the table has ZERO rows (a brand-new Customers or Prescriptions table,
    // for example) there's no row to reflect on, and rendering the <select> throws a NullReferenceException
    // deep inside MultiSelectList - a real crash, not a "no data" message. Building the list by hand here
    // sidesteps that entirely and simply renders an empty dropdown when the table is empty.
    public static class clsSelectListHelper
    {
        public static List<SelectListItem> FromDataTable(DataTable table, string valueField, string textField)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            foreach (DataRow row in table.Rows)
            {
                items.Add(new SelectListItem
                {
                    Value = row[valueField].ToString(),
                    Text = row[textField].ToString()
                });
            }

            return items;
        }
    }
}
