using AutoMapper;
using Ruddy.WEB.Models;
using Ruddy.WEB.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ruddy.WEB.Services
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<RestaurantUserViewModel, RestaurantUser>();
            CreateMap<RestaurantUser, RestaurantUserViewModel>();
            CreateMap<ApplicationUser, UserViewModel>();
            CreateMap<UserViewModel, ApplicationUser>();
            CreateMap<RestaurantViewModel, Restaurant>();
            CreateMap<Restaurant, RestaurantViewModel>();

            CreateMap<OrderedIngredient, OrderedIngredientViewModel>();

            CreateMap<OrderedItem, OrderedItemV2ViewModel>();

            CreateMap<Order, RestaurantOrderViewModel>()
                .ForMember("OrderPrice", opt => opt.MapFrom(oi => oi.OrderedItems.Sum(orI => orI.Price * orI.Count)))
                .ForMember("PromotionalOrderPrice", opt => opt.MapFrom(oi => oi.OrderedItems.Sum(orI => orI.PromotionalPrice ?? 0.0)))
                .ForMember("CustomerId", opt => opt.MapFrom(oi => oi.ApplicationUserId))
                .ForMember("CustomerFirstName", opt => opt.MapFrom(oi => oi.ApplicationUser.FirstName))
                .ForMember("CustomerLastName", opt => opt.MapFrom(oi => oi.ApplicationUser.LastName))
                .ForMember("CustomerPhoneNumber", opt => opt.MapFrom(oi => oi.ApplicationUser.PhoneNumber));
        }
    }
}
