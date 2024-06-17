using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Enums;
using Ruddy.WEB.Models;
using Ruddy.WEB.Services;
using Ruddy.WEB.ViewModels;

namespace Ruddy.WEB.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMapper _mapper;
        private readonly IMediaStorageService _mediaStorageService;

        public RestaurantsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IMapper mapper, IMediaStorageService mediaStorageService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _mapper = mapper;
            _mediaStorageService = mediaStorageService;
        }

        // GET: api/Restaurants
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<Restaurant>>> GetRestaurants()
        {

            return await _context.Restaurants.Where(x => x.adminRestaurantIncludeMenu && (x.Latitude != 0 || x.Longitude != 0)).Include(x => x.Times).ThenInclude(x => x.Pauses).AsSplitQuery().AsNoTracking().ToListAsync();

        }
        [HttpGet("{id}")]
        public async Task<ActionResult<List<SubCategory>>> GetRestaurantMenuById(int id)
        {
            var menu = (await _context.Restaurants
                .Include(d => d.Menu)
                    .ThenInclude(p => p.DishCategories)
                        .ThenInclude(h => h.Dish)
                            .ThenInclude(g => g.Components)
                                .ThenInclude(q => q.Ingredient)
                 .Include(d => d.Menu)
                    .ThenInclude(p => p.DishCategories)
                        .ThenInclude(h => h.Dish)
                            .ThenInclude(g => g.DietaryType).AsSplitQuery()
                            .FirstOrDefaultAsync(r => r.Id == id).ConfigureAwait(false)).Menu;

            if (menu == null)
            {
                return NotFound();
            }

            return menu;
        }

        [HttpGet("All/{id}")]
        public async Task<ActionResult<Restaurant>> GetRestaurantById(int id)
        {
            var menu = await _context.Restaurants.Include(t => t.Times).ThenInclude(t => t.Pauses).FirstOrDefaultAsync(r => r.Id == id);

            if (menu == null)
            {
                return NotFound();
            }

            return menu;
        }

        [HttpGet("search")]
        public async Task<ActionResult<Restaurant>> GetRestaurantById([FromQuery] DishType? dishType, [FromQuery] Dietary[] dietaries, [FromQuery] int? calories, [FromQuery] bool? IsPromo, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                bool isprom = (bool)(IsPromo != null ? IsPromo : false);
                if (dishType == null && dietaries.Length <= 0 && calories == null)
                {
                    if (isprom)
                    {
                        List<int?> dietaryParams0 = new List<int?>();
                        for (int i = 0; i < 7; i++)
                        {
                            dietaryParams0.Add(null);
                        }
                        List<SPModel> sPModels0 = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", null, dietaryParams0[0], dietaryParams0[1], dietaryParams0[2], dietaryParams0[3], dietaryParams0[4], dietaryParams0[5], dietaryParams0[6], null, isprom).ToListAsync();
                        List<int?> RestaurantFinalIds0 = sPModels0.Select(x => x.RestaurantId).ToList();
                        List<Restaurant> restaurants00 = _context.Restaurants
                        .Include(x => x.Times)
                        .ThenInclude(x => x.Pauses)
                        .Where(x => RestaurantFinalIds0.Contains(x.Id)).AsSplitQuery().ToList();
                        if (restaurants00 == null)
                        {
                            return NotFound();
                        }
                        return Ok(restaurants00);
                    }
                    else
                    {
                        List<Restaurant> restaurants0 = await _context.Restaurants.Where(x => x.adminRestaurantIncludeMenu && (x.Latitude != 0 || x.Longitude != 0)).Include(x => x.Times).ThenInclude(x => x.Pauses).AsSplitQuery().AsNoTracking().ToListAsync();
                        return Ok(restaurants0);
                    }
                }
                else
                {
                    if (dietaries.Length > 0)
                    {
                        if (dishType != null && (int)dishType >= 0)
                        {
                            if (calories != null && calories != 0)
                            {
                                List<int?> dietaryParams = new List<int?>();
                                for (int i = 0; i < 7; i++)
                                {
                                    if (dietaries.Count() <= i)
                                    {
                                        dietaryParams.Add(-1);
                                    }
                                    else
                                    {
                                        dietaryParams.Add((int)dietaries[i]);
                                    }
                                }
                                if (isprom)
                                {
                                    List<SPModel> sPModels = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", calories, dietaryParams[0], dietaryParams[1], dietaryParams[2], dietaryParams[3], dietaryParams[4], dietaryParams[5], dietaryParams[6], dishType, isprom).ToListAsync();
                                    List<int?> RestaurantFinalIds = sPModels.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurants == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants);
                                }
                                else
                                {
                                    List<SPModel> sPModelsN = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", calories, dietaryParams[0], dietaryParams[1], dietaryParams[2], dietaryParams[3], dietaryParams[4], dietaryParams[5], dietaryParams[6], dishType, null).ToListAsync();
                                    List<int?> RestaurantFinalIdsN = sPModelsN.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurantsN = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIdsN.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurantsN == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurantsN);
                                }

                            }
                            else
                            {
                                List<int?> dietaryParams1 = new List<int?>();
                                for (int i = 0; i < 7; i++)
                                {
                                    if (dietaries.Count() <= i)
                                    {
                                        dietaryParams1.Add(-1);
                                    }
                                    else
                                    {
                                        dietaryParams1.Add((int)dietaries[i]);
                                    }
                                }
                                if (isprom)
                                {
                                    List<SPModel> sPModels1 = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", null, dietaryParams1[0], dietaryParams1[1], dietaryParams1[2], dietaryParams1[3], dietaryParams1[4], dietaryParams1[5], dietaryParams1[6], dishType, isprom).ToListAsync();
                                    List<int?> RestaurantFinalIds1 = sPModels1.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants1 = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds1.Contains(x.Id)).AsSplitQuery().ToList(); ;
                                    if (restaurants1 == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants1);
                                }
                                else
                                {
                                    List<SPModel> sPModels1N = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", null, dietaryParams1[0], dietaryParams1[1], dietaryParams1[2], dietaryParams1[3], dietaryParams1[4], dietaryParams1[5], dietaryParams1[6], dishType, null).ToListAsync();
                                    List<int?> RestaurantFinalIds1N = sPModels1N.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants1N = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds1N.Contains(x.Id)).AsSplitQuery().ToList(); ;
                                    if (restaurants1N == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants1N);
                                }
                            }
                        }
                        else
                        {
                            if (calories != null && calories != 0)
                            {
                                List<int?> dietaryParams2 = new List<int?>();
                                for (int i = 0; i < 7; i++)
                                {
                                    if (dietaries.Count() <= i)
                                    {
                                        dietaryParams2.Add(-1);
                                    }
                                    else
                                    {
                                        dietaryParams2.Add((int)dietaries[i]);
                                    }
                                }
                                if (isprom)
                                {
                                    List<SPModel> sPModels2 = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", calories, dietaryParams2[0], dietaryParams2[1], dietaryParams2[2], dietaryParams2[3], dietaryParams2[4], dietaryParams2[5], dietaryParams2[6], null, isprom).ToListAsync();
                                    List<int?> RestaurantFinalIds2 = sPModels2.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants2 = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds2.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurants2 == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants2);
                                }
                                else
                                {
                                    List<SPModel> sPModels2N = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", calories, dietaryParams2[0], dietaryParams2[1], dietaryParams2[2], dietaryParams2[3], dietaryParams2[4], dietaryParams2[5], dietaryParams2[6], null, null).ToListAsync();
                                    List<int?> RestaurantFinalIds2N = sPModels2N.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants2N = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds2N.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurants2N == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants2N);
                                }
                            }
                            else
                            {
                                List<int?> dietaryParams3 = new List<int?>();
                                for (int i = 0; i < 7; i++)
                                {
                                    if (dietaries.Count() <= i)
                                    {
                                        dietaryParams3.Add(-1);
                                    }
                                    else
                                    {
                                        dietaryParams3.Add((int)dietaries[i]);
                                    }
                                }
                                if (isprom)
                                {
                                    List<SPModel> sPModels3 = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", null, dietaryParams3[0], dietaryParams3[1], dietaryParams3[2], dietaryParams3[3], dietaryParams3[4], dietaryParams3[5], dietaryParams3[6], null, isprom).ToListAsync();
                                    List<int?> RestaurantFinalIds3 = sPModels3.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants3 = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds3.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurants3 == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants3);
                                }
                                else
                                {
                                    List<SPModel> sPModels3N = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", null, dietaryParams3[0], dietaryParams3[1], dietaryParams3[2], dietaryParams3[3], dietaryParams3[4], dietaryParams3[5], dietaryParams3[6], null, null).ToListAsync();
                                    List<int?> RestaurantFinalIds3N = sPModels3N.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants3N = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds3N.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurants3N == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants3N);
                                }
                            }
                        }
                    }
                    else
                    {
                        if (dishType != null && (int)dishType >= 0)
                        {
                            if (calories != null && calories != 0)
                            {
                                List<int?> dietaryParams4 = new List<int?>();
                                for (int i = 0; i < 7; i++)
                                {

                                    dietaryParams4.Add(null);
                                }
                                if (isprom)
                                {
                                    List<SPModel> sPModels4 = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", calories, dietaryParams4[0], dietaryParams4[1], dietaryParams4[2], dietaryParams4[3], dietaryParams4[4], dietaryParams4[5], dietaryParams4[6], dishType, isprom).ToListAsync();
                                    List<int?> RestaurantFinalIds4 = sPModels4.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants4 = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds4.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurants4 == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants4);
                                }
                                else
                                {
                                    List<SPModel> sPModels4N = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", calories, dietaryParams4[0], dietaryParams4[1], dietaryParams4[2], dietaryParams4[3], dietaryParams4[4], dietaryParams4[5], dietaryParams4[6], dishType, null).ToListAsync();
                                    List<int?> RestaurantFinalIds4N = sPModels4N.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants4N = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds4N.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurants4N == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants4N);
                                }
                            }
                            else
                            {
                                List<int?> dietaryParams5 = new List<int?>();
                                for (int i = 0; i < 7; i++)
                                {

                                    dietaryParams5.Add(null);
                                }
                                if (isprom)
                                {
                                    List<SPModel> sPModels5 = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", null, dietaryParams5[0], dietaryParams5[1], dietaryParams5[2], dietaryParams5[3], dietaryParams5[4], dietaryParams5[5], dietaryParams5[6], dishType, isprom).ToListAsync();
                                    List<int?> RestaurantFinalIds5 = sPModels5.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants5 = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds5.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurants5 == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants5);
                                }
                                else
                                {
                                    List<SPModel> sPModels5N = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", null, dietaryParams5[0], dietaryParams5[1], dietaryParams5[2], dietaryParams5[3], dietaryParams5[4], dietaryParams5[5], dietaryParams5[6], dishType, null).ToListAsync();
                                    List<int?> RestaurantFinalIds5N = sPModels5N.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants5N = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds5N.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurants5N == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants5N);
                                }
                            }
                        }
                        else
                        {
                            if (calories != null && calories != 0)
                            {
                                List<int?> dietaryParams6 = new List<int?>();
                                for (int i = 0; i < 7; i++)
                                {

                                    dietaryParams6.Add(null);
                                }
                                if (isprom)
                                {
                                    List<SPModel> sPModels6 = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", calories, dietaryParams6[0], dietaryParams6[1], dietaryParams6[2], dietaryParams6[3], dietaryParams6[4], dietaryParams6[5], dietaryParams6[6], null, isprom).ToListAsync();
                                    List<int?> RestaurantFinalIds6 = sPModels6.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants6 = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds6.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurants6 == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants6);
                                }
                                else
                                {
                                    List<SPModel> sPModels6N = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", calories, dietaryParams6[0], dietaryParams6[1], dietaryParams6[2], dietaryParams6[3], dietaryParams6[4], dietaryParams6[5], dietaryParams6[6], null, null).ToListAsync();
                                    List<int?> RestaurantFinalIds6N = sPModels6N.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants6N = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds6N.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurants6N == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants6N);
                                }
                            }
                            else
                            {
                                if (isprom)
                                {
                                    List<int?> dietaryParams7 = new List<int?>();
                                    for (int i = 0; i < 7; i++)
                                    {
                                        dietaryParams7.Add(null);
                                    }
                                    List<SPModel> sPModels7 = await _context.SpModels.FromSqlRaw("exec getCalories @calory={0},@d1={1},@d2={2},@d3={3},@d4={4},@d5={5},@d6={6},@d7={7},@dishType={8},@ispromo={9}", null, dietaryParams7[0], dietaryParams7[1], dietaryParams7[2], dietaryParams7[3], dietaryParams7[4], dietaryParams7[5], dietaryParams7[6], null, isprom).ToListAsync();
                                    List<int?> RestaurantFinalIds7 = sPModels7.Select(x => x.RestaurantId).ToList();
                                    List<Restaurant> restaurants7 = _context.Restaurants
                                    .Include(x => x.Times)
                                    .ThenInclude(x => x.Pauses)
                                    .Where(x => RestaurantFinalIds7.Contains(x.Id)).AsSplitQuery().ToList();
                                    if (restaurants7 == null)
                                    {
                                        return NotFound();
                                    }
                                    return Ok(restaurants7);
                                }
                                else
                                {
                                    List<Restaurant> restaurants7N = await _context.Restaurants.Where(x => x.adminRestaurantIncludeMenu && (x.Latitude != 0 || x.Longitude != 0)).Include(x => x.Times).ThenInclude(x => x.Pauses).AsSplitQuery().AsNoTracking().ToListAsync();
                                    return Ok(restaurants7N);
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception)
            {
                List<Restaurant> ErrorRestaurants = new List<Restaurant>();

                return Ok(ErrorRestaurants);
            }
        }

        // PUT: api/Restaurants/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> PutRestaurant(int id, RestaurantViewModel restaurant)
        {
            var newRestaurant = await _context.Restaurants.Include(h => h.Times).FirstOrDefaultAsync(u => u.Id == id);
            if (restaurant.Times.Count != 0)
            {
                _context.Times.RemoveRange(newRestaurant.Times);
                _context.SaveChanges();
                _context.Times.AddRange(restaurant.Times);
                _context.SaveChanges();
            }
            newRestaurant.Name = restaurant.Name;
            newRestaurant.Address = restaurant.Address;
            newRestaurant.FirstPhoneNumber = restaurant.FirstPhoneNumber;
            newRestaurant.SecondPhoneNumber = restaurant.SecondPhoneNumber;
            newRestaurant.Longitude = restaurant.Longitude;
            newRestaurant.Latitude = restaurant.Latitude;
            newRestaurant.RestaurantCategory = restaurant.RestaurantCategory;
            newRestaurant.Description = restaurant.Description;
            newRestaurant.VAT = restaurant.VAT;
            newRestaurant.Facebook = restaurant.Facebook;
            newRestaurant.Instagram = restaurant.Instagram;
            newRestaurant.Whatsapp = restaurant.Whatsapp;
            newRestaurant.Twitter = restaurant.Twitter;
            newRestaurant.Website = restaurant.Website;
            newRestaurant.Mail = restaurant.Mail;
            newRestaurant.Times = restaurant.Times;

            _context.Restaurants.Update(newRestaurant);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RestaurantExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Restaurants
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Restaurant>> PostRestaurant(RestaurantViewModel model)
        {
            var restaurantUser = await _context.RestaurantUsers.Include(r => r.Restaurants).FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            var restaurant = _mapper.Map<Restaurant>(model);

            restaurant.RestaurantUserId = restaurantUser.Id;

            _context.Restaurants.Add(restaurant);
            try
            {
                await _context.SaveChangesAsync();
                //restaurantUser.Restaurants.Add(restaurant);

                // тawait _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (RestaurantExists(restaurant.Id))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetRestaurant", new { id = restaurant.Id }, restaurant);
        }

        [HttpPost("uploadimagesecond/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> UploadImageLogo(IFormFile image, [FromRoute] int id)
        {
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(u => u.Id == id);

            if (image != null)
            {
                restaurant.Logo = await _mediaStorageService.SaveMediaAsync(image);
                _context.Restaurants.Update(restaurant);
                await _context.SaveChangesAsync();
            }
            else
            {
                return BadRequest();
            }
            return Ok();
        }
        [HttpPost("uploadimagefirst/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> UploadImageBackground(IFormFile image, [FromRoute] int id)
        {
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(u => u.Id == id);

            if (image != null)
            {
                restaurant.Background = await _mediaStorageService.SaveMediaAsync(image);
                _context.Restaurants.Update(restaurant);
                await _context.SaveChangesAsync();
            }
            else
            {
                return BadRequest();
            }
            return Ok();
        }
        // DELETE: api/Restaurants/5
        [HttpDelete("{id}")]
        // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<Restaurant>> DeleteRestaurant(int id)
        {
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(y => y.Id == id);

            await _context.Dishes.Where(y => y.RestaurantId == id)
            .ForEachAsync(d => d.RestaurantId = null);
            await _context.Orders.Where(y => y.RestaurantId == id).ForEachAsync(d => d.RestaurantId = null);
            await _context.SubCategories.Where(y => y.RestaurantId == id).ForEachAsync(d => d.RestaurantId = null);
            await _context.Times.Where(y => y.RestaurantId == id).ForEachAsync(d => d.RestaurantId = null);
            //restaurant.Menu
            await _context.SaveChangesAsync();

            if (restaurant == null)
            {
                return NotFound();
            }

            _context.Restaurants.Remove(restaurant);
            await _context.SaveChangesAsync();

            return restaurant;
        }
        // DELETE: api/Restaurants
        [AllowAnonymous]
        [HttpDelete]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> DeleteAllRestaurants()
        {
            var restaurants = _context.Restaurants
    .Include(g => g.Times)
    .Include(h => h.Menu);
            _context.Restaurants.RemoveRange(restaurants);
            await _context.SaveChangesAsync();

            return Ok();
        }
        private bool RestaurantExists(int id)
        {
            return _context.Restaurants.Any(e => e.Id == id);
        }
    }
}
