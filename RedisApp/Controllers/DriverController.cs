using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RedisApp.Data;
using RedisApp.Models;
using RedisApp.Services;

namespace RedisApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ICacheService _cacheService;

        public DriverController(AppDbContext db, ICacheService cacheService)
        {
            _db = db;
            _cacheService = cacheService;
        }
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            // check cache data
            var cacheData = _cacheService.GetData<IEnumerable<Driver>>("drivers");
            if (cacheData != null && cacheData.Count()  > 0)
            {
                return Ok(cacheData);
            }

            // fetch from the database
            cacheData = (IEnumerable<Driver>?)await _db.Drivers.ToListAsync();

            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            _cacheService.SetData<IEnumerable<Driver>>("drivers", cacheData, expiryTime);

            return Ok(cacheData);
        }

        [HttpPost("AddDriver")]
        public async Task<IActionResult> Post (Driver request)
        {
            var addedObj = await _db.Drivers.AddAsync(request);

            var expiryTime = DateTimeOffset.Now.AddSeconds(30);
            _cacheService.SetData<Driver>($"driver{request.Id}", addedObj.Entity, expiryTime);

            await _db.SaveChangesAsync();

            return Ok(addedObj.Entity);
        }

        [HttpDelete("DeleteDriver")]
        public async Task<IActionResult> Delete(int id)
        {
            var exist = _db.Drivers.FirstOrDefaultAsync(x => x.Id == id);

            if (exist != null)
            {
                _db.Remove(exist);
                _cacheService.RemoveData($"driver{id}");
                await _db.SaveChangesAsync();

                return NoContent();
            }

            return NotFound();

        }
    }
}
