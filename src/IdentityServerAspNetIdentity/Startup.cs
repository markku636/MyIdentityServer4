﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IdentityServerAspNetIdentity
{
   public class Startup
   {
      public IWebHostEnvironment Environment { get; }
      public IConfiguration Configuration { get; }

      public Startup(IWebHostEnvironment environment, IConfiguration configuration) {
         Environment = environment;
         Configuration = configuration;
      }

      public void ConfigureServices(IServiceCollection services) {
         services.AddControllersWithViews();

         services.AddDbContext<ApplicationDbContext>(options =>
             options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));

         services.AddIdentity<ApplicationUser, IdentityRole>()
             .AddEntityFrameworkStores<ApplicationDbContext>()
             .AddDefaultTokenProviders();



         var builder = services.AddIdentityServer(options => {
            options.Events.RaiseErrorEvents = true;
            options.Events.RaiseInformationEvents = true;
            options.Events.RaiseFailureEvents = true;
            options.Events.RaiseSuccessEvents = true;

            // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
            options.EmitStaticAudienceClaim = true;
         })
             .AddInMemoryIdentityResources(Config.IdentityResources)
             .AddInMemoryApiScopes(Config.ApiScopes)
             .AddInMemoryClients(Config.Clients)
             .AddAspNetIdentity<ApplicationUser>().AddProfileService<ImplicitProfileService>(); // identityServer4要携带自定义的Claim，仅仅传递Claim是不行的 还需要实现IProfileService方法才行





         // not recommended for production - you need to store your key material somewhere secure
         builder.AddDeveloperSigningCredential();

         services.AddAuthentication()
             .AddGoogle(options => {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "439471082857-d8oeqq7dsbafc5c4bq3nmsh51p42jelv.apps.googleusercontent.com";
                options.ClientSecret = "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";
                ;
                options.ClaimActions.MapJsonKey("urn:google:picture", "picture", "url");

                options.ClaimActions.MapJsonKey("urn:google:locale", "locale", "string");
                options.ClaimActions.MapJsonKey("nation", "nation");

                options.Events = new OAuthEvents {
                   OnCreatingTicket = context => {
                      //var picture = context.User.ro
                      //context.User
                      var surname = "test";
                      context.Identity.AddClaim(new Claim(ClaimTypes.Surname, surname));

                      return Task.FromResult(0);
                   }
                };
             }).AddGitHub(options => {
                options.ClientId = "Iv1.6744f15c68d729c9";
                options.ClientSecret = "1f93721080cff1ebe5b3f74fd0d33f53182e1091";
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.SaveTokens = true;
                options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                options.CorrelationCookie.IsEssential = true;
             }).AddDiscord("BDiscord", options =>
             {
                options.ClientId = "829495062073180162";
                options.ClientSecret = "Gyjh4m2Aw2iVWk2GkfnTgTu-WaEfo6cG";
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;                
                options.SaveTokens = true;
                options.Scope.Add("guilds.join");
                options.Scope.Add("email");
                options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                options.CorrelationCookie.IsEssential = true;

             }).AddFacebook(options =>
             {
                options.AppId = "518075099514324";
                options.AppSecret = "88a1ff92458f159ad82297fdc401c3b9";
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.SaveTokens = true;
                options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                options.CorrelationCookie.IsEssential = true;
             });

         //https://localhost:5001/signin-github



      }

      public void Configure(IApplicationBuilder app) {
         if (Environment.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
         }

         app.UseStaticFiles();

         app.UseRouting();
         app.UseIdentityServer();
         app.UseAuthentication();

         app.UseAuthorization();
         app.UseEndpoints(endpoints => {
            endpoints.MapDefaultControllerRoute();
         });
      }
   }
}