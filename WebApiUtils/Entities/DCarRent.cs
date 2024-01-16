using System;

namespace WebApiUtils.Entities
{
    public class DCarRent : DEntityWithId
    {
        public int CarId { get; set; }
        public int ClientId { get; set; }
        public int SalerId { get; set; }
        public DateTime OpenDate { get; set; }
        public int RentDays { get; set; }
        public DateTime? CloseDate { get; set; }
        public int? Penalty { get; set; }
    }
}
