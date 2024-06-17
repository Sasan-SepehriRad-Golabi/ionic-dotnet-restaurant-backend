using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Ruddy.WEB.Interface
{
    public interface IPersonExcelFileToObjectWriter
    {
        public Task<Dictionary<string, object>> UploadFile(IFormFile file);

    }

}