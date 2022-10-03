using BusinessObject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Repository.Interfaces;
using Repository.Models.Stores;
using System.ComponentModel.DataAnnotations;
using System.Net.Mime;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly IStoreRepository _storeReposiotry;

        public StoresController(IStoreRepository storeReposiotry)
        {
            _storeReposiotry = storeReposiotry;
        }

        [HttpGet]
        [Authorize]
        public ActionResult<IEnumerable<AdminDashboardStore>> GetStores(string? search, string? order_by, string? filter, [Range(1, int.MaxValue)] int size = 10, [Range(1, int.MaxValue)] int page = 1)
        {
            var result = new AdminDashboardStorePage();
            result.Stores = _storeReposiotry.GetStores(search, order_by, filter, size, page);
            result.TotalPage = (int)Math.Ceiling((decimal)_storeReposiotry.CountDashboardStores(search, order_by, filter) / size);
            return Ok(result);
        }

        [HttpGet("{guid}")]
        [Authorize]
        public async Task<ActionResult<Store>> GetStoresById(Guid guid)
        {
            var result = await _storeReposiotry.GetById(guid);
            return Ok(result);
        }

        [HttpPost]
        [Consumes(MediaTypeNames.Application.Json)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize]
        public ActionResult<Store> PostStore(Store Store)
        {
            Guid id = Guid.NewGuid();
            Store prod = new Store()
            {
                Address = Store.Address,
                Description = Store.Description,
                Id = Store.Id,
                PhotoUrl = Store.PhotoUrl,
                Name = Store.Name,
                UserId = Store.UserId,
                Rating = Store.Rating,
                Status = Store.Status,
            };
            try
            {
                _storeReposiotry.Add(prod);
            }
            catch (DbUpdateException)
            {
                if (_storeReposiotry.GetById(prod.Id!.Value) != null)
                    return Conflict();
            }
            return Ok(prod);
        }

        [HttpDelete("{guid}")]
        [Authorize]
        public async Task<ActionResult> DeleteStore(Guid? guid)
        {
            Store? store = await _storeReposiotry.Delete(guid);
            return Ok("Delete Store " + store!.Name + " success");
        }

        [HttpPut("{guid}")]
        [Authorize]
        public async Task<ActionResult> PutStore(Guid guid, Store Store)
        {
            try
            {
                if (guid != Store.Id) return BadRequest();
                Store? beforeUpdateObj = await _storeReposiotry.GetById(Store.Id.Value);
                if (beforeUpdateObj == null) throw new Exception("Store that need to update does not exist");
                Store updateObj = new Store()
                {
                    Address = Store.Address == null ? beforeUpdateObj.Address : Store.Address,
                    Description = Store.Description == null ? beforeUpdateObj.Description : Store.Description,
                    Id = Store.Id == null ? beforeUpdateObj.Id : Store.Id,
                    PhotoUrl = Store.PhotoUrl == null ? beforeUpdateObj.PhotoUrl : Store.PhotoUrl,
                    Name = Store.Name == null ? beforeUpdateObj.Name : Store.Name,
                    UserId = Store.UserId == null ? beforeUpdateObj.UserId : Store.UserId,
                    Rating = Store.Rating == null ? beforeUpdateObj.Rating : Store.Rating,
                    Status = Store.Status == null ? beforeUpdateObj.Status : Store.Status,
                };
                await _storeReposiotry.Update(updateObj);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (_storeReposiotry.GetById(guid) == null)
                {
                    return NotFound();
                }
                throw;
            }
            return NoContent();
        }
    }
}
