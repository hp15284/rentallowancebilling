using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentAllowanceBilling.Web.Models;

// One row of the paper form's table: તારીખ | ક્યાંથી | ક્યાં | નીકળ્યા સમય | પરત આવ્યા સમય | ભાડુ | ભથ્થુ | કુલ
public class RentAllowanceBillTrip
{
    public int Id { get; set; }

    public int RentAllowanceBillId { get; set; }
    public RentAllowanceBill? RentAllowanceBill { get; set; }

    [Required]
    public DateTime TravelDate { get; set; }

    [Required, StringLength(150)]
    public string FromPlace { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string ToPlace { get; set; } = string.Empty;

    public TimeSpan? DepartureTime { get; set; }

    public TimeSpan? ReturnTime { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Fare { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Allowance { get; set; }

    [NotMapped]
    public decimal Total => Fare + Allowance;
}
