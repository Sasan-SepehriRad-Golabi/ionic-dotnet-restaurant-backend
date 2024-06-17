using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ruddy.WEB.Models;

namespace Ruddy.WEB.DataAccess
{
    public class AppDbContextForMenuInserting : DbContext
    {
        public DbSet<Discount> Discounts { get; set; }
        public DbSet<Coupons> Coupons { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<AlternativeIngredient> AlternativeIngredients { get; set; }
        public DbSet<Ingredient> Ingredients { get; set; }
        public DbSet<Dish> Dishes { get; set; }
        public DbSet<DietaryType> DietaryTypes { get; set; }
        public DbSet<DishComponent> DishComponents { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<SubCategory> SubCategories { get; set; }
        public DbSet<Friend> Friends { get; set; }
        public DbSet<Restaurant> Restaurants { get; set; }
        // public DbSet<AdminRestaurant> AdminRestaurants { get; set; }
        public DbSet<RestaurantUser> RestaurantUsers { get; set; }
        public DbSet<DishCategory> DishCategories { get; set; }
        public DbSet<Time> Times { get; set; }
        public DbSet<Pause> Pauses { get; set; }
        public DbSet<FcmTokens> FcmTokens { get; set; }
        public DbSet<CraftedComponent> CraftedComponents { get; set; }
        public DbSet<SPModel> SpModels { get; set; }

        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderedItem> OrderedItems { get; set; }
        public DbSet<OrderedIngredient> OrderedIngredients { get; set; }
        public DbSet<ItemСharacteristics> ItemСharacteristics { get; set; }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Data Source.0.1;Initial Catalog=myLocalDb;User ID=ttt;Password=S@123456;TrustServerCertificate=True;Connection Timeout=5000;");
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<DishCategory>()
                .HasKey(bc => new { bc.DishId, bc.SubCategoryId });
            builder.Entity<DishCategory>()
                .HasOne(bc => bc.Dish)
                .WithMany(b => b.DishCategories)
                .HasForeignKey(bc => bc.DishId);
            builder.Entity<DishCategory>()
                .HasOne(bc => bc.SubCategory)
                .WithMany(c => c.DishCategories)
                .HasForeignKey(bc => bc.SubCategoryId);

            builder.Entity<CraftedComponentIngridient>()
            .HasKey(t => new { t.CraftedComponentId, t.IngredientId });

            builder.Entity<CraftedComponentIngridient>()
                .HasOne(sc => sc.CraftedComponent)
                .WithMany(s => s.CraftedComponentIngridients)
                .HasForeignKey(sc => sc.CraftedComponentId);

            builder.Entity<DiscountDish>()
                .HasKey(bc => new { bc.DiscountId, bc.DishId });
            builder.Entity<DiscountDish>()
                .HasOne(bc => bc.Dish)
                .WithMany(b => b.DiscountDishes)
                .HasForeignKey(bc => bc.DishId);
            builder.Entity<DiscountDish>()
                .HasOne(bc => bc.Discount)
                .WithMany(c => c.DiscountDishes)
                .HasForeignKey(bc => bc.DiscountId);
        }
    }
}