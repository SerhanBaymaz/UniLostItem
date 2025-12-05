using System;
using Application.SerhanKitaplar.DTOs;
using AutoMapper;
using Domain;

namespace Application.Core;

public class MappingProfiles : Profile
{
    public MappingProfiles()
    {
        CreateMap<SerhanKitap, SerhanKitap>();
        CreateMap<CreateSerhanKitapDto, SerhanKitap>();
    }
}
