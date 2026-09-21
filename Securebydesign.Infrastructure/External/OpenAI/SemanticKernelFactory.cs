using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Infrastructure.External.OpenAI
{
    public static class SemanticKernelFactory
    {
        public static Kernel CreateKernel(IConfiguration config)
        {
            var builder = Kernel.CreateBuilder();

            var openAIApiKey = config["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API Key is required");

            builder.AddOpenAIChatCompletion(
                modelId: config["OpenAI:ModelId"] ?? "gpt-4",
                apiKey: openAIApiKey);

            builder.AddOpenAITextEmbeddingGeneration(
                modelId: config["OpenAI:EmbeddingModelId"] ?? "text-embedding-3-small",
                apiKey: openAIApiKey);

            return builder.Build();
        }
    }
}
