using Securebydesign.Application.Interfaces;
using Securebydesign.Application.Services;
using Securebydesign.Infrastructure.External.OpenAI;
using Securebydesign.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Securebydesign.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //services.AddScoped<IFeatureRepository, FeatureRepository>();
            //services.AddScoped<IDocumentRepository, DocumentRepository>();
            //services.AddScoped<IDocumentSecretRepository, DocumentSecretRepository>();

            //services.AddScoped<IConfluenceClient, ConfluenceClient>();
            //services.AddScoped<IDocumentProcessor, DocumentProcessor>();


            // -----------------------------
            // Semantic Kernel
            // -----------------------------
            services.AddSingleton<Kernel>(sp =>
            {
                return SemanticKernelFactory.CreateKernel(configuration);
            });


            return services;
        }
    }
}
