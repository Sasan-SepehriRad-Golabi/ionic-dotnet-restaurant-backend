using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Interface;
using Ruddy.WEB.Models;
using Ruddy.WEB.UnitOfWork;

namespace Ruddy.WEB.Handler
{
    public interface IRestaurantFileHandler
    {
        Task<ActionResult> UploadFileRestaurants(IFormFile file, ApplicationDbContext context);
        Task<ActionResult> UploadFileMenu(IFormFile file, ApplicationDbContext context);
        Task<ActionResult> UploadFileMenuS3(IFormFile file, ApplicationDbContext context);
        Task<ActionResult> saveToJournal(int? dishId, ApplicationUser user, ApplicationDbContext context);

    }
    public class RestaurantFileHandler : IRestaurantFileHandler
    {
        private readonly IRestaurantExcelFileToObjectWriter _exceltoobjecwritert;
        private readonly ApplicationDbContext _mycontext;
        public RestaurantFileHandler(IRestaurantExcelFileToObjectWriter exceltoobjecwritert, ApplicationDbContext context)
        {
            _exceltoobjecwritert = exceltoobjecwritert;
            _mycontext = context;
        }
        public async Task<ActionResult> saveToJournal(int? dishId, ApplicationUser user, ApplicationDbContext context)
        {
            var x = await _exceltoobjecwritert.saveToJournal(dishId, user, context);
            if (x == "ok")
            {
                using (MyUnitOfWork myunit = new MyUnitOfWork(context))
                {
                    string res = await myunit.complete();
                    return new OkObjectResult("Done Successfully");
                }
            }
            else
            {
                return new BadRequestResult();
            }
        }
        public async Task<ActionResult> UploadFileRestaurants(IFormFile file, ApplicationDbContext context)
        {
            Dictionary<string, object> result = await _exceltoobjecwritert.UploadFileRestaurants(file, context);
            if (result.Keys.First<string>() == "Error")
            {
                return new ObjectResult(result.Values.First().ToString());
            }
            else
            {
                if (result.Values.First() is List<Restaurant>)
                {
                    using (MyUnitOfWork myunit = new MyUnitOfWork(context))
                    {
                        string res = await myunit.complete();
                        return new ObjectResult(res);
                    }
                }
                else
                {
                    return new ObjectResult("Something is not ok, call admin.");
                }
            }
        }
        public async Task<ActionResult> UploadFileMenu(IFormFile file, ApplicationDbContext context)
        {
            // await _exceltoobjecwritert.UploadFileMenu(file, context);
            // return new ObjectResult("ok");
            Dictionary<string, object> result = await _exceltoobjecwritert.UploadFileMenu(file, context);
            if (result.Keys.First<string>() == "Error")
            {
                return new ObjectResult(result.Values.First().ToString());
            }
            else
            {
                return new ObjectResult(result.Values.First().ToString());
                //     if (result.Values.First() is List<Restaurant>)
                //     {
                //         using (MyUnitOfWork myunit = new MyUnitOfWork(context))
                //         {
                //             // await myunit.restaurantRepository.addRange(result.Values.First() as List<Restaurant>);
                //             string res = await myunit.complete();
                //             return new ObjectResult(res);
                //         }
                //     }
                //     else
                //     {
                //         return new ObjectResult("Something is not ok, call admin.");
                //     }
            }
        }
        public async Task<ActionResult> UploadFileMenuS3(IFormFile file, ApplicationDbContext context)
        {
            // await _exceltoobjecwritert.UploadFileMenu(file, context);
            // return new ObjectResult("ok");
            Dictionary<string, object> result = await _exceltoobjecwritert.UploadFileMenuS3(file, context);
            if (result.Keys.First<string>() == "Error")
            {
                return new ObjectResult(result.Values.First().ToString());
            }
            else
            {
                return new ObjectResult(result.Values.First().ToString());
                //     if (result.Values.First() is List<Restaurant>)
                //     {
                //         using (MyUnitOfWork myunit = new MyUnitOfWork(context))
                //         {
                //             // await myunit.restaurantRepository.addRange(result.Values.First() as List<Restaurant>);
                //             string res = await myunit.complete();
                //             return new ObjectResult(res);
                //         }
                //     }
                //     else
                //     {
                //         return new ObjectResult("Something is not ok, call admin.");
                //     }
            }
        }
    }
}