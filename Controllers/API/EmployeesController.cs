using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Interfaces;
using Ruddy.WEB.Models;
using Ruddy.WEB.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Ruddy.WEB.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Owner")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class EmployeesController : ControllerBase
    {
        private readonly UserManager<Account> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly SignInManager<Account> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public EmployeesController(IMapper mapper, UserManager<Account> userManager,
            ApplicationDbContext context, RoleManager<IdentityRole> roleManager, SignInManager<Account> signInManager
            )
        {
            _context = context;
            _mapper = mapper;
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
        }

        // GET: api/<EmployeesController>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkerViewModel>>> Get()
        {
            var owner = await _userManager.FindByNameAsync(User.Identity.Name) as RestaurantUser;

            var employees = await _context.RestaurantUsers.Include(ru => ru.Restaurants).Where(ru => ru.StaffLink == owner.StaffLink && owner.Id != ru.Id).ToListAsync();

            


            return Ok(employees.Select(ru => new WorkerViewModel()
            {
                Id = ru.Id,
                UserName = ru.UserName,
                RestaurantIds = _userManager.GetRolesAsync(ru).Result.ToList()
            }));
        }

        // GET api/<EmployeesController>/5
        [HttpGet("{id}")]
        private string Get(int id)
        {
            return "value";
        }

        // POST api/<EmployeesController>
        [HttpPost]
        public async Task<ActionResult> Post([FromBody] PostWorkerViewModel model)
        {
            var owner = await _userManager.FindByNameAsync(User.Identity.Name) as RestaurantUser;

            var user = new RestaurantUser
            {
                UserName = model.UserName,
                Email = model.UserName,
                StaffLink = owner.StaffLink
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                foreach(var u in model.RestaurantIds)
                {
                    if(await _roleManager.RoleExistsAsync(u.ToString()))
                    {
                        await _userManager.AddToRoleAsync(user, u.ToString());
                    }
                    else
                    {
                        await _roleManager.CreateAsync(new IdentityRole(u.ToString()));
                        await _userManager.AddToRoleAsync(user, u.ToString());
                    }
                }


                return Ok();
            }
            else if (result.Errors.Any())
            {
                List<string> errors = new List<string>();
                foreach (var error in result.Errors)
                {
                    errors.Add(error.Description);
                }
                return BadRequest(errors);
            }
            else
            {
                return BadRequest(result);
            }
        }

        // PUT api/<EmployeesController>/5
        [HttpPut("{id}")]
        private void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<EmployeesController>/
        [HttpDelete("{id}")]
        public async Task Delete(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            await _userManager.DeleteAsync(user);
        }
    }
}
