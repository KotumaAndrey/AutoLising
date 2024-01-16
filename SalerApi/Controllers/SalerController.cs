using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiUtils.BaseApi;

namespace SalerApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class SalerController : ApiWithNameController { }
}
