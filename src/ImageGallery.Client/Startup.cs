using IdentityModel;
using ImageGallery.Client.HttpHandler;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System;
using System.IdentityModel.Tokens.Jwt;
using static IdentityModel.JwtClaimTypes;

namespace ImageGallery.Client
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            //removing default claim type
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews()
                 .AddJsonOptions(opts => opts.JsonSerializerOptions.PropertyNamingPolicy = null);

            services.AddAuthorization(options =>
            {
                options.AddPolicy("MidePolicy", c =>
                 {
                     c.RequireAuthenticatedUser();
                     c.RequireClaim("country", "be");
                     c.RequireClaim("subscription", "Payee");
                 });
            });

            services.AddHttpContextAccessor();
            services.AddTransient<BearerTokenHandler>();
            // create an HttpClient used for accessing the API and add the handler use to handle Bearer token
            services.AddHttpClient("APIClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44366/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            }).AddHttpMessageHandler<BearerTokenHandler>();

            services.AddHttpClient("IDPClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44397/");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
                 .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                 {
                     options.AccessDeniedPath = "/Authorization/AccessDenied";
                 })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
             {
                 options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                 options.Authority = "https://localhost:44397/";
                 options.ClientId = "csdclientgallery";
                 options.ResponseType = "code";
                 // options.UsePkce = false;
                 options.ClaimActions.Remove("address");
                 options.ClaimActions.Remove("nbf");
                 options.ClaimActions.DeleteClaim("idp");
                 options.ClaimActions.MapUniqueJsonKey("role", "role");
                 options.ClaimActions.MapUniqueJsonKey("country", "country");
                 options.ClaimActions.MapUniqueJsonKey("subscription", "subscription");
                 //this scope are added by default by d middleware
                 //options.Scope.Add("openid");
                 //options.Scope.Add("profile");
                 options.Scope.Add("address");
                 options.Scope.Add("roles");
                 options.Scope.Add("country");
                 options.Scope.Add("subscription");
                 options.Scope.Add("mideimageapis");
                 options.Scope.Add("offline_access");
                 options.SaveTokens = true;
                 options.ClientSecret = "secret";
                 options.GetClaimsFromUserInfoEndpoint = true;
                 //telling the framework wher to find the roles
                 options.TokenValidationParameters = new TokenValidationParameters
                 {
                     NameClaimType = JwtClaimTypes.Name,
                     RoleClaimType = JwtClaimTypes.Role
                 };
             });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseStaticFiles();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Shared/Error");
                // The default HSTS value is 30 days. You may want to change this for
                // production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Gallery}/{action=Index}/{id?}");
            });
        }
    }
}