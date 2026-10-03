namespace Pharmacy.Business
{
    // A single medicine being sold. The unit price is NOT part of the line:
    // the database (usp_CreateSale) always uses the current Inventory selling price.
    public class clsSaleLine
    {
        public int MedicineID { get; set; }
        public int Quantity { get; set; }
    }
}
