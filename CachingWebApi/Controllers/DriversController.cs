using CachingWebApi.Data;
using CachingWebApi.Models;
using CachingWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CachingWebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly ApplicationDataContext _context;
        private readonly ICacheService _cacheService;

        public DriversController(ApplicationDataContext context, ICacheService cacheService)
        {
            _context = context;
            _cacheService = cacheService;
        }

        [HttpGet("get-all-drivers")]
        public async Task<IActionResult> Get()
        {
            //проверям кэш
            var cacheData = _cacheService.GetData<List<Driver>>("drivers");
            
            if(cacheData != null && cacheData.Count > 0)
            {
                return Ok(cacheData);
            }
            await Task.Delay(3000);
            cacheData = await _context.Drivers.ToListAsync();

            //устанавливаем время жизни кэша
            var expiry = DateTimeOffset.Now.AddSeconds(120);

            //добавляем в кэш
            _cacheService.SetData<List<Driver>>("drivers",cacheData,expiry);

            return Ok(cacheData);
        }

        [HttpPost("add-new-driver")]
        public async Task<IActionResult> Post(Driver value)
        {
            var addedObj = await _context.Drivers.AddAsync(value);
            var expiry = DateTimeOffset.Now.AddSeconds(30);

            _cacheService.SetData<Driver>($"driver{value.Id}", addedObj.Entity,expiry);

            await _context.SaveChangesAsync();

            return Ok(addedObj.Entity);
        }

        [HttpDelete("delete-driver")]
        public async Task<IActionResult> Delete(int id)
        {
            var exist = await _context.Drivers.FirstOrDefaultAsync(x=>x.Id == id);

            if (exist != null)
            {
                _context.Remove(exist);
                _cacheService.RemoveData($"driver{id}");
                await _context.SaveChangesAsync();

                return NoContent();
            }

            return NotFound();
        }

    }
}
