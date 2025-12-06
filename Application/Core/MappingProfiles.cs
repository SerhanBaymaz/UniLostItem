using System;
using Application.Features.SerhanKitaplar.Queries.Common.DTOs;
using Application.Features.SerhanKitaplar.Commands.CreateSerhanKitap;
using Application.Features.SerhanKitaplar.Commands.EditSerhanKitap;
using AutoMapper;
using Domain;

namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        // ========== QUERIES (Read) ==========
        // Entity → DTO
        CreateMap<SerhanKitap, GetSerhanKitapDto>();


        // ========== COMMANDS (Write) ==========
        // DTO → Entity (Id is ignored for creation and editing)
        CreateMap<CreateSerhanKitapDto, SerhanKitap>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<EditSerhanKitapDto, SerhanKitap>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
