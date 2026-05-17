using AutoMapper;
using GiftBoxy.Application.DTOs.Product;
using GiftBoxy.Domain.Entities;

namespace GiftBoxy.Application.Mappers
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<ProductCreateDto, Product>();

            CreateMap<ProductUpdateDto, Product>();

            //CreateMap<Product, ProductGetDto>()
            //    .ForMember(dest => dest.CategoryName,
            //        opt => opt.MapFrom(src => src.Category.Name));
        }
    }
}
