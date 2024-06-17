using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ruddy.WEB.Migrations
{
    public partial class mig1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConnectedAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerAccountId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BirthDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    Height = table.Column<double>(type: "float", nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    LevelOfActivity = table.Column<int>(type: "int", nullable: true),
                    ProfileImage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaffLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LogoLink = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsStripeAccountCompleted = table.Column<bool>(type: "bit", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "bulkModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    dishIdBeforeBulk = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bulkModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Discounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    From = table.Column<DateTime>(type: "datetime2", nullable: false),
                    To = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Percent = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ItemСharacteristics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Energy_KCal = table.Column<double>(type: "float", nullable: false),
                    Energy_Kj = table.Column<double>(type: "float", nullable: false),
                    Proteins_G = table.Column<double>(type: "float", nullable: false),
                    Fat_G = table.Column<double>(type: "float", nullable: false),
                    Carbohydrates_G = table.Column<double>(type: "float", nullable: false),
                    Sugars_G = table.Column<double>(type: "float", nullable: false),
                    Starch_G = table.Column<double>(type: "float", nullable: false),
                    Water_G = table.Column<double>(type: "float", nullable: false),
                    FattyAcidsSaturated_G = table.Column<double>(type: "float", nullable: false),
                    Glucose_G = table.Column<double>(type: "float", nullable: false),
                    Lactose_G = table.Column<double>(type: "float", nullable: false),
                    Fructose_G = table.Column<double>(type: "float", nullable: false),
                    Sucrose_G = table.Column<double>(type: "float", nullable: false),
                    Maltose_G = table.Column<double>(type: "float", nullable: false),
                    GaLactose_G = table.Column<double>(type: "float", nullable: false),
                    FibresSum_G = table.Column<double>(type: "float", nullable: false),
                    FA_C12_0_G = table.Column<double>(type: "float", nullable: false),
                    FA_C14_0_G = table.Column<double>(type: "float", nullable: false),
                    FA_C16_0_G = table.Column<double>(type: "float", nullable: false),
                    FA_monounsat_sum_G = table.Column<double>(type: "float", nullable: false),
                    FA_polyunsat_sum_G = table.Column<double>(type: "float", nullable: false),
                    FA_omega_3_sum_G = table.Column<double>(type: "float", nullable: false),
                    FA_C20_5_n_3_cis_EPA_G = table.Column<double>(type: "float", nullable: false),
                    FA_C22_6_n_3_cis_DHA_G = table.Column<double>(type: "float", nullable: false),
                    FA_C18_3_n_3_cis_linolenic_G = table.Column<double>(type: "float", nullable: false),
                    FA_omega_6_sum_G = table.Column<double>(type: "float", nullable: false),
                    FA_C18_2_n_6_cis_linoleic_G = table.Column<double>(type: "float", nullable: false),
                    FA_trans_sum_G = table.Column<double>(type: "float", nullable: false),
                    Cholesterol_MG = table.Column<double>(type: "float", nullable: false),
                    Alcohol_G = table.Column<double>(type: "float", nullable: false),
                    Polyols_sum_G = table.Column<double>(type: "float", nullable: false),
                    Sodium_MG = table.Column<double>(type: "float", nullable: false),
                    Potassium_MG = table.Column<double>(type: "float", nullable: false),
                    Calcium_MG = table.Column<double>(type: "float", nullable: false),
                    Phosphorus_MG = table.Column<double>(type: "float", nullable: false),
                    Magnesium_MG = table.Column<double>(type: "float", nullable: false),
                    Iron_MG = table.Column<double>(type: "float", nullable: false),
                    Copper_MG = table.Column<double>(type: "float", nullable: false),
                    Zinc_MG = table.Column<double>(type: "float", nullable: false),
                    Iodide_MG = table.Column<double>(type: "float", nullable: false),
                    Selenium_MCG = table.Column<double>(type: "float", nullable: false),
                    VitA_Activity_MCG = table.Column<double>(type: "float", nullable: false),
                    VitB1_MG = table.Column<double>(type: "float", nullable: false),
                    VitB2_MG = table.Column<double>(type: "float", nullable: false),
                    VitB12_MCG = table.Column<double>(type: "float", nullable: false),
                    Folate_MCG = table.Column<double>(type: "float", nullable: false),
                    VitC_MG = table.Column<double>(type: "float", nullable: false),
                    VitD_MCG = table.Column<double>(type: "float", nullable: false),
                    VitE_MG = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ItemСharacteristics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SpModels",
                columns: table => new
                {
                    DishId = table.Column<int>(type: "int", nullable: true),
                    compNumber = table.Column<int>(type: "int", nullable: true),
                    mainEnergy = table.Column<int>(type: "int", nullable: true),
                    RestaurantId = table.Column<int>(type: "int", nullable: true),
                    DishType = table.Column<int>(type: "int", nullable: true),
                    Dietary = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Coupons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CouponId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Enable = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Coupons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Coupons_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CraftedComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameFr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEng = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameNl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RestaurantUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CraftedComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CraftedComponents_AspNetUsers_RestaurantUserId",
                        column: x => x.RestaurantUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FcmTokens",
                columns: table => new
                {
                    FcmToken = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FcmTokens", x => x.FcmToken);
                    table.ForeignKey(
                        name: "FK_FcmTokens_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Friends",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendAccountId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId = table.Column<int>(type: "int", nullable: false),
                    ApplicationUserId1 = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friends", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Friends_AspNetUsers_ApplicationUserId1",
                        column: x => x.ApplicationUserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Restaurants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FirstPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecondPhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    RestaurantCategory = table.Column<int>(type: "int", nullable: false),
                    VAT = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Facebook = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Instagram = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Whatsapp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Twitter = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Website = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Background = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsUnofficialRestaurant = table.Column<bool>(type: "bit", nullable: false),
                    chainCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RestaurantUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    IsAddByAdmin = table.Column<bool>(type: "bit", nullable: false),
                    adminRestaurantIncludeMenu = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Restaurants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Restaurants_AspNetUsers_RestaurantUserId",
                        column: x => x.RestaurantUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameFr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEng = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameEs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NameNl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Group = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Energy_KCal = table.Column<double>(type: "float", nullable: true),
                    Energy_Kj = table.Column<double>(type: "float", nullable: true),
                    Proteins_G = table.Column<double>(type: "float", nullable: true),
                    Fat_G = table.Column<double>(type: "float", nullable: true),
                    Carbohydrates_G = table.Column<double>(type: "float", nullable: true),
                    Sugars_G = table.Column<double>(type: "float", nullable: true),
                    Starch_G = table.Column<double>(type: "float", nullable: true),
                    Water_G = table.Column<double>(type: "float", nullable: true),
                    FattyAcidsSaturated_G = table.Column<double>(type: "float", nullable: true),
                    Glucose_G = table.Column<double>(type: "float", nullable: true),
                    Lactose_G = table.Column<double>(type: "float", nullable: true),
                    Fructose_G = table.Column<double>(type: "float", nullable: true),
                    Sucrose_G = table.Column<double>(type: "float", nullable: true),
                    Maltose_G = table.Column<double>(type: "float", nullable: true),
                    GaLactose_G = table.Column<double>(type: "float", nullable: true),
                    FibresSum_G = table.Column<double>(type: "float", nullable: true),
                    FA_C12_0_G = table.Column<double>(type: "float", nullable: true),
                    FA_C14_0_G = table.Column<double>(type: "float", nullable: true),
                    FA_C16_0_G = table.Column<double>(type: "float", nullable: true),
                    FA_monounsat_sum_G = table.Column<double>(type: "float", nullable: true),
                    FA_polyunsat_sum_G = table.Column<double>(type: "float", nullable: true),
                    FA_omega_3_sum_G = table.Column<double>(type: "float", nullable: true),
                    FA_C20_5_n_3_cis_EPA_G = table.Column<double>(type: "float", nullable: true),
                    FA_C22_6_n_3_cis_DHA_G = table.Column<double>(type: "float", nullable: true),
                    FA_C18_3_n_3_cis_linolenic_G = table.Column<double>(type: "float", nullable: true),
                    FA_omega_6_sum_G = table.Column<double>(type: "float", nullable: true),
                    FA_C18_2_n_6_cis_linoleic_G = table.Column<double>(type: "float", nullable: true),
                    FA_trans_sum_G = table.Column<double>(type: "float", nullable: true),
                    Cholesterol_MG = table.Column<double>(type: "float", nullable: true),
                    Alcohol_G = table.Column<double>(type: "float", nullable: true),
                    Polyols_sum_G = table.Column<double>(type: "float", nullable: true),
                    Sodium_MG = table.Column<double>(type: "float", nullable: true),
                    Potassium_MG = table.Column<double>(type: "float", nullable: true),
                    Calcium_MG = table.Column<double>(type: "float", nullable: true),
                    Phosphorus_MG = table.Column<double>(type: "float", nullable: true),
                    Magnesium_MG = table.Column<double>(type: "float", nullable: true),
                    Iron_MG = table.Column<double>(type: "float", nullable: true),
                    Copper_MG = table.Column<double>(type: "float", nullable: true),
                    Zinc_MG = table.Column<double>(type: "float", nullable: true),
                    Iodide_MG = table.Column<double>(type: "float", nullable: true),
                    Selenium_MCG = table.Column<double>(type: "float", nullable: true),
                    VitA_Activity_MCG = table.Column<double>(type: "float", nullable: true),
                    VitB1_MG = table.Column<double>(type: "float", nullable: true),
                    VitB2_MG = table.Column<double>(type: "float", nullable: true),
                    VitB12_MCG = table.Column<double>(type: "float", nullable: true),
                    Folate_MCG = table.Column<double>(type: "float", nullable: true),
                    VitC_MG = table.Column<double>(type: "float", nullable: true),
                    VitD_MCG = table.Column<double>(type: "float", nullable: true),
                    VitE_MG = table.Column<double>(type: "float", nullable: true),
                    isChainInsertedByAdmin = table.Column<int>(type: "int", nullable: false),
                    CraftedComponentId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ingredients_CraftedComponents_CraftedComponentId",
                        column: x => x.CraftedComponentId,
                        principalTable: "CraftedComponents",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    PromotionalPrice = table.Column<double>(type: "float", nullable: false),
                    IsPromotional = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DishType = table.Column<int>(type: "int", nullable: true),
                    Weight = table.Column<double>(type: "float", nullable: true),
                    RestaurantId = table.Column<int>(type: "int", nullable: true),
                    isChainInsertedByAdmin = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Orders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TypeOfPayment = table.Column<int>(type: "int", nullable: false),
                    OrderStatus = table.Column<int>(type: "int", nullable: false),
                    PaymentIntentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    RestaurantId = table.Column<int>(type: "int", nullable: true),
                    IsPaymentSuccess = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Orders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Orders_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Orders_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RestaurantRecievers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RestaurantId = table.Column<int>(type: "int", nullable: false),
                    FcmTokensFcmToken = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RestaurantRecievers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RestaurantRecievers_FcmTokens_FcmTokensFcmToken",
                        column: x => x.FcmTokensFcmToken,
                        principalTable: "FcmTokens",
                        principalColumn: "FcmToken");
                    table.ForeignKey(
                        name: "FK_RestaurantRecievers_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SubCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    RestaurantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SubCategories_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Times",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Day = table.Column<int>(type: "int", nullable: false),
                    OpeningTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ClosingTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RestaurantId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Times", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Times_Restaurants_RestaurantId",
                        column: x => x.RestaurantId,
                        principalTable: "Restaurants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AlternativeIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MainIngredientId = table.Column<int>(type: "int", nullable: false),
                    AltIngredientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlternativeIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AlternativeIngredients_Ingredients_MainIngredientId",
                        column: x => x.MainIngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CraftedComponentIngridient",
                columns: table => new
                {
                    CraftedComponentId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CraftedComponentIngridient", x => new { x.CraftedComponentId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_CraftedComponentIngridient_CraftedComponents_CraftedComponentId",
                        column: x => x.CraftedComponentId,
                        principalTable: "CraftedComponents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CraftedComponentIngridient_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DietaryTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Dietary = table.Column<int>(type: "int", nullable: false),
                    DishId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DietaryTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DietaryTypes_MenuItems_DishId",
                        column: x => x.DishId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DiscountDish",
                columns: table => new
                {
                    DiscountId = table.Column<int>(type: "int", nullable: false),
                    DishId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DiscountDish", x => new { x.DiscountId, x.DishId });
                    table.ForeignKey(
                        name: "FK_DiscountDish_Discounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Discounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DiscountDish_MenuItems_DishId",
                        column: x => x.DishId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DishComponents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DishId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    IngredientType = table.Column<int>(type: "int", nullable: false),
                    SubstituteGroup = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishComponents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DishComponents_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DishComponents_MenuItems_DishId",
                        column: x => x.DishId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderedItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<double>(type: "float", nullable: false),
                    PromotionalPrice = table.Column<double>(type: "float", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Count = table.Column<int>(type: "int", nullable: false),
                    ItemСharacteristicsId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderedItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderedItems_ItemСharacteristics_ItemСharacteristicsId",
                        column: x => x.ItemСharacteristicsId,
                        principalTable: "ItemСharacteristics",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrderedItems_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DishCategories",
                columns: table => new
                {
                    DishId = table.Column<int>(type: "int", nullable: false),
                    SubCategoryId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DishCategories", x => new { x.DishId, x.SubCategoryId });
                    table.ForeignKey(
                        name: "FK_DishCategories_MenuItems_DishId",
                        column: x => x.DishId,
                        principalTable: "MenuItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DishCategories_SubCategories_SubCategoryId",
                        column: x => x.SubCategoryId,
                        principalTable: "SubCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pauses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PauseStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PauseEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pauses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pauses_Times_TimeId",
                        column: x => x.TimeId,
                        principalTable: "Times",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OrderedIngredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IngredientNameFr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IngredientNameEng = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IngredientNameEs = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IngredientNameNl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IngredientType = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<double>(type: "float", nullable: false),
                    Weight = table.Column<double>(type: "float", nullable: false),
                    OrderedItemId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderedIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderedIngredients_OrderedItems_OrderedItemId",
                        column: x => x.OrderedItemId,
                        principalTable: "OrderedItems",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlternativeIngredients_MainIngredientId",
                table: "AlternativeIngredients",
                column: "MainIngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_ApplicationUserId",
                table: "Coupons",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CraftedComponentIngridient_IngredientId",
                table: "CraftedComponentIngridient",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_CraftedComponents_RestaurantUserId",
                table: "CraftedComponents",
                column: "RestaurantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DietaryTypes_DishId",
                table: "DietaryTypes",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_DiscountDish_DishId",
                table: "DiscountDish",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_DishCategories_SubCategoryId",
                table: "DishCategories",
                column: "SubCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_DishComponents_DishId",
                table: "DishComponents",
                column: "DishId");

            migrationBuilder.CreateIndex(
                name: "IX_DishComponents_IngredientId",
                table: "DishComponents",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_FcmTokens_AccountId",
                table: "FcmTokens",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Friends_ApplicationUserId1",
                table: "Friends",
                column: "ApplicationUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Ingredients_CraftedComponentId",
                table: "Ingredients",
                column: "CraftedComponentId");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_RestaurantId",
                table: "MenuItems",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderedIngredients_OrderedItemId",
                table: "OrderedIngredients",
                column: "OrderedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderedItems_ItemСharacteristicsId",
                table: "OrderedItems",
                column: "ItemСharacteristicsId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderedItems_OrderId",
                table: "OrderedItems",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_ApplicationUserId",
                table: "Orders",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RestaurantId",
                table: "Orders",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Pauses_TimeId",
                table: "Pauses",
                column: "TimeId");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantRecievers_FcmTokensFcmToken",
                table: "RestaurantRecievers",
                column: "FcmTokensFcmToken");

            migrationBuilder.CreateIndex(
                name: "IX_RestaurantRecievers_RestaurantId",
                table: "RestaurantRecievers",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_RestaurantUserId",
                table: "Restaurants",
                column: "RestaurantUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubCategories_RestaurantId",
                table: "SubCategories",
                column: "RestaurantId");

            migrationBuilder.CreateIndex(
                name: "IX_Times_RestaurantId",
                table: "Times",
                column: "RestaurantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlternativeIngredients");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "bulkModels");

            migrationBuilder.DropTable(
                name: "Coupons");

            migrationBuilder.DropTable(
                name: "CraftedComponentIngridient");

            migrationBuilder.DropTable(
                name: "DietaryTypes");

            migrationBuilder.DropTable(
                name: "DiscountDish");

            migrationBuilder.DropTable(
                name: "DishCategories");

            migrationBuilder.DropTable(
                name: "DishComponents");

            migrationBuilder.DropTable(
                name: "Friends");

            migrationBuilder.DropTable(
                name: "OrderedIngredients");

            migrationBuilder.DropTable(
                name: "Pauses");

            migrationBuilder.DropTable(
                name: "RestaurantRecievers");

            migrationBuilder.DropTable(
                name: "SpModels");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Discounts");

            migrationBuilder.DropTable(
                name: "SubCategories");

            migrationBuilder.DropTable(
                name: "Ingredients");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "OrderedItems");

            migrationBuilder.DropTable(
                name: "Times");

            migrationBuilder.DropTable(
                name: "FcmTokens");

            migrationBuilder.DropTable(
                name: "CraftedComponents");

            migrationBuilder.DropTable(
                name: "ItemСharacteristics");

            migrationBuilder.DropTable(
                name: "Orders");

            migrationBuilder.DropTable(
                name: "Restaurants");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
