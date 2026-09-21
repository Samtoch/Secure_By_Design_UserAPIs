using Securebydesign.Application.Interfaces;
using Securebydesign.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace Securebydesign.Infrastructure.External
{
    public class HttpRequests : IHttpRequests
    {
        private readonly ILogger<HttpRequests> _logger;

        public HttpRequests(ILogger<HttpRequests> logger)
        {
            _logger = logger;
        }

        private void ApplyAuthorization(HttpClient client, AuthOptions? auth)
        {
            if (auth == null || auth.AuthType == AuthType.None)
                return;

            switch (auth.AuthType)
            {
                case AuthType.Basic:
                    if (string.IsNullOrEmpty(auth.KeyOrName) || string.IsNullOrEmpty(auth.Value))
                        throw new ArgumentException("Basic auth requires KeyOrName and Value");

                    var credentials = $"{auth.KeyOrName}:{auth.Value}";
                    var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64);
                    break;

                case AuthType.Bearer:
                    if (string.IsNullOrEmpty(auth.Value))
                        throw new ArgumentException("Bearer auth requires Value");

                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.Value);
                    break;

                case AuthType.ApiKey:
                    if (string.IsNullOrEmpty(auth.KeyOrName) || string.IsNullOrEmpty(auth.Value))
                        throw new ArgumentException("ApiKey auth requires KeyOrName (header name) and Value (key)");

                    client.DefaultRequestHeaders.Add(auth.KeyOrName, auth.Value);
                    break;

                default:
                    throw new ArgumentException($"Unsupported AuthType: {auth.AuthType}");
            }
        }

        public async Task<string> PostAsync(string baseUrl, string endpointUrl, object body, AuthOptions? auth = null)
        {
            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/")
                };

                ApplyAuthorization(client, auth);

                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync(endpointUrl.TrimStart('/'), content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning($"Error Status: {response.StatusCode}, Content: {responseContent}");

                return responseContent;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PostAsync: {ex}");
                throw;
            }
        }

        public async Task<string> PutAsync(string baseUrl, string endpointUrl, object body, AuthOptions? auth = null)
        {
            try
            {
                using var client = new HttpClient
                {
                    BaseAddress = new Uri(baseUrl.EndsWith("/") ? baseUrl : baseUrl + "/")
                };

                ApplyAuthorization(client, auth);

                var json = JsonConvert.SerializeObject(body);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync(endpointUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning($"Error Status: {response.StatusCode}, Content: {responseContent}");

                return responseContent;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in PutAsync: {ex}");
                throw;
            }
        }

        public async Task<string> GetAsync(string url, AuthOptions? auth = null)
        {
            try
            {
                using var client = new HttpClient();

                ApplyAuthorization(client, auth);

                var response = await client.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    _logger.LogWarning($"Error Status: {response.StatusCode}, Content: {content}");

                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in GetAsync: {ex}");
                throw;
            }
        }
    }
}
