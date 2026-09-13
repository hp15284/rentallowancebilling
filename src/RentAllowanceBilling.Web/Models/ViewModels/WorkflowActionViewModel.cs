using System.ComponentModel.DataAnnotations;

namespace RentAllowanceBilling.Web.Models.ViewModels;

// Used by Branch Manager recommendation, Accountant verification and
// Approving Authority sanction steps — each is a lightweight remark +
// optional amount confirmation before advancing the bill's status.
public class WorkflowActionViewModel
{
    public int BillId { get; set; }

    [StringLength(500)]
    [Display(Name = "Remark")]
    public string? Remark { get; set; }

    [Display(Name = "Amount (Rs.)")]
    public decimal? Amount { get; set; }

    [StringLength(300)]
    [Display(Name = "Amount in Words")]
    public string? AmountInWords { get; set; }
}

public class RejectBillViewModel
{
    public int BillId { get; set; }

    [Required, StringLength(500)]
    [Display(Name = "Reason for Rejection")]
    public string Reason { get; set; } = string.Empty;
}
