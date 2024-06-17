using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ruddy.WEB.Models;

public class SwaggerBasicAuthMiddleware
{
    private readonly IConfiguration _configuration;
    private readonly UserManager<Ruddy.WEB.Models.Account> _userManager;
    private readonly SignInManager<Ruddy.WEB.Models.Account> _signInManager;
    private readonly RequestDelegate next;
    public SwaggerBasicAuthMiddleware(IApplicationBuilder app, IConfiguration configuration, RequestDelegate next)
    {
        var scope = app.ApplicationServices.CreateScope();
        var userManager = (UserManager<Ruddy.WEB.Models.Account>)scope.ServiceProvider.GetService(typeof(UserManager<Ruddy.WEB.Models.Account>));
        var signInManager = (SignInManager<Ruddy.WEB.Models.Account>)scope.ServiceProvider.GetService(typeof(SignInManager<Ruddy.WEB.Models.Account>));
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        this.next = next;
    }
    private DateTime dt { get; set; }
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            string authHeader = context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic"))
            {
                var header = AuthenticationHeaderValue.Parse(authHeader);
                var inBytes = Convert.FromBase64String(header.Parameter);
                var credentials = Encoding.UTF8.GetString(inBytes).Split(':');
                var username = credentials[0];
                var password = credentials[1];
                if (username.Equals(_configuration["secureSwagger:user"])
                  && password.Equals(_configuration["secureSwagger:key"]))
                {
                    await next.Invoke(context).ConfigureAwait(false);
                }
                else
                {
                    context.Response.Headers["WWW-Authenticate"] = "Basic";
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                }
            }
            else
            {
                context.Response.Headers["WWW-Authenticate"] = "Basic";
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }
        else
        {
            await next.Invoke(context).ConfigureAwait(false);
        }
    }

    private string createJwtToken(string username)
    {
        var claims = new List<Claim>
            {

                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("Email", username),
                new Claim(JwtRegisteredClaimNames.UniqueName, username)
            };
        claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = DateTime.Now.AddDays(Convert.ToDouble(_configuration["Jwt:ExpireDays"]));

        var token = new JwtSecurityToken(
            _configuration["Jwt:Issuer"],
            _configuration["Jwt:Issuer"],
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler()).WriteToken(token);
    }
}