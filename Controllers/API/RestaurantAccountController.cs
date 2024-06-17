using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Ruddy.WEB.Interfaces;
using Ruddy.WEB.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.IO;
using AutoMapper;
using Stripe;
using Ruddy.WEB.Models;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Services;

namespace Ruddy.WEB.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class RestaurantAccountController : ControllerBase
    {
        private readonly UserManager<Models.Account> _userManager;
        private readonly SignInManager<Models.Account> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHasher _hasher;
        private readonly IMediaStorageService _mediaStorageService;

        public RestaurantAccountController(IMapper mapper, UserManager<Models.Account> userManager,
            SignInManager<Models.Account> signInManager, ApplicationDbContext db,
        IConfiguration configuration, RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender, IWebHostEnvironment hostEnvironment, IHasher hasher,
            IMediaStorageService mediaStorageService
            )
        {
            _hasher = hasher;
            _db = db;
            _mapper = mapper;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _webHostEnvironment = hostEnvironment;
            _mediaStorageService = mediaStorageService;
        }

        [HttpPost("register")]
        public async Task<object> Register([FromBody] RestaurantUserRegisterViewModel model)
        {
            var user = new RestaurantUser
            {
                Email = model.Email,
                UserName = model.Email,
                EmailConfirmed = false,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CompanyName = model.CompanyName,
                StaffLink = Guid.NewGuid().ToString("N")
            };

            /*
            user.Discounts = new Domain.Discount
            {
                Percent = 0
            };
            */

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Owner");

                await _signInManager.SignInAsync(user, false);

                var code = _userManager.GenerateEmailConfirmationTokenAsync(user);

                var callbackUrl = Url.Action(
                    "ConfirmEmail",
                    "Account",
                    new { userId = user.Id, code = code.Result },
                    protocol: HttpContext.Request.Scheme);

                /*_emailSender.SendEmailAsync(model.Email, "Confirm your account",
                     $"Click to the link to confirm your account: <a href='{callbackUrl}'>link</a>").Wait();
                */
                return Ok(new { token = GenerateJwtToken(model.Email, user).Result, role = _userManager.GetRolesAsync(user).Result.FirstOrDefault() });
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

        [HttpGet("getuserbytoken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<RestaurantUserViewModel>> GetUserByToken()
        {
            var user = await _db.RestaurantUsers.Include(m => m.Restaurants).FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            var resultUser = _mapper.Map<RestaurantUserViewModel>(user);
            resultUser.OrdersNum = (await _db.Orders.Include(o => o.Restaurant).Where(h => h.Restaurant.RestaurantUserId == user.Id).ToListAsync()).Count;

            return resultUser;
        }

        [HttpGet("getuserrestaurantsbytoken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<Restaurant>>> GetUserRestaurantsByToken()
        {
            var restaurants = _db.RestaurantUsers.Include(u => u.Restaurants).ThenInclude(t => t.Times).ThenInclude(t => t.Pauses)
                .FirstOrDefault(u => u.Email == User.Identity.Name).Restaurants;

            return restaurants;
        }

        [HttpGet("CreateConnectedAccount")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> CreateCustomerAccount()
        {
            var user = _db.RestaurantUsers
                .Include(u => u.Restaurants).ThenInclude(t => t.Times)
                .ThenInclude(t => t.Pauses).Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            var accountOptions = new AccountCreateOptions
            {
                Type = "express",
                Country = "BE",
                Email = user.UserName,
                Capabilities = new AccountCapabilitiesOptions()
                {
                    CardPayments = new AccountCapabilitiesCardPaymentsOptions()
                    {
                        Requested = true
                    },
                    Transfers = new AccountCapabilitiesTransfersOptions()
                    {
                        Requested = true
                    }
                },
                DefaultCurrency = "eur"
            };
            var accService = new AccountService();
            var accountId = accService.Create(accountOptions).Id;
            user.ConnectedAccountId = accountId;

            await _db.SaveChangesAsync();

            return Ok("Connected account created successfully");
        }

        [HttpGet("StripeBalance")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<StripeBalanceViewModel>> GetStripeBalance()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name).ConfigureAwait(false);

            var service = new BalanceService();
            var requestOptions = new RequestOptions();
            requestOptions.StripeAccount = user.ConnectedAccountId;
            var balance = service.Get(requestOptions);

            return new StripeBalanceViewModel()
            {
                Available = balance?.Available.FirstOrDefault().Amount / 100d ?? 0.0,
                Pending = balance?.Pending.FirstOrDefault().Amount / 100d ?? 0.0
            };
        }

        [HttpPut("RegisterFCMToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RegisterFCMToken([FromBody] RestaurantFcmTokenViewModel fcmTokenModel)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var token = await _db.FcmTokens.Include(t => t.RestaurantRecievers).FirstOrDefaultAsync(t => t.FcmToken == fcmTokenModel.FcmToken);

            if (string.IsNullOrEmpty(fcmTokenModel.FcmToken)) return BadRequest("FcmToken can't be null or empty");

            if (token != null)
            {
                token.AccountId = user.Id;
                token.RestaurantRecievers.Clear();
                if (fcmTokenModel.RestaurantIds != null)
                {
                    token.RestaurantRecievers.AddRange(fcmTokenModel.RestaurantIds.Select(r => new RestaurantRecievers() { RestaurantId = r }).ToList());
                }
            }
            else
            {
                var newToken = new FcmTokens()
                {
                    FcmToken = fcmTokenModel.FcmToken,
                    AccountId = user.Id
                    //RestaurantRecievers = fcmTokenModel.RestaurantIds?.Select(r => new RestaurantRecievers() { RestaurantId = r }).ToList()
                };
                if (fcmTokenModel.RestaurantIds != null)
                {
                    newToken.RestaurantRecievers = fcmTokenModel.RestaurantIds.Select(r => new RestaurantRecievers() { RestaurantId = r }).ToList();
                }
                _db.FcmTokens.Add(newToken);
            }

            await _db.SaveChangesAsync();

            return Ok("Token was added");
        }

        [HttpGet("Payout")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> Payout()
        {
            var user = _db.RestaurantUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            var serviceBal = new BalanceService();
            var balanceRequestOptions = new RequestOptions();
            balanceRequestOptions.StripeAccount = user.ConnectedAccountId;
            var balance = serviceBal.Get(balanceRequestOptions);


            var options = new PayoutCreateOptions
            {
                Amount = balance.Available.FirstOrDefault().Amount,
                Currency = "eur",
            };

            var requestOptions = new RequestOptions();
            requestOptions.StripeAccount = user.ConnectedAccountId;

            var payService = new PayoutService();
            var payout = payService.Create(options, requestOptions);

            return Ok();

        }
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("CreateLinkForPayout")]
        public async Task<IActionResult> CreateLinkForPayout()
        {
            var user = await _db.RestaurantUsers.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            var options = new AccountLinkCreateOptions
            {
                Account = user.ConnectedAccountId,
                RefreshUrl = "https" + "://" + HttpContext.Request.Host + HttpContext.Request.Path,
                ReturnUrl = "https://restaurant.ruddy.app/",
                Type = "account_onboarding",

            };
            var service = new AccountLinkService();
            var accountLink = service.Create(options);

            return Ok(accountLink.Url);
        }

        [HttpPut("updateuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> UpdateUser([FromBody] RestaurantUserUpdateViewModel model)
        {
            var user = await _db.RestaurantUsers.FirstOrDefaultAsync(u => u.UserName == User.Identity.Name);

            //var user = await _userManager.FindByNameAsync

            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.BirthDate = model.DateOfBirth;
            user.CompanyName = model.CompanyName;
            if (model.Password != null)
            {
                string userToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                await _userManager.ResetPasswordAsync(user, userToken, model.Password);
            }

            if (!string.IsNullOrEmpty(model.Email))
            {
                await _userManager.SetEmailAsync(user, model.Email);
                await _userManager.SetUserNameAsync(user, model.Email);
                await _userManager.UpdateNormalizedEmailAsync(user);
                await _userManager.UpdateNormalizedUserNameAsync(user);
            }

            //_db.RestaurantUsers.Update(user);

            await _db.SaveChangesAsync();

            return Ok();

        }

        [HttpPost("uploadimage")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> UploadImage(IFormFile image)
        {
            var user = await _db.RestaurantUsers.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            if (image != null)
            {
                user.LogoLink = await _mediaStorageService.SaveMediaAsync(image);
                _db.RestaurantUsers.Update(user);
                await _db.SaveChangesAsync();
            }
            else
            {
                return BadRequest();
            }
            return Ok();
        }

        [HttpGet("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest();
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest();
            }
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                user.EmailConfirmed = true;
                return Ok("You successfully confirmed your account by email");
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPost("Support")]
        public async Task<IActionResult> Support(SupportViewModel model)
        {
            _emailSender.SendEmailAsync(model.Email, model.Subject, model.Message).Wait();
            return Ok();
        }

        [HttpPost("login")]
        public async Task<object> Login([FromBody] LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = await _userManager.FindByEmailAsync(model.Email) as RestaurantUser;
                var role = _userManager.GetRolesAsync(appUser).Result;

                if (role.Contains("Owner"))
                {
                    return new { token = GenerateJwtToken(model.Email, appUser).Result, role = "Owner" };
                }
                else
                {
                    var restaurantsStaff = await _db.RestaurantUsers.Where(ru => ru.StaffLink == appUser.StaffLink).ToListAsync();
                    //&& _userManager.IsInRoleAsync(ru, "Owner").Result
                    var restaurantsOwner = restaurantsStaff.FirstOrDefault(ru => _userManager.IsInRoleAsync(ru, "Owner").Result);

                    if (restaurantsOwner == null)
                    {
                        return NotFound("Owner was not found");
                    }

                    return new { token = GenerateJwtToken(restaurantsOwner.Email, restaurantsOwner).Result, role = role };
                }

            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> ProcessWebhookEvent()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            // Uncomment and replace with a real secret. You can find your endpoint's
            // secret in your webhook settings.
            //const string webhookSecret = "whsec_pyf1x8ZDXMPGqIkeSHEzAZXNCdoEV0T8"; //test webhook

            const string webhookSecret = "whsec_yiEsndVb4Qo3kIobMEiBubVIpafND1bl";

            // Verify webhook signature and extract the event.
            // See https://stripe.com/docs/webhooks/signatures for more information.

            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], webhookSecret);

            if (stripeEvent.Type == Events.AccountUpdated)
            {
                var account = stripeEvent.Data.Object as Stripe.Account;
                //return Ok(account.PayoutsEnabled);
                if (account.PayoutsEnabled == true)
                {
                    var restUser = await _db.RestaurantUsers.FirstOrDefaultAsync(Ruddy => Ruddy.ConnectedAccountId == account.Id);
                    restUser.IsStripeAccountCompleted = true;
                    await _db.SaveChangesAsync();
                }
            }

            return Ok(stripeEvent);
        }

        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgot)
        {
            RestaurantUser user = await _db.RestaurantUsers.FirstOrDefaultAsync(u => u.Email == forgot.Email);
            if (user == null) return BadRequest();

            Random randomPassword = new Random();
            var password = randomPassword.Next(10, 99).ToString();
            password += CreatePassword(3) + "!";
            string userToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, userToken, password);
            user.EmailConfirmed = true;
            _db.RestaurantUsers.Update(user);
            //_db.Update(user);
            _db.SaveChanges();

            _emailSender.SendEmailAsync(
            user.Email,
            "New Password",
            @"<h3>Hi, " + user.FirstName + " " + user.LastName + @"</h3><p>Your request of changing the password has processed. Use the new password below to sign in:<p/>"
              + password +
            @"<h3>Thanks,
              </br>Ruddy</h4>"
            ).Wait();

            return Ok();
        }
        private string CreatePassword(int length)
        {
            const string valid = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            const string valid1 = "abcdefghijklmnopqrstuvwxyz";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            for (int i = 0; i < 5; i++)
            {
                res.Append(valid1[rnd.Next(valid1.Length)]);
            }

            return res.ToString();
        }
        private async Task<object> GenerateJwtToken(string email, Models.Account user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("Email", email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, _userManager.GetRolesAsync(user).Result.Aggregate("", (sum, next) =>
                {
                    if (string.IsNullOrEmpty(sum))
                    {
                        sum = next;
                    }
                    else
                    {
                        sum += " ," + next;
                    }
                    return sum;
                })),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

            var token = new JwtSecurityToken(
                _configuration["Jwt:Issuer"],
                _configuration["Jwt:Issuer"],
                claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
