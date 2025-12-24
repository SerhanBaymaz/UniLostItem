using Application.Core;
using Application.Features.SerhanKitaplar.Validators;
using FluentValidation;
using MediatR;

namespace API.Extensions;

public static class ApplicationExtensions
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        services.AddMediatR(x =>
        {
            x.RegisterServicesFromAssemblyContaining<MappingProfiles>();
            x.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddAutoMapper(typeof(MappingProfiles).Assembly);
        services.AddValidatorsFromAssemblyContaining<CreateSerhanKitapCommandValidator>();
    }
}
