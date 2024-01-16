using System;

namespace WebApiUtils.Entities
{
    public class DCarRentLinked : DEntityWithId
    {
        public DCar? Car { get; set; }
        public DEntityWithName? Client { get; set; }
        public DEntityWithName? Saler { get; set; }
        public DateTime OpenDate { get; set; }
        public int RentDays { get; set; }
        public DateTime? CloseDate { get; set; }
        public int? Penalty { get; set; }
    }
}
