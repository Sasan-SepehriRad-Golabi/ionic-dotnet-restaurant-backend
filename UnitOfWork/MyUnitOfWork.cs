using System;
using System.Threading.Tasks;
using Ruddy.WEB.DataAccess;

namespace Ruddy.WEB.UnitOfWork
{
    public class MyUnitOfWork : IDisposable
    {
        private ApplicationDbContext _myappcontext;
        public Ruddy.WEB.UnitOfWork.Repository.RestaurantRepository restaurantRepository;
        public MyUnitOfWork(ApplicationDbContext myAppContext)
        {
            _myappcontext = myAppContext;
            restaurantRepository = new Ruddy.WEB.UnitOfWork.Repository.RestaurantRepository(myAppContext);

        }
        public async Task<string> complete()
        {
            try
            {
                int num = await _myappcontext.SaveChangesAsync();
                return string.Format("{0} objects got written into database", num);
            }
            catch (System.Exception e)
            {

                return string.Format("Error in saving objects to database: {0}", e.Message);
            }
        }

        public void Dispose()
        {
            _myappcontext.Dispose();
        }
    }
}