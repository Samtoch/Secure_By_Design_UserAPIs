using Securebydesign.Application.DTOs;
using Securebydesign.Application.Interfaces;
using Securebydesign.Application.Interfaces.AuthServices;
using Securebydesign.Application.Mappings;
using Securebydesign.Application.Services;
using Securebydesign.Domain.Entities;
using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Securebydesign.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IAuthService, AuthService>();

            services.AddScoped<IUserService, UserService>();

            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<DocumentSecretProfile>();
            });

            //var barDto = mapper.Map<BarDto>(bar);

            // services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            return services;
        }
    }
}
