using System;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;
using AutoMapper;
using Domain;
using Domain.Common;

namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // ========== QUERIES (Read) ==========
        // Entity → DTO: Ignore audit fields that don't exist in DTO
        CreateMap<SerhanKitap, GetSerhanKitapDto>()
            .IgnoreBaseEntityAuditFields();

        // ========== COMMANDS (Write) ==========
        // DTO → Entity: Ignore all BaseEntity properties (set by handlers, not from DTOs)
        CreateMap<CreateSerhanKitapDto, SerhanKitap>()
            .IgnoreAllBaseEntityProperties();

        CreateMap<EditSerhanKitapDto, SerhanKitap>()
            .IgnoreAllBaseEntityProperties();
    }
}


/// <summary>
/// Extension methods for AutoMapper Profile to reduce repetition when ignoring BaseEntity properties
/// </summary>
public static class AutoMapperExtensions
{
    /// <summary>
    /// Ignores all BaseEntity audit properties in a source member (for Entity→DTO mappings)
    /// </summary>
    public static IMappingExpression<TSource, TDestination> IgnoreBaseEntityAuditFields<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> mappingExpression)
        where TSource : BaseEntity
    {
        return mappingExpression
            .ForSourceMember(src => src.CreatedDate, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.CreatedBy, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.UpdatedDate, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.UpdatedBy, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.IsDeleted, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.IsActive, opt => opt.DoNotValidate());
    }

    /// <summary>
    /// Ignores all BaseEntity properties (including Id) in a destination member (for DTO→Entity mappings)
    /// </summary>
    public static IMappingExpression<TSource, TDestination> IgnoreAllBaseEntityProperties<TSource, TDestination>(
        this IMappingExpression<TSource, TDestination> mappingExpression)
        where TDestination : BaseEntity
    {
        return mappingExpression
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedDate, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());
    }
}

