using AutoMapper;
using Core.Dtos;
using Core.Entites;

namespace InventoryService.Extintions;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<CreateProductDto, Product>();
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => src.Category));
        
        CreateMap<Brand, BrandDto>(); // Mapping configuration for Brand
        CreateMap<Category, CategoryDto>(); // Mapping configuration for Category
        
        CreateMap<BrandDto, Brand>(); // Mapping configuration for Brand
        CreateMap<CategoryDto, Category>(); // Mapping configuration for Category
        
        
    }
}