using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ruddy.WEB.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Ruddy.WEB.Services
{
    public class DiscountsHostedService : IHostedService
    {
        //private readonly ApplicationDbContext _context;
        private readonly IServiceScopeFactory _scopeFactory;
        private Timer _timer;

        public DiscountsHostedService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            //SheduledTask

            
            TimeSpan interval = TimeSpan.FromHours(24);
            //calculate time to run the first time & delay to set the timer
            //DateTime.Today gives time of midnight 00.00
            var nextRunTime = DateTime.Today.AddDays(1).AddHours(1);
            var curTime = DateTime.Now;
            var firstInterval = nextRunTime.Subtract(curTime);

            Action action = () =>
            {
                var t1 = Task.Delay(firstInterval);
                t1.Wait();
                //remove inactive accounts at expected time
                BackgroundProcessing(null);
                //now schedule it to be called every 24 hours for future
                // timer repeates call to RemoveScheduledAccounts every 24 hours.
                _timer = new Timer(
                    BackgroundProcessing,
                    null,
                    TimeSpan.Zero,
                    interval
                );
            };

            // no need to await this call here because this task is scheduled to run much much later.
            Task.Run(action);
            return Task.CompletedTask;
            

            //
            // timer repeates call to RemoveScheduledAccounts every 24 hours.
            /*
            _timer = new Timer(
                BackgroundProcessing,
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(2)
            );

            return Task.CompletedTask;
            */
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        private async void BackgroundProcessing(object state)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                await RemoveInactiveDiscounts(context);
                await ActivateNewDiscounts(context);
            }
            
        }

        private async Task RemoveInactiveDiscounts(ApplicationDbContext _context)
        {
            var expiredDiscounts = await _context.Discounts.Include(d => d.DiscountDishes).Where(d => d.To.Date < DateTime.UtcNow.Date).SelectMany(d => d.DiscountDishes.Select(dd => dd.DishId)).ToListAsync();
            var dishes = _context.Dishes.Where(d => expiredDiscounts.Contains(d.Id)).ToList();
            foreach (var d in dishes)
            {
                d.IsPromotional = false;
                d.PromotionalPrice = default(double);
            }

            await _context.SaveChangesAsync();
        }

        private async Task ActivateNewDiscounts(ApplicationDbContext _context)
        {
            var newDiscounts = await _context.Discounts.Include(d => d.DiscountDishes).ThenInclude(dd => dd.Dish).Where(d => d.From.Date == DateTime.UtcNow.Date).ToListAsync();

            newDiscounts.ForEach(nd => nd.DiscountDishes.ForEach(nddd =>
            {
                nddd.Dish.IsPromotional = true;
                nddd.Dish.PromotionalPrice = nddd.Dish.Price * ((100 - nd.Percent) / 100.0);
            }));

            await _context.SaveChangesAsync();
        }
    }
}
