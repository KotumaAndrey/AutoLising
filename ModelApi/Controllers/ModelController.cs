using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiUtils.BaseApi;

namespace ModelApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class ModelController : ApiWithNameController { }
}
