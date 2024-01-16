namespace WebApiUtils.Entities
{
    public class DCarLinked : DEntityWithName
    {
        public DEntityWithName? Brand { get; set; }
        public DEntityWithName? Model { get; set; }
    }
}
