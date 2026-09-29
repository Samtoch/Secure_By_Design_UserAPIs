using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.SemanticKernel;
using NLog;
using NLog.Web;
using Securebydesign.Api.Filters;
using Securebydesign.Api.Middleware;
using Securebydesign.Application;
using Securebydesign.Application.DTOs.Auth;
using Securebydesign.Application.Interfaces;
using Securebydesign.Application.Interfaces.AuthServices;
using Securebydesign.Application.Interfaces.EmailServices;
using Securebydesign.Application.Interfaces.EmailTemplate;
using Securebydesign.Application.Interfaces.Repositories;
using Securebydesign.Application.Jobs;
using Securebydesign.Application.Services;
using Securebydesign.Application.Services.AuthServices;
using Securebydesign.Infrastructure;
using Securebydesign.Infrastructure.EmailServices;
using Securebydesign.Infrastructure.EmailTemplates;
using Securebydesign.Infrastructure.External;
using Securebydesign.Infrastructure.External.OpenAI;
using Securebydesign.Infrastructure.Repositories;
using System.Net;
using System.Text;
using Securebydesign.Api.Extensions;

namespace Securebydesign.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            DotNetEnv.Env.TraversePath().NoClobber().Load();
            var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Bind JWT and Token settings from configuration
                builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
                builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

                // ── AUTHENTICATION: who are you? ──
                var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>()
                    ?? throw new InvalidOperationException("The JwtSettings section is missing.");
                var signingKey = JwtTokenValidation.CreateSigningKey(jwtSettings.Key);   // fails fast on a weak key

                builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(options =>
                    {
                        options.MapInboundClaims = false;   // keep "sub" and "role" exactly as issued
                        options.TokenValidationParameters = JwtTokenValidation.CreateParameters(jwtSettings, signingKey);
                    });

                // ── AUTHORISATION: what are you allowed to do? ──
                builder.Services.AddSecureByDesignAuthorization();
                builder.Services.AddSignupRateLimiting();

                // ── CORS: allowed origins come from configuration, per environment ──
                // appsettings.Production.json should list only the HTTPS production origins.
                string[] allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowLandingPage",
                        policy => policy.WithOrigins(allowedOrigins)
                                        .AllowAnyMethod()
                                        .AllowAnyHeader());
                });

                // Argon2id cost settings from appsettings.json (defaults apply if the section is missing)
                builder.Services.Configure<Argon2idOptions>(builder.Configuration.GetSection("PasswordHashing:Argon2id"));

                // Register every algorithm under its own key
                builder.Services.AddKeyedSingleton<IPasswordHasher, Argon2idPasswordHasher>("Argon2id");
                builder.Services.AddKeyedSingleton<IPasswordHasher, Sha256PasswordHasher>("SHA256");

                // The plain IPasswordHasher is whichever algorithm the config names.
                string algorithm = builder.Configuration["PasswordHashing:Algorithm"] ?? "Argon2id";
                builder.Services.AddSingleton<IPasswordHasher>(sp => sp.GetRequiredKeyedService<IPasswordHasher>(algorithm));

                // ── FORWARDED HEADERS: trust ONLY the Nginx proxy ──
                // Clearing KnownProxies/KnownNetworks let any client fake its IP with X-Forwarded-For,
                // which would bypass per-IP rate limiting. Loopback stays trusted by default;
                // add Nginx's address here if it runs on another host or container.
                builder.Services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    foreach (string proxy in builder.Configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [])
                        options.KnownProxies.Add(IPAddress.Parse(proxy));
                });

                builder.Services.AddHttpContextAccessor();

                builder.Logging.ClearProviders();
                builder.Host.UseNLog();

                builder.Services.AddApplication(builder.Configuration);
                builder.Services.AddInfrastructure(builder.Configuration);

                builder.Services.AddScoped<IJwtService, JwtService>();
                builder.Services.AddScoped<IEmailTemplate, EmailTemplate>();
                builder.Services.AddScoped<IEmailService, EmailService>();
                builder.Services.AddScoped<IHttpRequests, HttpRequests>();
                builder.Services.AddScoped<IUserRepository, UserRepository>();
                builder.Services.AddScoped<IAuthRepository, AuthRepository>();

                builder.Services.AddScoped<EmailBlast>();

                builder.Services.AddHangfire(config => config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(options =>
                        options.UseNpgsqlConnection(
                        builder.Configuration.GetConnectionString("DefaultConnection"))));

                builder.Services.AddHangfireServer(options =>
                {
                    options.WorkerCount = 2;
                });

                builder.Services.AddSingleton<Kernel>(sp => SemanticKernelFactory.CreateKernel(builder.Configuration));

                builder.Services.AddControllers();
                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Securebydesign API",
                        Version = "v1"
                    });

                    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Name = "Authorization",
                        Type = SecuritySchemeType.Http,
                        Scheme = "Bearer",
                        BearerFormat = "JWT",
                        In = ParameterLocation.Header,
                        Description = "Enter your JWT token below. Example: eyJhbGci..."
                    });

                    options.AddSecurityRequirement(new OpenApiSecurityRequirement
                    {
                        {
                            new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id   = "Bearer"
                                }
                            },
                            Array.Empty<string>()
                        }
                    });
                });

                builder.Services.AddSignalR();

                var app = builder.Build();

                app.UseForwardedHeaders();
                app.UseMiddleware<ExceptionMiddleware>();
                app.UseCors("AllowLandingPage");

                // Swagger publishes a full map of your API: development only
                if (app.Environment.IsDevelopment())
                {
                    app.UseSwagger();
                    app.UseSwaggerUI(c =>
                    {
                        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Securebydesign API v1");
                    });
                }

                app.UseStaticFiles();

                // NOTE: files here are public to anyone who knows the URL (no auth check).
                // Don't store identity documents or other private uploads in this folder.
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider("/uploads"),
                    RequestPath = "/uploads"
                });

                app.UseRouting();
                app.UseAuthentication();   // 1. who are you?
                app.UseAuthorization();    // 2. what can you do?
                app.UseRateLimiter();      // 3. how often?

                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
#if DEBUG
                    Authorization = new[] { new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter() }
#else
                    Authorization = new[] { new HangfireAuthFilter() }   // make sure this requires the Admin role
#endif
                });

                using (var scope = app.Services.CreateScope())
                {
                    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

                    recurringJobManager.AddOrUpdate<EmailBlast>(
                        recurringJobId: "feed-notification-job",
                        methodCall: job => job.ExecuteAsync(),
                        cronExpression: "0 8,11,19 * * *",
                        options: new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
                }

                app.MapControllers();   // was mapped twice

                app.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Application failed to start");
                throw;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }
    }
}