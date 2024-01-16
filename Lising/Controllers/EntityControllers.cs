using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiUtils.ApiAddresses;
using WebApiUtils;
using WebApiUtils.Entities;

namespace Lising.Controllers
{
    public abstract class EntityControllers : Controller
    {
        protected abstract string IndexTitle { get; }
        protected abstract string CreateTitle { get; }
        protected abstract string ControllerName { get; }
        protected abstract NamedApiMethods ApiAddresses { get; }

        protected IActionResult ReturnIndexWithList(IEnumerable<DEntityWithName> items)
        {
            ViewData["Title"] = IndexTitle;
            ViewData["Controller"] = ControllerName;
            return View("Views/EntityViews/Index.cshtml", items);
        }

        protected IActionResult ReturnCreateWithErrorText(string? errorText, string name = "")
        {
            ViewData["Title"] = CreateTitle;
            ViewData["ErrorText"] = errorText;
            ViewData["EntityName"] = name;
            return View("Views/EntityViews/Create.cshtml");
        }

        protected IActionResult ReturnError(string? errorText)
        {
            ViewData["ErrorText"] = errorText;
            return View("Views/Shared/ErrorView.cshtml");
        }

        public IActionResult Index()
        {
            using (var client = new WebApiUtils.HttpClient())
            {
                var items = client.GetAllFrom<DEntityWithName>(ApiAddresses);

                if (items is null) return ReturnError("Request error");
                if (!items.IsSuccess) return ReturnError(items.Message);

                return ReturnIndexWithList(items.Data!);
            }
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            return ReturnCreateWithErrorText(null);
        }

        [HttpPost]
        [Route("create")]
        public IActionResult CreatePost()
        {
            var name = Request.Form["name"].ToString().Trim();

            using (var client = new WebApiUtils.HttpClient())
            {
                var item = new DEntityWithName { Name = name };
                var response = client.AddFrom(ApiAddresses, item);

                if (response is null) return ReturnError("Request error");
                if (!response.IsSuccess) return ReturnCreateWithErrorText(response.Message, name);

                return base.RedirectToAction("Index");
            }
        }
    }

    [Authorize]
    [Route("/brands")]
    public class BrandController : EntityControllers
    {
        protected override string IndexTitle => "Brand menu";
        protected override string CreateTitle => "Create Brand";
        protected override string ControllerName => "Brand";
        protected override NamedApiMethods ApiAddresses => ApiDictionary.BrandApi;
    }

    [Authorize]
    [Route("/models")]
    public class ModelController : EntityControllers
    {
        protected override string IndexTitle => "Models menu";
        protected override string CreateTitle => "Create Book Models";
        protected override string ControllerName => "Model";
        protected override NamedApiMethods ApiAddresses => ApiDictionary.ModelApi;
    }

    [Authorize]
    [Route("/saler")]
    public class SalerController : EntityControllers
    {
        protected override string IndexTitle => "Saler menu";
        protected override string CreateTitle => "Create Saler";
        protected override string ControllerName => "Saler";
        protected override NamedApiMethods ApiAddresses => ApiDictionary.SalerApi;
    }

    [Authorize]
    [Route("/client")]
    public class ClientController : EntityControllers
    {
        protected override string IndexTitle => "Client menu";
        protected override string CreateTitle => "Create Client";
        protected override string ControllerName => "Client";
        protected override NamedApiMethods ApiAddresses => ApiDictionary.ClientApi;
    }
}
