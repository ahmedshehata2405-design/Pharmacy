namespace Pharmacy.Business
{
    // A single medicine entry on a prescription. Not a standalone saved entity -
    // it only exists as part of a clsPrescription's Lines list.
    public class clsPrescriptionLine
    {
        public int MedicineID { get; set; }
        public string Dosage { get; set; }
        public int Quantity { get; set; }
    }
}
