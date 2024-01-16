using WebApiUtils.Entities;

namespace Lising.Models
{
    public class CreateRentModel
    {
        public IEnumerable<DCar> Cars { get; set; }
        public IEnumerable<DEntityWithName> Clients { get; set; }
        public IEnumerable<DEntityWithName> Salers { get; set; }
        public string ErrorText { get; set; }
    }
}
