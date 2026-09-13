namespace RentAllowanceBilling.Web.Models;

public enum BillStatus
{
    Draft = 0,
    PendingBranchManager = 1,
    PendingAccountant = 2,
    PendingApproval = 3,
    Approved = 4,
    Rejected = 5
}
