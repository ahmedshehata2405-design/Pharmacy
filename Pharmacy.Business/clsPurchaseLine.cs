namespace Pharmacy.Business
{
    // A single line in a purchase invoice. Not a standalone saved entity -
    // it only exists as part of a clsPurchase's Lines list.
    public class clsPurchaseLine
    {
        public int MedicineID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public decimal LineTotal => Quantity * UnitPrice;
    }
}
