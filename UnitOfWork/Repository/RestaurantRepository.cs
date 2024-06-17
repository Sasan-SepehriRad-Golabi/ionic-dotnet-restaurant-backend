using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Models;

namespace Ruddy.WEB.UnitOfWork.Repository
{
    public class RestaurantRepository
    {
        private ApplicationDbContext _myappcontext;
        public RestaurantRepository(ApplicationDbContext myappcontext)
        {
            _myappcontext = myappcontext;
        }
        public async Task<List<Restaurant>> getAll()
        {
            return await _myappcontext.Restaurants.Include(x => x.Times).ToListAsync();
        }
        public void addRestaurant(Restaurant resaurant)
        {
            _myappcontext.Set<Restaurant>().Add(resaurant);
        }
        public async Task addRange(List<Restaurant> restaurants)
        {
            await _myappcontext.Set<Restaurant>().AddRangeAsync(restaurants);
        }
        public void deleteRestaurant(Restaurant resaurant)
        {
            _myappcontext.Set<Restaurant>().Remove(resaurant);
        }

    }
}