using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using AutoMapper;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Text;
using Microsoft.AspNetCore.SignalR;
using Ruddy.WEB.Interfaces;
using Ruddy.WEB.Utils;
using Ruddy.WEB.Services;
using Microsoft.OpenApi.Models;
using Ruddy.WEB.Options;
using Stripe;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using System.IO;
using Ruddy.WEB.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ruddy.WEB.DataAccess;

namespace Ruddy.WEB
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            // var defaultApp = FirebaseApp.Create(new AppOptions()
            // {
            //     Credential = GoogleCredential.FromFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "key.json")),
            // });
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mapperConfig = new MapperConfiguration(mc =>
            {
                mc.AddProfile(new MappingProfile());
            });

            services.AddHostedService<DiscountsHostedService>();
            //services.AddSingleton<IHostedService, DiscountsHostedService>();
            services.AddScoped<INotificationService, NotificationService>();
            IMapper mapper = mapperConfig.CreateMapper();
            services.AddSingleton(mapper);
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("AwsRdsConnectionString")));
            services.AddIdentity<Models.Account, IdentityRole>(options => options.SignIn.RequireConfirmedEmail = false)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            services.AddTransient<Ruddy.WEB.Interface.IRestaurantExcelFileToObjectWriter, Ruddy.WEB.Utils.RestaurantExcelFileToObjectWriter>();
            services.AddTransient<Ruddy.WEB.Handler.IRestaurantFileHandler, Ruddy.WEB.Handler.RestaurantFileHandler>();
            services.AddSignalR(o =>
            {
                o.EnableDetailedErrors = true;
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
                .AddCookie(cfg => { cfg.SlidingExpiration = true; })
                .AddJwtBearer(cfg =>
                {
                    cfg.RequireHttpsMetadata = false;
                    cfg.SaveToken = true;
                    cfg.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidIssuer = Configuration["Jwt:Issuer"],
                        ValidAudience = Configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["Jwt:Key"])),
                        ClockSkew = TimeSpan.Zero,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = false,
                        ValidateIssuerSigningKey = true
                    };
                    cfg.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];

                            // If the request is for our hub...
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) &&
                                (path.StartsWithSegments("/orderhub")))
                            {
                                // Read the token out of the query string
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            services.AddAuthentication();
            services.AddTransient<IEmailSender, EmailSender>();
            services.AddSingleton<IHasher, Hasher>();

            services.AddSingleton<IMediaStorageService, MediaStorageService>(cnf => new MediaStorageService(
                Configuration.GetSection("AWS_S3_config")["awsAccessKeyId"],
                Configuration.GetSection("AWS_S3_config")["awsSecretAccessKey"],
                Configuration.GetSection("AWS_S3_config")["bucketName"]
            ));

            services.AddControllersWithViews()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
        options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
    });
            services.AddSwaggerGen();
            // services.AddSwaggerGen(c =>
            // {
            //     c.SwaggerDoc("v1", new OpenApiInfo { Title = "List of Methods", Version = "1" });
            //     c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //     {
            //         Description = "JWT Authorization header using the Bearer scheme.",
            //         Name = "Authorization",
            //         In = ParameterLocation.Header,
            //         Scheme = "bearer",
            //         Type = SecuritySchemeType.Http,
            //         BearerFormat = "JWT"
            //     });
            //     c.AddSecurityRequirement(new OpenApiSecurityRequirement
            //         {
            //             {
            //                 new OpenApiSecurityScheme
            //                 {
            //                     Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            //                 },
            //                 new List<string>()
            //             }
            //         });
            // });
            services.AddRazorPages();
            services.Configure<StripeOptions>(Configuration.GetSection("Stripe"));
            services.AddCors(
                 options => options.AddPolicy("AllowCors",
                     builder =>
                     {
                         builder
                             .AllowAnyOrigin()
                             .AllowAnyHeader()
                             .AllowAnyMethod();
                     })
             );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            StripeConfiguration.ApiKey = Configuration.GetSection("Stripe")["SecretKey"];
            if (env.IsDevelopment())
            {
                //app.UseDeveloperExceptionPage();
                //app.UseDatabaseErrorPage();
            }
            else
            {
                //app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                //app.UseHsts();
            }
            //app.UseCors("AllowCors");
            app.UseCors(options =>
                options.AllowAnyMethod()
                .AllowAnyOrigin()
                .AllowAnyHeader()
                );
            app.UseMiddleware<SwaggerBasicAuthMiddleware>(app);
            app.UseSwagger();

            app.UseDeveloperExceptionPage();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseHttpsRedirection();
            app.UseStaticFiles();


            app.UseRouting();
            app.UseSwaggerUI(c =>
                       {
                           c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
                           c.RoutePrefix = string.Empty;
                       });
            // app.UseCors("AllowCors");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
                endpoints.MapHub<OrdersHub>("/orderhub");
            });
        }
    }
}
