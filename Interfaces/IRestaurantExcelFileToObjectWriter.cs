using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.Models;

namespace Ruddy.WEB.Interface
{
    public interface IRestaurantExcelFileToObjectWriter
    {
        public Task<Dictionary<string, object>> UploadFileRestaurants(IFormFile file, ApplicationDbContext context);
        public Task<Dictionary<string, object>> UploadFileMenu(IFormFile file, ApplicationDbContext context);
        public Task<Dictionary<string, object>> UploadFileMenuS3(IFormFile file, ApplicationDbContext context);
        public Task<string> saveToJournal(int? dishId, ApplicationUser user, ApplicationDbContext context);

    }

}