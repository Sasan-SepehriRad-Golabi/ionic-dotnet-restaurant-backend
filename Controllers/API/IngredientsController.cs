using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using LinqToExcel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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
    public class IngredientsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<Account> _userManager;

        public IngredientsController(IWebHostEnvironment webHostEnvironment, ApplicationDbContext context, UserManager<Account> userManager)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _userManager = userManager;
        }
       
        [HttpPost("uploadXLSXfile")]
        private async Task<object> ParseXlsxFile()
        {

            if (_context.Ingredients.Any())
            {

                _context.Database.ExecuteSqlRaw("delete from OrderedIngredients");
                await _context.SaveChangesAsync();
                _context.Database.ExecuteSqlRaw("delete from Ingredients");
                await _context.SaveChangesAsync();

            }
            
            var excel = new ExcelQueryFactory("wwwroot/NewDatabaseENGFRES.xlsx");

            var newlist = excel.Worksheet("already in database").Select(i => new Dictionary<string, string>()
            {
                { "Group", i["Group"] },
                { "Sub Group", i["Sub Group"] },
                { "Ingredient ENG", i["Ingredient ENG"] },
                { "Ingredient FR", i["Ingredient FR"] },
                { "Ingredient ES", i["Ingredient ES"] },
                { "Ingredient NL", i["Ingredient NL"] },
                { "Energy with fibres (kcal)", i["Energy with fibres (kcal)"] },
                { "Energy with fibres (kJ)", i["Energy with fibres (kJ)"] },
                { "Proteins (g)", i["Proteins (g)"] },
                { "Fat (g)", i["Fat (g)"] },
                { "Carbohydrates (g)", i["Carbohydrates (g)"] },
                { "Sugars (g)", i["Sugars (g)"] },
                { "Starch (g)", i["Starch (g)"] },
                { "Water (g)", i["Water (g)"] },
                { "Fatty acids saturated (g)", i["Fatty acids saturated (g)"] },
                { "Glucose (g)", i["Glucose (g)"] },
                { "Lactose (g)", i["Lactose (g)"] },
                { "Fructose (g)", i["Fructose (g)"] },
                { "Sucrose (g)", i["Sucrose (g)"] },
                { "Maltose (g)", i["Maltose (g)"] },
                { "GaLactose (g)", i["GaLactose (g)"] },
                { "Fibres, sum (g)", i["Fibres, sum (g)"] },
                { "FA C12:0 (g)", i["FA C12:0 (g)"] },
                { "FA C14:0 (g)", i["FA C14:0 (g)"] },
                { "FA C16:0 (g)", i["FA C16:0 (g)"] },
                { "FA, monounsat., sum (g)", i["FA, monounsat#, sum (g)"] },
                { "FA, polyunsat, sum (g)", i["FA, polyunsat, sum (g)"] },
                { "FA, omega 3, sum (g)", i["FA, omega 3, sum (g)"] },
                { "FA C20:5 n-3 cis EPA (g)", i["FA C20:5 n-3 cis EPA (g)"] },
                { "FA C22:6 n-3 cis DHA (g)", i["FA C22:6 n-3 cis DHA (g)"] },
                { "FA C18:3 n-3 cis linolenic (g)", i["FA C18:3 n-3 cis linolenic (g)"] },
                { "FA, omega 6, sum (g)", i["FA, omega 6, sum (g)"] },
                { "FA C18:2 n-6 cis linoleic (g)", i["FA C18:2 n-6 cis linoleic (g)"] },
                { "FA, trans, sum (g)", i["FA, trans, sum (g)"] },
                { "Cholesterol (mg)", i["Cholesterol (mg)"] },
                { "Alcohol (g)", i["Alcohol (g)"] },
                { "Polyols, sum (g)", i["Polyols, sum (g)"] },
                { "Sodium (mg)", i["Sodium (mg)"] },
                { "Potassium (mg)", i["Potassium (mg)"] },
                { "Calcium (mg)", i["Calcium (mg)"] },
                { "Phosphorus (mg)", i["Phosphorus (mg)"] },
                { "Magnesium (mg)", i["Magnesium (mg)"] },
                { "Iron (mg)", i["Iron (mg)"] },
                { "Copper (mg)", i["Copper (mg)"] },
                { "Zinc (mg)", i["Zinc (mg)"] },
                { "Iodide (µg)", i["Iodide (µg)"] },
                { "Selenium (µg)", i["Selenium (µg)"] },
                { "VitA - Activity (µg)", i["VitA - Activity (µg)"] },
                { "VitB1 (mg)", i["VitB1 (mg)"] },
                { "VitB2 (mg)", i["VitB2 (mg)"] },
                { "VitB12 (µg)", i["VitB12 (µg)"] },
                { "Folate (µg)", i["Folate (µg)"] },
                { "VitC (mg)", i["VitC (mg)"] },
                { "VitD (µg)", i["VitD (µg)"] },
                { "VitE (mg)", i["VitE (mg)"] }
            });

            var test = newlist.ToList();

            var ingridient = test.Select(i => new Ingredient()
            {
                Group = i["Group"],
                SubGroup = i["Sub Group"],
                NameEng = i["Ingredient ENG"],
                NameFr = i["Ingredient FR"],
                NameEs = i["Ingredient ES"],
                NameNl = i["Ingredient NL"],
                Energy_KCal = Parse(i["Energy with fibres (kcal)"]),
                Energy_Kj = Parse(i["Energy with fibres (kJ)"]),
                Proteins_G = Parse(i["Proteins (g)"]),
                Fat_G = Parse(i["Fat (g)"]),
                Carbohydrates_G = Parse(i["Carbohydrates (g)"]),
                Sugars_G = Parse(i["Sugars (g)"]),
                Starch_G = Parse(i["Starch (g)"]),
                Water_G = Parse(i["Water (g)"]),
                FattyAcidsSaturated_G = Parse(i["Fatty acids saturated (g)"]),
                Glucose_G = Parse(i["Glucose (g)"]),
                Lactose_G = Parse(i["Lactose (g)"]),
                Fructose_G = Parse(i["Fructose (g)"]),
                Sucrose_G = Parse(i["Sucrose (g)"]),
                Maltose_G = Parse(i["Maltose (g)"]),
                GaLactose_G = Parse(i["GaLactose (g)"]),
                FibresSum_G = Parse(i["Fibres, sum (g)"]),
                FA_C12_0_G = Parse(i["FA C12:0 (g)"]),
                FA_C14_0_G = Parse(i["FA C14:0 (g)"]),
                FA_C16_0_G = Parse(i["FA C16:0 (g)"]),
                FA_monounsat_sum_G = Parse(i["FA, monounsat., sum (g)"]),
                FA_polyunsat_sum_G = Parse(i["FA, polyunsat, sum (g)"]),
                FA_omega_3_sum_G = Parse(i["FA, omega 3, sum (g)"]),
                FA_C20_5_n_3_cis_EPA_G = Parse(i["FA C20:5 n-3 cis EPA (g)"]),
                FA_C22_6_n_3_cis_DHA_G = Parse(i["FA C22:6 n-3 cis DHA (g)"]),
                FA_C18_3_n_3_cis_linolenic_G = Parse(i["FA C18:3 n-3 cis linolenic (g)"]),
                FA_omega_6_sum_G = Parse(i["FA, omega 6, sum (g)"]),
                FA_C18_2_n_6_cis_linoleic_G = Parse(i["FA C18:2 n-6 cis linoleic (g)"]),
                FA_trans_sum_G = Parse(i["FA, trans, sum (g)"]),
                Cholesterol_MG = Parse(i["Cholesterol (mg)"]),
                Alcohol_G = Parse(i["Alcohol (g)"]),
                Polyols_sum_G = Parse(i["Polyols, sum (g)"]),
                Sodium_MG = Parse(i["Sodium (mg)"]),
                Potassium_MG = Parse(i["Potassium (mg)"]),
                Calcium_MG = Parse(i["Calcium (mg)"]),
                Phosphorus_MG = Parse(i["Phosphorus (mg)"]),
                Magnesium_MG = Parse(i["Magnesium (mg)"]),
                Iron_MG = Parse(i["Iron (mg)"]),
                Copper_MG = Parse(i["Copper (mg)"]),
                Zinc_MG = Parse(i["Zinc (mg)"]),
                Iodide_MG = Parse(i["Iodide (µg)"]),
                Selenium_MCG = Parse(i["Selenium (µg)"]),
                VitA_Activity_MCG = Parse(i["VitA - Activity (µg)"]),
                VitB1_MG = Parse(i["VitB1 (mg)"]),
                VitB2_MG = Parse(i["VitB2 (mg)"]),
                VitB12_MCG = Parse(i["VitB12 (µg)"]),
                Folate_MCG = Parse(i["Folate (µg)"]),
                VitC_MG = Parse(i["VitC (mg)"]),
                VitD_MCG = Parse(i["VitD (µg)"]),
                VitE_MG = Parse(i["VitE (mg)"])
                
            }).ToList();

            await _context.Ingredients.AddRangeAsync(ingridient);

            await _context.SaveChangesAsync();

            return ingridient;
        }

        [HttpPost("uploadXLSXfileAdd")]
        private async Task<object> ParseXlsxFileAdd()
        {
            var excel = new ExcelQueryFactory("wwwroot/newIngredients09_04.xlsx");

            var newlist = excel.Worksheet("Sheet1").Select(i => new Dictionary<string, string>()
            {
                { "Group", i["Group"] },
                { "Sub Group", i["Sub Group"] },
                { "Ingredient ENG", i["Ingredient ENG"] },
                { "Ingredient FR", i["Ingredient FR"] },
                { "Ingredient ES", i["Ingredient ES"] },
                { "Ingredient NL", i["Ingredient NL"] },
                { "Energy with fibres (kcal)", i["Energy with fibres (kcal)"] },
                { "Energy with fibres (kJ)", i["Energy with fibres (kJ)"] },
                { "Proteins (g)", i["Proteins (g)"] },
                { "Fat (g)", i["Fat (g)"] },
                { "Carbohydrates (g)", i["Carbohydrates (g)"] },
                { "Sugars (g)", i["Sugars (g)"] },
                { "Starch (g)", i["Starch (g)"] },
                { "Water (g)", i["Water (g)"] },
                { "Fatty acids saturated (g)", i["Fatty acids saturated (g)"] },
                { "Glucose (g)", i["Glucose (g)"] },
                { "Lactose (g)", i["Lactose (g)"] },
                { "Fructose (g)", i["Fructose (g)"] },
                { "Sucrose (g)", i["Sucrose (g)"] },
                { "Maltose (g)", i["Maltose (g)"] },
                { "GaLactose (g)", i["GaLactose (g)"] },
                { "Fibres, sum (g)", i["Fibres, sum (g)"] },
                { "FA C12:0 (g)", i["FA C12:0 (g)"] },
                { "FA C14:0 (g)", i["FA C14:0 (g)"] },
                { "FA C16:0 (g)", i["FA C16:0 (g)"] },
                { "FA, monounsat., sum (g)", i["FA, monounsat#, sum (g)"] },
                { "FA, polyunsat, sum (g)", i["FA, polyunsat, sum (g)"] },
                { "FA, omega 3, sum (g)", i["FA, omega 3, sum (g)"] },
                { "FA C20:5 n-3 cis EPA (g)", i["FA C20:5 n-3 cis EPA (g)"] },
                { "FA C22:6 n-3 cis DHA (g)", i["FA C22:6 n-3 cis DHA (g)"] },
                { "FA C18:3 n-3 cis linolenic (g)", i["FA C18:3 n-3 cis linolenic (g)"] },
                { "FA, omega 6, sum (g)", i["FA, omega 6, sum (g)"] },
                { "FA C18:2 n-6 cis linoleic (g)", i["FA C18:2 n-6 cis linoleic (g)"] },
                { "FA, trans, sum (g)", i["FA, trans, sum (g)"] },
                { "Cholesterol (mg)", i["Cholesterol (mg)"] },
                { "Alcohol (g)", i["Alcohol (g)"] },
                { "Polyols, sum (g)", i["Polyols, sum (g)"] },
                { "Sodium (mg)", i["Sodium (mg)"] },
                { "Potassium (mg)", i["Potassium (mg)"] },
                { "Calcium (mg)", i["Calcium (mg)"] },
                { "Phosphorus (mg)", i["Phosphorus (mg)"] },
                { "Magnesium (mg)", i["Magnesium (mg)"] },
                { "Iron (mg)", i["Iron (mg)"] },
                { "Copper (mg)", i["Copper (mg)"] },
                { "Zinc (mg)", i["Zinc (mg)"] },
                { "Iodide (µg)", i["Iodide (µg)"] },
                { "Selenium (µg)", i["Selenium (µg)"] },
                { "VitA - Activity (µg)", i["VitA - Activity (µg)"] },
                { "VitB1 (mg)", i["VitB1 (mg)"] },
                { "VitB2 (mg)", i["VitB2 (mg)"] },
                { "VitB12 (µg)", i["VitB12 (µg)"] },
                { "Folate (µg)", i["Folate (µg)"] },
                { "VitC (mg)", i["VitC (mg)"] },
                { "VitD (µg)", i["VitD (µg)"] },
                { "VitE (mg)", i["VitE (mg)"] }
            });

            var test = newlist.ToList();

            var ingridient = test.Select(i => new Ingredient()
            {
                Group = i["Group"],
                SubGroup = i["Sub Group"],
                NameEng = i["Ingredient ENG"],
                NameFr = i["Ingredient FR"],
                NameEs = i["Ingredient ES"],
                NameNl = i["Ingredient NL"],
                Energy_KCal = Parse(i["Energy with fibres (kcal)"]),
                Energy_Kj = Parse(i["Energy with fibres (kJ)"]),
                Proteins_G = Parse(i["Proteins (g)"]),
                Fat_G = Parse(i["Fat (g)"]),
                Carbohydrates_G = Parse(i["Carbohydrates (g)"]),
                Sugars_G = Parse(i["Sugars (g)"]),
                Starch_G = Parse(i["Starch (g)"]),
                Water_G = Parse(i["Water (g)"]),
                FattyAcidsSaturated_G = Parse(i["Fatty acids saturated (g)"]),
                Glucose_G = Parse(i["Glucose (g)"]),
                Lactose_G = Parse(i["Lactose (g)"]),
                Fructose_G = Parse(i["Fructose (g)"]),
                Sucrose_G = Parse(i["Sucrose (g)"]),
                Maltose_G = Parse(i["Maltose (g)"]),
                GaLactose_G = Parse(i["GaLactose (g)"]),
                FibresSum_G = Parse(i["Fibres, sum (g)"]),
                FA_C12_0_G = Parse(i["FA C12:0 (g)"]),
                FA_C14_0_G = Parse(i["FA C14:0 (g)"]),
                FA_C16_0_G = Parse(i["FA C16:0 (g)"]),
                FA_monounsat_sum_G = Parse(i["FA, monounsat., sum (g)"]),
                FA_polyunsat_sum_G = Parse(i["FA, polyunsat, sum (g)"]),
                FA_omega_3_sum_G = Parse(i["FA, omega 3, sum (g)"]),
                FA_C20_5_n_3_cis_EPA_G = Parse(i["FA C20:5 n-3 cis EPA (g)"]),
                FA_C22_6_n_3_cis_DHA_G = Parse(i["FA C22:6 n-3 cis DHA (g)"]),
                FA_C18_3_n_3_cis_linolenic_G = Parse(i["FA C18:3 n-3 cis linolenic (g)"]),
                FA_omega_6_sum_G = Parse(i["FA, omega 6, sum (g)"]),
                FA_C18_2_n_6_cis_linoleic_G = Parse(i["FA C18:2 n-6 cis linoleic (g)"]),
                FA_trans_sum_G = Parse(i["FA, trans, sum (g)"]),
                Cholesterol_MG = Parse(i["Cholesterol (mg)"]),
                Alcohol_G = Parse(i["Alcohol (g)"]),
                Polyols_sum_G = Parse(i["Polyols, sum (g)"]),
                Sodium_MG = Parse(i["Sodium (mg)"]),
                Potassium_MG = Parse(i["Potassium (mg)"]),
                Calcium_MG = Parse(i["Calcium (mg)"]),
                Phosphorus_MG = Parse(i["Phosphorus (mg)"]),
                Magnesium_MG = Parse(i["Magnesium (mg)"]),
                Iron_MG = Parse(i["Iron (mg)"]),
                Copper_MG = Parse(i["Copper (mg)"]),
                Zinc_MG = Parse(i["Zinc (mg)"]),
                Iodide_MG = Parse(i["Iodide (µg)"]),
                Selenium_MCG = Parse(i["Selenium (µg)"]),
                VitA_Activity_MCG = Parse(i["VitA - Activity (µg)"]),
                VitB1_MG = Parse(i["VitB1 (mg)"]),
                VitB2_MG = Parse(i["VitB2 (mg)"]),
                VitB12_MCG = Parse(i["VitB12 (µg)"]),
                Folate_MCG = Parse(i["Folate (µg)"]),
                VitC_MG = Parse(i["VitC (mg)"]),
                VitD_MCG = Parse(i["VitD (µg)"]),
                VitE_MG = Parse(i["VitE (mg)"])

            }).ToList();

            await _context.Ingredients.AddRangeAsync(ingridient);

            await _context.SaveChangesAsync();

            return ingridient;
        }

        private static double? Parse(string input)
        {
            var processingString = input.Where(c => c != ' ').Aggregate("", (sum, next) => sum += next);
            processingString = processingString.Replace(',', '.');
            double result;

            if (double.TryParse(processingString, out result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        // GET: api/Ingredients
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<Ingredient>>> GetIngredients()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            return await _context.Ingredients.AsNoTracking()
                .Include(i => i.CraftedComponent)
                .Where(i => i.CraftedComponentId == null || i.CraftedComponent.RestaurantUserId == user.Id).ToListAsync();
        }

        // GET: api/Ingredients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ingredient>> GetIngredient(int id)
        {

            var ingredient = await _context.Ingredients.FindAsync(id);

            if (ingredient == null)
            {
                return NotFound();
            }

            return ingredient;
        }

        // PUT: api/Ingredients/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutIngredient(int id, Ingredient ingredient)
        {
            if (id != ingredient.Id)
            {
                return BadRequest();
            }

            _context.Entry(ingredient).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!IngredientExists(id))
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

        // POST: api/Ingredients
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Ingredient>> PostIngredient(Ingredient ingredient)
        {
            _context.Ingredients.Add(ingredient);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetIngredient", new { id = ingredient.Id }, ingredient);
        }

        // DELETE: api/Ingredients/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Ingredient>> DeleteIngredient(int id)
        {
            var ingredient = await _context.Ingredients.FindAsync(id);
            if (ingredient == null)
            {
                return NotFound();
            }

            _context.Ingredients.Remove(ingredient);
            await _context.SaveChangesAsync();

            return ingredient;
        }

        private bool IngredientExists(int id)
        {
            return _context.Ingredients.Any(e => e.Id == id);
        }
    }
}
