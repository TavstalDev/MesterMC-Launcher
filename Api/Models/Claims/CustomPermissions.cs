namespace Tavstal.MesterMC.Api.Models.Claims;

public static class CustomPermissions
{
    public static class Account
    {
        public static class Create
        {
            
        }
        
        public static class Update 
        {
            public const string Profile = "mmc.permission.account.update.profile";
            public const string Password = "mmc.permission.account.update.password";
            public const string Billing = "mmc.permission.account.update.billing";
            public const string Email = "mmc.permission.account.update.email";
                
            // Elevated Permissions
            public const string OtherProfile = "mmc.permission.account.update.other.profile";
            public const string OtherPassword = "mmc.permission.account.update.other.password";
            public const string OtherBilling = "mmc.permission.account.update.other.billing";
            public const string OtherEmail = "mmc.permission.account.update.other.email";
        }
        
        public static class Delete
        {
            public const string Session = "mmc.permission.account.delete.session";
                
            // Elevated Permissions
            public const string OtherSession = "mmc.permission.account.delete.other.session";
        }
    }
}