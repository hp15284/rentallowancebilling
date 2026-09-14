using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentAllowanceBilling.Web.Models;

// A basic-pay slab and the allowance it entitles an employee to.
// Rent allowance rules key the allowance amount off where the employee's
// basic pay falls, so slabs are matched by MinBasicPay/MaxBasicPay range.
public class BasicPayWithAllowance
{
    public int Id { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal MinBasicPay { get; set; }

    // Null means "and above" - the top, open-ended slab.
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MaxBasicPay { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal AllowanceAmount { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsApplicableFor(decimal basicPay) =>
        basicPay >= MinBasicPay && (MaxBasicPay is null || basicPay <= MaxBasicPay);
}
