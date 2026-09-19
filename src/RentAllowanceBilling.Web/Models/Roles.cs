namespace RentAllowanceBilling.Web.Models;

public static class Roles
{
    public const string Admin = "Admin";
    public const string Employee = "Employee";
    public const string BranchManager = "BranchManager";

    public static readonly string[] All =
    {
        Admin, Employee, BranchManager
    };
}
