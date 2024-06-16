using Application.DTOs;
using AutoMapper;
using Infrastructure.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Execution, ExecutionResultDto>();
    }
}