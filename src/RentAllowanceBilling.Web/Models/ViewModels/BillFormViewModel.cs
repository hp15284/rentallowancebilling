using System.ComponentModel.DataAnnotations;

namespace RentAllowanceBilling.Web.Models.ViewModels;

public class BillFormViewModel
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    [StringLength(150)]
    public string? EmployeeName { get; set; }

    [StringLength(100)]
    public string? Designation { get; set; }

    public decimal BasicSalary { get; set; }

    public int BranchId { get; set; }

    [Display(Name = "Branch")]
    public List<Branch>? Branches { get; set; }

    [Required]
    [Display(Name = "Period From")]
    [DataType(DataType.Date)]
    public DateTime PeriodFrom { get; set; } = DateTime.Today;

    [Required]
    [Display(Name = "Period To")]
    [DataType(DataType.Date)]
    public DateTime PeriodTo { get; set; } = DateTime.Today;

    [Required, StringLength(300)]
    [Display(Name = "Purpose of Visit / Deputation")]
    public string PurposeOfVisit { get; set; } = string.Empty;

    [Display(Name = "Manager on Leave From")]
    [DataType(DataType.Date)]
    public DateTime? ManagerOnLeaveFrom { get; set; }

    [Display(Name = "Manager on Leave To")]
    [DataType(DataType.Date)]
    public DateTime? ManagerOnLeaveTo { get; set; }

    [Display(Name = "Charge Handed Over On")]
    [DataType(DataType.Date)]
    public DateTime? ChargeHandedOverDate { get; set; }

    public List<TripRowViewModel> Trips { get; set; } = new() { new TripRowViewModel() };
}

public class TripRowViewModel
{
    [Display(Name = "Date")]
    [DataType(DataType.Date)]
    public DateTime? TravelDate { get; set; }

    [Display(Name = "From")]
    public string? FromPlace { get; set; }

    [Display(Name = "To")]
    public string? ToPlace { get; set; }

    [Display(Name = "Departure Time")]
    public string? DepartureTime { get; set; }

    [Display(Name = "Return Time")]
    public string? ReturnTime { get; set; }

    [Display(Name = "Fare (Rs.)")]
    public decimal Fare { get; set; }

    [Display(Name = "Allowance (Rs.)")]
    public decimal Allowance { get; set; }
}
