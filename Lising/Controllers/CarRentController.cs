using Azure.Core;
using Lising.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApiUtils;
using WebApiUtils.ApiAddresses;
using WebApiUtils.Entities;
using WebApiUtils.Rabbit;

namespace Lising.Controllers
{
    [Authorize]
    [Route("/rent")]
    public class CarRentController : Controller
    {
        public IActionResult Index()
        {
            using (var client = new WebApiUtils.HttpClient())
            {
                try
                {
                    var rents = client.GetAllFrom<DCarRent>(ApiDictionary.CarRentApi);
                    var items = rents.Data.Select(item => MapRentToLinks(item, client)).ToArray();
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
            using (var client = new WebApiUtils.HttpClient())
            {
                var model = PrepareCreateRentModel(client);
                return base.View("Create", model);
            }
        }

        [HttpPost]
        [Route("create")]
        public IActionResult CreatePost()
        {
            var carIdInput = Request.Form["carId"];
            var clientIdInput = Request.Form["clientId"];
            var salerIdInput = Request.Form["salerId"];

            var rentDateInput = Request.Form["rentDate"];
            var rentDaysInput = Request.Form["rentDays"];

            using (var client = new WebApiUtils.HttpClient())
            {
                var item = new DCarRent();

                if (DateTime.TryParse(rentDateInput, out DateTime rentDate))
                {
                    item.OpenDate = rentDate;
                }
                else
                {
                    return CreateWithError(client, "Error: Incorrect open rent date");
                }

                if (int.TryParse(rentDaysInput, out int rentDays))
                {
                    item.RentDays = rentDays;
                }
                else
                {
                    return CreateWithError(client, "Error: Incorrect rent days");
                }

                if (int.TryParse(carIdInput, out int carId))
                {
                    item.CarId = carId;
                }
                else
                {
                    return CreateWithError(client, "Error: Incorrect car id");
                }

                if (int.TryParse(clientIdInput, out int clientId))
                {
                    item.ClientId = clientId;
                }
                else
                {
                    return CreateWithError(client, "Error: Incorrect client id");
                }

                if (int.TryParse(salerIdInput, out int salerId))
                {
                    item.SalerId = salerId;
                }
                else
                {
                    return CreateWithError(client, "Error: Incorrect saler id");
                }

                var response = client.AddFrom(ApiDictionary.CarRentApi, item);
                if (response is null) return ReturnError("Request error");
                if (!response.IsSuccess)
                {
                    return CreateWithError(client, response.Message);
                }

                return base.RedirectToAction("Index");
            }
        }

        [HttpGet]
        [Route("close/{id}")]
        public IActionResult Close(int id)
        {
            using (var client = new WebApiUtils.HttpClient())
            {
                try
                {
                    var model = PrepareCloseRentModel(client, id);
                    if (model.Rent is null)
                    {
                        model.ErrorText = "Error: Incorrect rent id";
                    }
                    return base.View("Close", model);
                }
                catch (Exception ex)
                {
                    return ReturnError(ex.Message);
                }
            }
        }

        [HttpPost]
        [Route("calculate/{id}")]
        public IActionResult CloseCalculate(int id)
        {
            using (var client = new WebApiUtils.HttpClient())
            {
                try
                {
                    var model = PrepareCloseRentModel(client, id);
                    if (model.Rent is null)
                    {
                        model.ErrorText = "Error: Incorrect rent id";
                        return base.View("Close", model);
                    }

                    var rentEndDateInput = Request.Form["rentEndDate"];
                    var penaltyInput = Request.Form["penalty"];

                    if (DateTime.TryParse(rentEndDateInput, out DateTime rentEndDate))
                    {
                        model.EndDate = rentEndDate;
                    }
                    else
                    {
                        model.ErrorText = "Error: Incorrect rent end date";
                        return base.View("Close", model);
                    }

                    if (int.TryParse(penaltyInput, out int penaltyByDay))
                    {
                        model.PenaltyByDay = penaltyByDay;
                    }
                    else
                    {
                        model.ErrorText = "Error: Incorrect penalty";
                        return base.View("Close", model);
                    }

                    var response = client.CreateRequest()
                        .SetMethodGet()
                        .SetUri($"{ApiDictionary.CarRentApi.Calculate}?rentId={id}&closeDate={rentEndDate}&penaltyByDay={penaltyByDay}")
                        .SendAsync().Result.Content
                        .ReadFromJsonAsync(typeof(DResponse<DCarRent>)).Result as DResponse<DCarRent>;

                    if (response is null) throw new Exception($"Request error for rent (id: {id}) calculation");
                    else if (!response.IsSuccess) model.ErrorText = response.Message;
                    else if (response.Data is null) throw new Exception($"Request error for rent (id: {id}) calculation");

                    model.Rent = MapRentToLinks(response.Data, client);
                    model.EndDate = model.Rent.CloseDate;
                    model.PenaltyByDay = penaltyByDay;
                    model.TotalPenalty = model.Rent.Penalty;

                    return base.View("Close", model);
                }
                catch (Exception ex)
                {
                    return ReturnError(ex.Message);
                }
            }
        }

        [HttpPost]
        [Route("close/{id}")]
        public IActionResult ClosePost(int id)
        {
            using (var client = new WebApiUtils.HttpClient())
            {
                var model = PrepareCloseRentModel(client, id);
                if (model.Rent is null)
                {
                    model.ErrorText = "Error: Incorrect rent id";
                    return base.View("Close", model);
                }

                var rentEndDateInput = Request.Form["rentEndDate"];
                var penaltyInput = Request.Form["penalty"];

                if (!DateTime.TryParse(rentEndDateInput, out DateTime rentEndDate))
                {
                    model.ErrorText = "Error: Incorrect rent end date";
                    return base.View("Close", model);
                }
                else
                {
                    model.Rent.CloseDate = rentEndDate;
                }

                if (!int.TryParse(penaltyInput, out int penalty))
                {
                    model.ErrorText = "Error: Incorrect penalty";
                    return base.View("Close", model);
                }
                else
                {
                    model.Rent.Penalty = penalty;
                }

                var sender = new RabbitSender(RabbitConfig.CloseRentQueue);
                sender.SendMessage(model.Rent);

                return base.RedirectToAction("Index");
            }
        }

        private IActionResult ReturnError(string? errorText)
        {
            ViewData["ErrorText"] = errorText;
            return View("Views/Shared/ErrorView.cshtml");
        }

        private DCarRentLinked MapRentToLinks(DCarRent item, WebApiUtils.HttpClient client)
        {
            if (item is null) return null;

            var result = new DCarRentLinked
            {
                Id = item.Id,
                Car = null,
                Client = null,
                Saler = null,
                OpenDate = item.OpenDate,
                RentDays = item.RentDays,
                CloseDate = item.CloseDate,
                Penalty = item.Penalty,
            };

            result.Car = GetCarPart<DCar>(client, ApiDictionary.CarApi, item.Id, item.CarId, "Car");
            result.Client = GetCarPart<DEntityWithName>(client, ApiDictionary.ClientApi, item.Id, item.ClientId, "Client");
            result.Saler = GetCarPart<DEntityWithName>(client, ApiDictionary.SalerApi, item.Id, item.SalerId, "Saler");

            return result;
        }

        private T GetCarPart<T>(WebApiUtils.HttpClient client, BaseApiMethods api, int itemId, int partId, string name)
            where T : class
        {
            var part = client.GetByIdFrom<T>(api, partId);

            if (part is null) throw new Exception($"Request error for rent (id: {itemId}) {name} (id: {partId})");
            if (!part.IsSuccess) throw new Exception($"Request error for rent (id: {itemId}) {name} (id: {partId})");
            if (part.Data is null) throw new Exception($"{name} (id: {partId}) for rent (id: {itemId}) not exists");

            return part.Data;
        }

        private CreateRentModel PrepareCreateRentModel(WebApiUtils.HttpClient client)
        {
            var cars = client.GetAllFrom<DCar>(ApiDictionary.CarApi);
            var clients = client.GetAllFrom<DEntityWithName>(ApiDictionary.ClientApi);
            var salers = client.GetAllFrom<DEntityWithName>(ApiDictionary.SalerApi);

            return new CreateRentModel
            {
                Cars = cars.Data,
                Clients = clients.Data,
                Salers = salers.Data
            };
        }

        private IActionResult CreateWithError(WebApiUtils.HttpClient client, string message)
        {
            var model = PrepareCreateRentModel(client);
            model.ErrorText = message;
            return View("Create", model);
        }

        private CloseRentModel PrepareCloseRentModel(WebApiUtils.HttpClient client, int id)
        {
            var rent = client.GetByIdFrom<DCarRent>(ApiDictionary.CarRentApi, id);
            return new CloseRentModel
            {
                RentId = id,
                Rent = MapRentToLinks(rent.Data, client),
            };
        }
    }
}
