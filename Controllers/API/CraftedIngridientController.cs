using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Models;
using Ruddy.WEB.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ruddy.WEB.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class CraftedIngridientController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Ruddy.WEB.Models.Account> _userManager;

        public CraftedIngridientController(ApplicationDbContext context, UserManager<Ruddy.WEB.Models.Account> userManager)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: api/<CraftedIngridientController>
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<CraftedComponentGetViewModel>>> Get()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var craftedIngridients = await _context.CraftedComponents.AsNoTracking()
                .Include(cc => cc.CraftedComponentIngridients)
                    .ThenInclude(cci => cci.Ingredient)
                .Where(ci => ci.RestaurantUserId == user.Id).Select(cc => new CraftedComponentGetViewModel()
            {
                Id = cc.Id,
                NameEng = cc.NameEng,
                NameEs = cc.NameEs,
                NameFr = cc.NameFr,
                NameNl = cc.NameNl,
                CraftedIngidients = cc.CraftedComponentIngridients.Select(cci => new CraftedIngidient() { Ingridient = cci.Ingredient, Weight = cci.Weight }).ToList()
            }).ToListAsync();

            return craftedIngridients;
        }

        // GET api/<CraftedIngridientController>/5
        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CraftedComponentGetViewModel>> Get(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var craftedIngridient = await _context.CraftedComponents.AsNoTracking()
                .Include(cc => cc.CraftedComponentIngridients)
                    .ThenInclude(cci => cci.Ingredient)
                .FirstOrDefaultAsync(ci => ci.RestaurantUserId == user.Id && ci.Id == id);

            var returnModel = new CraftedComponentGetViewModel()
                {
                    Id = craftedIngridient.Id,
                    NameEng = craftedIngridient.NameEng,
                    NameEs = craftedIngridient.NameEs,
                    NameFr = craftedIngridient.NameFr,
                    NameNl = craftedIngridient.NameNl,
                    CraftedIngidients = craftedIngridient.CraftedComponentIngridients.Select(cci => new CraftedIngidient() { Ingridient = cci.Ingredient, Weight = cci.Weight }).ToList()
                };

            return returnModel;
        }

        // POST api/<CraftedIngridientController>
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Post([FromBody] CraftedComponentPostViewModel viewModel)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var craftedComponent = new CraftedComponent()
            {
                RestaurantUserId = user.Id,
                CraftedComponentIngridients = viewModel.CraftedComponentIngridients.Select(cci => new Models.CraftedComponentIngridient()
                {
                    IngredientId = cci.Id,
                    Weight = cci.Weight
                }).ToList(),
                NameEng = viewModel.NameEng,
                NameEs = viewModel.NameEs,
                NameFr = viewModel.NameFr,
                NameNl=viewModel.NameNl
            };
            
            await _context.CraftedComponents.AddAsync(craftedComponent);
            await _context.SaveChangesAsync();

            var ingridientIds = viewModel.CraftedComponentIngridients.Select(cci => cci.Id);

            var ingridients = await _context.Ingredients.Where(i => ingridientIds.Contains(i.Id)).ToListAsync();

            var components = viewModel.CraftedComponentIngridients.OrderBy(cci => cci.Id).Zip(ingridients, (x, y) => x.Id == y.Id ? new { Ingridient = y, Weight = x.Weight } : null);

            var newIngridient = new Ingredient();

            PropertyInfo[] piInstance = typeof(Ingredient).GetProperties().Where(t => t.PropertyType == typeof(double?)).ToArray();

            var fullWeight = components.Aggregate(0.0, (sum, next) => sum += next.Weight);

            foreach (var prp in piInstance)
            {
                double? result = 0;
                foreach (var c in components)
                {
                    var property = prp.GetValue(c.Ingridient) as double?;
                    result += property != null ? property * (c.Weight / 100) : 0;
                }
                prp.SetValue(newIngridient, result != 0 ? result / fullWeight * 100 : null);
            }

            newIngridient.NameEng = viewModel.NameEng;
            newIngridient.NameEs = viewModel.NameEs;
            newIngridient.NameFr = viewModel.NameFr;
            newIngridient.NameNl = viewModel.NameNl;
            newIngridient.CraftedComponentId = craftedComponent.Id;

            await _context.Ingredients.AddAsync(newIngridient);
            await _context.SaveChangesAsync();

            return Ok();
        }

        // PUT api/<CraftedIngridientController>/5
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Put(int id, [FromBody] CraftedComponentPostViewModel viewModel)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var craftedIngridient = await _context.CraftedComponents
                .Include(cc => cc.CraftedComponentIngridients)
                    .ThenInclude(cci => cci.Ingredient)
                .FirstOrDefaultAsync(ci => ci.RestaurantUserId == user.Id && ci.Id == id);

            _context.RemoveRange(craftedIngridient.CraftedComponentIngridients);

            craftedIngridient.NameEng = viewModel.NameEng;
            craftedIngridient.NameEs = viewModel.NameEs;
            craftedIngridient.NameFr = viewModel.NameFr;
            craftedIngridient.NameNl = viewModel.NameNl;

            var craftedComponentIngridients = viewModel.CraftedComponentIngridients.Select(cci => new CraftedComponentIngridient()
            {
                CraftedComponentId = craftedIngridient.Id,
                IngredientId = cci.Id,
                Weight = cci.Weight
            }).ToList();

            await _context.AddRangeAsync(craftedComponentIngridients);

            var ingridientIds = viewModel.CraftedComponentIngridients.Select(cci => cci.Id);

            var ingridients = await _context.Ingredients.Where(i => ingridientIds.Contains(i.Id)).ToListAsync();

            var components = viewModel.CraftedComponentIngridients.OrderBy(cci => cci.Id).Zip(ingridients, (x, y) => x.Id == y.Id ? new { Ingridient = y, Weight = x.Weight } : null);

            var newIngridient = await _context.Ingredients.FirstOrDefaultAsync(i => i.CraftedComponentId == id);

            if (newIngridient == null)
            {
                newIngridient = new Ingredient();
            }

            PropertyInfo[] piInstance = typeof(Ingredient).GetProperties().Where(t => t.PropertyType == typeof(double?)).ToArray();

            var fullWeight = components.Aggregate(0.0, (sum, next) => sum += next.Weight);

            foreach (var prp in piInstance)
            {
                double? result = 0;
                foreach (var c in components)
                {
                    var property = prp.GetValue(c.Ingridient) as double?;
                    result += property != null ? property * (c.Weight / 100) : 0;
                }
                prp.SetValue(newIngridient, result != 0 ? result / fullWeight * 100 : null);
            }

            newIngridient.NameEng = viewModel.NameEng;
            newIngridient.NameEs = viewModel.NameEs;
            newIngridient.NameFr = viewModel.NameFr;
            newIngridient.NameNl = viewModel.NameNl;
            newIngridient.CraftedComponentId = craftedIngridient.Id;

            await _context.SaveChangesAsync();

            return Ok();
        }

        // DELETE api/<CraftedIngridientController>/5
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> Delete(int id)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var ingridient = await _context.Ingredients.FirstOrDefaultAsync(i => i.CraftedComponentId == id);
            if(ingridient != null)
            {
                _context.Ingredients.Remove(ingridient);
            }

            var craftedIngridient = await _context.CraftedComponents.AsNoTracking()
                .Include(cc => cc.CraftedComponentIngridients)
                    .ThenInclude(cci => cci.Ingredient)
                .FirstOrDefaultAsync(ci => ci.RestaurantUserId == user.Id && ci.Id == id);

            if(craftedIngridient == null)
            {
                return NotFound($"Crafted ingridient with id: {id}, was not found");
            }

            _context.RemoveRange(craftedIngridient.CraftedComponentIngridients);
            _context.CraftedComponents.Remove(craftedIngridient);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
