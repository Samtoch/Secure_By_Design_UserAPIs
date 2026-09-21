using Securebydesign.Application.DTOs;
using Securebydesign.Application.DTOs.Users;
using Securebydesign.Domain.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace Securebydesign.Application.Mappings
{
    public class DocumentSecretProfile : Profile
    {
        public DocumentSecretProfile()
        {
            CreateMap<SignupResponse, User>();
            CreateMap<User, LoginResponse>();
        }
    }
}
