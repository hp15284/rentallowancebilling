using Microsoft.AspNetCore.Identity;

namespace RentAllowanceBilling.Web.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? BranchId { get; set; }
    public Branch? Branch { get; set; }
}
