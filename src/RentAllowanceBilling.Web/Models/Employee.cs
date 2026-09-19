using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentAllowanceBilling.Web.Models;

public class Employee
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    // Provident Fund account number, used to look the employee up when raising a bill.
    [StringLength(50)]
    public string? PfNumber { get; set; }

    [Required, StringLength(100)]
    public string Designation { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal BasicSalary { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public bool IsActive { get; set; } = true;

    // Linked login account (an employee may not yet have a user account).
    public string? ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }

    public ICollection<RentAllowanceBill> Bills { get; set; } = new List<RentAllowanceBill>();
}
