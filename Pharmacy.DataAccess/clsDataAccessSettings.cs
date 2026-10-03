namespace Pharmacy.DataAccess
{
    public static class clsDataAccessSettings
    {
        // Set once at startup from appsettings.json (see Pharmacy.Web/Program.cs)
        public static string ConnectionString { get; set; }
    }
}
