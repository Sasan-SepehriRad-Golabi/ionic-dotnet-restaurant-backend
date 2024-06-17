using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Models;
using System.Linq;

namespace Ruddy.WEB.Controllers.API;

[Route("api/[controller]")]
[ApiController]
public class MyAdminLoginController : ControllerBase
{
    private UserManager<Models.Account> _userManager;
    private SignInManager<Models.Account> _signInManager;
    private ApplicationDbContext _db;
    public MyAdminLoginController(UserManager<Models.Account> userManager, SignInManager<Models.Account> signInManager, ApplicationDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _db = db;
    }

    [HttpGet]
    public ActionResult checkAdmin()
    {
        return Ok("sss");
    }
    [HttpPost("login")]
    public async Task<ActionResult> loginAdmin([FromBody] AdminUser model)
    {
        var result = await _signInManager.PasswordSignInAsync(model.userName, model.Password, false, false);
        if (result.Succeeded)
        {
            var user = _userManager.Users.SingleOrDefault(x => x.UserName == model.userName);
            return Ok("loggedIn");
        }
        else
        {
            return Ok("NotPermitted");
        }
    }
    [HttpPost("register")]
    public async Task<ActionResult> registerAdmin([FromBody] AdminUser model)
    {
        Account newUser = new Account()
        {
            UserName = model.userName
        };
        var result = await _userManager.CreateAsync(newUser, model.Password);
        if (result.Succeeded)
        {
            await _signInManager.SignInAsync(newUser, false);
            return Ok("Done");
        }
        else
        {
            if (result.Errors.Count() > 0)
            {
                return Ok(result.Errors.First().Description.ToString());
            }
            else
            {
                return Ok("Not Succeeded");
            }
        }
    }

}


