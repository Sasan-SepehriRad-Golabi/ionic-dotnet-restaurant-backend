using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Models;
using Ruddy.WEB.ViewModels;

namespace Ruddy.WEB.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DiscountsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DiscountsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Discounts/5
        [HttpGet]
        public async Task<ActionResult<List<DiscountGetViewModel>>> GetDiscount()
        {
            var restaurantUser = await _context.RestaurantUsers.Include(m => m.Restaurants).FirstOrDefaultAsync(ru => ru.UserName == User.Identity.Name);

            var restaurantIds = restaurantUser.Restaurants.Select(r => (int?)r.Id);

            var discounts = await _context.Discounts
                .Include(d => d.DiscountDishes)
                .ThenInclude(dd => dd.Dish).ToListAsync();

            var resultDiscounts = discounts.Where(d => d.DiscountDishes.TrueForAll(dd => restaurantIds.Contains(dd.Dish.RestaurantId)))
                .Select(d => new DiscountGetViewModel()
                {
                    Id = d.Id,
                    Name = d.Name,
                    From = d.From,
                    To = d.To,
                    Percent = d.Percent,
                    Dishes = d.DiscountDishes.Select(dd => new DishInDiscount()
                    {
                        Dish = dd.Dish,
                        RestaurantId = dd.Dish.RestaurantId,
                        RestaurantName = dd.Dish.Restaurant.Name
                    }).ToList()
                }).ToList();

            resultDiscounts.ForEach(rd => rd.Dishes.ForEach(rdd =>
            {
                rdd.Dish.Restaurant = null;
                rdd.Dish.DiscountDishes = null;
            }));

            return Ok(resultDiscounts);
        }

        [HttpPost]
        public async Task<ActionResult> PostDiscount([FromBody] DiscountPostViewModel model)
        {
            var newDiscount = new Discount()
            {
                Name = model.Name,
                From = model.From,
                To = model.To,
                Percent = model.Percent,
                DiscountDishes = model.DishesId.Select(d => new DiscountDish() { DishId = d }).ToList()
            };

            if (model.From.Date <= DateTime.UtcNow.Date && model.To.Date >= DateTime.UtcNow.Date)
            {
                var dishes = _context.Dishes.Where(d => model.DishesId.Contains(d.Id)).ToList();
                foreach (var d in dishes)
                {
                    d.IsPromotional = true;
                    d.PromotionalPrice = d.Price * ((100 - model.Percent) / 100.0);
                }
            }

            _context.Discounts.Add(newDiscount);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteDiscount(int id)
        {
            var disc = await _context.Discounts.Include(d => d.DiscountDishes).FirstOrDefaultAsync(d => d.Id == id);
            var dishesIds = disc.DiscountDishes.Select(dd => dd.DishId).ToList();
            if(disc.From.Date <= DateTime.UtcNow.Date && disc.To.Date >= DateTime.UtcNow.Date)
            {
                var dishes = _context.Dishes.Where(d => dishesIds.Contains(d.Id)).ToList();
                foreach(var d in dishes)
                {
                    d.IsPromotional = false;
                    d.PromotionalPrice = default(double);
                }
            }

            _context.Discounts.Remove(disc);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
