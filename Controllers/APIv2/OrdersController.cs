using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Hubs;
using Ruddy.WEB.Services;
using Ruddy.WEB.ViewModels;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using Ruddy.WEB.Models;
using System.Threading.Tasks;
using Ruddy.WEB.Enums;

namespace Ruddy.WEB.Controllers.APIv2
{
    [Route("api/v2/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrdersHub> _hubContext;
        private readonly INotificationService _notificationService;
        private readonly UserManager<Ruddy.WEB.Models.Account> _userManager;
        private readonly IMapper _mapper;

        public OrdersController(ApplicationDbContext context, IHubContext<OrdersHub> hubContext, IMapper mapper, INotificationService notificationService, UserManager<Ruddy.WEB.Models.Account> userManager)
        {
            _context = context;
            _hubContext = hubContext;
            _mapper = mapper;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<RestaurantOrderViewModel>>> GetRestaurantOrders()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            var result = new List<RestaurantOrderViewModel>();

            if (user == null)
            {
                return NotFound("User user was not found");
            }
            else if (user is ApplicationUser)
            {
                user = user as ApplicationUser;
                result = await _context.Orders.Include(o => o.OrderedItems)
                .Where(g => g.ApplicationUserId == user.Id)
                .Include(o => o.ApplicationUser)
                .Include(el => el.OrderedItems)
                    .ThenInclude(oi => oi.OrderedIngredients)
                .Select(o => _mapper.Map<RestaurantOrderViewModel>(o))
                .ToListAsync();
            }
            else if (user is RestaurantUser)
            {
                user = user as RestaurantUser;
                result = await _context.Orders.Include(o => o.OrderedItems)
                .Where(g => g.Restaurant.RestaurantUserId == user.Id)
                .Include(o => o.ApplicationUser)
                .Include(el => el.OrderedItems)
                    .ThenInclude(oi => oi.OrderedIngredients)
                .Select(o => _mapper.Map<RestaurantOrderViewModel>(o))
                .ToListAsync();
            }
            else
            {
                return BadRequest();
            }


            return result;
        }

        // GET: api/Orders/5
        [HttpGet("Last")]
        public async Task<ActionResult<RestaurantOrderViewModel>> GetLastOrder()
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            Models.Order? order;
            if (user != null)
            {
                order = await _context.Orders
                    .Include(el => el.OrderedItems)
                        .ThenInclude(oi => oi.OrderedIngredients)
                    .Where(o => o.ApplicationUserId == user.Id).OrderByDescending(o => o.CreationDate).FirstOrDefaultAsync();
            }
            else
            {
                order = null;
            }

            if (order == null)
            {
                return NotFound();
            }

            return _mapper.Map<RestaurantOrderViewModel>(order);
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<RestaurantOrderViewModel>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return _mapper.Map<RestaurantOrderViewModel>(order);
        }

        [HttpGet("Count")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<OrdersCountViewModel>> GetOrder([FromQuery] Status orderStatus)
        {
            var restaurantUser = await _userManager.FindByNameAsync(User.Identity.Name) as RestaurantUser;
            var restaurantsIds = await _context.Restaurants.Where(r => r.RestaurantUserId == restaurantUser.Id).Select(r => r.Id).ToListAsync();

            var ordersCount = await _context.Orders.Where(o => o.OrderStatus == orderStatus && restaurantsIds.Contains(o.RestaurantId ?? 0)).ToListAsync();

            var result = new List<OrdersCountViewModel>();

            foreach (var id in restaurantsIds)
            {
                result.Add(new OrdersCountViewModel()
                {
                    RestaurantId = id,
                    Count = ordersCount.Where(o => o.RestaurantId == id).Count()
                });
            }

            if (ordersCount == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrder(int id, Ruddy.WEB.Models.Order order)
        {
            var dish = await _context.Dishes.Include(d => d.Components).ThenInclude(c => c.Ingredient).FirstOrDefaultAsync();

            var newOrderedItem = new ItemСharacteristics();

            dish.Components.ForEach(oc => newOrderedItem.AddIngridient(oc.Ingredient, oc.Weight));


            return NoContent();
        }

        [HttpPatch("{id}/Status")]
        public async Task<IActionResult> PutOrder(int id, [FromQuery] Status status)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            var order = await _context.Orders.Include(o => o.Restaurant).FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            order.OrderStatus = status;

            await _context.SaveChangesAsync();
            try
            {
                await _hubContext.Clients.Group(order.Restaurant.RestaurantUserId).SendAsync("CancelOrder", new { RestaurantId = order.RestaurantId });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            if (status == Status.Rejected)
            {
                await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Restaurant has reject your order", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.OrderStatus}" } });
                await _notificationService.SendNotificationToRestaurants(order.Restaurant.RestaurantUserId, order.RestaurantId, $"NEW MESSAGE FROM RUDDY", "Order was cancelled", new Dictionary<string, string>() { { "restaurantId", $"{order.RestaurantId}" }, { "orderStatus", $"{order.OrderStatus}" } });
            }

            switch (order.OrderStatus)
            {
                case Status.IsReceived:
                    await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Restaurant has received your order", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.OrderStatus}" } });
                    break;
                case Status.PaymentSuccessfull:
                    await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Payment was successful", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.OrderStatus}" } });
                    break;
                case Status.BeingPrepared:
                    await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Your order is being prepared", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.OrderStatus}" } });
                    break;
                case Status.RuddyForPickup:
                    await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Your order is Ruddy. Go pick it up ;)", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.OrderStatus}" } });
                    break;
                case Status.Success:
                    await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Enjoy your meal :). Check out your health journal to know more about what you just ate.", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.OrderStatus}" } });
                    break;
                    /*
                case Status.Rejected:
                    await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Restaurant has reject your order", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
                    break;
                    */
            }

            return Ok();
        }

        // POST: api/Orders
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<OrderSignalRViewModel>> PostOrder(OrderViewModel model)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);
            //var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.Id == model.RestarauntId);
            if (restaurant == null || user == null)
            {
                return NotFound();
            }
            var order = new Models.Order
            {
                //Price = model.Price,
                //PromotionalPrice = model.PromotinalPrice,
                ApplicationUserId = user.Id,
                CreationDate = DateTime.UtcNow,
                Restaurant = restaurant
            };

            var orderedItems = new List<OrderedItem>();

            order.OrderedItems = orderedItems;

            var dishesIds = model.DishesIdCount.Select(d => d.Id).ToList();

            var dishes = await _context.Dishes.Include(d => d.Components).ThenInclude(c => c.Ingredient).Where(d => dishesIds.Contains(d.Id)).ToListAsync();

            foreach (var dId in model.DishesIdCount)
            {
                var dish = dishes.FirstOrDefault(d => d.Id == dId.Id);

                var orderedComponents = dish.Components.Where(c => dId.DishComponentsIds.Contains(c.Id) || c.IngredientType == IngredientType.MainIngredient).ToList();

                var newOrderedItem = new OrderedItem
                {
                    Name = dish.Name,
                    Price = dish.Price + orderedComponents.Where(c => c.IngredientType == IngredientType.PaidIngredient).Sum(oc => oc.Price),
                    //PromotionalPrice = d.PromotionalPrice,
                    Image = dish.Image,
                    Comment = dId.Comment,
                    Count = dId.Count,
                    OrderedIngredients = orderedComponents.Where(c => c.IngredientType != IngredientType.MainIngredient)
                    .Select(c => new OrderedIngredient()
                    {
                        IngredientType = c.IngredientType,
                        IngredientNameEng = c.Ingredient.NameEng,
                        IngredientNameEs = c.Ingredient.NameEs,
                        IngredientNameFr = c.Ingredient.NameFr,
                        IngredientNameNl = c.Ingredient.NameNl,
                        Price = c.Price,
                        Weight = c.Weight
                    }).ToList(),
                    ItemСharacteristics = new ItemСharacteristics()
                };

                if (dish.IsPromotional)
                {
                    newOrderedItem.PromotionalPrice = dish.PromotionalPrice + orderedComponents.Where(c => c.IngredientType == IngredientType.PaidIngredient).Sum(oc => oc.Price);
                }
                else
                {
                    newOrderedItem.PromotionalPrice = null;
                }

                orderedComponents.ForEach(oc => newOrderedItem.ItemСharacteristics.AddIngridient(oc.Ingredient, oc.Weight));

                orderedItems.Add(newOrderedItem);
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.Group(order.Restaurant.RestaurantUserId).SendAsync("ReceiveOrder", new { RestaurantId = order.RestaurantId });

            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationToRestaurants(restaurant.RestaurantUserId, restaurant.Id, $"NEW MESSAGE FROM RUDDY", "You have a new order", new Dictionary<string, string>() { { "restaurantId", $"{order.RestaurantId}" }, { "orderStatus", $"{order.OrderStatus}" } });

            return Ok();
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        private async Task<ActionResult<Ruddy.WEB.Models.Order>> DeleteOrder(int id)
        {
            var order = await _context.Orders.Include(h => h.OrderedItems).FirstOrDefaultAsync(g => g.Id == id);
            if (order == null)
            {
                return NotFound();
            }

            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();

            return order;
        }

        [HttpGet("Pay/{orderId}")]
        public async Task<object> Pay(int orderId)
        {
            var order = await _context.Orders.Include(o => o.OrderedItems).Include(h => h.ApplicationUser).Include(a => a.Restaurant).ThenInclude(r => r.RestaurantUser).FirstOrDefaultAsync(i => i.Id == orderId).ConfigureAwait(true);

            try
            {
                string paymentIntent = "";
                if (order.OrderedItems.Sum(oi => oi.PromotionalPrice != null ? oi.PromotionalPrice : oi.Price) != 0.0)
                {
                    //paymentIntent = Charge(order.Price, order.Price, order.ApplicationUser.CustomerAccountId, order.Restaurant.RestaurantUser.ConnectedAccountId);
                    paymentIntent = CreateInvoice(order, order.ApplicationUser.CustomerAccountId, order.Restaurant.RestaurantUser.ConnectedAccountId);

                    /*
                    var cusOptions = new CustomerBalanceTransactionCreateOptions
                    {
                        Amount = Convert.ToInt32(order.Price * 100),
                        Currency = "eur",
                    };

                    var service = new CustomerBalanceTransactionService();
                    var transaction = service.Create(order.Restaurant.RestaurantUser.CustomerAccountId, cusOptions);
                    */
                }

                if (!string.IsNullOrEmpty(paymentIntent))
                {
                    order.PaymentIntentId = paymentIntent;
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();

        }

        private string CreateInvoice(Ruddy.WEB.Models.Order order, string recipientCustomerId, string restarauntStripeId)
        {
            var productCreateOptions = new ProductCreateOptions
            {
                Name = $"Order #{order.Id}",
            };
            var productService = new ProductService();
            var product = productService.Create(productCreateOptions);

            var orderPrice = order.OrderedItems.Sum(oi => oi.PromotionalPrice != null ? oi.PromotionalPrice * oi.Count : oi.Price * oi.Count);

            var priceCreateOptions = new PriceCreateOptions
            {
                UnitAmount = (long)(orderPrice * 100),
                Currency = "eur",
                Product = product.Id,
            };
            var priceService = new PriceService();
            var price = priceService.Create(priceCreateOptions);

            var invoiceItemCreateOptions = new InvoiceItemCreateOptions
            {
                Customer = recipientCustomerId,
                Price = price.Id
            };

            var invoiceItemService = new InvoiceItemService();
            invoiceItemService.Create(invoiceItemCreateOptions);


            InvoiceCreateOptions options = new InvoiceCreateOptions();

            //options = new InvoiceCreateOptions
            // {
            //     Customer = recipientCustomerId,
            //     CollectionMethod = "charge_automatically",
            //     //ApplicationFeeAmount = Convert.ToInt64((orderPrice * 100) * 0.1) + Convert.ToInt64(((orderPrice * 100) * 0.1) * 0.21),
            //     TransferData = new InvoiceTransferDataOptions()
            //     {
            //         Destination = restarauntStripeId,
            //         Amount = (long)(orderPrice * 100) - (Convert.ToInt64((orderPrice * 100) * 0.1) + Convert.ToInt64(((orderPrice * 100) * 0.1) * 0.21))
            //     }
            // };

            var couponService = new CouponService();

            Coupon couponId;

            Invoice result;

            try
            {
                var couponsId = "";
                var userCouponId = _context.Coupons.Where(x => x.ApplicationUserId == order.ApplicationUserId && x.Enable).OrderBy(x => x.Id).ToList();
                foreach (var item in userCouponId)
                {
                    couponId = couponService.Get(item.CouponId);


                    if (couponId.Valid)
                    {
                        couponsId = item.CouponId;
                        break;
                    }

                }

                couponId = couponService.Get(couponsId);
                if (!couponId.Valid)
                    throw new Exception("invalid coupone");

                double amountOff = (double)couponId?.AmountOff;
                double percentOff = (double)couponId?.PercentOff;

                options.Customer = recipientCustomerId;
                options.CollectionMethod = "charge_automatically";
                //ApplicationFeeAmount = Convert.ToInt64((orderPrice * 100) * 0.1) + Convert.ToInt64(((orderPrice * 100) * 0.1) * 0.21),
                options.TransferData = new InvoiceTransferDataOptions()
                {
                    Destination = restarauntStripeId,
                    Amount = percentOff > 0 ?
                    (long)((orderPrice - (orderPrice * amountOff / 100) / 100) * 100) - (Convert.ToInt64(((orderPrice - (orderPrice * amountOff / 100) / 100) * 100) * 0.1) + Convert.ToInt64((((orderPrice - (orderPrice * amountOff / 100) / 100) * 100) * 0.1) * 0.21))
                    : (long)((orderPrice - amountOff / 100) * 100) - (Convert.ToInt64(((orderPrice - amountOff / 100) * 100) * 0.1) + Convert.ToInt64((((orderPrice - amountOff / 100) * 100) * 0.1) * 0.21))
                };
                options.Discounts = new List<InvoiceDiscountOptions>()
                {
                    new InvoiceDiscountOptions()
                    {
                        Coupon = couponId.Id
                    }
                };


                //options.Discounts = new List<InvoiceDiscountOptions>()
                //{
                //    new InvoiceDiscountOptions()
                //    {
                //        Coupon = couponId.Id
                //    }
                //};
            }
            catch (Exception ex)
            {

                options.Customer = recipientCustomerId;
                options.CollectionMethod = "charge_automatically";
                //ApplicationFeeAmount = Convert.ToInt64((orderPrice * 100) * 0.1) + Convert.ToInt64(((orderPrice * 100) * 0.1) * 0.21),
                options.TransferData = new InvoiceTransferDataOptions()
                {
                    Destination = restarauntStripeId,
                    Amount = (long)(orderPrice * 100) - (Convert.ToInt64(((orderPrice) * 100) * 0.1) + Convert.ToInt64(((orderPrice * 100) * 0.1) * 0.21))
                };

                var test = ex.Message;
            }
            finally
            {
                var service = new InvoiceService();
                var invoice = service.Create(options);
                result = service.Pay(invoice.Id);
            }

            return result.Id;
        }

    }
}
