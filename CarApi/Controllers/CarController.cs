using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiUtils.ApiAddresses;
using WebApiUtils;
using WebApiUtils.BaseApi;
using WebApiUtils.Entities;

namespace CarApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CarController : BaseApiWithNameController<DCar, BaseWithNameRepository<DCar>>
    {
        protected override string connectionString => Environment.GetEnvironmentVariable("ConnectionString")!;
        protected override BaseWithNameRepository<DCar> repository => new BaseWithNameRepository<DCar>(connectionString);

        public override DResponse<DCar> Add(DCar item)
        {
            using (var client = new WebApiUtils.HttpClient())
            {
                var brandResponce = CheckOtherObjectExists(item.BrandId, client, ApiDictionary.BrandApi.GetById, "Brand", item);
                if (brandResponce is not null) return brandResponce;

                var modelResponce = CheckOtherObjectExists(item.ModelId, client, ApiDictionary.ModelApi.GetById, "Model", item);
                if (modelResponce is not null) return modelResponce;

                return base.Add(item);
            }
        }

        public override DResponse<DCar> Update(DCar item)
        {
            using (var client = new WebApiUtils.HttpClient())
            {
                var brandResponce = CheckOtherObjectExists(item.BrandId, client, ApiDictionary.BrandApi.GetById, "Brand", item);
                if (brandResponce is not null) return brandResponce;

                var modelResponce = CheckOtherObjectExists(item.ModelId, client, ApiDictionary.ModelApi.GetById, "Model", item);
                if (modelResponce is not null) return modelResponce;

                return base.Update(item);
            }
        }
    }
}
