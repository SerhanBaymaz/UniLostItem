using System;
using Application.Features.LostItems.Commands.CreateLostItem;
using Application.Features.ItemClaims.Commands.CreateItemClaim;
using AutoMapper;
using Domain;
using Domain.Common;

namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<CreateLostItemDto, LostItem>()
            .IgnoreAllBaseEntityProperties()
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.ItemType, opt => opt.MapFrom(src => src.ItemType))
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.ImageUrl, opt => opt.Ignore())
            .ForMember(dest => dest.ImagePublicId, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.Claims, opt => opt.Ignore())
            .ForSourceMember(src => src.ImageStream, opt => opt.DoNotValidate())
            .ForSourceMember(src => src.ImageFileName, opt => opt.DoNotValidate());

        CreateMap<CreateItemClaimDto, ItemClaim>()
            .IgnoreAllBaseEntityProperties()
            .ForMember(dest => dest.ClaimantId, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.ExpiresAt, opt => opt.Ignore())
            .ForMember(dest => dest.ExtensionCount, opt => opt.Ignore())
            .ForMember(dest => dest.LostItem, opt => opt.Ignore())
            .ForMember(dest => dest.Claimant, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerComment, opt => opt.Ignore())
            .ForMember(dest => dest.OwnerResponseDate, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ReviewedDate, opt => opt.Ignore())
            .ForMember(dest => dest.AdminComment, opt => opt.Ignore());
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

