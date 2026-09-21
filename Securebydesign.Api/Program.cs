using Securebydesign.Api.Middleware;
using Securebydesign.Application;
using Securebydesign.Application.DTOs.Auth;
using Securebydesign.Application.Interfaces;
using Securebydesign.Application.Interfaces.EmailServices;
using Securebydesign.Application.Interfaces.EmailTemplate;
using Securebydesign.Application.Interfaces.Repositories;
using Securebydesign.Application.Jobs;
using Securebydesign.Application.Services;
using Securebydesign.Infrastructure;
using Securebydesign.Infrastructure.EmailServices;
using Securebydesign.Infrastructure.EmailTemplates;
using Securebydesign.Infrastructure.External;
using Securebydesign.Infrastructure.External.OpenAI;
using Securebydesign.Infrastructure.Repositories;
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
using System.Text;
using Securebydesign.Api.Filters;

namespace Securebydesign.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var logger = LogManager.Setup().LoadConfigurationFromFile("nlog.config").GetCurrentClassLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                // Bind settings
                builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
                builder.Services.Configure<TokenSettings>(builder.Configuration.GetSection("TokenSettings"));

                // JWT Authentication
                var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
                var key = Encoding.UTF8.GetBytes(jwtSettings!.Key);

                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; // set true in production
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = jwtSettings.Audience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });

                builder.Services.AddAuthorization();

                // ADD CORS POLICY
                builder.Services.AddCors(options =>
                {
                    options.AddPolicy("AllowLandingPage",
                        policy => policy.WithOrigins("https://ashuam.com", "https://www.ashuam.com", "http://127.0.0.1:5173", "http://105.115.5.71:5173")
                                        .AllowAnyMethod()
                                        .AllowAnyHeader());
                });


                // Configure Forwarded Headers to trust Nginx proxy
                builder.Services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                    // Clear default loopback networks so it trusts your Nginx proxy setup
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();
                });

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

                // Hub abstraction — bridges Application layer to SignalR without circular dependency
                //builder.Services.AddScoped<IMessagingHub, MessagingHubService>();

                builder.Services.AddHangfire(config => config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UsePostgreSqlStorage(options =>
                        options.UseNpgsqlConnection(
                        builder.Configuration.GetConnectionString("DefaultConnection"))));

                builder.Services.AddHangfireServer(options =>
                {
                    options.WorkerCount = 2;   // keep low — this is a background job server, not a web worker
                });

                builder.Services.AddSingleton<Kernel>(sp =>
                {
                    return SemanticKernelFactory.CreateKernel(builder.Configuration);
                });

                builder.Services.AddControllers();

                builder.Services.AddEndpointsApiExplorer();

                builder.Services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo
                    {
                        Title = "Securebydesign API",
                        Version = "v1"
                    });

                    // Add the Authorize button and Bearer input to Swagger UI
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

                // Middleware
                // -----------------------------
                app.UseMiddleware<ExceptionMiddleware>();

                // ENABLE CORS (Must be placed after UseRouting but before UseAuthorization/Endpoints)
                app.UseCors("AllowLandingPage");

                //if (app.Environment.IsDevelopment())
                //{
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ashuam API v1");
                });
                //}

                app.UseStaticFiles(); // Serves files from wwwroot by default

                // Serves files from external physical paths like /uploads
                app.UseStaticFiles(new StaticFileOptions
                {
                    FileProvider = new PhysicalFileProvider("/uploads"),
                    RequestPath = "/uploads"
                });

                // Make sure these are in the right order in the pipeline
                app.UseRouting();
                app.UseAuthentication();
                app.UseAuthorization();
                app.UseHangfireDashboard("/hangfire", new DashboardOptions
                {
#if DEBUG
                    Authorization = new[]
                {
                    new Hangfire.Dashboard.LocalRequestsOnlyAuthorizationFilter()
                }
#else
                    Authorization = new[] { new HangfireAuthFilter() }
#endif
                });

                // ── SCHEDULE RECURRING JOB ──
                // Cron: 9AM, 12PM, 8PM — WAT is UTC+1, so adjust accordingly
                // "0 8,11,19 * * *" = 9AM, 12PM, 8PM WAT (UTC+1)
                using (var scope = app.Services.CreateScope())
                {
                    var recurringJobManager = scope.ServiceProvider
                        .GetRequiredService<IRecurringJobManager>();

                    recurringJobManager.AddOrUpdate<EmailBlast>(
                        recurringJobId: "feed-notification-job",
                        methodCall: job => job.ExecuteAsync(),
                        cronExpression: "0 8,11,19 * * *",
                        options: new RecurringJobOptions
                        {
                            TimeZone = TimeZoneInfo.Utc
                        }
                    );
                }

                app.MapControllers();

                app.MapControllers();

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