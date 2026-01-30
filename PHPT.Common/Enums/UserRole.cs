namespace PHPT.Common.Enums;

public enum UserRole
{
    Admin = 1,
    FrontDeskStaff = 2
}

public static class UserRoles
{
    public const string Admin = "Admin";
    public const string FrontDeskStaff = "FrontDeskStaff";
    public const string All = "Admin,FrontDeskStaff";
}
