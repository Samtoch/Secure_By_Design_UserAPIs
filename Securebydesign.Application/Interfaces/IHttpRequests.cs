using Securebydesign.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Interfaces
{
    public interface IHttpRequests
    {
        Task<string> GetAsync(string url, AuthOptions? auth = null);
        Task<string> PostAsync(string baseUrl, string endpointUrl, object body, AuthOptions? auth = null);
        Task<string> PutAsync(string baseUrl, string endpointUrl, object body, AuthOptions? auth = null);
    }
}
