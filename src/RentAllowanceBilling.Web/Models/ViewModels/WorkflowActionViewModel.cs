using System.ComponentModel.DataAnnotations;

namespace RentAllowanceBilling.Web.Models.ViewModels;

// Used by the Branch Manager's recommend-and-approve step: a remark plus
// the final approved amount before the bill's status advances.
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
