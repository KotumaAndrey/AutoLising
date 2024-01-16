using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiUtils.BaseApi;

namespace BrandApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class BrandController : ApiWithNameController { }
}
