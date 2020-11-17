using AdvantageTool.Data;
using AdvantageTool.Models;
using AdvantageTool.Services.Rsa;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static IdentityServer4.IdentityServerConstants;

namespace AdvantageTool
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var rsa = new RsaKeyService(_env, TimeSpan.FromDays(30));
            services.AddSingleton(provider => rsa);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDatabaseDeveloperPageExceptionFilter();

            services.AddDefaultIdentity<IdentityUser>(options => options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+/ ")
                .AddEntityFrameworkStores<ApplicationDbContext>();
            services.AddControllersWithViews();

            var builder = services.AddIdentityServer()
                .AddInMemoryIdentityResources(IdentityServerConfig.IdentityResources)
                //.AddInMemoryApiResources(IdentityServerConfig.ApiScopes)
                .AddInMemoryClients(IdentityServerConfig.Clients)
                .AddSigningCredential(rsa.GetKey(), RsaSigningAlgorithm.RS256);

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = IdentityConstants.ApplicationScheme;
                    options.DefaultChallengeScheme = "oidc";
                })
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.LoginPath = new PathString("/Identity/Account/ExternalLogin");
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = IdentityConstants.ExternalScheme;
                    options.SaveTokens = true;
                    options.ClientId = Configuration["OpenIdConfig:ClientId"];
                    options.Authority = Configuration["OpenIdConfig:Issuer"];
                    // The Platform MUST send the id_token via the OAuth 2 Form Post
                    // See https://www.imsglobal.org/spec/security/v1p0/#successful-authentication
                    options.ResponseType = OpenIdConnectResponseType.IdToken;
                    // See http://openid.net/specs/oauth-v2-form-post-response-mode-1_0.html
                    options.ResponseMode = OpenIdConnectResponseMode.FormPost;
                    options.Prompt = OpenIdConnectPrompt.None;
                    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;
                    options.DisableTelemetry = true;

                    options.Configuration = new OpenIdConnectConfiguration
                    {
                        AuthorizationEndpoint = Configuration["OpenIdConfig:AuthorizationEndpoint"],
                        TokenEndpoint = Configuration["OpenIdConfig:TokenEndpoint"],
                        Issuer = Configuration["OpenIdConfig:Issuer"],
                        JwksUri = Configuration["OpenIdConfig:JwksUri"]
                    };

                    options.CallbackPath = new PathString("/LtiTool");

                    options.Scope.Clear();
                    options.Scope.Add(OpenIdConnectScope.OpenId);

                    options.Events = new OpenIdConnectEvents
                    {
                        //OnRemoteFailure = HandleOnRemoteFailure,
                        // Authenticate the request starting at step 5 in the OpenId Implicit Flow
                        // See https://www.imsglobal.org/spec/security/v1p0/#platform-originating-messages
                        // See https://openid.net/specs/openid-connect-core-1_0.html#ImplicitFlowSteps
                        OnRedirectToIdentityProvider = context =>
                        {
                            if (context.Properties.Items.TryGetValue(nameof(OidcModel.LoginHint), out var login_hint))
                                context.ProtocolMessage.LoginHint = login_hint;

                            if (context.Properties.Items.TryGetValue(nameof(OidcModel.LtiMessageHint), out var lti_message_hint))
                                context.ProtocolMessage.SetParameter("lti_message_hint", lti_message_hint);

                            if (context.Properties.Items.TryGetValue(nameof(OidcModel.ClientId), out var clientId))
                                context.ProtocolMessage.ClientId = clientId;

                            context.ProtocolMessage.Prompt = OpenIdConnectPrompt.None;

                            if (context.Properties.Items.Count < 4)
                            {
                                context.Response.Redirect("Home/Index");
                            }

                            return Task.CompletedTask;
                        },
                    };

                    // Using the options.TokenValidationParameters, validate four things:
                    //
                    // 1. The Issuer Identifier for the Platform MUST exactly match the value of the iss
                    //    (Issuer) Claim (therefore the Tool MUST previously have been made aware of this
                    //    identifier.
                    // 2. The Tool MUST Validate the signature of the ID Token according to JSON Web Signature
                    //    RFC 7515, Section 5; using the Public Key for the Platform which collected offline.
                    // 3. The Tool MUST validate that the aud (audience) Claim contains its client_id value
                    //    registered as an audience with the Issuer identified by the iss (Issuer) Claim. The
                    //    aud (audience) Claim MAY contain an array with more than one element. The Tool MUST
                    //    reject the ID Token if it does not list the client_id as a valid audience, or if it
                    //    contains additional audiences not trusted by the Tool.
                    // 4. The current time MUST be before the time represented by the exp Claim;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateTokenReplay = true,
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        RequireSignedTokens = true,
                        ValidateIssuerSigningKey = true,
                        ValidAudience = Configuration["OpenIdConfig:ClientId"],
                        ValidIssuer = Configuration["OpenIdConfig:Issuer"],
                        ValidateLifetime = true,
                        IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                        {
                            var keySetJson = new WebClient().DownloadString(Configuration["OpenIdConfig:JwksUri"]);
                            var keySet = JsonConvert.DeserializeObject<JsonWebKeySet>(keySetJson);
                            var key = keySet.Keys.SingleOrDefault(k => k.Kid == kid);

                            return new List<JsonWebKey> { key };
                        },
                        ClockSkew = TimeSpan.FromMinutes(5.0)
                    };
                });

            services.AddSingleton(Configuration.GetSection("OpenIdConfig").Get<OidcModel>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseMigrationsEndPoint();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseIdentityServer();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
