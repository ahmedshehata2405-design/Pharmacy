using Pharmacy.DataAccess;

namespace Pharmacy.Business
{
    public enum enMode { AddNew = 0, Update = 1 }
    public enum enRole { Admin = 1, Pharmacist = 2, Cashier = 3 }

    public class clsUser
    {
        public enMode Mode = enMode.AddNew;

        public int UserID { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public int RoleID { get; set; }
        public int BranchID { get; set; }
        public bool IsActive { get; set; }

        public clsUser()
        {
            this.UserID = -1;
            this.Username = "";
            this.PasswordHash = "";
            this.Email = "";
            this.RoleID = (int)enRole.Cashier;
            this.BranchID = -1;
            this.IsActive = true;

            Mode = enMode.AddNew;
        }

        private clsUser(int userID, string username, string passwordHash, string email,
            int roleID, int branchID, bool isActive)
        {
            this.UserID = userID;
            this.Username = username;
            this.PasswordHash = passwordHash;
            this.Email = email;
            this.RoleID = roleID;
            this.BranchID = branchID;
            this.IsActive = isActive;

            Mode = enMode.Update;
        }

        public static clsUser Find(int userID)
        {
            string username = "", passwordHash = "", email = "";
            int roleID = 0, branchID = 0;
            bool isActive = false;

            bool isFound = clsUserData.GetUserInfoByID(userID, ref username, ref passwordHash,
                ref email, ref roleID, ref branchID, ref isActive);

            if (isFound)
                return new clsUser(userID, username, passwordHash, email, roleID, branchID, isActive);

            return null;
        }

        public static clsUser Find(string username)
        {
            int userID = -1, roleID = 0, branchID = 0;
            string passwordHash = "", email = "";
            bool isActive = false;

            bool isFound = clsUserData.GetUserInfoByUsername(username, ref userID, ref passwordHash,
                ref email, ref roleID, ref branchID, ref isActive);

            if (isFound)
                return new clsUser(userID, username, passwordHash, email, roleID, branchID, isActive);

            return null;
        }

        private bool _AddNewUser()
        {
            // Hash the raw password once, on the way into the database
            this.PasswordHash = clsPasswordHasher.HashPassword(this.PasswordHash);

            this.UserID = clsUserData.AddNewUser(this.Username, this.PasswordHash, this.Email,
                this.RoleID, this.BranchID);

            return (this.UserID != -1);
        }

        private bool _UpdateUser()
        {
            return clsUserData.UpdateUser(this.UserID, this.Username, this.PasswordHash, this.Email,
                this.RoleID, this.BranchID, this.IsActive);
        }

        public bool Save()
        {
            switch (Mode)
            {
                case enMode.AddNew:
                    if (_AddNewUser())
                    {
                        Mode = enMode.Update;
                        return true;
                    }
                    return false;

                case enMode.Update:
                    return _UpdateUser();
            }

            return false;
        }

        public bool Delete()
        {
            return clsUserData.DeleteUser(this.UserID);
        }

        public static bool IsUsernameExist(string username)
        {
            return clsUserData.IsUsernameExist(username);
        }

        /// <summary>
        /// Validates login credentials against the Users table.
        /// Returns the logged-in clsUser, or null if the username/password is wrong or the account is disabled.
        /// </summary>
        public static clsUser Login(string username, string password)
        {
            clsUser user = clsUser.Find(username);

            if (user == null)
                return null;

            if (!user.IsActive)
                return null;

            bool isPasswordCorrect = clsPasswordHasher.VerifyPassword(password, user.PasswordHash);

            return isPasswordCorrect ? user : null;
        }
    }
}
