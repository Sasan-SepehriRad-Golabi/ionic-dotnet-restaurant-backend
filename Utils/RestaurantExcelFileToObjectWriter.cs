using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using Ruddy.WEB.Models;
using Ruddy.WEB.Interface;
using Ruddy.WEB.DataAccess;
using Ruddy.WEB.UnitOfWork;
using Microsoft.EntityFrameworkCore;
using Ruddy.WEB.Enums;
using System.IO;
using Amazon.Runtime;
using Microsoft.Extensions.Configuration;
using Amazon.S3;
using Amazon.S3.Transfer;

namespace Ruddy.WEB.Utils
{

    public class RestaurantExcelFileToObjectWriter : IRestaurantExcelFileToObjectWriter
    {
        public IConfiguration _config;
        public RestaurantExcelFileToObjectWriter(IConfiguration config)
        {
            _config = config;
        }
        public Restaurant setValueWithTypeChecking(PropertyInfo property, Restaurant resaurant, object value, ref string? chainCode)
        {
            if (property.PropertyType == typeof(string))
            {

                property.SetValue(resaurant, value.ToString());
            }
            else if (property.PropertyType == typeof(Uri))
            {
                try
                {
                    property.SetValue(resaurant, value);
                }
                catch
                {
                }
            }
            else if (property.PropertyType == typeof(double))
            {
                if (property.Name.ToLower() == "latitude" || property.Name.ToLower() == "longitude")
                {
                    property.SetValue(resaurant, (double)value);
                }
                else
                {
                    double x;
                    try
                    {
                        x = Convert.ToDouble((Convert.ToDouble(value.ToString(), CultureInfo.InvariantCulture)).ToString("0.00000", CultureInfo.InvariantCulture));
                    }
                    catch
                    {
                        x = 0;
                    }
                    property.SetValue(resaurant, x);
                }

            }
            else if (property.PropertyType == typeof(int))
            {
                int xx = 0;
                try
                {
                    xx = (int)Convert.ToDouble(value.ToString(), CultureInfo.InvariantCulture);
                    property.SetValue(resaurant, xx);
                }
                catch
                {

                }

            }
            else if (property.PropertyType == typeof(bool))
            {
                if (property.Name.ToLower() == "isunofficialrestaurant" && (value == null || value.ToString() == ""))
                {
                    property.SetValue(resaurant, true);
                    resaurant.GetType().GetProperty("chainCode").SetValue(resaurant, chainCode);
                }
                else if ((property.Name.ToLower() == "isunofficialrestaurant"))
                {
                    try
                    {
                        bool isofficial = Convert.ToBoolean((Convert.ToInt32(value.ToString())));
                        if (isofficial)
                        {
                            property.SetValue(resaurant, false);
                            resaurant.GetType().GetProperty("chainCode").SetValue(resaurant, chainCode);
                        }
                        else
                        {
                            property.SetValue(resaurant, true);
                            resaurant.GetType().GetProperty("chainCode").SetValue(resaurant, chainCode);
                        }
                    }
                    catch
                    {
                        property.SetValue(resaurant, true);
                        resaurant.GetType().GetProperty("chainCode").SetValue(resaurant, chainCode);
                    }
                }
                else
                {
                    property.SetValue(resaurant, Convert.ToBoolean((int)Convert.ToDouble(value.ToString(), CultureInfo.InvariantCulture)));
                }
            }
            else if (property.PropertyType == typeof(Ruddy.WEB.Enums.RestaurantCategory))
            {
                property.SetValue(resaurant, (Ruddy.WEB.Enums.RestaurantCategory)((int)value));
            }
            else
            {
                property.SetValue(resaurant, null);

            }
            return resaurant;
        }

