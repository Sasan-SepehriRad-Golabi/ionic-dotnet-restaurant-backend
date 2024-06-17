using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Handler;
using Ruddy.WEB.Models;
using Ruddy.WEB.ViewModels;
using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace Ruddy.WEB.Controllers.API;


[Route("api/[controller]")]
[ApiController]
public class UploadRestaurantController : ControllerBase
{
    private readonly IRestaurantFileHandler _fileHandler;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<Models.Account> _userManager;
    private readonly SignInManager<Models.Account> _signInManager;

    public UploadRestaurantController(UserManager<Models.Account> userManager,
            SignInManager<Models.Account> signInManager, ApplicationDbContext context, IRestaurantFileHandler fileHandler)
    {
        _userManager = userManager;
        _fileHandler = fileHandler;
        _signInManager = signInManager;
        _context = context;
    }

    [HttpPost("Restaurants")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
    public async Task<IActionResult> UploadFileRestaurants(IFormFile file)
    {
        // return Ok("jjjj");
        return await _fileHandler.UploadFileRestaurants(file, _context);
    }
    [HttpPost("Menu")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UploadFileMenu(IFormFile file)
    {
        return await _fileHandler.UploadFileMenu(file, _context);
    }
    [HttpPost("MenuS3")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> UploadFileMenuS3(IFormFile file)
    {
        return await _fileHandler.UploadFileMenuS3(file, _context);
    }
    [HttpPost("DeleteChainRestaurant")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> DeleteChainRestaurant(string ChainCode)
    {
        return await DeleteChainRestaurant(ChainCode, _context);
    }

    private async Task<ActionResult> DeleteChainRestaurant(string chainCode, ApplicationDbContext context)
    {
        int rowAffectedCount = await context.Database.ExecuteSqlRawAsync("exec deleteChainrestaurants @chainCode={0}", chainCode.ToLower());
        List<bulkModel> bms = context.bulkModels.FromSqlRaw("exec afterinsertCsvFilesFromS3ToRDS").ToList();
        if (bms[0].dishIdBeforeBulk == 1)
        {
            return Ok("Process Done Successfully");
        }
        else
        {
            return BadRequest("Process faield...No change in database happened");
        }

    }
    [HttpPost("DeleteRegularRestaurant")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> DeleteRegularRestaurant(int restaurantId)
    {
        return await DeleteRegularRestaurant(restaurantId, _context);
    }

    private async Task<ActionResult> DeleteRegularRestaurant(int restaurantId, ApplicationDbContext context)
    {
        int rowAffectedCount = await context.Database.ExecuteSqlRawAsync("exec deleteRegularrestaurants @id={0}", restaurantId);
        List<bulkModel> bms = context.bulkModels.FromSqlRaw("exec afterinsertCsvFilesFromS3ToRDS").ToList();
        if (bms[0].dishIdBeforeBulk == 1)
        {
            return Ok("Process Done Successfully");
        }
        else
        {
            return BadRequest("Process faield...No change in database happened");
        }
    }
    [HttpPost("saveToJournal")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<ActionResult> saveToJournal([FromBody] addToJournal value)
    {
        var user = await _userManager.FindByNameAsync(User.Identity.Name) as ApplicationUser;
        if (user == null)
        {
            return NotFound("user does not exist");
        }
        else
        {
            var dishId = value.dishId;
            return await _fileHandler.saveToJournal(dishId, user, _context);
        }
    }
    [HttpPost("checkIfUserSignedIn")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<string> checkIfUserSignedIn([FromBody] CheckIfUserSignedIn value)
    {
        bool res = _signInManager.IsSignedIn(User);
        if (res)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            if (user == null)
            {
                return "NotOk";
            }
            else
            {
                return "Ok";
            }
        }
        else
        {
            return "NotOk";
        }
    }

}
