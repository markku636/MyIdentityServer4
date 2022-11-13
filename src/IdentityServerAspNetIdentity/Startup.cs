// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4;
using IdentityServer4.Services;
using IdentityServerAspNetIdentity.Data;
using IdentityServerAspNetIdentity.Models;
using IdentityServerHost.Quickstart.UI;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

            services.AddTransient<IProfileService, ProfileService>();

            services.AddSingleton<ICorsPolicyService>((container) => {
                var logger = container.GetRequiredService<ILogger<DefaultCorsPolicyService>>();
                return new DefaultCorsPolicyService(logger) {
                    AllowAll = true
                };
            });

            var builder = services.AddIdentityServer(options => {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://identityserver4.readthedocs.io/en/latest/topics/resources.html
                options.EmitStaticAudienceClaim = true;
            }).AddTestUsers(TestUsers.Users)
                .AddInMemoryIdentityResources(Config.IdentityResources)
                .AddInMemoryApiScopes(Config.ApiScopes)
                .AddInMemoryClients(Config.Clients)

                .AddAspNetIdentity<ApplicationUser>()
                .AddProfileService<ProfileService>(); // identityServer4要携带自定义的Claim，仅仅传递Claim是不行的 还需要实现IProfileService方法才行

            // not recommended for production - you need to store your key material somewhere secure
            builder.AddDeveloperSigningCredential();

            services.AddAuthentication(options =>
            {
               //options.DefaultScheme = "Cookies";
               //options.DefaultChallengeScheme = "oidc";

            })
           //.AddOpenIdConnect("oidc", options => {
           //   options.Authority = "https://localhost:5001";
           //   options.TokenValidationParameters.RequireAudience = true;
           //   options.RequireHttpsMetadata = false;
           //   options.SignInScheme = "Cookies";
           //   options.ClientId = "nextjs";
           //   options.ClientSecret = "secret";
           //   options.ResponseType = "code";
           //   //options.RequireHttpsMetadata = true;
           //   options.SaveTokens = true;
           //   options.GetClaimsFromUserInfoEndpoint = true;
           //   options.Scope.Add("openid");
           //   options.Scope.Add("profile");
           //   options.Scope.Add("api1");

           //   options.ClaimActions.MapJsonKey("externalId", "id");
           //   options.ClaimActions.MapJsonKey("email", "email");

           //   options.Events = new OpenIdConnectEvents {
           //      OnRedirectToIdentityProvider = context =>
           //      {
           //         context.ProtocolMessage.SetParameter("myContext",
           //         context.Request.Query["myContext"]);

           //         return Task.FromResult(0);
           //      }
           //   };


           //})


                .AddGoogle(options => {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ClaimActions.MapUniqueJsonKey("xType", "xType");

                    // register your IdentityServer with Google at https://console.developers.google.com
                    // enable the Google+ API
                    // set the redirect URI to https://localhost:5001/signin-google
                    options.ClientId = "439471082857-d8oeqq7dsbafc5c4bq3nmsh51p42jelv.apps.googleusercontent.com";
                    options.ClientSecret = "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";
                    options.Scope.Add("profile");
                    options.ClaimActions.MapJsonKey("picture", "picture", "url");
                    options.ClaimActions.MapJsonKey("externalId", "id");
                    options.ClaimActions.MapJsonKey("email", "email");

                    options.Events = new OAuthEvents {
                        OnCreatingTicket = context => {
                            var user = context.User;

                            return Task.CompletedTask;
                        }
                    };
                   options.SaveTokens = true;
                }).AddGitHub(options => {
                    options.ClaimActions.MapUniqueJsonKey("picture", "avatar_url");
                    options.ClaimActions.MapJsonKey("externalId", "id");

                   // github email default not public Keep my email addresses private disable
                   options.ClaimActions.MapJsonKey("email", "email");
                   options.Events = new OAuthEvents {
                        OnCreatingTicket = context => {
                            var picture = context.User;

                            return Task.CompletedTask;
                        }
                    };

                    options.ClientId = "Iv1.6744f15c68d729c9";
                    options.ClientSecret = "1f93721080cff1ebe5b3f74fd0d33f53182e1091";
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SaveTokens = true;
                    options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                    options.CorrelationCookie.IsEssential = true;
                }).AddDiscord("BDiscord", options => {
                    options.ClaimActions.MapJsonKey("externalId", "id");
                    options.ClaimActions.MapJsonKey("email", "email");
                   options.SaveTokens = true;
                   options.Events = new OAuthEvents {
                        OnCreatingTicket = context => {
                            var nameidentifier = context.User.GetProperty("id").GetString();
                            var hash = context.User.GetProperty("avatar").GetString();
                            var picture = $"https://cdn.discordapp.com/avatars/{nameidentifier}/{hash}.webp?size=128";
                            context.Identity.AddClaim(new Claim("picture", picture));

                            return Task.CompletedTask;
                        }
                    };

                    options.ClientId = "829495062073180162";
                    options.ClientSecret = "Gyjh4m2Aw2iVWk2GkfnTgTu-WaEfo6cG";
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SaveTokens = true;
                    options.Scope.Add("guilds.join");
                    options.Scope.Add("email");

                    options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
                    options.CorrelationCookie.IsEssential = true;
                }).AddFacebook(options => {
                    
                    options.Events = new OAuthEvents {
                        OnCreatingTicket = context => {
                            var token = context.TokenResponse;
                            var id = context.User.GetProperty("id").GetString();

                            var picture = $"https://graph.facebook.com/{id}/picture?height={50}&width={50}&access_token={token.AccessToken}";
                            //var picture = $"https://graph.facebook.com/me?fields=picture.type(large)&access_token={identity}";
                            context.Identity.AddClaim(new Claim("picture", picture));

                            return Task.CompletedTask;
                        }
                    };
                   options.ClaimActions.MapJsonKey("email", "email");
                    options.ClaimActions.MapJsonKey("externalId", "id");
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