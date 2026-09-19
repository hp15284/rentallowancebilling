using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentAllowanceBilling.Web.Models;

public class RentAllowanceBill
{
    public int Id { get; set; }

    // જા.નં. - office reference number, assigned on submission.
    [StringLength(50)]
    public string? BillNumber { get; set; }

    public int BranchId { get; set; }
    public Branch? Branch { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal BasicSalary { get; set; }

    // હું તા. ___ થી તા. ___ સુધી
    [Required]
    public DateTime PeriodFrom { get; set; }

    [Required]
    public DateTime PeriodTo { get; set; }

    // શાખાએ/મુકામે ___ ના કામ સારુ ગયેલ / ડેપ્યુટેશન કારણ
    [Required, StringLength(300)]
    public string PurposeOfVisit { get; set; } = string.Empty;

    // શાખાના બ્રાન્ય મેનેજરશ્રી/કેશીયર રજા ઉપર હોઈ (વૈકલ્પિક)
    public DateTime? ManagerOnLeaveFrom { get; set; }
    public DateTime? ManagerOnLeaveTo { get; set; }

    // Uploaded scanned copy of the manager/cashier's leave report, relative to wwwroot.
    [StringLength(260)]
    public string? LeaveReportCopyPath { get; set; }

    // તા. ___ ના રોજ ચાર્જ સંભાળેલ
    public DateTime? ChargeHandedOverDate { get; set; }

    public BillStatus Status { get; set; } = BillStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByUserId { get; set; }

    public DateTime? SubmittedAt { get; set; }

    // Branch Manager recommendation (પ્રતિ, શ્રી આસી. જનરલ મેનેજર / બ્રાન્ય મેનેજર સહી)
    public string? BranchManagerUserId { get; set; }
    public DateTime? BranchManagerActionAt { get; set; }
    [StringLength(500)]
    public string? BranchManagerRemark { get; set; }

    // Accountant verification (આંકડા અધિકારી/એકાઉન્ટન્ટ)
    public string? AccountantUserId { get; set; }
    public DateTime? AccountantActionAt { get; set; }
    [StringLength(500)]
    public string? AccountantRemark { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? VerifiedAmount { get; set; }

    // Approving authority sanction (આસી. જનરલ મેનેજર/ચીફ એક્ઝીક્યુટીવ)
    public string? ApprovingAuthorityUserId { get; set; }
    public DateTime? ApprovingAuthorityActionAt { get; set; }
    [StringLength(500)]
    public string? ApprovingAuthorityRemark { get; set; }
    [Column(TypeName = "decimal(18,2)")]
    public decimal? ApprovedAmount { get; set; }
    [StringLength(300)]
    public string? ApprovedAmountInWords { get; set; }

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public ICollection<RentAllowanceBillTrip> Trips { get; set; } = new List<RentAllowanceBillTrip>();

    [NotMapped]
    public decimal TotalFare => Trips.Sum(t => t.Fare);

    [NotMapped]
    public decimal TotalAllowance => Trips.Sum(t => t.Allowance);

    [NotMapped]
    public decimal TotalAmount => Trips.Sum(t => t.Fare + t.Allowance);
}
