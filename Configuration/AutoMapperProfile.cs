using AutoMapper;
using CarBook.Models;
using Microsoft.AspNetCore.Identity;

namespace CarBook.Configuration
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {

            CreateMap<IdentityUser,ApplicationUser>();

        }
    }
}
