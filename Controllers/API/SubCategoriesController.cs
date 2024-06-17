using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Models;
using Ruddy.WEB.ViewModels;

namespace Ruddy.WEB.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubCategoriesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Models.Account> _userManager;

        public SubCategoriesController(ApplicationDbContext context, UserManager<Models.Account> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: api/SubCategories
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<SubCategory>>> GetSubCategories()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var result = await _context.SubCategories
                .Include(sc => sc.Restaurant)
                .Where(sc => sc.Restaurant.RestaurantUserId == user.Id).ToListAsync();

            result.ForEach(sc => sc.Restaurant = null);

            return result;

        }

        // GET: api/SubCategories/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SubCategory>> GetSubCategory(int id)
        {
            var subCategory = await _context.SubCategories.Include(h=>h.DishCategories).ThenInclude(p => p.Dish).FirstOrDefaultAsync(a=>a.Id == id);

            if (subCategory == null)
            {
                return NotFound();
            }

            return subCategory;
        }

        // PUT: api/SubCategories/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubCategory(int id, [FromBody]SubCategoryPutViewModel name)
        {
            var subCategory = await _context.SubCategories.FirstOrDefaultAsync(sc => sc.Id == id);

            if(subCategory == null)
            {
                return NotFound("Sub category was not found");
            }

            subCategory.Name = name.Name;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/SubCategories
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("{restaurantId}")]
        public async Task<ActionResult<SubCategory>> PostSubCategory(SubCategory subCategory, [FromRoute] int restaurantId)
        {
            var restaurant = await _context.Restaurants.Include(h=>h.Menu).FirstOrDefaultAsync(m=>m.Id==restaurantId);
            _context.SubCategories.Add(subCategory);
            await _context.SaveChangesAsync();

            restaurant.Menu.Add(subCategory);
            _context.Restaurants.Update(restaurant);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetSubCategory", new { id = subCategory.Id }, subCategory);
        }

        // DELETE: api/SubCategories/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<SubCategory>> DeleteSubCategory(int id)
        {
            var subCategory = await _context.SubCategories.FirstOrDefaultAsync(b=>b.Id==id);
            if (subCategory == null)
            {
                return NotFound();
            }

            _context.DishCategories.RemoveRange(_context.DishCategories.Where(h=>h.SubCategoryId == id));
            _context.SubCategories.Update(subCategory);
            _context.SubCategories.Remove(subCategory);
            await _context.SaveChangesAsync();

            return subCategory;
        }

        private bool SubCategoryExists(int id)
        {
            return _context.SubCategories.Any(e => e.Id == id);
        }
    }
}
