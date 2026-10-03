using System.Data;
using Pharmacy.DataAccess;

namespace Pharmacy.Business
{
    public class clsCategory
    {
        public enMode Mode = enMode.AddNew;

        public int CategoryID { get; set; }
        public string CategoryName { get; set; }

        public clsCategory()
        {
            this.CategoryID = -1;
            this.CategoryName = "";

            Mode = enMode.AddNew;
        }

        private clsCategory(int categoryID, string categoryName)
        {
            this.CategoryID = categoryID;
            this.CategoryName = categoryName;

            Mode = enMode.Update;
        }

        public static clsCategory Find(int categoryID)
        {
            string categoryName = "";
            bool isFound = clsCategoryData.GetCategoryInfoByID(categoryID, ref categoryName);

            if (isFound)
                return new clsCategory(categoryID, categoryName);

            return null;
        }

        private bool _AddNewCategory()
        {
            this.CategoryID = clsCategoryData.AddNewCategory(this.CategoryName);
            return (this.CategoryID != -1);
        }

        private bool _UpdateCategory()
        {
            return clsCategoryData.UpdateCategory(this.CategoryID, this.CategoryName);
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewCategory())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateCategory();
            }

            return false;
        }

        public bool Delete()
        {
            return clsCategoryData.DeleteCategory(this.CategoryID);
        }

        public static DataTable GetAllCategories()
        {
            return clsCategoryData.GetAllCategories();
        }
    }
}
