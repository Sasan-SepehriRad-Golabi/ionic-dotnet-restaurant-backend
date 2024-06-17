using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Ruddy.WEB.Services
{
    public interface IMediaStorageService
    {
        //public Task<Uri> GetMediaUriByNameAsync(string fileName);
        public Task<Uri> SaveMediaAsync(IFormFile mediaFile);
        public Task DeleteMediaAsync(Uri uri);
    }
}
