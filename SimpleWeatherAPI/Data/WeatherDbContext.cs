using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SimpleWeatherAPI.Models.Dtos;

namespace SimpleWeatherAPI.Data
{
    public class WeatherDbContext: IdentityDbContext
    {
        public WeatherDbContext(DbContextOptions<WeatherDbContext> options) : base(options)
        {

        }

    }
}
