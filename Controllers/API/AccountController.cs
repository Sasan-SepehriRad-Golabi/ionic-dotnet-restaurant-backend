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
using Ruddy.WEB.Services;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Models;
using Ruddy.WEB.Enums;

namespace Ruddy.WEB.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<Models.Account> _userManager;
        private readonly SignInManager<Models.Account> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHasher _hasher;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IMediaStorageService _mediaStorageService;

        public AccountController(IMapper mapper, UserManager<Models.Account> userManager,
            SignInManager<Models.Account> signInManager, ApplicationDbContext db,
        IConfiguration configuration, RoleManager<IdentityRole> roleManager,
            IEmailSender emailSender, IWebHostEnvironment hostEnvironment, IHasher hasher,
            INotificationService notificationService, IMediaStorageService mediaStorageService
            )
        {
            _mapper = mapper;
            _hasher = hasher;
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _emailSender = emailSender;
            _webHostEnvironment = hostEnvironment;
            _notificationService = notificationService;
            _mediaStorageService = mediaStorageService;
        }

        [HttpPost("register")]
        public async Task<object> Register([FromBody] RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                EmailConfirmed = false,
                PhoneNumber = model.PhoneNumber
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                //await _userManager.AddToRoleAsync(user, "user");

                await _signInManager.SignInAsync(user, false);
                // var code = _userManager.GenerateEmailConfirmationTokenAsync(user);

                // var callbackUrl = Url.Action(
                //     "ConfirmEmail",
                //     "Account",
                //     new { userId = user.Id, code = code.Result },
                //     protocol: HttpContext.Request.Scheme);

                /*_emailSender.SendEmailAsync(model.Email, "Confirm your account",
                     $"Click to the link to confirm your account: <a href='{callbackUrl}'>link</a>").Wait();
                */
                return Ok(new { token = GenerateJwtToken(model.Email, user).Result/*, role = _userManager.GetRolesAsync(user).Result.FirstOrDefault()*/ });
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
        [HttpGet("CreateCustomerAccount")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> CreateCustomerAccount()
        {
            var user = _db.ApplicationUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            var options = new CustomerCreateOptions { Email = user.Email, Name = user.FirstName + " " + user.LastName };
            var service = new CustomerService();
            var customer = service.Create(options);
            user.CustomerAccountId = customer.Id;

            var couponOptions = new CouponCreateOptions
            {
                Id = customer.Id,
                Name = customer.Id,
                AmountOff = 50,
                Duration = "once",
                Currency = "eur",
                MaxRedemptions = 30,
                RedeemBy = DateTime.UtcNow.AddMonths(1)
            };
            var couponService = new CouponService();
            couponService.Create(couponOptions);

            await _db.SaveChangesAsync();

            var model = new Coupons()
            {
                ApplicationUserId = user.Id,
                CouponId = couponOptions.Id,
                Enable = true
            };
            _db.Coupons.Add(model);
            _db.SaveChanges();

            return Ok("Customer account created successfully");
        }

        [HttpGet("CreatePaymentIntent")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<string>> CreatePaymentIntent()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name).ConfigureAwait(true) as ApplicationUser;
            if (user == null)
            {
                return NotFound("User not found");
            }


            var paymentIntentOptions = new PaymentIntentCreateOptions
            {
                Amount = 100,
                Currency = "eur",
                Customer = user.CustomerAccountId,
                PaymentMethodTypes = new List<string>()
                {
                    "card",
                    "bancontact"
                }

            };

            var paymentIntentService = new PaymentIntentService();

            var paymentIntent = paymentIntentService.Create(paymentIntentOptions);

            return Ok(paymentIntent.Id);
        }
        [HttpGet("CreatePaymentSecret")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<string>> CreatePaymentSecret()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name).ConfigureAwait(true) as ApplicationUser;
            if (user == null)
            {
                return NotFound("User not found");
            }

            var order = await _db.Orders.Where(o => o.ApplicationUserId == user.Id).OrderByDescending(o => o.CreationDate).FirstOrDefaultAsync();
            order.TypeOfPayment = TypeOfPayment.Bancontact;
            _db.Orders.Update(order);
            var paymentIntentOptions = new PaymentIntentCreateOptions
            {
                Amount = 100,
                Currency = "eur",
                Customer = "cus_JVmTVb6looEHQy",
                PaymentMethodTypes = new List<string>()
                {
                    "bancontact"
                }


            };

            var paymentIntentService = new PaymentIntentService();

            var paymentIntent = paymentIntentService.Create(paymentIntentOptions);

            return Ok(paymentIntent.ClientSecret);
        }
        [HttpGet("CreatePaymentMethod/{paymentIntent}/{tokenCard}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> CreatePaymentMethod(string paymentIntent, string tokenCard)
        {
            var user = _db.ApplicationUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            //link card to customer
            var cardOptions = new CardCreateOptions
            {
                Source = tokenCard
            };
            var cardService = new CardService();
            var card = cardService.Create(user.CustomerAccountId, cardOptions);



            /*
            var customerUpdateOptions = new CustomerUpdateOptions
            {
                
                Source = new AnyOf<string, CardCreateNestedOptions>(card.Id)
            };
            var customerService = new CustomerService();
            customerService.Update(user.CustomerAccountId, customerUpdateOptions);
            */
            //var options = new PaymentMethodCreateOptions
            //{
            //    Customer = user.CustomerAccountId,
            //    PaymentMethod = paymentMethodId,
            //};

            //var requestOptions = new RequestOptions
            //{
            //    StripeAccount = user.ConnectedAccountId,
            //};

            //var pmService = new PaymentMethodService();

            //var paymentMethod = pmService.Create(options, requestOptions);

            var customerUpdateOptions = new CustomerUpdateOptions
            {
                DefaultSource = card.Id
            };
            var customerService = new CustomerService();
            customerService.Update(user.CustomerAccountId, customerUpdateOptions);

            var service = new PaymentIntentService();
            service.Cancel(paymentIntent);

            return Ok(card);
        }

        [HttpGet("PaymentMethods")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<List<PaymentMethodViewModel>>> GetPaymentMethods()
        {
            var user = _db.ApplicationUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            var customerService = new CustomerService();
            var customerAccount = customerService.Get(user.CustomerAccountId);

            var options = new PaymentMethodListOptions
            {
                Customer = user.CustomerAccountId,
                Type = "card",


            };
            var options2 = new PaymentMethodListOptions
            {
                Customer = user.CustomerAccountId,
                Type = "bancontact",


            };
            var service = new PaymentMethodService();
            StripeList<PaymentMethod> paymentMethods = service.List(
              options
            );
            paymentMethods.Data.AddRange(service.List(options2).Data);

            var paymentMethodModels = paymentMethods.Select(pm => new PaymentMethodViewModel()
            {
                Id = pm.Id,
                ExpMonth = pm.Card.ExpMonth.ToString(),
                ExpYear = pm.Card.ExpYear.ToString(),
                Last4 = pm.Card.Last4,
                Brand = pm.Card.Brand,
                IsDefault = pm.Id == customerAccount.DefaultSourceId
            });

            return Ok(paymentMethodModels);
        }

        [HttpDelete("PaymentMethods/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> GetCoupons(string id)
        {
            var service = new PaymentMethodService();
            service.Detach(id);
            return Ok();
        }

        [HttpGet("Coupons")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CouponViewModel>> GetCoupons()
        {
            var user = _db.ApplicationUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            var service = new CouponService();
            Coupon couponId;
            try
            {
                var couponsId = "";
                var userCouponId = _db.Coupons.Where(x => x.ApplicationUserId == user.Id && x.Enable).OrderByDescending(x => x.Id).ToList();
                foreach (var item in userCouponId)
                {
                    couponId = service.Get(item.CouponId);


                    if (couponId.Valid)
                    {
                        couponsId = item.CouponId;
                        break;
                    }

                }

                var coupon = service.Get(couponsId);
                if (!coupon.Valid)
                    throw new Exception("invalid coupone");
                return Ok(new CouponViewModel()
                {
                    AmountOff = coupon.AmountOff,
                    ExpireTime = coupon.RedeemBy ?? DateTime.Now.AddMonths(1),
                    RedemptionsCount = coupon.MaxRedemptions ?? 1 - coupon.TimesRedeemed,
                    CouponId = coupon.Id,
                    PercentOff = coupon.PercentOff

                });
            }
            catch (StripeException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("ListCoupons")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CouponListViewModel>> GetListCoupons()
        {
            var user = _db.ApplicationUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            var coupons = await _db.Coupons.Where(u => u.ApplicationUserId == user.Id).OrderByDescending(x => x.Id).Select(x => new Coupons
            {
                CouponId = x.CouponId,
                Id = x.Id,
                Enable = x.Enable
            }).ToListAsync();
            var service = new CouponService();
            try
            {
                var model = new List<CouponListViewModel>();
                foreach (var item in coupons)
                {
                    var coupon = service.Get(item.CouponId);

                    if (coupon.Valid)
                    {

                        model.Add(new CouponListViewModel()
                        {
                            Id = item.Id,
                            //Name = (coupon.PercentOff > 0 ? $"\n Percent Off: {coupon.PercentOff}%": $"\n Amount Off: {coupon.AmountOff} {coupon.Currency}"),
                            Name = coupon.Name,
                            CouponId = item.CouponId,
                            PercentOff = coupon.PercentOff,
                            AmountOff = coupon.AmountOff,
                            ExpireTime = coupon.RedeemBy != null ? coupon.RedeemBy.Value.ToShortDateString() : null,
                            RedemptionsCount = coupon.MaxRedemptions - coupon.TimesRedeemed,
                            Currency = coupon.Currency
                        });
                    }
                }

                return Ok(model);
            }
            catch (StripeException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("AddCoupons")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CouponViewModel>> AddCoupons(AddCouponViewModel couponViewModel)
        {
            var user = _db.ApplicationUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            var service = new CouponService();
            try
            {
                var coupon = service.Get(couponViewModel.CouponId);
                if (coupon.Valid)
                {
                    var coupons = await _db.Coupons.Where(u => u.ApplicationUserId == user.Id && u.CouponId == couponViewModel.CouponId).FirstOrDefaultAsync();
                    if (coupons == null)
                    {
                        var model = new Coupons()
                        {
                            ApplicationUserId = user.Id,
                            CouponId = coupon.Id,
                            Enable = couponViewModel.Enable,
                        };
                        _db.Coupons.Add(model);
                        _db.SaveChanges();
                    }
                    else
                    {
                        return Conflict();
                    }


                }

                return Ok();
            }
            catch (StripeException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPut("Coupons")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CouponViewModel>> UpdateCoupons(UpdateCouponViewModel couponViewModel)
        {
            var user = _db.ApplicationUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            try
            {
                var coupon = await _db.Coupons.Where(x => x.Id == couponViewModel.Id && x.ApplicationUserId == user.Id).FirstOrDefaultAsync();
                coupon.Enable = couponViewModel.Enable;
                _db.SaveChanges();

                return Ok();
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpDelete("Coupons")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<CouponViewModel>> DeleteCoupons(UpdateCouponViewModel couponViewModel)
        {
            var user = _db.ApplicationUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            try
            {
                var coupon = await _db.Coupons.Where(x => x.Id == couponViewModel.Id && x.ApplicationUserId == user.Id).FirstOrDefaultAsync();
                _db.Coupons.Remove(coupon);
                _db.SaveChanges();

                return Ok();
            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPut("DefaultPaymentMethod/{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult> SetDefaultPaymentMethod(string id)
        {
            var user = _db.ApplicationUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();

            var customerUpdateOptions = new CustomerUpdateOptions
            {
                DefaultSource = id
            };
            var customerService = new CustomerService();
            customerService.Update(user.CustomerAccountId, customerUpdateOptions);

            return Ok();
        }

        [HttpGet("getuserbytoken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> GetUserByToken()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            // var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            if (user == null)
            {
                return NotFound();
            }

            var resultUser = _mapper.Map<UserViewModel>(user);

            var options = new PaymentMethodListOptions
            {
                Customer = user.CustomerAccountId,
                Type = "card",
            };
            // // var service = new PaymentMethodService();
            // // StripeList<PaymentMethod> paymentMethods = service.List(options);

            // // resultUser.Last4 = paymentMethods.FirstOrDefault(pm => pm.Card != null)?.Card.Last4;

            //resultUser.Orders.ForEach(o => o.ApplicationUser = null);

            return resultUser;
        }

        [HttpGet("{userId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> GetUserById(string userId)
        {
            var user = await _db.ApplicationUsers.Include(m => m.Friends).Include(n => n.Orders).ThenInclude(h => h.OrderedItems).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound();
            }
            var resultUser = _mapper.Map<UserViewModel>(user);

            var options = new PaymentMethodListOptions
            {
                Customer = user.CustomerAccountId,
                Type = "card",
            };
            var service = new PaymentMethodService();
            StripeList<PaymentMethod> paymentMethods = service.List(options);

            resultUser.Last4 = paymentMethods.FirstOrDefault(pm => pm.Card != null)?.Card.Last4;

            return resultUser;
        }
        //TODO changing password
        [HttpPut("updateuser")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> UpdateUser([FromBody] UserViewModel model)
        {
            var user = _db.ApplicationUsers.Where(u => u.UserName == User.Identity.Name).FirstOrDefault();
            user.PhoneNumber = model.PhoneNumber;
            user.Gender = model.Gender;
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Gender = model.Gender;
            user.Height = model.Height;
            user.Weight = model.Weight;
            user.BirthDate = model.BirthDate;
            user.LevelOfActivity = model.LevelOfActivity;

            _db.ApplicationUsers.Update(user);

            await _db.SaveChangesAsync();

            return Ok();

        }

        [HttpPut("Credentials")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> PutCredential([FromBody] LoginViewModel model)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (!string.IsNullOrEmpty(model?.Email))
            {
                var emailResult = await _userManager.SetEmailAsync(user, model.Email);
                var userNameResult = await _userManager.SetUserNameAsync(user, model.Email);
                if (!emailResult.Succeeded || !userNameResult.Succeeded)
                {
                    return BadRequest(emailResult.Errors.Aggregate("", (sum, next) => sum += next.Description + " ") + " " + userNameResult.Errors.Aggregate("", (sum, next) => sum += next.Description + " "));
                }
            }

            if (!string.IsNullOrEmpty(model?.Password))
            {
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, resetToken, model.Password);

                if (!passwordResult.Succeeded)
                {
                    return BadRequest(passwordResult.Errors.Aggregate("", (sum, next) => sum += next.Description + " "));
                }
            }

            return new { token = GenerateJwtToken(model.Email, user).Result };
        }

        [HttpPost("uploadimage")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<object> UploadImage(IFormFile image)
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);

            if (image != null)
            {
                user.ProfileImage = await _mediaStorageService.SaveMediaAsync(image);
                _db.ApplicationUsers.Update(user);
                await _db.SaveChangesAsync();
            }
            else
            {
                return BadRequest();
            }
            return Ok(user.ProfileImage);
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

        [HttpPost("SupportToRuddy")]
        public async Task<IActionResult> SupportToRuddy(SupportViewModel model)
        {
            await _emailSender.SendEmailAsync("hello@ruddy.app", model.Subject, $"{model.Message}\n message by {model.Email}");
            await _emailSender.SendEmailAsync(model.Email, model.Subject, $"{model.Message}\n message by {model.Email}");
            return Ok();
        }

        [HttpPost("login")]
        public async Task<object> Login([FromBody] LoginViewModel model)
        {
            var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, false, false);

            if (result.Succeeded)
            {
                var appUser = _userManager.Users.SingleOrDefault(r => r.UserName == model.Email);

                return new { token = GenerateJwtToken(model.Email, appUser).Result };
            }
            else
            {
                return Unauthorized();
            }
        }
        [AllowAnonymous]
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgot)
        {
            var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Email == forgot.Email);
            if (user == null) return BadRequest();

            Random randomPassword = new Random();
            var password = randomPassword.Next(10, 99).ToString();
            password += CreatePassword(3) + "A!";
            string userToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, userToken, password);
            user.EmailConfirmed = true;
            _db.ApplicationUsers.Update(user);
            //_db.Update(user);
            _db.SaveChanges();

            _emailSender.SendEmailAsync(
            user.Email,
            "New Password",
            @"<h3>Hi, " + user.FirstName + " " + user.LastName + @"</h3><p>Your request of changing the password has processed. Use the new password below to sign in:<p/>"
              + password +
            @"</br><p>Thanks, Ruddy</p>"
            ).Wait();

            return Ok();
        }

        [HttpGet("Statistics")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<StatisticsViewModel>> GetStatistics()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name) as ApplicationUser;

            if (user == null)
            {
                return NotFound("User not found");
            }

            var orders = await _db.Orders
                .Include(o => o.OrderedItems)
                    .ThenInclude(oi => oi.ItemСharacteristics)
                .Where(o => o.CreationDate.AddMonths(1) >= DateTime.UtcNow && o.ApplicationUserId == user.Id && o.OrderStatus == Status.Success).ToListAsync();
            var orderStats = new List<OrderСharacteristics>();

            foreach (var o in orders)
            {
                var oCharacteristics = new OrderСharacteristics();
                oCharacteristics.OrderDate = o.CreationDate;
                o.OrderedItems.ForEach(oi => oCharacteristics.AddIngridient(oi.ItemСharacteristics, oi.Count));
                orderStats.Add(oCharacteristics);
            };

            orderStats.OrderBy(os => os.OrderDate);

            var stats = new StatisticsViewModel()
            {
                BirthDate = user.BirthDate,
                Gender = user.Gender,
                Height = user.Height,
                Weight = user.Weight,

                LevelOfActivity = user.LevelOfActivity,
                OrderСharacteristics = orderStats
            };



            return Ok(stats);
        }

        [HttpPut("RegisterFCMToken")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> RegisterFCMToken([FromQuery] string fcmToken)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            var tokent = await _db.FcmTokens.FirstOrDefaultAsync(t => t.FcmToken == fcmToken);

            if (string.IsNullOrEmpty(fcmToken)) return BadRequest("FcmToken can't be null or empty");

            if (tokent != null)
            {
                tokent.AccountId = user.Id;
            }
            else
            {
                _db.FcmTokens.Add(new FcmTokens() { FcmToken = fcmToken, AccountId = user.Id });
            }

            await _db.SaveChangesAsync();

            return Ok("Token was added");
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
        private async Task<object> GenerateJwtToken(string email, IdentityUser user)
        {
            var claims = new List<Claim>
            {

                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Email", email),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.UserName)
            };
            if(!string.IsNullOrEmpty(_configuration["Jwt:Admin"]))
            {
 var x = _configuration["Jwt:Admin"].ToString();
            if (email == _configuration["Jwt:Admin"].ToString())
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            }
            // else
            // {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id));
            // }
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
