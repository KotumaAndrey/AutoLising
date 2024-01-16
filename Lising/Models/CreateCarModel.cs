using WebApiUtils.Entities;

namespace Lising.Models
{
    public class CreateCarModel
    {
        public string Name { get; set; }
        public IEnumerable<DEntityWithName> Brands { get; set; }
        public IEnumerable<DEntityWithName> Models { get; set; }
        public string ErrorText { get; set; }
    }
}
