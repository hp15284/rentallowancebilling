namespace RentAllowanceBilling.Web.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Employee = "Employee";
    public const string BranchManager = "BranchManager";
    public const string Accountant = "Accountant";
    public const string ApprovingAuthority = "ApprovingAuthority";

    public static readonly string[] All =
    {
        Admin, Employee, BranchManager, Accountant, ApprovingAuthority
    };
}
