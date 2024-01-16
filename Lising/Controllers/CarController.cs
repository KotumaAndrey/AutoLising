using Lising.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiUtils;
using WebApiUtils.ApiAddresses;
using WebApiUtils.Entities;

namespace Lising.Controllers
{
    [Authorize]
    [Route("/car")]
    public class CarController : Controller
    {
        public IActionResult Index()
        {
            using (var client = new WebApiUtils.HttpClient())
            {
                try
                {
                    var Cars = client.GetAllFrom<DCar>(ApiDictionary.CarApi);
                    var items = Cars.Data.Select(item => MapCarToLinks(item, client)).ToArray();
                    return base.View("Index", items);
                }
                catch (Exception ex)
                {
                    return ReturnError(ex.Message);
                }
            }
        }

        [HttpGet]
        [Route("create")]
        public IActionResult Create()
        {
            using (var db = new WebApiUtils.HttpClient())
            {
                var model = PrepareCreateCarModel(db);
                return base.View("Create", model);
            }
        }

        [HttpPost]
        [Route("create")]
        public IActionResult CreatePost()
        {
            var name = Request.Form["name"].ToString().Trim();
            var brandIdInput = Request.Form["brandId"].ToString();
            var modelIdInput = Request.Form["modelId"].ToString();

            using (var client = new WebApiUtils.HttpClient())
            {
                var item = new DCar
                {
                    Name = name
                };

                if (brandIdInput is not null && brandIdInput.Length > 0)
                {
                    if (int.TryParse(brandIdInput, out int brandId))
                    {
                        item.BrandId = brandId;
                    }
                    else
                    {
                        var model = PrepareCreateCarModel(client);
                        model.ErrorText = $"Error: Incorrect brand id";
                        return base.View("Create", model);
                    }
                }

                if (modelIdInput is not null && modelIdInput.Length > 0)
                {
                    if (int.TryParse(modelIdInput, out int modelId))
                    {
                        item.ModelId = modelId;
                    }
                    else
                    {
                        var model = PrepareCreateCarModel(client);
                        model.ErrorText = $"Error: Incorrect model id";
                        return base.View("Create", model);
                    }
                }

                var response = client.AddFrom(ApiDictionary.CarApi, item);

                if (response is null) return ReturnError("Request error");
                if (!response.IsSuccess)
                {
                    var model = PrepareCreateCarModel(client);
                    model.ErrorText = response.Message;
                    return base.View("Create", model);
                }

                return base.RedirectToAction("Index");
            }
        }

        private IActionResult ReturnError(string? errorText)
        {
            ViewData["ErrorText"] = errorText;
            return View("Views/Shared/ErrorView.cshtml");
        }

        private DCarLinked MapCarToLinks(DCar item, WebApiUtils.HttpClient client)
        {
            var result = new DCarLinked
            {
                Id = item.Id,
                Name = item.Name,
                Brand = null,
                Model = null,
            };

            if (item.BrandId is not null)
            {
                result.Brand = GetCarPart(client, ApiDictionary.BrandApi, item.Id, (int)item.BrandId, "Brand");
            }

            if (item.ModelId is not null)
            {
                result.Model = GetCarPart(client, ApiDictionary.ModelApi, item.Id, (int)item.ModelId, "Model");
            }

            return result;
        }

        private DEntityWithName GetCarPart(WebApiUtils.HttpClient client, BaseApiMethods api, int itemId, int partId, string name)
        {
            var part = client.GetByIdFrom<DEntityWithName>(api, partId);

            if (part is null) throw new Exception($"Request error for Car (id: {itemId}) {name} (id: {partId})");
            if (!part.IsSuccess) throw new Exception($"Request error for Car (id: {itemId}) {name} (id: {partId})");
            if (part.Data is null) throw new Exception($"{name} (id: {partId}) for Car (id: {itemId}) not exists");

            return part.Data;
        }

        private CreateCarModel PrepareCreateCarModel(WebApiUtils.HttpClient client)
        {
            var Brands = client.GetAllFrom<DEntityWithName>(ApiDictionary.BrandApi);
            var Models = client.GetAllFrom<DEntityWithName>(ApiDictionary.ModelApi);

            return new CreateCarModel
            {
                Brands = Brands.Data,
                Models = Models.Data,
            };
        }
    }
}
