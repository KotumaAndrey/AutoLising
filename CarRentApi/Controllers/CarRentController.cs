using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using WebApiUtils;
using WebApiUtils.ApiAddresses;
using WebApiUtils.BaseApi;
using WebApiUtils.Entities;

namespace CarRentApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class CarRentController : BaseApiController<DCarRent, BaseRepository<DCarRent>>
    {
        protected override string connectionString => Environment.GetEnvironmentVariable("ConnectionString")!;
        protected override BaseRepository<DCarRent> repository => new BaseRepository<DCarRent>(connectionString);

        public override DResponse<DCarRent> Add(DCarRent item)
        {
            using (var client = new WebApiUtils.HttpClient())
            {
                var valudationResult = ValidateRent(item, client);
                if (valudationResult is not null) 
                    return valudationResult;

                return base.Add(item);
            }
        }

        public override DResponse<DCarRent> Update(DCarRent item)
        {
            using (var client = new WebApiUtils.HttpClient())
            {
                var valudationResult = ValidateRent(item, client);
                if (valudationResult is not null) 
                    return valudationResult;

                return base.Update(item);
            }
        }

        private DResponse<DCarRent>? ValidateRent(DCarRent item, WebApiUtils.HttpClient client)
        {
            var CarResponce = CheckOtherObjectExists(item.CarId, client, ApiDictionary.CarApi.GetById, "Car", item);
            if (CarResponce is not null) 
                return CarResponce;

            var clientResponce = CheckOtherObjectExists(item.ClientId, client, ApiDictionary.ClientApi.GetById, "Client", item);
            if (clientResponce is not null) 
                return clientResponce;

            var salerResponce = CheckOtherObjectExists(item.SalerId, client, ApiDictionary.SalerApi.GetById, "Saler", item);
            if (salerResponce is not null) 
                return salerResponce;

            if (item.RentDays <= 0)
            {
                return DResponse<DCarRent>.Error("RentDays must be positive", item);
            }

            if (item.CloseDate is not null)
            {
                if (item.CloseDate < item.OpenDate)
                {
                    return DResponse<DCarRent>.Error("CloseDate must be greater then OpenDate", item);
                }

                if (item.Penalty is null) 
                    return DResponse<DCarRent>.Error("Penalty must be not null", item);
            }

            if (item.Penalty is not null)
            {
                if (item.Penalty < 0)
                {
                    return DResponse<DCarRent>.Error("Penalty must be greater or equal than 0", item);
                }

                if (item.CloseDate is null) 
                    return DResponse<DCarRent>.Error("CloseDate must be not null", item);
            }

            return null;
        }

        [HttpGet("/calculate")]
        public DResponse<DCarRent> CalculateRentGet(int rentId, DateTime closeDate, int penaltyByDay)
        {
            var item = repository.GetById(rentId);
            if (item is null) 
                return DResponse<DCarRent>.Error($"Rent with id {rentId} not exists");
            if (item.CloseDate is not null) 
                return DResponse<DCarRent>.Error($"Rent with id {rentId} already closed", item);

            item.CloseDate = closeDate;
            if (item.CloseDate < item.OpenDate)
            {
                return DResponse<DCarRent>.Error("CloseDate must be greater then OpenDate", item);
            }

            if (penaltyByDay < 0)
            {
                return DResponse<DCarRent>.Error("Penalty must be greater or equal to 0");
            }

            var rentDays = (int)(item.CloseDate - item.OpenDate).Value.TotalDays;
            if (rentDays < item.RentDays)
            {
                item.Penalty = 0;
            }
            else
            {
                item.Penalty = (rentDays - item.RentDays) * penaltyByDay;
            }

            return DResponse<DCarRent>.Success(item);
        }

        [HttpPost("/close")]
        public DResponse<DCarRent> CloseRentPost(int rentId, DateTime closeDate, int penalty)
        {
            var item = repository.GetById(rentId);
            if (item is null) 
                return DResponse<DCarRent>.Error($"Rent with id {rentId} not exists");
            if (item.CloseDate is not null) 
                return DResponse<DCarRent>.Error($"Rent with id {rentId} already closed", item);

            item.CloseDate = closeDate;
            if (item.CloseDate < item.OpenDate)
            {
                return DResponse<DCarRent>.Error("CloseDate must be greater then OpenDate", item);
            }

            item.Penalty = penalty;
            if (penalty < 0)
            {
                return DResponse<DCarRent>.Error("Penalty must be greater or equal than 0");
            }

            return base.Update(item);
        }
    }
}
