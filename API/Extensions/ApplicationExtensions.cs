using Application.Core;
using Application.Features.LostItems.Commands.CreateLostItem;
using FluentValidation;
using MediatR;

namespace API.Extensions;

public static class ApplicationExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddLogging();

        services.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<MappingProfiles>();
            x.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddAutoMapper(_ => { }, typeof(MappingProfiles).Assembly);
        services.AddValidatorsFromAssemblyContaining<CreateLostItemCommandValidator>();
    }
}
