using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Models;
using Ruddy.WEB.Services;
using Ruddy.WEB.ViewModels;

namespace Ruddy.WEB.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class DishesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IMediaStorageService _mediaStorageService;

        public DishesController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment, IMediaStorageService mediaStorageService)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _mediaStorageService = mediaStorageService;
        }

        // GET: api/Dishes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Dish>>> GetDishes()
        {
            return await _context.Dishes.ToListAsync();
        }

        // GET: api/Dishes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Dish>> GetDish(int id)
        {
            var dish = await _context.Dishes.Include(g => g.DietaryType).Include(p => p.Components).ThenInclude(c => c.Ingredient).FirstOrDefaultAsync(b => b.Id == id);

            if (dish == null)
            {
                return NotFound();
            }

            return dish;
        }

        // PUT: api/Dishes/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutDish(int id, DishViewModel dish)
        {
            Dish newDish = await _context.Dishes.Include(a => a.DietaryType).Include(d => d.Components).FirstOrDefaultAsync(g => g.Id == id);
            if (dish.DietaryType.Count != 0)
            {
                _context.DietaryTypes.RemoveRange(newDish.DietaryType);
                _context.SaveChanges();
                _context.DietaryTypes.AddRange(dish.DietaryType);
                _context.SaveChanges();
            }
            newDish.Name = dish.Name;
            newDish.Summary = dish.Summary;
            newDish.Price = dish.Price;
            newDish.PromotionalPrice = dish.PromotionalPrice;
            newDish.IsPromotional = dish.IsPromotional;
            newDish.DishType = dish.DishType;
            newDish.Description = dish.Description;
            newDish.DietaryType = dish.DietaryType;
            newDish.Weight = dish.Weight;
            newDish.IsActive = dish.IsActive;
            //_context.Dishes.Update(newDish);

            /*
            var newComponentIds = dish.Components.Where(c => c.Id != 0).Select(c => c.Id);
            await _context.DishComponents.Where(dc => newComponentIds.Contains(dc.Id)).ForEachAsync(dc =>
            {
                var newComponent = dish.Components.FirstOrDefault(newDC => newDC.Id == dc.Id);
                dc.IngredientId = newComponent.IngredientId;
                dc.DishId = newComponent.DishId;
                dc.Weight = newComponent.Weight;
                dc.Price = newComponent.Price;
                dc.IngredientType = newComponent.IngredientType;
                dc.SubstituteGroup = newComponent.SubstituteGroup;
            });

            var componentsToBeDeleted = _context.DishComponents.Where(dc => !newComponentIds.Contains(dc.Id) && dc.DishId == id);
            var componentsToBeDeletedIds = await componentsToBeDeleted.Select(ctbd => new int?(ctbd.Id)).ToListAsync();

            var orderedComponentsToBeDeleted = await _context.OrderedIngredients.Where(oi => componentsToBeDeletedIds.Contains(oi.DishComponentId)).ToListAsync();

            _context.OrderedIngredients.RemoveRange(orderedComponentsToBeDeleted);
            await _context.SaveChangesAsync();

            _context.DishComponents.RemoveRange(componentsToBeDeleted);

            await _context.DishComponents.AddRangeAsync(dish.Components.Where(c => c.Id == 0));
            */

            dish.Components.ForEach(c => c.Id = 0);

            _context.DishComponents.RemoveRange(newDish.Components);

            foreach(var d in dish.Components)
            {
                _context.DishComponents.Add(d);
                await _context.SaveChangesAsync();
            }
            
            try
            {
                _context.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DishExists(id))
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

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> PatchStatus(int id, [FromBody] DishStatus dishStatus)
        {
            var dish = await _context.Dishes.FirstOrDefaultAsync(d => d.Id == id);

            dish.IsActive = dishStatus.Status;

            await _context.SaveChangesAsync();

            return Ok();
        }


            // POST: api/Dishes
            // To protect from overposting attacks, enable the specific properties you want to bind to, for
            // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Dish>> PostDish(DishViewModel dish)
        {
            if (dish.DietaryType != null)
            {
                _context.DietaryTypes.AddRange(dish.DietaryType);
                _context.SaveChanges();
            }
            Dish newDish = new Dish
            {
                Name = dish.Name,
                Summary = dish.Summary,
                Price = dish.Price,
                PromotionalPrice = dish.PromotionalPrice,
                IsPromotional = dish.IsPromotional,
                DishType = dish.DishType,
                Description = dish.Description,
                DietaryType = dish.DietaryType,
                Weight = dish.Weight,
                IsActive = dish.IsActive,
                RestaurantId = dish.RestaurantId
            };

            _context.Dishes.Add(newDish);
            await _context.SaveChangesAsync();

            return Ok(newDish);
        }
        [HttpPost("addToSubcategory/{dishId}/{categoryId}")]
        public async Task<object> AddDishToSubCategory([FromRoute] int dishId, [FromRoute] int categoryId)
        {
            var dish = await _context.Dishes.FirstOrDefaultAsync(f => f.Id == dishId);
            var category = _context.SubCategories.Include(b => b.DishCategories).FirstOrDefault(u => u.Id == categoryId);
            _context.DishCategories.Add(new DishCategory {
                Dish = dish,
                SubCategory = category
            });
            var restaurant = _context.Restaurants.Include(g => g.Menu).FirstOrDefault(a => a.Menu.Contains(category));
            dish.RestaurantId = restaurant.Id;
            _context.Dishes.Update(dish);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPut("Move/{dishId}")]
        public async Task<object> MoveDish([FromRoute] int dishId, [FromBody] MoveDishViewModel model)
        {

            var dish = await _context.Dishes.Include(d => d.DishCategories).FirstOrDefaultAsync(f => f.Id == dishId);
            dish.RestaurantId = model.RestaurantId;
            dish.DishType = model.DishType;
            _context.DishCategories.RemoveRange(dish.DishCategories);

            _context.DishCategories.Add(new DishCategory
            {
                Dish = dish,
                SubCategoryId = model.SubCategoryId
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("{dishId}")]
        public async Task<ActionResult<Dish>> ComponentsDish([FromRoute] int dishId, ComponentsDishViewModel dish)
        {
            _context.DishComponents.AddRange(dish.DishComponents);
            _context.SaveChanges();

            Dish newDish = _context.Dishes.Include(s => s.Components).FirstOrDefault(u => u.Id == dishId);

            newDish.Components.AddRange(dish.DishComponents);

            _context.Dishes.Update(newDish);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("CopyFrom/{dishId}")]
        public async Task<ActionResult<Dish>> ComponentsDish([FromRoute] int dishId)
        {
            var dish = await _context.Dishes.Include(d => d.DietaryType).AsNoTracking().FirstOrDefaultAsync(d => d.Id == dishId);
            var dishComponents = await _context.DishComponents.AsNoTracking().Where(dc => dc.DishId == dishId).ToListAsync();

            if(dish.DietaryType.Count > 0)
            {
                dish.DietaryType.ForEach(dt => dt.Id = default(int));
            }

            dish.Id = default(int);

            _context.Add(dish);

            await _context.SaveChangesAsync();


            dishComponents.ForEach(c => { c.DishId = dish.Id; c.Id = default(int); });

            foreach(var d in dishComponents)
            {
                _context.DishComponents.Add(d);
                _context.SaveChanges();
            }

            var dishCategory = await _context.DishCategories.FirstOrDefaultAsync(dc => dc.DishId == dishId);

            _context.DishCategories.Add(new DishCategory {
                Dish = dish,
                SubCategoryId = dishCategory.SubCategoryId
            });

            await _context.SaveChangesAsync();

            return Ok();
        }

        
        [HttpPut("{dishId}/components")]
        public async Task<ActionResult<Dish>> PutComponent([FromRoute] int dishId, ComponentsDishViewModel dish)
        {
            /*
            _context.DishComponents.AddRange(dish.DishComponents);
            _context.SaveChanges();

            Dish newDish = _context.Dishes.Include(s => s.Components).FirstOrDefault(u => u.Id == dishId);

            newDish.Components.AddRange(dish.DishComponents);

            _context.Dishes.Update(newDish);
            await _context.SaveChangesAsync();
            */

            

            Dish newDish = _context.Dishes.Include(s => s.Components).FirstOrDefault(u => u.Id == dishId);



            var exceptDish = newDish.Components.Except(dish.DishComponents).Select(dc =>new int?(dc.Id)).ToList();
            

            await _context.SaveChangesAsync();

            var intersectIngridient = newDish.Components.Intersect(dish.DishComponents).Select(dc => new int?(dc.Id)).ToList();

            _context.DishComponents.Where(dc => intersectIngridient.Contains(dc.Id));



            _context.DishComponents.RemoveRange(newDish.Components);

            await _context.SaveChangesAsync();

            return Ok();
        }
    

        [HttpDelete("deletecomponent/{dishId}")]
        public async Task<ActionResult<Dish>> DeleteComponent([FromRoute] int dishId)
        {
            Dish newDish = _context.Dishes.Include(s => s.Components).FirstOrDefault(u => u.Id == dishId);
            _context.DishComponents.RemoveRange(newDish.Components);

            await _context.SaveChangesAsync();

            return Ok();
        }
        [HttpPost("uploadimage/{dishId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> UploadImage(IFormFile image, [FromRoute] int dishId)
        {
            var dish = await _context.Dishes.FirstOrDefaultAsync(d => d.Id == dishId);

            dish.Image = await _mediaStorageService.SaveMediaAsync(image);

            _context.Dishes.Update(dish);

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE: api/Dishes/5
        [HttpDelete("{dishId}/{categoryId}")]
        public async Task<ActionResult<Dish>> DeleteDish(int dishId, int categoryId)
        {
            var dish = await _context.Dishes.Include(d=>d.Components).Include(s=>s.DietaryType).FirstOrDefaultAsync(u=>u.Id==dishId);
            var category = await _context.SubCategories.Include(m=>m.DishCategories).FirstOrDefaultAsync(u => u.Id == categoryId);
            if (dish == null)
            {
                return NotFound();
            }
            if (category == null)
            {
                return NotFound();
            }
            DishCategory dishCategory = await _context.DishCategories.FirstOrDefaultAsync(h => h.DishId == dishId && h.SubCategoryId == categoryId);
            _context.DishCategories.Remove(dishCategory);
            await _context.SaveChangesAsync();

            return dish;
        }

        private bool DishExists(int id)
        {
            return _context.Dishes.Any(e => e.Id == id);
        }
    }
}