        public async Task<Dictionary<string, object>> UploadFileRestaurants(IFormFile file, ApplicationDbContext context)
        {
            Dictionary<string, object> myDic = new Dictionary<string, object>();
            List<Restaurant> alreadyRestaurants;
            Restaurant foundedRestaurant = null;
            alreadyRestaurants = await context.Restaurants.Include(x => x.Times).AsSplitQuery().ToListAsync();
            try
            {
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var reader = ExcelReaderFactory.CreateReader(file.OpenReadStream()))
                {
                    var excelDataSet = reader.AsDataSet();
                    if (excelDataSet.Tables.Count > 1)
                    {
                        myDic.Add("Error", "More than one sheet");
                        return myDic;
                    }
                    DataTable ExcelSheet = excelDataSet.Tables[0];
                    object?[] columnNames = (ExcelSheet).Rows[0].ItemArray;
                    List<string> EditedColumnNames = new List<string>();
                    bool isThereOpenTime = false;
                    bool isThereCloseTime = false;
                    bool isThereDayTime = false;
                    bool isTherePauses = false;
                    bool isChain = false;
                    foreach (var obj in columnNames)
                    {
                        try
                        {
                            if (obj != null)
                            {
                                string columnName = Regex.Replace(obj.ToString(), @"\s+", "");
                                switch (columnName.ToLower())
                                {
                                    case "restaurantname":
                                        columnName = "name";
                                        break;
                                    case "chain":
                                        isChain = true;
                                        break;
                                    case "addressline1":
                                        columnName = "address";
                                        break;
                                    case "isofficialrestaurant":
                                        columnName = "isunofficialrestaurant";
                                        break;
                                    case "backgroundimage":
                                        columnName = "background";
                                        break;
                                }
                                EditedColumnNames.Add(columnName);
                            }
                            else
                            {
                                EditedColumnNames.Add("");
                            }
                        }
                        catch (System.Exception)
                        {

                            string columnName = "";
                            EditedColumnNames.Add(columnName);
                        }
                    }
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        string? excelRestaurantName = "none";
                        double? excelRestaurantLongitude = (double)0.0;
                        double? excelRestaurantLatitude = (double)0.0;
                        string chainCode = "";
                        Time initTime = new Time();
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            if (EditedColumnNames[k].ToLower() == "closetime")
                            {
                                isThereCloseTime = true;
                                try
                                {
                                    // cells[k] = "00:00";
                                    var x = DateTime.UtcNow;
                                    var d = cells[k].ToString();
                                    var y = d.Split(":");
                                    // var z = DateTime.ParseExact(String.Format("{0}/{1}/{2} {3}:{4}", x.Month, x.Day, x.Year, y[0], y[1]), "M/dd/yyyy HH:mm", CultureInfo.InvariantCulture);
                                    initTime.ClosingTime = DateTime.Parse(d, CultureInfo.GetCultureInfo("en-GB"));
                                }
                                catch (Exception e)
                                {
                                    isThereCloseTime = false;
                                }
                                continue;
                            }
                            if (EditedColumnNames[k].ToLower() == "opentime")
                            {
                                isThereOpenTime = true;
                                try
                                {
                                    // cells[k] = "00:00";
                                    var x = DateTime.UtcNow;
                                    var d = cells[k].ToString();
                                    var y = d.Split(":");
                                    // var z = DateTime.ParseExact(String.Format("{0}/{1}/{2} {3}:{4}", x.Month, x.Day, x.Year, y[0], y[1]), "M/dd/yyyy HH:mm", CultureInfo.InvariantCulture);
                                    initTime.OpeningTime = DateTime.Parse(d, CultureInfo.GetCultureInfo("en-GB"));
                                }
                                catch (Exception e)
                                {
                                    isThereOpenTime = false;
                                }
                                continue;
                            }
                            if (EditedColumnNames[k].ToLower() == "daytime")
                            {
                                isThereDayTime = true;
                                try
                                {
                                    switch (cells[k].ToString().ToLower())
                                    {
                                        case "sunday":
                                            initTime.Day = (DayOfWeek)0;
                                            break;
                                        case "monday":
                                            initTime.Day = (DayOfWeek)1;
                                            break;
                                        case "tuesday":
                                            initTime.Day = (DayOfWeek)2;
                                            break;
                                        case "wednesday":
                                            initTime.Day = (DayOfWeek)3;
                                            break;
                                        case "thursday":
                                            initTime.Day = (DayOfWeek)4;
                                            break;
                                        case "friday":
                                            initTime.Day = (DayOfWeek)5;
                                            break;
                                        case "saturday":
                                            initTime.Day = (DayOfWeek)6;
                                            break;
                                        default:
                                            isThereDayTime = false;
                                            break;
                                    }
                                }
                                catch
                                {
                                    isThereDayTime = false;
                                }
                                continue;
                            }
                            if (EditedColumnNames[k].ToLower() == "name")
                            {
                                excelRestaurantName = cells[k].ToString();
                                continue;
                            }
                            if (EditedColumnNames[k].ToLower() == "longitude")
                            {
                                try
                                {
                                    double x = Convert.ToDouble(cells[k].ToString(), CultureInfo.InvariantCulture);
                                    string x1 = x.ToString("0.00000", CultureInfo.InvariantCulture);
                                    excelRestaurantLongitude = Convert.ToDouble(x1, CultureInfo.InvariantCulture);
                                    continue;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                    excelRestaurantLongitude = (float)0.0;
                                    continue;
                                }
                            }
                            if (EditedColumnNames[k].ToLower() == "latitude")
                            {
                                try
                                {
                                    double x = Convert.ToDouble(cells[k].ToString(), CultureInfo.InvariantCulture);
                                    string x1 = x.ToString("0.00000", CultureInfo.InvariantCulture);
                                    excelRestaurantLatitude = Convert.ToDouble(x1, CultureInfo.InvariantCulture);
                                    continue;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                    excelRestaurantLatitude = (float)0.0;
                                    continue;
                                }
                            }


                        }
                        foundedRestaurant = alreadyRestaurants.Find((x) => nameWithoutSpacesandLower(x.Name) == nameWithoutSpacesandLower(excelRestaurantName) && x.Longitude == excelRestaurantLongitude && x.Latitude == excelRestaurantLatitude);
                        if (foundedRestaurant != null)
                        {
                            var properties = foundedRestaurant.GetType().GetProperties();
                            for (int k = 0; k < EditedColumnNames.Count(); k++)
                            {
                                if (EditedColumnNames[k].ToLower() == "chain")
                                {
                                    if (cells[k] != null)
                                    {
                                        cells[k] = cells[k].ToString().ToLower();
                                        chainCode = cells[k].ToString().ToLower();
                                        chainCode = Regex.Replace(chainCode, @"\t|\n|\r", "");
                                        continue;
                                    }
                                    else
                                    {
                                        cells[k] = "";
                                        chainCode = "";
                                        continue;
                                    }

                                }
                                if (columnNames[k] != null && EditedColumnNames[k] != "" && EditedColumnNames[k].ToLower() != "closetime" && EditedColumnNames[k].ToLower() != "opentime" && EditedColumnNames[k].ToLower() != "daytime")
                                {
                                    foreach (System.Reflection.PropertyInfo item in properties)
                                    {
                                        if (item.Name.ToLower() == EditedColumnNames[k].ToString().ToLower())
                                        {
                                            if (item.Name.ToLower() == "restaurantcategory")
                                            {
                                                string a = cells[k] != null ? cells[k].ToString() : "not provided";
                                                string editted_a = Regex.Replace(a, @"\s+", "").ToLower();
                                                if (editted_a == "fastfood")
                                                {
                                                    cells[k] = 1;
                                                }
                                                else if (editted_a.Contains("fine"))
                                                {
                                                    cells[k] = 2;
                                                }
                                                else if (editted_a.Contains("casual"))
                                                {
                                                    cells[k] = 0;
                                                }
                                                else if (editted_a == "bar")
                                                {
                                                    cells[k] = 3;
                                                }
                                                else if (editted_a == "foodtruck")
                                                {
                                                    cells[k] = 4;
                                                }
                                                else
                                                {
                                                    cells[k] = 5;
                                                }
                                            }
                                            if (item.Name.ToLower() == "latitude")
                                            {
                                                cells[k] = excelRestaurantLatitude;
                                            }
                                            if (item.Name.ToLower() == "longitude")
                                            {
                                                cells[k] = excelRestaurantLongitude;
                                            }
                                            if (item.Name.ToLower() == "background" && cells[k] != null && cells[k].ToString() != "")
                                            {

                                                cells[k] = new Uri("https://web-api-media-storage.s3.eu-central-1.amazonaws.com/" + cells[k]?.ToString(), UriKind.Absolute);
                                            }
                                            if (item.Name.ToLower() == "logo" && cells[k] != null && cells[k].ToString() != "")
                                            {
                                                cells[k] = new Uri("https://web-api-media-storage.s3.eu-central-1.amazonaws.com/" + cells[k]?.ToString(), UriKind.Absolute);

                                            }
                                            setValueWithTypeChecking(item, foundedRestaurant, cells[k], ref chainCode);
                                        }
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            if (foundedRestaurant.Times != null)
                            {
                                if (isThereCloseTime && isThereOpenTime && isThereDayTime)
                                {
                                    Time updatedTime = foundedRestaurant.Times.Find(x => x.Day == initTime.Day);
                                    if (updatedTime != null)
                                    {
                                        updatedTime.OpeningTime = initTime.OpeningTime;
                                        updatedTime.ClosingTime = initTime.ClosingTime;
                                        updatedTime.Restaurant = foundedRestaurant;
                                        context.Times.Update(updatedTime);
                                    }
                                    else
                                    {
                                        initTime.Restaurant = foundedRestaurant;
                                        context.Times.Add(initTime);
                                    }
                                }
                                else if (isThereCloseTime && isThereOpenTime && !isThereDayTime)
                                {
                                    List<Time> times = new List<Time>();
                                    for (int i = 0; i <= 6; i++)
                                    {
                                        Time newTime = new Time();
                                        newTime.ClosingTime = initTime.ClosingTime;
                                        newTime.OpeningTime = initTime.OpeningTime;
                                        newTime.Day = (DayOfWeek)i;
                                        newTime.Restaurant = foundedRestaurant;
                                        times.Add(newTime);

                                    }
                                    foundedRestaurant.Times = times;
                                    context.Restaurants.Update(foundedRestaurant);
                                    // for (int i = 0; i <= 6; i++)
                                    // {
                                    //     Time updatedTime = foundedRestaurant.Times.Find(x => x.Day == (DayOfWeek)i);
                                    //     if (updatedTime != null)
                                    //     {
                                    //         updatedTime.OpeningTime = initTime.OpeningTime;
                                    //         updatedTime.ClosingTime = initTime.ClosingTime;
                                    //         context.Times.Update(updatedTime);
                                    //     }
                                    //     else
                                    //     {
                                    //         initTime.Restaurant = foundedRestaurant;
                                    //         initTime.Day = (DayOfWeek)i;
                                    //         context.Times.Add(initTime);
                                    //     }
                                    // }
                                }
                            }
                            else
                            {
                                if (isThereCloseTime && isThereOpenTime && isThereDayTime)
                                {
                                    initTime.Restaurant = foundedRestaurant;
                                    context.Times.Add(initTime);

                                }
                                else if (isThereCloseTime && isThereOpenTime && !isThereDayTime)
                                {
                                    List<Time> times = new List<Time>();
                                    for (int i = 0; i <= 6; i++)
                                    {
                                        Time newTime = new Time();
                                        newTime.ClosingTime = initTime.ClosingTime;
                                        newTime.OpeningTime = initTime.OpeningTime;
                                        newTime.Day = (DayOfWeek)i;
                                        newTime.Restaurant = foundedRestaurant;
                                        times.Add(newTime);

                                    }
                                    foundedRestaurant.Times = times;
                                    context.Restaurants.Update(foundedRestaurant);
                                }
                            }
                        }
                        else
                        {
                            var restaurant = new Restaurant();
                            var properties = restaurant.GetType().GetProperties();
                            for (int k = 0; k < EditedColumnNames.Count(); k++)
                            {
                                if (EditedColumnNames[k].ToLower() == "chain")
                                {
                                    if (cells[k] != null)
                                    {
                                        cells[k] = cells[k].ToString().ToLower();
                                        chainCode = cells[k].ToString().ToLower();
                                        chainCode = Regex.Replace(chainCode, @"\t|\n|\r", "");
                                        continue;
                                    }
                                    else
                                    {
                                        cells[k] = "";
                                        chainCode = "";
                                        continue;
                                    }

                                }
                                if (columnNames[k] != null && EditedColumnNames[k] != "" && EditedColumnNames[k].ToLower() != "closetime" && EditedColumnNames[k].ToLower() != "opentime" && EditedColumnNames[k].ToLower() != "daytime")
                                {
                                    foreach (System.Reflection.PropertyInfo item in properties)
                                    {
                                        if (item.Name.ToLower() == EditedColumnNames[k].ToString().ToLower())
                                        {
                                            if (item.Name.ToLower() == "restaurantcategory")
                                            {
                                                string a = cells[k] != null ? cells[k].ToString() : "not provided";
                                                string editted_a = Regex.Replace(a, @"\s+", "").ToLower();
                                                if (editted_a == "fastfood")
                                                {
                                                    cells[k] = 1;
                                                }
                                                else if (editted_a.Contains("fine"))
                                                {
                                                    cells[k] = 2;
                                                }
                                                else if (editted_a.Contains("casual"))
                                                {
                                                    cells[k] = 0;
                                                }
                                                else if (editted_a == "bar")
                                                {
                                                    cells[k] = 3;
                                                }
                                                else if (editted_a == "foodtruck")
                                                {
                                                    cells[k] = 4;
                                                }
                                                else
                                                {
                                                    cells[k] = 5;
                                                }
                                            }
                                            if (item.Name.ToLower() == "latitude")
                                            {
                                                cells[k] = excelRestaurantLatitude;
                                            }
                                            if (item.Name.ToLower() == "longitude")
                                            {
                                                cells[k] = excelRestaurantLongitude;
                                            }
                                            if (item.Name.ToLower() == "background" && cells[k] != null && cells[k].ToString() != "")
                                            {

                                                cells[k] = new Uri("https://web-api-media-storage.s3.eu-central-1.amazonaws.com/" + cells[k]?.ToString(), UriKind.Absolute);
                                            }
                                            if (item.Name.ToLower() == "logo" && cells[k] != null && cells[k].ToString() != "")
                                            {
                                                cells[k] = new Uri("https://web-api-media-storage.s3.eu-central-1.amazonaws.com/" + cells[k]?.ToString(), UriKind.Absolute);

                                            }

                                            setValueWithTypeChecking(item, restaurant, cells[k], ref chainCode);
                                        }
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            if (isThereCloseTime && isThereOpenTime && isThereDayTime)
                            {
                                List<Time> times = new List<Time>();
                                initTime.Restaurant = restaurant;
                                times.Add(initTime);
                                restaurant.Times = times;

                            }
                            else if (isThereCloseTime && isThereOpenTime && !isThereDayTime)
                            {
                                List<Time> times = new List<Time>();
                                for (int i = 0; i <= 6; i++)
                                {
                                    Time testTime = new Time()
                                    {
                                        OpeningTime = initTime.OpeningTime,
                                        ClosingTime = initTime.ClosingTime,
                                        Restaurant = restaurant,
                                        Day = (DayOfWeek)i
                                    };
                                    times.Add(testTime);

                                }
                                restaurant.Times = times;
                            }
                            context.Restaurants.Add(restaurant);
                        }
                    }
                }
            }
            catch (System.Exception e)
            {

                myDic.Add("Error", e.Message);
                return myDic;
            }
            myDic.Add("ok", context.Restaurants.ToList());
            return myDic;

        }
        public string nameWithoutSpacesandLower(string name)
        {
            string s = "";
            if (!string.IsNullOrEmpty(name))
            {
                s = Regex.Replace(name.ToLower(), @"\s+", "");
            }
            return s;
        }
        public async Task<Dictionary<string, Object>> uploadToS3(string fileContent, string filename)
        {
            File.WriteAllText(Path.Join(Directory.GetCurrentDirectory(), filename), fileContent.ToString());
            var returnDic = new Dictionary<string, object>();
            returnDic["result"] = new { statusCode = 0, desc = $"{filename} added to S3" };
            return returnDic;
            // try
            // {
            //     BasicAWSCredentials credentials = new BasicAWSCredentials(_config["AWS_Keys:accessKey"],
            // _config["AWS_Keys:secretAccessKeys"]);
            //     AmazonS3Config config = new AmazonS3Config()
            //     {
            //         RegionEndpoint = Amazon.RegionEndpoint.EUCentral1
            //     };
            //     byte[] bytes = Encoding.UTF8.GetBytes(fileContent.ToString());
            //     MemoryStream ms = new MemoryStream(bytes);
            //     TransferUtilityUploadRequest uploadRequest = new TransferUtilityUploadRequest()
            //     {
            //         InputStream = ms,
            //         BucketName = "my-csv-bucket-s21176",
            //         CannedACL = S3CannedACL.NoACL,
            //         Key = filename
            //     };
            //     using (var client = new AmazonS3Client())
            //     {
            //         TransferUtility transferUtility = new TransferUtility(client);
            //         await transferUtility.UploadAsync(uploadRequest);
            //         var returnDic = new Dictionary<string, object>();
            //         returnDic["result"] = new { statusCode = 0, desc = $"{filename} added to S3" };
            //         return returnDic;
            //     }
            // }
            // catch (AmazonS3Exception s3Ex)
            // {
            //     var returnDic = new Dictionary<string, object>();
            //     returnDic["result"] = new { statusCode = 1, desc = s3Ex.Message };
            //     return returnDic;
            // }
            // catch (System.Exception e)
            // {

            //     var returnDic = new Dictionary<string, object>();
            //     returnDic["result"] = new { statusCode = 2, desc = e.Message };
            //     return returnDic;
            // }
        }
        public async Task<Dictionary<string, object>> UploadFileMenuS3(IFormFile file, ApplicationDbContext context)
        {
            context.Database.SetCommandTimeout(3600);
            Dictionary<string, object> myDic = new Dictionary<string, object>();
            List<Restaurant> alreadyRestaurants;
            List<Restaurant> foundedRestaurant = null;
            alreadyRestaurants = await context.Restaurants.Include(x => x.Menu).ToListAsync();
            // try
            // {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var reader = ExcelReaderFactory.CreateReader(file.OpenReadStream()))
            {
                var excelDataSet = reader.AsDataSet();
                if (excelDataSet.Tables.Count > 1)
                {
                    myDic.Add("Error", "More than one sheet");
                    return myDic;
                }
                DataTable ExcelSheet = excelDataSet.Tables[0];
                object?[] columnNames = (ExcelSheet).Rows[0].ItemArray;
                List<string> EditedColumnNames = new List<string>();
                bool isChain = false;
                foreach (var obj in columnNames)
                {
                    try
                    {
                        if (obj != null)
                        {
                            string columnName = Regex.Replace(obj.ToString(), @"\s+", "");
                            switch (columnName.ToLower())
                            {
                                case "restaurantname":
                                    columnName = "name";
                                    break;
                                case "chain":
                                    columnName = "chain";
                                    isChain = true;
                                    break;
                                case "ingredientsubstitute":
                                    columnName = "substitutegroup";
                                    break;
                                case "calories(kcal)":
                                    columnName = "energy_kcal";
                                    break;
                                case "totalfat(grams)":
                                    columnName = "fat_g";
                                    break;
                                case "saturatedfat(grams)":
                                    columnName = "fattyacidssaturated_g";
                                    break;
                                case "totalcarbs(grams)":
                                    columnName = "carbohydrates_g";
                                    break;
                                case "sugars(grams)":
                                    columnName = "sugars_g";
                                    break;
                                case "dietaryfiber(grams)":
                                    columnName = "fibressum_g";
                                    break;
                                case "protein(grams)":
                                    columnName = "proteins_g";
                                    break;
                                case "sodium(mg)":
                                    columnName = "sodium_mg";
                                    break;
                                case "cholestrol(mg)":
                                    columnName = "cholesterol_mg";
                                    break;
                                case "potassium(mg)":
                                    columnName = "potassium_mg";
                                    break;
                                case "polyunsaturatedfat(grams)":
                                    columnName = "fa_polyunsat_sum_g";
                                    break;
                                case "monounsaturatedfat(grams)":
                                    columnName = "fa_monounsat_sum_g";
                                    break;
                                case "transfat(grams)":
                                    columnName = "fa_trans_sum_g";
                                    break;
                                case "vitamina(mg)":
                                    columnName = "vita_activity_mcg";
                                    break;
                                case "vitaminc(mg)":
                                    columnName = "vitc_mg";
                                    break;
                                case "calcium(mg)":
                                    columnName = "calcium_mg";
                                    break;
                                case "iron(mg)":
                                    columnName = "iron_mg";
                                    break;
                                case "dishimageid":
                                    columnName = "Image";
                                    break;
                            }
                            EditedColumnNames.Add(columnName.ToLower());
                        }
                        else
                        {
                            EditedColumnNames.Add("");
                        }
                    }
                    catch (System.Exception)
                    {

                        string columnName = "";
                        EditedColumnNames.Add(columnName);
                    }

                }
                List<Ingredient> alreadyIngredients = context.Ingredients.ToList();
                if (isChain)
                {
                    // context.bulkModels.FromSqlRaw("exec test5 @value={0}", 85).ToList();
                    //#################################################################################
                    //insert ingredients
                    List<Dictionary<string, object>> uploadResults = new List<Dictionary<string, object>>();
                    StringBuilder csvIngredientFile = new StringBuilder();
                    string excelRestaurantChainCodePrev = "";
                    List<Ingredient> ingredients = new List<Ingredient>();
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        double testDouble = 0;
                        Dictionary<string, object> excelCellArrayDict = new Dictionary<string, object>();
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];
                            if (EditedColumnNames[k] == "chain")
                            {
                                string tt = cells[k].ToString();
                                if (!string.IsNullOrEmpty(excelRestaurantChainCodePrev) && excelRestaurantChainCodePrev.ToLower() != tt.ToLower())
                                {
                                    try
                                    {
                                        throw new Exception("All chainCode Texts in the Excel File Should be the same.Nothing inserted.");
                                    }
                                    catch (Exception ee)
                                    {
                                        myDic.Add("Error is ", ee.Message);
                                        return myDic;
                                    }
                                }
                                excelRestaurantChainCodePrev = cells[k].ToString().ToLower();
                                excelRestaurantChainCodePrev = Regex.Replace(excelRestaurantChainCodePrev, @"\t|\n|\r", "");

                            }

                        }
                        double? Energy_KCal = checkIsNotNull(excelCellArrayDict, "Energy_KCal".ToLower(), excelCellArrayDict["Energy_KCal".ToLower()]) && double.TryParse(excelCellArrayDict["Energy_KCal".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Energy_KCal".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? Fat_G = checkIsNotNull(excelCellArrayDict, "Fat_G".ToLower(), excelCellArrayDict["Fat_G".ToLower()]) && double.TryParse(excelCellArrayDict["Fat_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Fat_G".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? FattyAcidsSaturated_G = checkIsNotNull(excelCellArrayDict, "FattyAcidsSaturated_G".ToLower(), excelCellArrayDict["FattyAcidsSaturated_G".ToLower()]) && double.TryParse(excelCellArrayDict["FattyAcidsSaturated_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["FattyAcidsSaturated_G".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? Carbohydrates_G = checkIsNotNull(excelCellArrayDict, "Carbohydrates_G".ToLower(), excelCellArrayDict["Carbohydrates_G".ToLower()]) && double.TryParse(excelCellArrayDict["Carbohydrates_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Carbohydrates_G".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? Sugars_G = checkIsNotNull(excelCellArrayDict, "Sugars_G".ToLower(), excelCellArrayDict["Sugars_G".ToLower()]) && double.TryParse(excelCellArrayDict["Sugars_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Sugars_G".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? FibresSum_G = checkIsNotNull(excelCellArrayDict, "FibresSum_G".ToLower(), excelCellArrayDict["FibresSum_G".ToLower()]) && double.TryParse(excelCellArrayDict["FibresSum_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["FibresSum_G".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? Proteins_G = checkIsNotNull(excelCellArrayDict, "Proteins_G".ToLower(), excelCellArrayDict["Proteins_G".ToLower()]) && double.TryParse(excelCellArrayDict["Proteins_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Proteins_G".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? Sodium_MG = checkIsNotNull(excelCellArrayDict, "Sodium_MG".ToLower(), excelCellArrayDict["Sodium_MG".ToLower()]) && double.TryParse(excelCellArrayDict["Sodium_MG".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Sodium_MG".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? Potassium_MG = checkIsNotNull(excelCellArrayDict, "Potassium_MG".ToLower(), excelCellArrayDict["Potassium_MG".ToLower()]) && double.TryParse(excelCellArrayDict["Potassium_MG".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Potassium_MG".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? FA_polyunsat_sum_G = checkIsNotNull(excelCellArrayDict, "FA_polyunsat_sum_G".ToLower(), excelCellArrayDict["FA_polyunsat_sum_G".ToLower()]) && double.TryParse(excelCellArrayDict["FA_polyunsat_sum_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["FA_polyunsat_sum_G".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? FA_monounsat_sum_G = checkIsNotNull(excelCellArrayDict, "FA_monounsat_sum_G".ToLower(), excelCellArrayDict["FA_monounsat_sum_G".ToLower()]) && double.TryParse(excelCellArrayDict["FA_monounsat_sum_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["FA_monounsat_sum_G".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? FA_trans_sum_G = checkIsNotNull(excelCellArrayDict, "FA_trans_sum_G".ToLower(), excelCellArrayDict["FA_trans_sum_G".ToLower()]) && double.TryParse(excelCellArrayDict["FA_trans_sum_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["FA_trans_sum_G".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? VitA_Activity_MCG = checkIsNotNull(excelCellArrayDict, "VitA_Activity_MCG".ToLower(), excelCellArrayDict["VitA_Activity_MCG".ToLower()]) && double.TryParse(excelCellArrayDict["VitA_Activity_MCG".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["VitA_Activity_MCG".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? VitC_MG = checkIsNotNull(excelCellArrayDict, "VitC_MG".ToLower(), excelCellArrayDict["VitC_MG".ToLower()]) && double.TryParse(excelCellArrayDict["VitC_MG".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["VitC_MG".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? Calcium_MG = checkIsNotNull(excelCellArrayDict, "Calcium_MG".ToLower(), excelCellArrayDict["Calcium_MG".ToLower()]) && double.TryParse(excelCellArrayDict["Calcium_MG".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Calcium_MG".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        double? Iron_MG = checkIsNotNull(excelCellArrayDict, "Iron_MG".ToLower(), excelCellArrayDict["Iron_MG".ToLower()]) && double.TryParse(excelCellArrayDict["Iron_MG".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Iron_MG".ToLower()], CultureInfo.InvariantCulture)) : 0;
                        int isChainInsertedByAdmin = 1;
                        csvIngredientFile.AppendLine($",{null},{null},{null},{null},{null},{null},{Energy_KCal},{null},{Proteins_G},{Fat_G},{Carbohydrates_G},{Sugars_G},{null},{null},{FattyAcidsSaturated_G},{null},{null},{null},{null},{null},{null},{FibresSum_G},{null},{null},{null},{FA_monounsat_sum_G},{FA_polyunsat_sum_G},{null},{null},{null},{null},{null},{null},{FA_trans_sum_G},{null},{null},{null},{Sodium_MG},{Potassium_MG},{Calcium_MG},{null},{null},{Iron_MG},{null},{null},{null},{null},{VitA_Activity_MCG},{null},{null},{null},{null},{VitC_MG},{null},{null},{1},{null}");
                    }
                    int ingredientBeforebulk = alreadyIngredients.Count > 0 ? alreadyIngredients.OrderByDescending(x => x.Id).First().Id : 0;
                    Dictionary<string, object> ingredientRes = await uploadToS3(csvIngredientFile.ToString(), "csvfileingredients.csv");
                    uploadResults.Add(ingredientRes);
                    //###################################################################################
                    //insert Dishes
                    List<Restaurant> restaurantsByChainCode = context.Restaurants.Where(x => x.chainCode != null && x.chainCode == excelRestaurantChainCodePrev.ToLower()).OrderBy(x => x.Id).ToList();
                    StringBuilder csvDishFile = new StringBuilder();
                    List<Dish> dishes = new List<Dish>();
                    int rac = await context.Database.ExecuteSqlRawAsync("exec getLasIdDishes");
                    List<bulkModel> bmsDishes = context.bulkModels.FromSqlRaw("exec afterinsertCsvFilesFromS3ToRDS").ToList();
                    int dishIdBeforeBulk = bmsDishes[0].dishIdBeforeBulk;
                    List<Restaurant> rests_new = context.Restaurants.Where(x => x.chainCode == excelRestaurantChainCodePrev.ToLower()).ToList();
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        Console.WriteLine($"start of row  {j}");
                        double testDouble = 0;
                        Dictionary<string, object> excelCellArrayDict = new Dictionary<string, object>();
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];

                        }
                        var insetedMenuCategory = (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dish")) ?
                        Category.Main : (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dessert")) ? Category.Dessert :
                        (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("drink")) ? Category.Drink : Category.Main;
                        List<Restaurant> rests = context.Restaurants.Where(x => x.chainCode == excelRestaurantChainCodePrev.ToLower()).ToList();
                        double price = double.TryParse(excelCellArrayDict["dishprice"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["dishprice"].ToString(), CultureInfo.InvariantCulture) : 0;
                        string image = "https://web-api-media-storage.s3.eu-central-1.amazonaws.com/" + excelCellArrayDict["image"]?.ToString();
                        int dishtype = (int)((insetedMenuCategory == Category.Main) ? DishType.Dish : insetedMenuCategory == Category.Drink ?
                                                                    DishType.Drink : insetedMenuCategory == Category.Dessert ?
                                                                     DishType.Desert : DishType.none);
                        for (int counter = 0; counter < rests.Count; counter++)
                        {
                            csvDishFile.AppendLine($",{excelCellArrayDict["dishname"].ToString()},{null},{1},{price},{0},{0},{null},{image},Dish,{dishtype},{100},{rests[counter].Id},{1}");
                        }
                        Console.WriteLine($"row {j} insertes");
                    }
                    Dictionary<string, object> dishRes = await uploadToS3(csvDishFile.ToString(), "csvfiledishes.csv");
                    uploadResults.Add(dishRes);
                    //#################################################################################
                    //insert DishComponents
                    StringBuilder csvDishComponentFile = new StringBuilder();
                    List<DishComponent> dishComponents = new List<DishComponent>();
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        double testDouble = 0;
                        Dictionary<string, object> excelCellArrayDict = new Dictionary<string, object>();
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];

                        }
                        int testInt = 0;
                        double price = double.TryParse(excelCellArrayDict["dishprice"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["dishprice"].ToString(), CultureInfo.InvariantCulture) : 0;
                        int length = restaurantsByChainCode.Count;
                        int substituteGroup = checkIsNotNull(excelCellArrayDict, "substitutegroup", "substitutegroup") && int.TryParse(excelCellArrayDict["substitutegroup"].ToString(), out testInt) ? Convert.ToInt32(excelCellArrayDict["substitutegroup"].ToString(), CultureInfo.InvariantCulture) : 0;
                        for (int counter = 0; counter < restaurantsByChainCode.Count; counter++)
                        {
                            int SubstituteGroup = checkIsNotNull(excelCellArrayDict, "substitutegroup", "substitutegroup") && int.TryParse(excelCellArrayDict["substitutegroup"].ToString(), out testInt) ? Convert.ToInt32(excelCellArrayDict["substitutegroup"].ToString(), CultureInfo.InvariantCulture) : 0;
                            csvDishComponentFile.AppendLine($",{(j - 1) * length + counter + dishIdBeforeBulk + 1},{ingredientBeforebulk + j},{100},{price},{0},{SubstituteGroup}");
                        }
                    }
                    Dictionary<string, object> dishComponentRes = await uploadToS3(csvDishComponentFile.ToString(), "csvfiledishcomponents.csv");
                    uploadResults.Add(dishComponentRes);
                    DishComponent? dishComponent = context.DishComponents.OrderByDescending(x => x.Id).FirstOrDefault();
                    int dishComponentIdBeforeBulk = dishComponent != null ? dishComponent.Id : 0;
                    //################################################################################
                    //insert Dietary Types
                    StringBuilder csvDietaryTypeFile = new StringBuilder();
                    List<DietaryType> dietaryTypes = new List<DietaryType>();
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        Console.WriteLine($"start of row  {j}");
                        int testInt = 0;
                        double testDouble = 0;
                        Dictionary<string, object> excelCellArrayDict = new Dictionary<string, object>();
                        var listOfDieteries = new List<int>();
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];

                        }
                        // if ((excelCellArrayDict["vegan"] != null && excelCellArrayDict["vegan"].ToString() != ""))
                        // {
                        //     try
                        //     {
                        //         int a = (int)Convert.ToDouble(excelCellArrayDict["vegan"].ToString(), CultureInfo.InstalledUICulture);
                        //         if (a == 1)
                        //         {
                        //             listOfDieteries.Add((int)Dietary.Vegan);
                        //         }
                        //         else
                        //         {
                        //             listOfDieteries.Add(10);
                        //         }
                        //     }
                        //     catch (System.Exception)
                        //     {
                        //         listOfDieteries.Add(10);

                        //     }
                        // }
                        // if ((excelCellArrayDict["vegetarian"] != null && excelCellArrayDict["vegetarian"].ToString() != ""))
                        // {
                        //     try
                        //     {
                        //         int a = (int)Convert.ToDouble(excelCellArrayDict["vegetarian"].ToString(), CultureInfo.InstalledUICulture);
                        //         if (a == 1)
                        //         {
                        //             listOfDieteries.Add((int)Dietary.Vegetarian);
                        //         }
                        //         else
                        //         {
                        //             listOfDieteries.Add(10);
                        //         }
                        //     }
                        //     catch (System.Exception)
                        //     {
                        //         listOfDieteries.Add(10);

                        //     }
                        // }
                        // if ((excelCellArrayDict["halal"] != null && excelCellArrayDict["halal"].ToString() != ""))
                        // {
                        //     try
                        //     {
                        //         int a = (int)Convert.ToDouble(excelCellArrayDict["halal"].ToString(), CultureInfo.InstalledUICulture);
                        //         if (a == 1)
                        //         {
                        //             listOfDieteries.Add((int)Dietary.Halal);
                        //         }
                        //         else
                        //         {
                        //             listOfDieteries.Add(10);
                        //         }
                        //     }
                        //     catch (System.Exception)
                        //     {

                        //         listOfDieteries.Add(10);
                        //     }
                        // }
                        // if ((excelCellArrayDict["glutenfree"] != null && excelCellArrayDict["glutenfree"].ToString() != ""))
                        // {
                        //     try
                        //     {
                        //         int a = (int)Convert.ToDouble(excelCellArrayDict["glutenfree"].ToString(), CultureInfo.InstalledUICulture);
                        //         if (a == 1)
                        //         {
                        //             listOfDieteries.Add((int)Dietary.GlutenFree);
                        //         }
                        //         else
                        //         {
                        //             listOfDieteries.Add(10);
                        //         }
                        //     }
                        //     catch (System.Exception)
                        //     {
                        //         listOfDieteries.Add(10);

                        //     }
                        // }
                        // if ((excelCellArrayDict["nutfree"] != null && excelCellArrayDict["nutfree"].ToString() != ""))
                        // {
                        //     try
                        //     {
                        //         int a = (int)Convert.ToDouble(excelCellArrayDict["nutfree"].ToString(), CultureInfo.InstalledUICulture);
                        //         if (a == 1)
                        //         {
                        //             listOfDieteries.Add((int)Dietary.NutFree);
                        //         }
                        //         else
                        //         {
                        //             listOfDieteries.Add(10);
                        //         }
                        //     }
                        //     catch (System.Exception)
                        //     {
                        //         listOfDieteries.Add(10);

                        //     }
                        // }
                        // if ((excelCellArrayDict["dairyfree"] != null && excelCellArrayDict["dairyfree"].ToString() != ""))
                        // {
                        //     try
                        //     {
                        //         int a = (int)Convert.ToDouble(excelCellArrayDict["dairyfree"].ToString(), CultureInfo.InstalledUICulture);
                        //         if (a == 1)
                        //         {
                        //             listOfDieteries.Add((int)Dietary.DairyFree);
                        //         }
                        //         else
                        //         {
                        //             listOfDieteries.Add(10);
                        //         }
                        //     }
                        //     catch (System.Exception)
                        //     {
                        //         listOfDieteries.Add(10);

                        //     }
                        // }
                        // if ((excelCellArrayDict["kosher"] != null && excelCellArrayDict["kosher"].ToString() != ""))
                        // {
                        //     try
                        //     {
                        //         int a = (int)Convert.ToDouble(excelCellArrayDict["kosher"].ToString(), CultureInfo.InstalledUICulture);
                        //         if (a == 1)
                        //         {
                        //             listOfDieteries.Add((int)Dietary.Kosher);
                        //         }
                        //         else
                        //         {
                        //             listOfDieteries.Add(10);
                        //         }
                        //     }
                        //     catch (System.Exception)
                        //     {
                        //         listOfDieteries.Add(10);

                        //     }
                        // }
                        if ((excelCellArrayDict["vegan"] != null && excelCellArrayDict["vegan"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["vegan"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.Vegan);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {
                                // listOfDieteries.Add(0);

                            }
                        }
                        if ((excelCellArrayDict["vegetarian"] != null && excelCellArrayDict["vegetarian"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["vegetarian"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.Vegetarian);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {
                                // listOfDieteries.Add(0);

                            }
                        }
                        if ((excelCellArrayDict["halal"] != null && excelCellArrayDict["halal"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["halal"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.Halal);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {

                                // listOfDieteries.Add(0);
                            }
                        }
                        if ((excelCellArrayDict["glutenfree"] != null && excelCellArrayDict["glutenfree"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["glutenfree"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.GlutenFree);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {
                                // listOfDieteries.Add(0);

                            }
                        }
                        if ((excelCellArrayDict["nutfree"] != null && excelCellArrayDict["nutfree"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["nutfree"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.NutFree);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {
                                // listOfDieteries.Add(0);

                            }
                        }
                        if ((excelCellArrayDict["dairyfree"] != null && excelCellArrayDict["dairyfree"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["dairyfree"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.DairyFree);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {
                                // listOfDieteries.Add(0);

                            }
                        }
                        if ((excelCellArrayDict["kosher"] != null && excelCellArrayDict["kosher"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["kosher"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.Kosher);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {
                                // listOfDieteries.Add(0);

                            }
                        }

                        List<Restaurant> rests = context.Restaurants.Where(x => x.chainCode == excelRestaurantChainCodePrev.ToLower()).ToList();
                        int length = rests.Count;
                        for (int counter = 0; counter < rests.Count; counter++)
                        {
                            for (int dietaryConter = 0; dietaryConter < listOfDieteries.Count; dietaryConter++)
                            {
                                // dietaryTypes.Add(new DietaryType()
                                // {
                                //     Dietary = (Dietary)listOfDieteries[dietaryConter],
                                //     DishId = (j - 1) * length + dishIdBeforeBulk + counter + 1

                                // });
                                csvDietaryTypeFile.AppendLine($",{listOfDieteries[dietaryConter]},{(j - 1) * length + dishIdBeforeBulk + counter + 1}");
                            }
                        }
                    }
                    Dictionary<string, object> dietaryTypesRes = await uploadToS3(csvDietaryTypeFile.ToString(), "csvfiledietarytypes.csv");
                    uploadResults.Add(dietaryTypesRes);
                    //########################################################################
                    //insert SubCategory
                    List<List<String>> tempRes1 = new List<List<string>>();
                    int rac1 = await context.Database.ExecuteSqlRawAsync("exec getLasIdSubcategories");
                    List<bulkModel> bmsSubcategories = context.bulkModels.FromSqlRaw("exec afterinsertCsvFilesFromS3ToRDS").ToList();
                    int subCategoryIdBeforeBulk = bmsSubcategories[0].dishIdBeforeBulk;
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        Dictionary<string, object> excelCellArrayDict = new Dictionary<string, object>();
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];

                        }
                        int insetedMenuCategory = (int)((excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dish")) ?
                        Category.Main : (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dessert")) ? Category.Dessert :
                        (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("drink")) ? Category.Drink : Category.Main);
                        string menuName = nameWithoutSpacesandLower(excelCellArrayDict["menuname"].ToString());
                        bool isAlreadyThere = false;
                        for (int tt = 0; tt < tempRes1.Count; tt++)
                        {
                            if ((tempRes1[tt])[0] == menuName && (tempRes1[tt])[1] == insetedMenuCategory.ToString())
                            {
                                isAlreadyThere = true;
                                break;
                            }

                        }
                        if (!isAlreadyThere)
                        {
                            tempRes1.Add(new List<string>() { menuName, insetedMenuCategory.ToString(), j.ToString() });
                        }
                    }
                    StringBuilder csvSubCategoryFile = new StringBuilder();
                    List<Restaurant> rests1 = context.Restaurants.Where(x => x.chainCode == excelRestaurantChainCodePrev.ToLower()).ToList();
                    for (int j = 0; j < tempRes1.Count; j++)
                    {
                        for (int counter = 0; counter < rests1.Count; counter++)
                        {
                            // subCategories.Add(new SubCategory()
                            // {
                            //     Name = (tempRes1[j])[0],
                            //     Category = (Category)(int.Parse((tempRes1[j])[1])),
                            //     RestaurantId = rests1[counter].Id

                            // });
                            csvSubCategoryFile.AppendLine($",{(tempRes1[j])[0]},{(tempRes1[j])[1]},{rests1[counter].Id}");
                        }
                    }
                    Dictionary<string, object> subCategoryRes = await uploadToS3(csvSubCategoryFile.ToString(), "csvfilesubcategories.csv");
                    uploadResults.Add(subCategoryRes);
                    // SubCategory? subCategory = context.SubCategories.OrderByDescending(x => x.Id).FirstOrDefault();
                    // int subCategoryIdBeforeBulk = subCategory != null ? subCategory.Id : 0;
                    //################################################################################
                    //insert DishCategory
                    StringBuilder csvDishCategoryFile = new StringBuilder();
                    int restLength = rests1.Count;
                    for (int c = 0; c < tempRes1.Count; c++)
                    {
                        int firstRow = int.Parse((tempRes1[c])[2]);
                        int numOfRows = 0;
                        if (c < tempRes1.Count - 1)
                        {
                            int a = int.Parse((tempRes1[c])[2]);
                            int b = int.Parse((tempRes1[c + 1])[2]);
                            numOfRows = b - a;
                        }
                        if (c == tempRes1.Count - 1)
                        {
                            int a = int.Parse((tempRes1[c])[2]);
                            int b = ExcelSheet.Rows.Count - 1;

                            numOfRows = b - a + 1;
                        }
                        for (int jj = 0; jj < rests1.Count; jj++)
                        {
                            for (int k = 0; k < numOfRows; k++)
                            {
                                // dishCategories.Add(new DishCategory()
                                // {
                                //     DishId = (firstRow - 1) * rests1.Count + k * restLength + dishIdBeforeBulk + jj + 1,
                                //     SubCategoryId = c * restLength + subCategoryIdBeforeBulk + jj + 1
                                // });
                                csvDishCategoryFile.AppendLine($"{(firstRow - 1) * rests1.Count + k * restLength + dishIdBeforeBulk + jj + 1},{c * restLength + subCategoryIdBeforeBulk + jj + 1}");
                            }
                        }
                    }

                    Dictionary<string, object> dishCategoriesRes = await uploadToS3(csvDishCategoryFile.ToString(), "csvfiledishcategories.csv");
                    uploadResults.Add(dishCategoriesRes);
                    int affectedRowsCount = await context.Database.ExecuteSqlRawAsync("exec insertCsvFilesFromS3ToRDS @level={0}", 0);
                    List<bulkModel> bms = context.bulkModels.FromSqlRaw("exec afterinsertCsvFilesFromS3ToRDS").ToList();
                    int res = bms[0].dishIdBeforeBulk;
                    if (res == 1)
                    {
                        int affectedRowsCount1 = await context.Database.ExecuteSqlRawAsync("exec bulinserts @chainCode={0}", excelRestaurantChainCodePrev.ToLower());
                        List<bulkModel> fbms = context.bulkModels.FromSqlRaw("exec afterinsertCsvFilesFromS3ToRDS").ToList();
                        if (fbms[0].dishIdBeforeBulk == 1)
                        {
                            myDic.Add("ok", "files inserted Successfully.");
                            return myDic;
                        }
                    }
                    else
                    {
                        myDic.Add("Error", "files did not uploaded to RDS Successfully.");
                        return myDic;
                    }
                }
                //###########################################################################
                // For Non Chain Restaurants
                if (!isChain)
                {
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        alreadyRestaurants = await context.Restaurants.Include(x => x.Menu).ToListAsync();
                        // List<Dish> alreadyDishes = await context.Dishes.ToListAsync();                    // {
                        System.Console.WriteLine("start of row " + j);
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        string excelRestaurantName = "none";
                        string excelRestaurantChainCode = "";
                        double excelRestaurantLongitude = (double)0.0;
                        double excelRestaurantLatitude = (double)0.0;
                        Dictionary<string, object> excelCellArrayDict = new Dictionary<string, object>();
                        Time initTime = new Time();
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            if (EditedColumnNames[k] == "chain")
                            {
                                excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];
                                excelRestaurantChainCode = cells[k].ToString();
                                excelRestaurantChainCode = Regex.Replace(excelRestaurantChainCode, @"\t|\n|\r", "");
                                continue;
                            }
                            if (EditedColumnNames[k] == "name")
                            {
                                excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];
                                excelRestaurantName = cells[k].ToString();
                                continue;
                            }
                            if (EditedColumnNames[k] == "longitude")
                            {
                                try
                                {
                                    double x = Convert.ToDouble(cells[k].ToString(), CultureInfo.InvariantCulture);
                                    string x1 = x.ToString("0.00000", CultureInfo.InvariantCulture);
                                    excelRestaurantLongitude = Convert.ToDouble(x1, CultureInfo.InvariantCulture);
                                    excelCellArrayDict[EditedColumnNames[k].ToString()] = excelRestaurantLongitude;
                                    continue;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                    excelRestaurantLongitude = (float)0.0;
                                    excelCellArrayDict[EditedColumnNames[k].ToString()] = excelRestaurantLongitude;
                                    continue;
                                }
                            }
                            if (EditedColumnNames[k] == "latitude")
                            {
                                try
                                {
                                    double x = Convert.ToDouble(cells[k].ToString(), CultureInfo.InvariantCulture);
                                    string x1 = x.ToString("0.00000", CultureInfo.InvariantCulture);
                                    excelRestaurantLatitude = Convert.ToDouble(x1, CultureInfo.InvariantCulture);
                                    excelCellArrayDict[EditedColumnNames[k].ToString()] = excelRestaurantLatitude;
                                    continue;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                    excelRestaurantLatitude = (float)0.0;
                                    excelCellArrayDict[EditedColumnNames[k].ToString()] = excelRestaurantLatitude;
                                    continue;
                                }
                            }
                            excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];

                        }
                        if (isChain)
                        {
                            foundedRestaurant = alreadyRestaurants.Where(x => nameWithoutSpacesandLower(x.Name) == nameWithoutSpacesandLower(excelRestaurantName) && x.chainCode != null && x.chainCode == nameWithoutSpacesandLower(excelRestaurantChainCode) && !x.IsUnofficialRestaurant).ToList();
                        }
                        else
                        {
                            foundedRestaurant = alreadyRestaurants.Where((x) => nameWithoutSpacesandLower(x.Name) == nameWithoutSpacesandLower(excelRestaurantName) && x.Longitude == excelRestaurantLongitude && x.Latitude == excelRestaurantLatitude).ToList();
                        }
                        var insetedMenuCategory = (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dish")) ?
                        Category.Main : (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dessert")) ? Category.Dessert :
                        (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("drink")) ? Category.Drink : Category.Main;
                        Ingredient? ing = !isChain ? alreadyIngredients.Where(x => nameWithoutSpacesandLower(x.NameEng) ==
                                  nameWithoutSpacesandLower(excelCellArrayDict["ingredient"].ToString())).ToList().FirstOrDefault() : null;
                        if (ing == null)
                        {

                            myDic.Add("Error is", $"in row {j}. there is no ingredient with name in the excel file-next rows will not be inserted");
                            return myDic;

                        }
                        if (foundedRestaurant != null && foundedRestaurant.Count > 0)
                        {
                            int ii = 0;
                            foundedRestaurant.ForEach(async foundedRes =>
                            {
                                SubCategory subCategory = foundedRes.Menu.ToList().Where(x =>
                                nameWithoutSpacesandLower(x.Name) == nameWithoutSpacesandLower(excelCellArrayDict["menuname"].ToString()) &&
                                x.Category == insetedMenuCategory
                                && (x.Restaurant != null
                                && x.Restaurant.Id == foundedRes.Id)).ToList().FirstOrDefault();
                                if (subCategory != null)
                                {
                                    Dish foundedDish = null;
                                    foundedDish = context.Dishes.Where(x => x.RestaurantId == foundedRes.Id).Where(x => nameWithoutSpacesandLower(x.Name) == nameWithoutSpacesandLower(excelCellArrayDict["dishname"].ToString())).FirstOrDefault();
                                    if (foundedDish != null)
                                    {
                                        int testInt = 0;
                                        double testDouble = 0;
                                        Ingredient? ing = !isChain ? alreadyIngredients.Where(x => nameWithoutSpacesandLower(x.NameEng) ==
                                             nameWithoutSpacesandLower(excelCellArrayDict["ingredient"].ToString())).ToList().FirstOrDefault() : null;
                                        DishComponent ndc = new DishComponent()
                                        {
                                            DishId = foundedDish.Id,
                                            Weight = (!isChain) ?
                                            (checkIsNotNull(excelCellArrayDict, "measurementingrams", "measurementingrams") && double.TryParse(excelCellArrayDict["measurementingrams"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["measurementingrams"].ToString(), CultureInfo.InvariantCulture) : 0) : 100,
                                            SubstituteGroup = checkIsNotNull(excelCellArrayDict, "substitutegroup", "substitutegroup") && int.TryParse(excelCellArrayDict["substitutegroup"].ToString(), out testInt) ? Convert.ToInt32(excelCellArrayDict["substitutegroup"].ToString(), CultureInfo.InvariantCulture) : 0,
                                            IngredientId = (ing != null) ? ing.Id : 0,
                                        };
                                        context.DishComponents.Add(ndc);
                                    }
                                    else
                                    {
                                        var listOfDietarTypes = new List<DietaryType>();
                                        var listOfDieteries = new List<Dietary>();
                                        if ((excelCellArrayDict["vegan"] != null && excelCellArrayDict["vegan"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["vegan"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.Vegan
                                                    });
                                                    listOfDieteries.Add(Dietary.Vegan);
                                                }
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        if ((excelCellArrayDict["vegetarian"] != null && excelCellArrayDict["vegetarian"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["vegetarian"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.Vegetarian
                                                    });
                                                }
                                                listOfDieteries.Add(Dietary.Vegetarian);
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        if ((excelCellArrayDict["halal"] != null && excelCellArrayDict["halal"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["halal"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.Halal
                                                    });
                                                    listOfDieteries.Add(Dietary.Halal);
                                                }
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        if ((excelCellArrayDict["glutenfree"] != null && excelCellArrayDict["glutenfree"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["glutenfree"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.GlutenFree
                                                    });
                                                    listOfDieteries.Add(Dietary.GlutenFree);
                                                }
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        if ((excelCellArrayDict["nutfree"] != null && excelCellArrayDict["nutfree"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["nutfree"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.NutFree
                                                    });
                                                    listOfDieteries.Add(Dietary.NutFree);
                                                }
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        if ((excelCellArrayDict["dairyfree"] != null && excelCellArrayDict["dairyfree"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["dairyfree"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.DairyFree
                                                    });
                                                    listOfDieteries.Add(Dietary.DairyFree);
                                                }
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        if ((excelCellArrayDict["kosher"] != null && excelCellArrayDict["kosher"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["kosher"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.Kosher
                                                    });
                                                    listOfDieteries.Add(Dietary.Kosher);
                                                }
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        int testInt = 0;
                                        double testDouble = 0;
                                        Ingredient? ing = !isChain ? alreadyIngredients.Where(x => nameWithoutSpacesandLower(x.NameEng) ==
                                             nameWithoutSpacesandLower(excelCellArrayDict["ingredient"].ToString())).ToList().FirstOrDefault() : null;
                                        DishCategory dishCategory = new DishCategory()
                                        {
                                            SubCategoryId = subCategory.Id,
                                            Dish = new Dish()
                                            {
                                                DishType = insetedMenuCategory == Category.Main ?
                                              DishType.Dish : insetedMenuCategory == Category.Drink ?
                                              DishType.Drink : insetedMenuCategory == Category.Dessert ?
                                               DishType.Desert : DishType.none,
                                                Name = excelCellArrayDict["dishname"].ToString(),
                                                Price = double.TryParse(excelCellArrayDict["dishprice"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["dishprice"].ToString(), CultureInfo.InvariantCulture) : 0,
                                                IsActive = IsActive.Yes,
                                                Restaurant = foundedRes,
                                                DietaryType = listOfDietarTypes,
                                                Image = new Uri("https://web-api-media-storage.s3.eu-central-1.amazonaws.com/" + excelCellArrayDict["image"]?.ToString(), UriKind.Absolute),
                                                Components = new List<DishComponent>(){
                                                        new DishComponent(){
                                                            Weight = (!isChain) ?
                                                        (checkIsNotNull(excelCellArrayDict, "measurementingrams", "measurementingrams") && double.TryParse(excelCellArrayDict["measurementingrams"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["measurementingrams"].ToString(), CultureInfo.InvariantCulture) : 0) : 100,
                                                            SubstituteGroup = checkIsNotNull(excelCellArrayDict, "substitutegroup", "substitutegroup") && int.TryParse(excelCellArrayDict["substitutegroup"].ToString(),out testInt) ? Convert.ToInt32(excelCellArrayDict["substitutegroup"].ToString(), CultureInfo.InvariantCulture) : 0,
                                                            IngredientId =  (ing != null) ? ing.Id : 0,
                                                        }
                                                        }
                                            }
                                        };
                                        context.DishCategories.Add(dishCategory);
                                    }
                                }
                                else
                                {
                                    var listOfDietarTypes = new List<DietaryType>();
                                    if ((excelCellArrayDict["vegan"] != null && excelCellArrayDict["vegan"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["vegan"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.Vegan
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    if ((excelCellArrayDict["vegetarian"] != null && excelCellArrayDict["vegetarian"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["vegetarian"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.Vegetarian
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    if ((excelCellArrayDict["halal"] != null && excelCellArrayDict["halal"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["halal"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.Halal
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    if ((excelCellArrayDict["glutenfree"] != null && excelCellArrayDict["glutenfree"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["glutenfree"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.GlutenFree
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    if ((excelCellArrayDict["nutfree"] != null && excelCellArrayDict["nutfree"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["nutfree"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.NutFree
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    if ((excelCellArrayDict["dairyfree"] != null && excelCellArrayDict["dairyfree"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["dairyfree"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.DairyFree
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    if ((excelCellArrayDict["kosher"] != null && excelCellArrayDict["kosher"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["kosher"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.Kosher
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    int testInt = 0;
                                    double testDouble = 0;
                                    Ingredient? ing = !isChain ? alreadyIngredients.Where(x => nameWithoutSpacesandLower(x.NameEng) ==
                                         nameWithoutSpacesandLower(excelCellArrayDict["ingredient"].ToString())).ToList().FirstOrDefault() : null;
                                    SubCategory newSubCategory = new SubCategory()
                                    {
                                        Name = excelCellArrayDict.ContainsKey("menucategory") ? excelCellArrayDict["menuname"].ToString() : "",
                                        Category = (excelCellArrayDict.ContainsKey("menucategory") && excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dish")) ?
                                        Category.Main :
                                        (excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dessert")) ?
                                        Category.Dessert :
                                        (excelCellArrayDict["menucategory"].ToString().ToLower().Contains("drink")) ?
                                        Category.Drink : Category.Main,
                                        RestaurantId = foundedRes.Id,
                                        DishCategories = new List<DishCategory>(){
                                            new DishCategory()
                                            {
                                                Dish=new Dish()
                                                {
                                                    DishType = (excelCellArrayDict.ContainsKey("menucategory") && excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dish")) ?
                                                    DishType.Dish :
                                                    (excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dessert")) ?
                                                    DishType.Desert :
                                                    (excelCellArrayDict["menucategory"].ToString().ToLower().Contains("drink")) ?
                                                        DishType.Drink : DishType.none,
                                                    DietaryType = listOfDietarTypes,
                                                    Name = excelCellArrayDict["dishname"].ToString(),
                                                    IsActive = IsActive.Yes,
                                                    Image = new Uri("https://web-api-media-storage.s3.eu-central-1.amazonaws.com/" + excelCellArrayDict["image"]?.ToString(), UriKind.Absolute),
                                                    Price = double.TryParse(excelCellArrayDict["dishprice"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["dishprice"].ToString(), CultureInfo.InvariantCulture) : 0,
                                                    RestaurantId = foundedRes.Id,
                                                    Components = new List<DishComponent>()
                                                    {
                                                        new DishComponent()
                                                        {
                                                                Weight = (!isChain) ?
                                                            (checkIsNotNull(excelCellArrayDict, "measurementingrams", "measurementingrams") && double.TryParse(excelCellArrayDict["measurementingrams"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["measurementingrams"].ToString(), CultureInfo.InvariantCulture) : 0) : 100,
                                                                SubstituteGroup = checkIsNotNull(excelCellArrayDict, "substitutegroup", "substitutegroup") && int.TryParse(excelCellArrayDict["substitutegroup"].ToString(),out testInt) ? Convert.ToInt32(excelCellArrayDict["substitutegroup"].ToString(), CultureInfo.InvariantCulture) : 0,
                                                                IngredientId = (ing != null) ? ing.Id : 0,
                                                        }
                                                    }

                                                }

                                            }
                                    }
                                    };
                                    context.SubCategories.Add(newSubCategory);
                                }
                                List<bulkModel> bms = context.bulkModels.FromSqlRaw("exec updateRegularRests @id={0}", foundedRes.Id).ToList();
                            });
                        }
                        else
                        {
                            var listOfDietarTypes = new List<DietaryType>();
                            if ((excelCellArrayDict["vegan"] != null && excelCellArrayDict["vegan"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["vegan"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.Vegan
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            if ((excelCellArrayDict["vegetarian"] != null && excelCellArrayDict["vegetarian"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["vegetarian"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.Vegetarian
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            if ((excelCellArrayDict["halal"] != null && excelCellArrayDict["halal"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["halal"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.Halal
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            if ((excelCellArrayDict["glutenfree"] != null && excelCellArrayDict["glutenfree"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["glutenfree"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.GlutenFree
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            if ((excelCellArrayDict["nutfree"] != null && excelCellArrayDict["nutfree"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["nutfree"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.NutFree
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            if ((excelCellArrayDict["dairyfree"] != null && excelCellArrayDict["dairyfree"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["dairyfree"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.DairyFree
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            if ((excelCellArrayDict["kosher"] != null && excelCellArrayDict["kosher"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["kosher"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.Kosher
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            int testInt = 0;
                            double testDouble = 0;
                            // Ingredient? ing = !isChain ? alreadyIngredients.Where(x => nameWithoutSpacesandLower(x.NameEng) ==
                            //             nameWithoutSpacesandLower(excelCellArrayDict["ingredient"].ToString())).ToList().FirstOrDefault() : null;
                            Restaurant restaurant = new Restaurant()
                            {
                                Name = nameWithoutSpacesandLower(excelRestaurantName),
                                Longitude = excelRestaurantLongitude,
                                Latitude = excelRestaurantLatitude,
                                IsAddByAdmin = true,
                                IsUnofficialRestaurant = (isChain) ? true : false,
                                Menu = new List<SubCategory>(){
                                    new SubCategory(){
                                        Name=excelCellArrayDict["menuname"].ToString(),
                                        Category=insetedMenuCategory,
                                        DishCategories=new List<DishCategory>(){
                                            new DishCategory(){
                                                  Dish=new Dish(){
                                             DishType= insetedMenuCategory==Category.Main?
                                    DishType.Dish :
                                    insetedMenuCategory==Category.Dessert ?
                                    DishType.Desert :
                                    insetedMenuCategory==Category.Drink?
                                    DishType.Drink : DishType.none,
                                                DietaryType=listOfDietarTypes,
                                                IsActive=IsActive.Yes,
                                                Name=excelCellArrayDict["dishname"].ToString(),
                                                    Price = double.TryParse(excelCellArrayDict["dishprice"].ToString(),out testDouble)?Convert.ToDouble(excelCellArrayDict["dishprice"].ToString(), CultureInfo.InvariantCulture):0,
                                                    Image = new Uri("https://web-api-media-storage.s3.eu-central-1.amazonaws.com/" + excelCellArrayDict["image"]?.ToString(), UriKind.Absolute),
                                        Components = new List<DishComponent>(){
                                            new DishComponent(){
                                                Weight = (!isChain) ?
                                    (checkIsNotNull(excelCellArrayDict, "measurementingrams", "measurementingrams") && double.TryParse(excelCellArrayDict["measurementingrams"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["measurementingrams"].ToString(), CultureInfo.InvariantCulture) : 0) : 100,
                                                SubstituteGroup = checkIsNotNull(excelCellArrayDict, "substitutegroup", "substitutegroup")&&int.TryParse(excelCellArrayDict["substitutegroup"].ToString(),out testInt) ? Convert.ToInt32(excelCellArrayDict["substitutegroup"].ToString(), CultureInfo.InvariantCulture) : 0,
                                                Ingredient =  ing
                                            }
                                        }

                                         }
                                    }
                                        }
                                    }
                                }
                            };
                            restaurant.adminRestaurantIncludeMenu = true;
                            context.Restaurants.Add(restaurant);
                        }
                        int num = context.SaveChanges();
                        Console.WriteLine($"row {j} inserted");
                    }
                }

            }
            // }
            // catch (System.Exception e)
            // {
            //     Console.WriteLine("Error is " + e.Message);
            //     context.bulkModels.FromSqlRaw("exec checkIngredients").ToList();
            //     myDic.Add("Error", e.Message);
            //     return myDic;
            // }
            myDic.Add("ok", "ok");
            return myDic;
        }

        public async Task<Dictionary<string, object>> UploadFileMenu(IFormFile file, ApplicationDbContext context)
        {
            context.Database.SetCommandTimeout(3600);
            Dictionary<string, object> myDic = new Dictionary<string, object>();
            List<Restaurant> alreadyRestaurants;
            List<Restaurant> foundedRestaurant = null;
            alreadyRestaurants = await context.Restaurants.Include(x => x.Menu).ToListAsync();
            // try
            // {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var reader = ExcelReaderFactory.CreateReader(file.OpenReadStream()))
            {
                var excelDataSet = reader.AsDataSet();
                if (excelDataSet.Tables.Count > 1)
                {
                    myDic.Add("Error", "More than one sheet");
                    return myDic;
                }
                DataTable ExcelSheet = excelDataSet.Tables[0];
                object?[] columnNames = (ExcelSheet).Rows[0].ItemArray;
                List<string> EditedColumnNames = new List<string>();
                bool isChain = false;
                foreach (var obj in columnNames)
                {
                    try
                    {
                        if (obj != null)
                        {
                            string columnName = Regex.Replace(obj.ToString(), @"\s+", "");
                            switch (columnName.ToLower())
                            {
                                case "restaurantname":
                                    columnName = "name";
                                    break;
                                case "chain":
                                    columnName = "chain";
                                    isChain = true;
                                    break;
                                case "ingredientsubstitute":
                                    columnName = "substitutegroup";
                                    break;
                                case "calories(kcal)":
                                    columnName = "energy_kcal";
                                    break;
                                case "totalfat(grams)":
                                    columnName = "fat_g";
                                    break;
                                case "saturatedfat(grams)":
                                    columnName = "fattyacidssaturated_g";
                                    break;
                                case "totalcarbs(grams)":
                                    columnName = "carbohydrates_g";
                                    break;
                                case "sugars(grams)":
                                    columnName = "sugars_g";
                                    break;
                                case "dietaryfiber(grams)":
                                    columnName = "fibressum_g";
                                    break;
                                case "protein(grams)":
                                    columnName = "proteins_g";
                                    break;
                                case "sodium(mg)":
                                    columnName = "sodium_mg";
                                    break;
                                case "cholestrol(mg)":
                                    columnName = "cholesterol_mg";
                                    break;
                                case "potassium(mg)":
                                    columnName = "potassium_mg";
                                    break;
                                case "polyunsaturatedfat(grams)":
                                    columnName = "fa_polyunsat_sum_g";
                                    break;
                                case "monounsaturatedfat(grams)":
                                    columnName = "fa_monounsat_sum_g";
                                    break;
                                case "transfat(grams)":
                                    columnName = "fa_trans_sum_g";
                                    break;
                                case "vitamina(mg)":
                                    columnName = "vita_activity_mcg";
                                    break;
                                case "vitaminc(mg)":
                                    columnName = "vitc_mg";
                                    break;
                                case "calcium(mg)":
                                    columnName = "calcium_mg";
                                    break;
                                case "iron(mg)":
                                    columnName = "iron_mg";
                                    break;
                                case "dishimageid":
                                    columnName = "Image";
                                    break;
                            }
                            EditedColumnNames.Add(columnName.ToLower());
                        }
                        else
                        {
                            EditedColumnNames.Add("");
                        }
                    }
                    catch (System.Exception)
                    {

                        string columnName = "";
                        EditedColumnNames.Add(columnName);
                    }

                }
                List<Ingredient> alreadyIngredients = context.Ingredients.ToList();
                if (isChain)
                {
                    //#################################################################################
                    //insert ingredients
                    StringBuilder csvIngredientFile = new StringBuilder();
                    string excelRestaurantChainCodePrev = "";
                    List<Ingredient> ingredients = new List<Ingredient>();
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        double testDouble = 0;
                        Dictionary<string, object> excelCellArrayDict = new Dictionary<string, object>();
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];
                            if (EditedColumnNames[k] == "chain")
                            {
                                string tt = cells[k].ToString();
                                if (!string.IsNullOrEmpty(excelRestaurantChainCodePrev) && excelRestaurantChainCodePrev.ToLower() != tt.ToLower())
                                {
                                    try
                                    {
                                        throw new Exception("All chainCode Texts in the Excel File Should be the same.Nothing inserted.");
                                    }
                                    catch (Exception ee)
                                    {
                                        myDic.Add("Error is ", ee.Message);
                                        return myDic;
                                    }
                                }
                                excelRestaurantChainCodePrev = cells[k].ToString().ToLower();
                                excelRestaurantChainCodePrev = Regex.Replace(excelRestaurantChainCodePrev, @"\t|\n|\r", "");

                            }

                        }
                        ingredients.Add(new Ingredient()
                        {
                            Energy_KCal = checkIsNotNull(excelCellArrayDict, "Energy_KCal".ToLower(), excelCellArrayDict["Energy_KCal".ToLower()]) && double.TryParse(excelCellArrayDict["Energy_KCal".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Energy_KCal".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            Fat_G = checkIsNotNull(excelCellArrayDict, "Fat_G".ToLower(), excelCellArrayDict["Fat_G".ToLower()]) && double.TryParse(excelCellArrayDict["Fat_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Fat_G".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            FattyAcidsSaturated_G = checkIsNotNull(excelCellArrayDict, "FattyAcidsSaturated_G".ToLower(), excelCellArrayDict["FattyAcidsSaturated_G".ToLower()]) && double.TryParse(excelCellArrayDict["FattyAcidsSaturated_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["FattyAcidsSaturated_G".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            Carbohydrates_G = checkIsNotNull(excelCellArrayDict, "Carbohydrates_G".ToLower(), excelCellArrayDict["Carbohydrates_G".ToLower()]) && double.TryParse(excelCellArrayDict["Carbohydrates_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Carbohydrates_G".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            Sugars_G = checkIsNotNull(excelCellArrayDict, "Sugars_G".ToLower(), excelCellArrayDict["Sugars_G".ToLower()]) && double.TryParse(excelCellArrayDict["Sugars_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Sugars_G".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            FibresSum_G = checkIsNotNull(excelCellArrayDict, "FibresSum_G".ToLower(), excelCellArrayDict["FibresSum_G".ToLower()]) && double.TryParse(excelCellArrayDict["FibresSum_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["FibresSum_G".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            Proteins_G = checkIsNotNull(excelCellArrayDict, "Proteins_G".ToLower(), excelCellArrayDict["Proteins_G".ToLower()]) && double.TryParse(excelCellArrayDict["Proteins_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Proteins_G".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            Sodium_MG = checkIsNotNull(excelCellArrayDict, "Sodium_MG".ToLower(), excelCellArrayDict["Sodium_MG".ToLower()]) && double.TryParse(excelCellArrayDict["Sodium_MG".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Sodium_MG".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            Potassium_MG = checkIsNotNull(excelCellArrayDict, "Potassium_MG".ToLower(), excelCellArrayDict["Potassium_MG".ToLower()]) && double.TryParse(excelCellArrayDict["Potassium_MG".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Potassium_MG".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            FA_polyunsat_sum_G = checkIsNotNull(excelCellArrayDict, "FA_polyunsat_sum_G".ToLower(), excelCellArrayDict["FA_polyunsat_sum_G".ToLower()]) && double.TryParse(excelCellArrayDict["FA_polyunsat_sum_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["FA_polyunsat_sum_G".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            FA_monounsat_sum_G = checkIsNotNull(excelCellArrayDict, "FA_monounsat_sum_G".ToLower(), excelCellArrayDict["FA_monounsat_sum_G".ToLower()]) && double.TryParse(excelCellArrayDict["FA_monounsat_sum_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["FA_monounsat_sum_G".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            FA_trans_sum_G = checkIsNotNull(excelCellArrayDict, "FA_trans_sum_G".ToLower(), excelCellArrayDict["FA_trans_sum_G".ToLower()]) && double.TryParse(excelCellArrayDict["FA_trans_sum_G".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["FA_trans_sum_G".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            VitA_Activity_MCG = checkIsNotNull(excelCellArrayDict, "VitA_Activity_MCG".ToLower(), excelCellArrayDict["VitA_Activity_MCG".ToLower()]) && double.TryParse(excelCellArrayDict["VitA_Activity_MCG".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["VitA_Activity_MCG".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            VitC_MG = checkIsNotNull(excelCellArrayDict, "VitC_MG".ToLower(), excelCellArrayDict["VitC_MG".ToLower()]) && double.TryParse(excelCellArrayDict["VitC_MG".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["VitC_MG".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            Calcium_MG = checkIsNotNull(excelCellArrayDict, "Calcium_MG".ToLower(), excelCellArrayDict["Calcium_MG".ToLower()]) && double.TryParse(excelCellArrayDict["Calcium_MG".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Calcium_MG".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            Iron_MG = checkIsNotNull(excelCellArrayDict, "Iron_MG".ToLower(), excelCellArrayDict["Iron_MG".ToLower()]) && double.TryParse(excelCellArrayDict["Iron_MG".ToLower()].ToString(), out testDouble) ? (Convert.ToDouble(excelCellArrayDict["Iron_MG".ToLower()], CultureInfo.InvariantCulture)) : 0,
                            isChainInsertedByAdmin = 1
                        });
                    }
                    int ingredientBeforebulk = alreadyIngredients.Count > 0 ? alreadyIngredients.OrderByDescending(x => x.Id).First().Id : 0;
                    context.AddRange(ingredients);
                    int a222 = await context.SaveChangesAsync();
                    //###################################################################################
                    //insert Dishes
                    List<Restaurant> restaurantsByChainCode = context.Restaurants.Where(x => x.chainCode != null && x.chainCode == excelRestaurantChainCodePrev.ToLower()).OrderBy(x => x.Id).ToList();
                    StringBuilder csvDishFile = new StringBuilder();
                    List<Dish> dishes = new List<Dish>();
                    List<Restaurant> restsiijonn = context.Restaurants.ToList();
                    List<Restaurant> rests_new = context.Restaurants.Where(x => x.chainCode == excelRestaurantChainCodePrev.ToLower()).ToList();
                    int rac = await context.Database.ExecuteSqlRawAsync("exec getLasIdDishes");
                    List<bulkModel> bmsDishes = context.bulkModels.FromSqlRaw("exec afterinsertCsvFilesFromS3ToRDS").ToList();
                    int dishIdBeforeBulk = bmsDishes[0].dishIdBeforeBulk;
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        Console.WriteLine($"start of row  {j}");
                        double testDouble = 0;
                        Dictionary<string, object> excelCellArrayDict = new Dictionary<string, object>();
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];

                        }
                        var insetedMenuCategory = (excelCellArrayDict.ContainsKey("menucategory") &&
                    excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dish")) ?
                    Category.Main : (excelCellArrayDict.ContainsKey("menucategory") &&
                    excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dessert")) ? Category.Dessert :
                    (excelCellArrayDict.ContainsKey("menucategory") &&
                    excelCellArrayDict["menucategory"].ToString().ToLower().Contains("drink")) ? Category.Drink : Category.Main;
                        for (int counter = 0; counter < rests_new.Count; counter++)
                        {
                            dishes.Add(new Dish()
                            {
                                Name = excelCellArrayDict["dishname"].ToString(),
                                Price = double.TryParse(excelCellArrayDict["dishprice"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["dishprice"].ToString(), CultureInfo.InvariantCulture) : 0,
                                Image = new Uri("https://web-api-media-storage.s3.eu-central-1.amazonaws.com/" + excelCellArrayDict["image"]?.ToString()),
                                DishType = ((insetedMenuCategory == Category.Main) ? DishType.Dish : insetedMenuCategory == Category.Drink ?
                                DishType.Drink : insetedMenuCategory == Category.Dessert ?
                                DishType.Desert : DishType.none),
                                RestaurantId = rests_new[counter].Id,
                                IsActive = IsActive.Yes
                            });
                        }
                        Console.WriteLine($"row {j} insertes");
                    }
                    // Dish? dish = context.Dishes.OrderByDescending(x => x.Id).FirstOrDefault();
                    // int dishIdBeforeBulk = dish != null ? dish.Id : 0;
                    context.Dishes.AddRange(dishes);
                    int a111 = await context.SaveChangesAsync();
                    //#################################################################################
                    //insert DishComponents
                    StringBuilder csvDishComponentFile = new StringBuilder();
                    List<DishComponent> dishComponents = new List<DishComponent>();
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        double testDouble = 0;
                        Dictionary<string, object> excelCellArrayDict = new Dictionary<string, object>();
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];

                        }
                        int testInt = 0;
                        double price = double.TryParse(excelCellArrayDict["dishprice"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["dishprice"].ToString(), CultureInfo.InvariantCulture) : 0;
                        int length = restaurantsByChainCode.Count;
                        int substituteGroup = checkIsNotNull(excelCellArrayDict, "substitutegroup", "substitutegroup") && int.TryParse(excelCellArrayDict["substitutegroup"].ToString(), out testInt) ? Convert.ToInt32(excelCellArrayDict["substitutegroup"].ToString(), CultureInfo.InvariantCulture) : 0;
                        for (int counter = 0; counter < restaurantsByChainCode.Count; counter++)
                        {
                            dishComponents.Add(new DishComponent()
                            {

                                DishId = (j - 1) * length + counter + dishIdBeforeBulk + 1,
                                IngredientId = ingredientBeforebulk + j,
                                Weight = 100,
                                Price = price,
                                IngredientType = IngredientType.MainIngredient,
                                SubstituteGroup = substituteGroup
                            });
                        }
                    }
                    DishComponent? dishComponent = context.DishComponents.OrderByDescending(x => x.Id).FirstOrDefault();
                    int dishComponentIdBeforeBulk = dishComponent != null ? dishComponent.Id : 0;
                    context.DishComponents.AddRange(dishComponents);
                    int a333 = await context.SaveChangesAsync();
                    //################################################################################
                    //insert Dietary Types
                    StringBuilder csvDietaryTypeFile = new StringBuilder();
                    List<DietaryType> dietaryTypes = new List<DietaryType>();
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        Console.WriteLine($"start of row  {j}");
                        int testInt = 0;
                        double testDouble = 0;
                        Dictionary<string, object> excelCellArrayDict = new Dictionary<string, object>();
                        var listOfDieteries = new List<int>();
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];

                        }
                        if ((excelCellArrayDict["vegan"] != null && excelCellArrayDict["vegan"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["vegan"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.Vegan);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {
                                // listOfDieteries.Add(0);

                            }
                        }
                        if ((excelCellArrayDict["vegetarian"] != null && excelCellArrayDict["vegetarian"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["vegetarian"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.Vegetarian);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {
                                // listOfDieteries.Add(0);

                            }
                        }
                        if ((excelCellArrayDict["halal"] != null && excelCellArrayDict["halal"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["halal"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.Halal);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {

                                // listOfDieteries.Add(0);
                            }
                        }
                        if ((excelCellArrayDict["glutenfree"] != null && excelCellArrayDict["glutenfree"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["glutenfree"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.GlutenFree);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {
                                // listOfDieteries.Add(0);

                            }
                        }
                        if ((excelCellArrayDict["nutfree"] != null && excelCellArrayDict["nutfree"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["nutfree"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.NutFree);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {
                                // listOfDieteries.Add(0);

                            }
                        }
                        if ((excelCellArrayDict["dairyfree"] != null && excelCellArrayDict["dairyfree"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["dairyfree"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.DairyFree);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {
                                // listOfDieteries.Add(0);

                            }
                        }
                        if ((excelCellArrayDict["kosher"] != null && excelCellArrayDict["kosher"].ToString() != ""))
                        {
                            try
                            {
                                int a = (int)Convert.ToDouble(excelCellArrayDict["kosher"].ToString(), CultureInfo.InstalledUICulture);
                                if (a == 1)
                                {
                                    listOfDieteries.Add((int)Dietary.Kosher);
                                }
                                else
                                {
                                    // listOfDieteries.Add(0);
                                }
                            }
                            catch (System.Exception)
                            {
                                // listOfDieteries.Add(0);

                            }
                        }

                        List<Restaurant> rests = context.Restaurants.Where(x => x.chainCode == excelRestaurantChainCodePrev.ToLower()).ToList();
                        int length = rests.Count;
                        for (int counter = 0; counter < rests.Count; counter++)
                        {
                            for (int dietaryConter = 0; dietaryConter < listOfDieteries.Count; dietaryConter++)
                            {

                                dietaryTypes.Add(new DietaryType()
                                {
                                    Dietary = (Dietary)listOfDieteries[dietaryConter],
                                    DishId = (j - 1) * length + dishIdBeforeBulk + counter + 1

                                });

                            }
                        }
                    }
                    context.DietaryTypes.AddRange(dietaryTypes);
                    int a444 = await context.SaveChangesAsync();
                    //########################################################################
                    //insert SubCategory
                    List<SubCategory> subCategories = new List<SubCategory>();
                    List<List<String>> tempRes1 = new List<List<string>>();
                    int rac1 = await context.Database.ExecuteSqlRawAsync("exec getLasIdSubcategories");
                    List<bulkModel> bmsSubcategories = context.bulkModels.FromSqlRaw("exec afterinsertCsvFilesFromS3ToRDS").ToList();
                    int subCategoryIdBeforeBulk = bmsSubcategories[0].dishIdBeforeBulk;
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        Dictionary<string, object> excelCellArrayDict = new Dictionary<string, object>();
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];

                        }
                        int insetedMenuCategory = (int)((excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dish")) ?
                        Category.Main : (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dessert")) ? Category.Dessert :
                        (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("drink")) ? Category.Drink : Category.Main);
                        string menuName = nameWithoutSpacesandLower(excelCellArrayDict["menuname"].ToString());
                        bool isAlreadyThere = false;
                        for (int tt = 0; tt < tempRes1.Count; tt++)
                        {
                            if ((tempRes1[tt])[0] == menuName && (tempRes1[tt])[1] == insetedMenuCategory.ToString())
                            {
                                isAlreadyThere = true;
                                break;
                            }

                        }
                        if (!isAlreadyThere)
                        {
                            tempRes1.Add(new List<string>() { menuName, insetedMenuCategory.ToString(), j.ToString() });
                        }
                    }
                    StringBuilder csvSubCategoryFile = new StringBuilder();
                    List<Restaurant> rests1 = context.Restaurants.Where(x => x.chainCode == excelRestaurantChainCodePrev.ToLower()).ToList();
                    for (int j = 0; j < tempRes1.Count; j++)
                    {
                        for (int counter = 0; counter < rests1.Count; counter++)
                        {
                            subCategories.Add(new SubCategory()
                            {
                                Name = (tempRes1[j])[0],
                                Category = (Category)(int.Parse((tempRes1[j])[1])),
                                RestaurantId = rests1[counter].Id

                            });
                        }
                    }
                    // SubCategory? subCategory = context.SubCategories.OrderByDescending(x => x.Id).FirstOrDefault();
                    // int subCategoryIdBeforeBulk = subCategory != null ? subCategory.Id : 0;
                    context.SubCategories.AddRange(subCategories);
                    int a555 = await context.SaveChangesAsync();
                    //################################################################################
                    //insert DishCategory
                    StringBuilder csvDishCategoryFile = new StringBuilder();
                    List<DishCategory> dishCategories = new List<DishCategory>();
                    int restLength = rests1.Count;
                    for (int c = 0; c < tempRes1.Count; c++)
                    {
                        int firstRow = int.Parse((tempRes1[c])[2]);
                        int numOfRows = 0;
                        if (c < tempRes1.Count - 1)
                        {
                            int a = int.Parse((tempRes1[c])[2]);
                            int b = int.Parse((tempRes1[c + 1])[2]);
                            numOfRows = b - a;
                        }
                        if (c == tempRes1.Count - 1)
                        {
                            int a = int.Parse((tempRes1[c])[2]);
                            int b = ExcelSheet.Rows.Count - 1;

                            numOfRows = b - a + 1;
                        }
                        for (int jj = 0; jj < rests1.Count; jj++)
                        {
                            for (int k = 0; k < numOfRows; k++)
                            {
                                dishCategories.Add(new DishCategory()
                                {
                                    DishId = (firstRow - 1) * rests1.Count + k * restLength + dishIdBeforeBulk + jj + 1,
                                    SubCategoryId = c * restLength + subCategoryIdBeforeBulk + jj + 1
                                });
                            }
                        }
                    }
                    context.DishCategories.AddRange(dishCategories);
                    int a666 = await context.SaveChangesAsync();
                    int countOfAffectedRows = await context.Database.ExecuteSqlRawAsync("exec updateChainRests @chainCode={0}", excelRestaurantChainCodePrev.ToLower());
                    List<bulkModel> bms = context.bulkModels.FromSqlRaw("exec afterinsertCsvFilesFromS3ToRDS").ToList();
                }
                //###########################################################################
                // For Non Chain Restaurants
                if (!isChain)
                {
                    for (int j = 1; j < ExcelSheet.Rows.Count; j++)
                    {
                        alreadyRestaurants = await context.Restaurants.Include(x => x.Menu).ToListAsync();
                        // List<Dish> alreadyDishes = await context.Dishes.ToListAsync();                    // {
                        System.Console.WriteLine("start of row " + j);
                        Object?[] cells = (ExcelSheet).Rows[j].ItemArray;
                        string excelRestaurantName = "none";
                        string excelRestaurantChainCode = "";
                        double excelRestaurantLongitude = (double)0.0;
                        double excelRestaurantLatitude = (double)0.0;
                        Dictionary<string, object> excelCellArrayDict = new Dictionary<string, object>();
                        Time initTime = new Time();
                        for (int k = 0; k < EditedColumnNames.Count(); k++)
                        {
                            if (EditedColumnNames[k] == "chain")
                            {
                                excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];
                                excelRestaurantChainCode = cells[k].ToString();
                                excelRestaurantChainCode = Regex.Replace(excelRestaurantChainCode, @"\t|\n|\r", "");
                                continue;
                            }
                            if (EditedColumnNames[k] == "name")
                            {
                                excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];
                                excelRestaurantName = cells[k].ToString();
                                continue;
                            }
                            if (EditedColumnNames[k] == "longitude")
                            {
                                try
                                {
                                    double x = Convert.ToDouble(cells[k].ToString(), CultureInfo.InvariantCulture);
                                    string x1 = x.ToString("0.00000", CultureInfo.InvariantCulture);
                                    excelRestaurantLongitude = Convert.ToDouble(x1, CultureInfo.InvariantCulture);
                                    excelCellArrayDict[EditedColumnNames[k].ToString()] = excelRestaurantLongitude;
                                    continue;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                    excelRestaurantLongitude = (float)0.0;
                                    excelCellArrayDict[EditedColumnNames[k].ToString()] = excelRestaurantLongitude;
                                    continue;
                                }
                            }
                            if (EditedColumnNames[k] == "latitude")
                            {
                                try
                                {
                                    double x = Convert.ToDouble(cells[k].ToString(), CultureInfo.InvariantCulture);
                                    string x1 = x.ToString("0.00000", CultureInfo.InvariantCulture);
                                    excelRestaurantLatitude = Convert.ToDouble(x1, CultureInfo.InvariantCulture);
                                    excelCellArrayDict[EditedColumnNames[k].ToString()] = excelRestaurantLatitude;
                                    continue;
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e.Message);
                                    excelRestaurantLatitude = (float)0.0;
                                    excelCellArrayDict[EditedColumnNames[k].ToString()] = excelRestaurantLatitude;
                                    continue;
                                }
                            }
                            excelCellArrayDict[EditedColumnNames[k].ToString()] = cells[k];

                        }
                        if (isChain)
                        {
                            foundedRestaurant = alreadyRestaurants.Where(x => nameWithoutSpacesandLower(x.Name) == nameWithoutSpacesandLower(excelRestaurantName) && x.chainCode != null && x.chainCode == nameWithoutSpacesandLower(excelRestaurantChainCode) && !x.IsUnofficialRestaurant).ToList();
                        }
                        else
                        {
                            foundedRestaurant = alreadyRestaurants.Where((x) => nameWithoutSpacesandLower(x.Name) == nameWithoutSpacesandLower(excelRestaurantName) && x.Longitude == excelRestaurantLongitude && x.Latitude == excelRestaurantLatitude).ToList();
                        }
                        var insetedMenuCategory = (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dish")) ?
                        Category.Main : (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dessert")) ? Category.Dessert :
                        (excelCellArrayDict.ContainsKey("menucategory") &&
                        excelCellArrayDict["menucategory"].ToString().ToLower().Contains("drink")) ? Category.Drink : Category.Main;
                        Ingredient? ing = !isChain ? alreadyIngredients.Where(x => nameWithoutSpacesandLower(x.NameEng) ==
                                  nameWithoutSpacesandLower(excelCellArrayDict["ingredient"].ToString())).ToList().FirstOrDefault() : null;
                        if (ing == null)
                        {

                            myDic.Add("Error is", $"in row {j}. there is no ingredient with name in the excel file-next rows will not be inserted");
                            return myDic;

                        }
                        if (foundedRestaurant != null && foundedRestaurant.Count > 0)
                        {
                            int ii = 0;
                            foundedRestaurant.ForEach(async foundedRes =>
                            {
                                SubCategory subCategory = foundedRes.Menu.ToList().Where(x =>
                                nameWithoutSpacesandLower(x.Name) == nameWithoutSpacesandLower(excelCellArrayDict["menuname"].ToString()) &&
                                x.Category == insetedMenuCategory
                                && (x.Restaurant != null
                                && x.Restaurant.Id == foundedRes.Id)).ToList().FirstOrDefault();
                                if (subCategory != null)
                                {
                                    Dish foundedDish = null;
                                    foundedDish = context.Dishes.Where(x => x.RestaurantId == foundedRes.Id).Where(x => nameWithoutSpacesandLower(x.Name) == nameWithoutSpacesandLower(excelCellArrayDict["dishname"].ToString())).FirstOrDefault();
                                    if (foundedDish != null)
                                    {
                                        int testInt = 0;
                                        double testDouble = 0;
                                        Ingredient? ing = !isChain ? alreadyIngredients.Where(x => nameWithoutSpacesandLower(x.NameEng) ==
                                             nameWithoutSpacesandLower(excelCellArrayDict["ingredient"].ToString())).ToList().FirstOrDefault() : null;
                                        DishComponent ndc = new DishComponent()
                                        {
                                            DishId = foundedDish.Id,
                                            Weight = (!isChain) ?
                                            (checkIsNotNull(excelCellArrayDict, "measurementingrams", "measurementingrams") && double.TryParse(excelCellArrayDict["measurementingrams"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["measurementingrams"].ToString(), CultureInfo.InvariantCulture) : 0) : 100,
                                            SubstituteGroup = checkIsNotNull(excelCellArrayDict, "substitutegroup", "substitutegroup") && int.TryParse(excelCellArrayDict["substitutegroup"].ToString(), out testInt) ? Convert.ToInt32(excelCellArrayDict["substitutegroup"].ToString(), CultureInfo.InvariantCulture) : 0,
                                            IngredientId = (ing != null) ? ing.Id : 0,
                                        };
                                        context.DishComponents.Add(ndc);
                                    }
                                    else
                                    {
                                        var listOfDietarTypes = new List<DietaryType>();
                                        var listOfDieteries = new List<Dietary>();
                                        if ((excelCellArrayDict["vegan"] != null && excelCellArrayDict["vegan"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["vegan"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.Vegan
                                                    });
                                                    listOfDieteries.Add(Dietary.Vegan);
                                                }
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        if ((excelCellArrayDict["vegetarian"] != null && excelCellArrayDict["vegetarian"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["vegetarian"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.Vegetarian
                                                    });
                                                }
                                                listOfDieteries.Add(Dietary.Vegetarian);
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        if ((excelCellArrayDict["halal"] != null && excelCellArrayDict["halal"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["halal"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.Halal
                                                    });
                                                    listOfDieteries.Add(Dietary.Halal);
                                                }
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        if ((excelCellArrayDict["glutenfree"] != null && excelCellArrayDict["glutenfree"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["glutenfree"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.GlutenFree
                                                    });
                                                    listOfDieteries.Add(Dietary.GlutenFree);
                                                }
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        if ((excelCellArrayDict["nutfree"] != null && excelCellArrayDict["nutfree"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["nutfree"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.NutFree
                                                    });
                                                    listOfDieteries.Add(Dietary.NutFree);
                                                }
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        if ((excelCellArrayDict["dairyfree"] != null && excelCellArrayDict["dairyfree"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["dairyfree"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.DairyFree
                                                    });
                                                    listOfDieteries.Add(Dietary.DairyFree);
                                                }
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        if ((excelCellArrayDict["kosher"] != null && excelCellArrayDict["kosher"].ToString() != ""))
                                        {
                                            try
                                            {
                                                int a = (int)Convert.ToDouble(excelCellArrayDict["kosher"].ToString(), CultureInfo.InstalledUICulture);
                                                if (a == 1)
                                                {
                                                    listOfDietarTypes.Add(new DietaryType()
                                                    {
                                                        Dietary = Dietary.Kosher
                                                    });
                                                    listOfDieteries.Add(Dietary.Kosher);
                                                }
                                            }
                                            catch (System.Exception)
                                            {


                                            }
                                        }
                                        int testInt = 0;
                                        double testDouble = 0;
                                        Ingredient? ing = !isChain ? alreadyIngredients.Where(x => nameWithoutSpacesandLower(x.NameEng) ==
                                             nameWithoutSpacesandLower(excelCellArrayDict["ingredient"].ToString())).ToList().FirstOrDefault() : null;
                                        DishCategory dishCategory = new DishCategory()
                                        {
                                            SubCategoryId = subCategory.Id,
                                            Dish = new Dish()
                                            {
                                                DishType = insetedMenuCategory == Category.Main ?
                                              DishType.Dish : insetedMenuCategory == Category.Drink ?
                                              DishType.Drink : insetedMenuCategory == Category.Dessert ?
                                               DishType.Desert : DishType.none,
                                                Name = excelCellArrayDict["dishname"].ToString(),
                                                Price = double.TryParse(excelCellArrayDict["dishprice"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["dishprice"].ToString(), CultureInfo.InvariantCulture) : 0,
                                                IsActive = IsActive.Yes,
                                                Restaurant = foundedRes,
                                                DietaryType = listOfDietarTypes,
                                                Image = new Uri("https://web-api-media-storage.s3.eu-central-1.amazonaws.com/" + excelCellArrayDict["image"]?.ToString(), UriKind.Absolute),
                                                Components = new List<DishComponent>(){
                                                        new DishComponent(){
                                                            Weight = (!isChain) ?
                                                        (checkIsNotNull(excelCellArrayDict, "measurementingrams", "measurementingrams") && double.TryParse(excelCellArrayDict["measurementingrams"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["measurementingrams"].ToString(), CultureInfo.InvariantCulture) : 0) : 100,
                                                            SubstituteGroup = checkIsNotNull(excelCellArrayDict, "substitutegroup", "substitutegroup") && int.TryParse(excelCellArrayDict["substitutegroup"].ToString(),out testInt) ? Convert.ToInt32(excelCellArrayDict["substitutegroup"].ToString(), CultureInfo.InvariantCulture) : 0,
                                                            IngredientId =  (ing != null) ? ing.Id : 0,
                                                        }
                                                        }
                                            }
                                        };
                                        context.DishCategories.Add(dishCategory);
                                    }
                                }
                                else
                                {
                                    var listOfDietarTypes = new List<DietaryType>();
                                    if ((excelCellArrayDict["vegan"] != null && excelCellArrayDict["vegan"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["vegan"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.Vegan
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    if ((excelCellArrayDict["vegetarian"] != null && excelCellArrayDict["vegetarian"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["vegetarian"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.Vegetarian
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    if ((excelCellArrayDict["halal"] != null && excelCellArrayDict["halal"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["halal"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.Halal
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    if ((excelCellArrayDict["glutenfree"] != null && excelCellArrayDict["glutenfree"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["glutenfree"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.GlutenFree
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    if ((excelCellArrayDict["nutfree"] != null && excelCellArrayDict["nutfree"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["nutfree"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.NutFree
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    if ((excelCellArrayDict["dairyfree"] != null && excelCellArrayDict["dairyfree"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["dairyfree"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.DairyFree
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    if ((excelCellArrayDict["kosher"] != null && excelCellArrayDict["kosher"].ToString() != ""))
                                    {
                                        try
                                        {
                                            int a = (int)Convert.ToDouble(excelCellArrayDict["kosher"].ToString(), CultureInfo.InstalledUICulture);
                                            if (a == 1)
                                            {
                                                listOfDietarTypes.Add(new DietaryType()
                                                {
                                                    Dietary = Dietary.Kosher
                                                });
                                            }
                                        }
                                        catch (System.Exception)
                                        {


                                        }
                                    }
                                    int testInt = 0;
                                    double testDouble = 0;
                                    Ingredient? ing = !isChain ? alreadyIngredients.Where(x => nameWithoutSpacesandLower(x.NameEng) ==
                                         nameWithoutSpacesandLower(excelCellArrayDict["ingredient"].ToString())).ToList().FirstOrDefault() : null;
                                    SubCategory newSubCategory = new SubCategory()
                                    {
                                        Name = excelCellArrayDict.ContainsKey("menucategory") ? excelCellArrayDict["menuname"].ToString() : "",
                                        Category = (excelCellArrayDict.ContainsKey("menucategory") && excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dish")) ?
                                        Category.Main :
                                        (excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dessert")) ?
                                        Category.Dessert :
                                        (excelCellArrayDict["menucategory"].ToString().ToLower().Contains("drink")) ?
                                        Category.Drink : Category.Main,
                                        RestaurantId = foundedRes.Id,
                                        DishCategories = new List<DishCategory>(){
                                            new DishCategory()
                                            {
                                                Dish=new Dish()
                                                {
                                                    DishType = (excelCellArrayDict.ContainsKey("menucategory") && excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dish")) ?
                                                    DishType.Dish :
                                                    (excelCellArrayDict["menucategory"].ToString().ToLower().Contains("dessert")) ?
                                                    DishType.Desert :
                                                    (excelCellArrayDict["menucategory"].ToString().ToLower().Contains("drink")) ?
                                                        DishType.Drink : DishType.none,
                                                    DietaryType = listOfDietarTypes,
                                                    Name = excelCellArrayDict["dishname"].ToString(),
                                                    IsActive = IsActive.Yes,
                                                    Image = new Uri("https://web-api-media-storage.s3.eu-central-1.amazonaws.com/" + excelCellArrayDict["image"]?.ToString(), UriKind.Absolute),
                                                    Price = double.TryParse(excelCellArrayDict["dishprice"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["dishprice"].ToString(), CultureInfo.InvariantCulture) : 0,
                                                    RestaurantId = foundedRes.Id,
                                                    Components = new List<DishComponent>()
                                                    {
                                                        new DishComponent()
                                                        {
                                                                Weight = (!isChain) ?
                                                            (checkIsNotNull(excelCellArrayDict, "measurementingrams", "measurementingrams") && double.TryParse(excelCellArrayDict["measurementingrams"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["measurementingrams"].ToString(), CultureInfo.InvariantCulture) : 0) : 100,
                                                                SubstituteGroup = checkIsNotNull(excelCellArrayDict, "substitutegroup", "substitutegroup") && int.TryParse(excelCellArrayDict["substitutegroup"].ToString(),out testInt) ? Convert.ToInt32(excelCellArrayDict["substitutegroup"].ToString(), CultureInfo.InvariantCulture) : 0,
                                                                IngredientId = (ing != null) ? ing.Id : 0,
                                                        }
                                                    }

                                                }

                                            }
                                    }
                                    };
                                    context.SubCategories.Add(newSubCategory);
                                }
                                List<bulkModel> bms = context.bulkModels.FromSqlRaw("exec updateRegularRests @id={0}", foundedRes.Id).ToList();
                            });
                        }
                        else
                        {
                            var listOfDietarTypes = new List<DietaryType>();
                            if ((excelCellArrayDict["vegan"] != null && excelCellArrayDict["vegan"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["vegan"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.Vegan
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            if ((excelCellArrayDict["vegetarian"] != null && excelCellArrayDict["vegetarian"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["vegetarian"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.Vegetarian
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            if ((excelCellArrayDict["halal"] != null && excelCellArrayDict["halal"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["halal"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.Halal
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            if ((excelCellArrayDict["glutenfree"] != null && excelCellArrayDict["glutenfree"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["glutenfree"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.GlutenFree
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            if ((excelCellArrayDict["nutfree"] != null && excelCellArrayDict["nutfree"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["nutfree"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.NutFree
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            if ((excelCellArrayDict["dairyfree"] != null && excelCellArrayDict["dairyfree"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["dairyfree"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.DairyFree
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            if ((excelCellArrayDict["kosher"] != null && excelCellArrayDict["kosher"].ToString() != ""))
                            {
                                try
                                {
                                    int a = (int)Convert.ToDouble(excelCellArrayDict["kosher"].ToString(), CultureInfo.InstalledUICulture);
                                    if (a == 1)
                                    {
                                        listOfDietarTypes.Add(new DietaryType()
                                        {
                                            Dietary = Dietary.Kosher
                                        });
                                    }
                                }
                                catch (System.Exception)
                                {


                                }
                            }
                            int testInt = 0;
                            double testDouble = 0;
                            // Ingredient? ing = !isChain ? alreadyIngredients.Where(x => nameWithoutSpacesandLower(x.NameEng) ==
                            //             nameWithoutSpacesandLower(excelCellArrayDict["ingredient"].ToString())).ToList().FirstOrDefault() : null;
                            Restaurant restaurant = new Restaurant()
                            {
                                Name = nameWithoutSpacesandLower(excelRestaurantName),
                                Longitude = excelRestaurantLongitude,
                                Latitude = excelRestaurantLatitude,
                                IsAddByAdmin = true,
                                IsUnofficialRestaurant = (isChain) ? true : false,
                                Menu = new List<SubCategory>(){
                                    new SubCategory(){
                                        Name=excelCellArrayDict["menuname"].ToString(),
                                        Category=insetedMenuCategory,
                                        DishCategories=new List<DishCategory>(){
                                            new DishCategory(){
                                                  Dish=new Dish(){
                                             DishType= insetedMenuCategory==Category.Main?
                                    DishType.Dish :
                                    insetedMenuCategory==Category.Dessert ?
                                    DishType.Desert :
                                    insetedMenuCategory==Category.Drink?
                                    DishType.Drink : DishType.none,
                                                DietaryType=listOfDietarTypes,
                                                IsActive=IsActive.Yes,
                                                Name=excelCellArrayDict["dishname"].ToString(),
                                                    Price = double.TryParse(excelCellArrayDict["dishprice"].ToString(),out testDouble)?Convert.ToDouble(excelCellArrayDict["dishprice"].ToString(), CultureInfo.InvariantCulture):0,
                                                    Image = new Uri("https://web-api-media-storage.s3.eu-central-1.amazonaws.com/" + excelCellArrayDict["image"]?.ToString(), UriKind.Absolute),
                                        Components = new List<DishComponent>(){
                                            new DishComponent(){
                                                Weight = (!isChain) ?
                                    (checkIsNotNull(excelCellArrayDict, "measurementingrams", "measurementingrams") && double.TryParse(excelCellArrayDict["measurementingrams"].ToString(), out testDouble) ? Convert.ToDouble(excelCellArrayDict["measurementingrams"].ToString(), CultureInfo.InvariantCulture) : 0) : 100,
                                                SubstituteGroup = checkIsNotNull(excelCellArrayDict, "substitutegroup", "substitutegroup")&&int.TryParse(excelCellArrayDict["substitutegroup"].ToString(),out testInt) ? Convert.ToInt32(excelCellArrayDict["substitutegroup"].ToString(), CultureInfo.InvariantCulture) : 0,
                                                Ingredient =  ing
                                            }
                                        }

                                         }
                                    }
                                        }
                                    }
                                }
                            };
                            restaurant.adminRestaurantIncludeMenu = true;
                            context.Restaurants.Add(restaurant);
                        }
                        int num = context.SaveChanges();
                        Console.WriteLine($"row {j} inserted");
                    }
                }

            }
            // }
            // catch (System.Exception e)
            // {
            //     Console.WriteLine("Error is " + e.Message);
            //     context.bulkModels.FromSqlRaw("exec checkIngredients").ToList();
            //     myDic.Add("Error", e.Message);
            //     return myDic;
            // }
            myDic.Add("ok", "ok");
            return myDic;
        }

        public bool checkIsNotNull(Dictionary<string, object> dict, string key, object v)
        {
            if (dict != null && dict.ContainsKey(key) && v != null && v.ToString() != "")
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public async Task<string> saveToJournal(int? dishId, ApplicationUser user, ApplicationDbContext context)
        {
            Dish foundedDishes = await context.Dishes.Where(x => x.Id == dishId).Include(x => x.Components).ThenInclude(x => x.Ingredient).FirstOrDefaultAsync();
            if (foundedDishes != null)
            {
                Order order = new Order()
                {
                    CreationDate = DateTime.UtcNow,
                    TypeOfPayment = TypeOfPayment.Card,
                    OrderStatus = Status.Success,
                    ApplicationUser = user,
                    ApplicationUserId = user.Id,
                    RestaurantId = foundedDishes.RestaurantId,
                };
                if (foundedDishes.Components != null)
                {
                    List<OrderedItem> orderedItems = new List<OrderedItem>();
                    foreach (DishComponent item in foundedDishes.Components)
                    {
                        order.OrderedItems.Add(new OrderedItem()
                        {
                            Name = foundedDishes.Name,
                            Price = item.Price,
                            Image = foundedDishes.Image,
                            Count = 1,
                            OrderedIngredients = setIngredientForOrderedIngredient(item),
                            ItemСharacteristics = setIngredientForItemCharestarestic(item)

                        });
                    }
                }
                context.Orders.Add(order);
            }
            return "ok";
        }
        private List<OrderedIngredient> setIngredientForOrderedIngredient(DishComponent item)
        {
            List<OrderedIngredient> orderedIngredients = new List<OrderedIngredient>();
            orderedIngredients.Add(new OrderedIngredient()
            {
                IngredientNameEng = item.Ingredient.NameEng,
                IngredientNameEs = item.Ingredient.NameEs,
                IngredientNameFr = item.Ingredient.NameFr,
                IngredientNameNl = item.Ingredient.NameNl,
                IngredientType = item.IngredientType,
                Price = item.Price,
                Weight = item.Weight
            });
            return orderedIngredients;
        }

        private ItemСharacteristics setIngredientForItemCharestarestic(DishComponent item)
        {
            ItemСharacteristics itemСharacteristics = new ItemСharacteristics();
            itemСharacteristics.AddIngridient(item.Ingredient, item.Weight);
            // foreach (PropertyInfo property in itemСharacteristics.GetType().GetProperties())
            // {
            //     foreach (PropertyInfo prop in item.Ingredient.GetType().GetProperties())
            //     {
            //         if (property.Name.ToLower() != "Id")
            //         {
            //             if (property.Name.ToLower() == prop.Name.ToLower())
            //             {
            //                 property.SetValue(itemСharacteristics, prop.GetValue(item));
            //             }
            //         }
            //     }
            // }
            return itemСharacteristics;

        }
    }

}