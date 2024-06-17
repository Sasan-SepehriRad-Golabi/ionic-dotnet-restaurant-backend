using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.EntityFrameworkCore;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Hubs;
using Ruddy.WEB.Services;
using Ruddy.WEB.ViewModels;
using Stripe;
using Ruddy.WEB.Models;
using Ruddy.WEB.Enums;

namespace Ruddy.WEB.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OrdersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<OrdersHub> _hubContext;
        private readonly INotificationService _notificationService;
        private readonly UserManager<Models.Account> _userManager;
        private readonly IMapper _mapper;

        public OrdersController(ApplicationDbContext context, IHubContext<OrdersHub> hubContext, IMapper mapper, INotificationService notificationService, UserManager<Models.Account> userManager)
        {
            _context = context;
            _hubContext = hubContext;
            _mapper = mapper;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        // GET: api/Orders
        [HttpGet]
        private async Task<ActionResult<IEnumerable<Models.Order>>> GetOrders()
        {
            var user = await _context.RestaurantUsers.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            var result = await _context.Orders.Include(o => o.Restaurant)
                //.Where(g => g.Restaurant.RestaurantUserId == user.Id)
                //.Include(o => o.ApplicationUser)
                //.Include(el => el.OrderedItems)
                //    .ThenInclude(e => e.Dish)
                //        .ThenInclude(p => p.Components)
                //            .ThenInclude(y => y.Ingredient)
                //.Include(el => el.OrderedItems)
                //    .ThenInclude(e => e.Dish)
                //        .ThenInclude(p => p.DietaryType)
                //.Include(el => el.OrderedItems)
                //    .ThenInclude(oi => oi.OrderedIngredients)
                .ToListAsync();

            result.ForEach(r =>
            {
                r.Restaurant.RestaurantUser = null;
                r.ApplicationUser.Orders = null;
            });

            return result;
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        private async Task<ActionResult<Models.Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        [HttpGet("Count")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        private async Task<ActionResult<OrdersCountViewModel>> GetOrder([FromQuery] Status orderStatus)
        {
            var restaurantUser = await _userManager.FindByNameAsync(User.Identity.Name) as RestaurantUser;
            var restaurantsIds = await _context.Restaurants.Where(r => r.RestaurantUserId == restaurantUser.Id).Select(r => r.Id).ToListAsync();

            //var ordersCount = await _context.Orders.Where(o => o.Status == orderStatus && restaurantsIds.Contains(o.RestaurantId ?? 0)).ToListAsync();

            var result = new List<OrdersCountViewModel>();

            //foreach(var id in restaurantsIds)
            //{
            //    result.Add(new OrdersCountViewModel()
            //    {
            //        RestaurantId = id,
            //        Count = ordersCount.Where(o => o.RestaurantId == id).Count()
            //    });
            //}

            //if (ordersCount == null)
            //{
            //    return NotFound();
            //}

            return Ok(result);
        }

        // PUT: api/Orders/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        private async Task<IActionResult> PutOrder(int id, Models.Order order)
        {
            var user = await _userManager.FindByNameAsync(User.Identity.Name);

            if (id != order.Id)
            {
                return BadRequest();
            }

            _context.Entry(order).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrderExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            //switch (order.Status)
            //{
            //    case Status.IsReceived:
            //        await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Restaurant has received your order", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
            //        break;
            //    case Status.PaymentSuccessfull:
            //        await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Payment was successful", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
            //        break;
            //    case Status.BeingPrepared:
            //        await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Your order is being prepared", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
            //        break;
            //    case Status.RuddyForPickup:
            //        await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Your order is Ruddy. Go pick it up ;)", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
            //        break;
            //    case Status.Success:
            //        await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Enjoy your meal :). Check out your health journal to know more about what you just ate.", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
            //        break;
            //    case Status.Rejected:
            //        await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Your order was rejected by the restaurant🙃", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
            //        break;
            //}

            return NoContent();
        }
        [HttpPost("webhook")]
        public async Task<IActionResult> OrderStatus()
        {
            try
            {
                var json = new StreamReader(HttpContext.Request.Body).ReadToEnd();
                var stripeEvent = EventUtility.ConstructEvent(json,
                    Request.Headers["Stripe-Signature"],
                    "whsec_e080867f5657784f60b4921d3f80e1b5b4cc2487aef73c6ea4485ea60baddb0c");
                PaymentIntent intent = null;

                switch (stripeEvent.Type)
                {
                    case "payment_intent.succeeded":
                        intent = (PaymentIntent)stripeEvent.Data.Object;
                        //_context.Orders
                        // Fulfil the customer's purchase

                        break;
                    case "payment_intent.payment_failed":
                        intent = (PaymentIntent)stripeEvent.Data.Object;

                        // Notify the customer that payment failed

                        break;
                    default:
                        // Handle other event types

                        break;
                }
                return new EmptyResult();

            }
            catch (StripeException e)
            {
                // Invalid Signature
                return BadRequest();
            }
        }
        [HttpPatch("{id}/Status")]
        private async Task<IActionResult> PutOrder(int id, [FromQuery] Status status)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            var order = await _context.Orders.Include(o => o.Restaurant).FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound("Order not found");
            }

            //order.Status = status;

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
                //await _notificationService.SendNotificationToRestaurants(order.Restaurant.RestaurantUserId, order.RestaurantId, $"NEW MESSAGE FROM RUDDY", "Order was cancelled", new Dictionary<string, string>() { { "restaurantId", $"{order.RestaurantId}" }, { "orderStatus", $"{order.Status}" } });
            }

            //switch (order.Status)
            //{
            //    case Status.IsReceived:
            //        await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Restaurant has received your order", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
            //        break;
            //    case Status.PaymentSuccessfull:
            //        await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Payment was successful", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
            //        break;
            //    case Status.BeingPrepared:
            //        await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Your order is being prepared", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
            //        break;
            //    case Status.RuddyForPickup:
            //        await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Your order is Ruddy. Go pick it up ;)", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
            //        break;
            //    case Status.Success:
            //        await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Enjoy your meal :). Check out your health journal to know more about what you just ate.", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
            //        break;
            //        /*
            //    case Status.Rejected:
            //        await _notificationService.SendNotificationToUser(order.ApplicationUserId, $"Order #{id}", "Restaurant has reject your order", new Dictionary<string, string>() { { "orderId", $"{order.Id}" }, { "status", $"{order.Status}" } });
            //        break;
            //        */
            //}

            return Ok();
        }

        // POST: api/Orders
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        private async Task<ActionResult<OrderSignalRViewModel>> PostOrder(OrderViewModel model)
        {
            var user = await _context.Accounts.FirstOrDefaultAsync(u => u.Email == User.Identity.Name);
            var restaurant = await _context.Restaurants.FirstOrDefaultAsync(r => r.Id == model.RestarauntId);
            if (restaurant == null || user == null)
            {
                return NotFound();
            }
            var order = new Models.Order
            {
                //Price = model.Price,
                //PromotionalPrice = model.PromotinalPrice,
                //ApplicationUserId = user.Id,
                //Date = DateTime.UtcNow,
                //Restaurant = restaurant
            };


            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            var orderSignalR = _mapper.Map<OrderSignalRViewModel>(order);
            orderSignalR.OrderedItems = new List<OrderedItemViewModel>();

            //foreach (var item in model.DishesIdCount)
            //{

            //    var newOrderedItem = new OrderedItem
            //    {
            //        Count = item.Count,
            //        Comment = item.Comment,
            //        Dish = await _context.Dishes.Include(d => d.Components).ThenInclude(c => c.Ingredient).FirstOrDefaultAsync(el => el.Id == item.Id)
            //    };
            //    order.OrderedItems.Add(newOrderedItem);

            //    var orderedIngridients = new List<OrderedIngredient>();
            //    foreach(var ingr in item.DishComponentsIds)
            //    {
            //        orderedIngridients.Add(new OrderedIngredient() { DishComponentId = ingr, OrderedItem = newOrderedItem });
            //    }

            //    await _context.OrderedIngredients.AddRangeAsync(orderedIngridients).ConfigureAwait(true);

            //    orderSignalR.OrderedItems.Add(_mapper.Map<OrderedItemViewModel>(newOrderedItem));
            //}


            await _hubContext.Clients.Group(order.Restaurant.RestaurantUserId).SendAsync("ReceiveOrder", new { RestaurantId = order.RestaurantId });

            await _context.SaveChangesAsync();

            //await _notificationService.SendNotificationToRestaurants(restaurant.RestaurantUserId, restaurant.Id, $"NEW MESSAGE FROM RUDDY", "You have a new order", new Dictionary<string, string>() { { "restaurantId", $"{order.RestaurantId}" }, { "orderStatus", $"{order.Status}" } });

            return Ok(orderSignalR);
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        private async Task<ActionResult<Models.Order>> DeleteOrder(int id)
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
        /*
        [HttpGet("Refund/{orderId}")]
        public async Task<ActionResult> Refund(int orderId)
        {
            var order = _context.Orders.Include(g=>g.ApplicationUser).Where(e => e.Id == orderId).FirstOrDefault();
            if (order == null) return BadRequest("Order is absent");

            try
            {
                if (order.PaymentIntentId != "")
                {

                    var refunds = new RefundService();
                    var refundOptions = new RefundCreateOptions
                    {
                        PaymentIntent = order.PaymentIntentId
                    };
                    var refund = refunds.Create(refundOptions);

                    var cusOptions = new CustomerBalanceTransactionCreateOptions
                    {
                        Amount = -Convert.ToInt32(order.Price * 100),
                        Currency = "eur",
                    };

                    var service = new CustomerBalanceTransactionService();
                    var cusTransaction = service.Create(order.ApplicationUser.CustomerAccountId, cusOptions);

                }

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            _context.Orders.Remove(order);
            _context.SaveChanges();

            return Ok();
        }
        */

        [HttpGet("Pay/{orderId}")]
        private async Task<object> Pay(int orderId)
        {
            var order = await _context.Orders.Include(h => h.ApplicationUser).Include(a => a.Restaurant).ThenInclude(r => r.RestaurantUser).FirstOrDefaultAsync(i => i.Id == orderId).ConfigureAwait(true);

            try
            {
                string paymentIntent = "";
                //if (order.Price != 0.0)
                //{
                //    //paymentIntent = Charge(order.Price, order.Price, order.ApplicationUser.CustomerAccountId, order.Restaurant.RestaurantUser.ConnectedAccountId);
                //    paymentIntent = CreateInvoice(order, order.ApplicationUser.CustomerAccountId, order.Restaurant.RestaurantUser.ConnectedAccountId);

                //    /*
                //    var cusOptions = new CustomerBalanceTransactionCreateOptions
                //    {
                //        Amount = Convert.ToInt32(order.Price * 100),
                //        Currency = "eur",
                //    };

                //    var service = new CustomerBalanceTransactionService();
                //    var transaction = service.Create(order.Restaurant.RestaurantUser.CustomerAccountId, cusOptions);
                //    */
                //}

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

        private string Charge(double totalPrice, double price, string recipientCustomerId, string restarauntStripeId)
        {
            var paymentIntentOptions = new PaymentIntentCreateOptions
            {
                Amount = Convert.ToInt64(totalPrice * 100),
                Currency = "eur",
                PaymentMethodTypes = new List<string>
                {
                    "card",
                    "bancontact"
                },
                Confirm = true,
                Customer = recipientCustomerId,

                ApplicationFeeAmount = Convert.ToInt64((totalPrice * 100) * 0.1) + Convert.ToInt64(((totalPrice * 100) * 0.1) * 0.21),
                TransferData = new PaymentIntentTransferDataOptions()
                {
                    //Amount = Convert.ToInt64(((totalPrice * 100) - 10) * 0.9),
                    Destination = restarauntStripeId
                }
            };
            /*
            var requestOptions = new RequestOptions();
            requestOptions.StripeAccount = restarauntStripeId;
            */
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = paymentIntentService.Create(paymentIntentOptions);


            //var options = new AccountCreateOptions
            //{
            //    Type = "express",
            //    Country = "AU",
            //    Email = "jeikkowalenko@gmail.com",
            //    //Capabilities = new AccountCapabilitiesOptions
            //    //{
            //    //    CardPayments = new AccountCapabilitiesCardPaymentsOptions
            //    //    {
            //    //        Requested = true,
            //    //    },
            //    //    Transfers = new AccountCapabilitiesTransfersOptions
            //    //    {
            //    //        Requested = true,

            //    //    },
            //    //    LegacyPayments = new AccountCapabilitiesLegacyPaymentsOptions
            //    //    {
            //    //        Requested = true,
            //    //    },
            //    //}
            //};
            //var service = new AccountService();
            //var accountId = service.Create(options).Id;

            //var transferOptions = new TransferCreateOptions
            //{
            //    Amount = Convert.ToInt32(price * 100),
            //    Currency = "usd",
            //    Destination = accountId,
            //    TransferGroup = $"TRANSACTION-{transactionId}",
            //};

            //var transferService = new TransferService();
            //var transfer = transferService.Create(transferOptions);

            return paymentIntent.Id;
        }

        private string CreateInvoice(Models.Order order, string recipientCustomerId, string restarauntStripeId)
        {
            var productCreateOptions = new ProductCreateOptions
            {
                Name = $"Order #{order.Id}",
            };
            var productService = new ProductService();
            var product = productService.Create(productCreateOptions);

            var priceCreateOptions = new PriceCreateOptions
            {
                //UnitAmount = (long)(order.Price * 100),
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



            var options = new InvoiceCreateOptions
            {
                Customer = recipientCustomerId,
                CollectionMethod = "charge_automatically",
                //ApplicationFeeAmount = Convert.ToInt64((order.Price * 100) * 0.1) + Convert.ToInt64(((order.Price * 100) * 0.1) * 0.21),
                TransferData = new InvoiceTransferDataOptions()
                {
                    Destination = restarauntStripeId,
                    //Amount = (long)(order.Price * 100) - Convert.ToInt64((order.Price * 100) * 0.1) + Convert.ToInt64(((order.Price * 100) * 0.1) * 0.21)
                }
            };

            var couponService = new CouponService();

            Coupon couponId;

            Invoice result;

            try
            {
                couponId = couponService.Get(recipientCustomerId);
                options.Discounts = new List<InvoiceDiscountOptions>()
                {
                    new InvoiceDiscountOptions()
                    {
                        Coupon = couponId.Id
                    }
                };
            }
            catch (Exception ex)
            {
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

        private bool OrderExists(int id)
        {
            return _context.Orders.Any(e => e.Id == id);
        }
    }
}
