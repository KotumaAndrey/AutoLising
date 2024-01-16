using System;
using WebApiUtils.Entities;

namespace WebApiUtils.BaseApi
{
    public abstract class ApiWithNameController : BaseApiWithNameController<DEntityWithName, BaseWithNameRepository<DEntityWithName>>
    {
        protected override string connectionString => Environment.GetEnvironmentVariable("ConnectionString")!;
        protected override BaseWithNameRepository<DEntityWithName> repository => new BaseWithNameRepository<DEntityWithName>(connectionString);
    }
}
