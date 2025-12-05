using System;
using Application.SerhanKitaplar.DTOs;
using Application.Core;
using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.SerhanKitaplar.Commands;

public class CreateSerhanKitap
{
    public class Command : IRequest<Result<string>>
    {
        public required CreateSerhanKitapDto SerhanKitapDto { get; set; }
    }

    public class Handler(AppDbContext context, IMapper mapper) : IRequestHandler<Command, Result<string>>
    {
        public async Task<Result<string>> Handle(Command request, CancellationToken cancellationToken)
        {
            var serhanKitap = mapper.Map<SerhanKitap>(request.SerhanKitapDto);

            context.SerhanKitaplar.Add(serhanKitap);

            var result = await context.SaveChangesAsync(cancellationToken) > 0;

            if (!result)
            {
                return Result<string>.Failure("Failed to create the serhan kitap", 400);
            }

            return Result<string>.Success("Serhan kitap created successfully", serhanKitap.Id);
        }
    }
}
