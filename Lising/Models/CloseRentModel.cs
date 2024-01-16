using WebApiUtils.Entities;

namespace Lising.Models
{
    public class CloseRentModel
    {
        public string? ErrorText { get; set; }
        public int RentId { get; set; }
        public DCarRentLinked? Rent { get; set; }
        public DateTime? EndDate { get; set; }
        public int? PenaltyByDay { get; set; }
        public int? TotalPenalty { get; set; }
    }
}
