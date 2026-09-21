using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.DTOs.Generic
{
    public class ApiResponse<T>
    {
        public string Message { get; set; }
        public int StatusCode { get; set; }
        public bool Flag { get; set; }
        public T Data { get; set; }
    }
}
