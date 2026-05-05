# CRUD Code Templates — UniLostItem Patterns

Templates matching the actual codebase patterns from `LostItems` and `ItemClaims` features.

## Create Command Template

### DTO

```csharp
namespace Application.Features.{Feature}.Commands.Create{Entity};

public class Create{Entity}Dto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    public ItemCategory Category { get; set; }
    public string? OptionalField { get; set; }
}
```

### Command

```csharp
using Application.Core;
using MediatR;

namespace Application.Features.{Feature}.Commands.Create{Entity};

public class Create{Entity}Command : IRequest<Result<string>>
{
    public required Create{Entity}Dto Create{Entity}Dto { get; set; }
}
```

### Validator

```csharp
using FluentValidation;

namespace Application.Features.{Feature}.Commands.Create{Entity};

public class Create{Entity}CommandValidator : AbstractValidator<Create{Entity}Command>
{
    public Create{Entity}CommandValidator()
    {
        RuleFor(x => x.Create{Entity}Dto)
            .NotNull().WithMessage("{Entity} bilgileri gereklidir");

        When(x => x.Create{Entity}Dto != null, () =>
        {
            RuleFor(x => x.Create{Entity}Dto!.Title)
                .NotEmpty().WithMessage("Başlık boş olamaz")
                .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir");

            RuleFor(x => x.Create{Entity}Dto!.Description)
                .NotEmpty().WithMessage("Açıklama boş olamaz")
                .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir");

            RuleFor(x => x.Create{Entity}Dto!.OptionalField)
                .MaximumLength(500).WithMessage("Opsiyonel alan en fazla 500 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.Create{Entity}Dto!.OptionalField));
        });
    }
}
```

### Handler (with ICurrentUserService)

```csharp
using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.Features.{Feature}.Commands.Create{Entity};

public class Create{Entity}CommandHandler : IRequestHandler<Create{Entity}Command, Result<string>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public Create{Entity}CommandHandler(IAppDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<string>> Handle(Create{Entity}Command request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<{Entity}>(request.Create{Entity}Dto);

        entity.CreatedDate = DateTime.UtcNow;
        entity.CreatedBy = _currentUserService.UserId;
        entity.UserId = _currentUserService.UserId!;

        _context.{Entity}Plural.Add(entity);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<string>.Failure("Kayıt oluşturulamadı", 400);
        }

        return Result<string>.Success("Kayıt başarıyla oluşturuldu", entity.Id);
    }
}
```

## Update Command Template

### DTO (no immutable fields)

```csharp
namespace Application.Features.{Feature}.Commands.Update{Entity};

public class Update{Entity}Dto
{
    public required string Title { get; set; }
    public required string Description { get; set; }
    // NO Status, NO ItemType — these are immutable after creation
}
```

### Command

```csharp
using Application.Core;
using MediatR;

namespace Application.Features.{Feature}.Commands.Update{Entity};

public class Update{Entity}Command : IRequest<Result<Unit>>
{
    public required string Id { get; set; }
    public required Update{Entity}Dto Update{Entity}Dto { get; set; }
}
```

### Handler (ownership + soft delete check + property-by-property mapping)

```csharp
using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.{Feature}.Commands.Update{Entity};

public class Update{Entity}CommandHandler : IRequestHandler<Update{Entity}Command, Result<Unit>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public Update{Entity}CommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(Update{Entity}Command request, CancellationToken cancellationToken)
    {
        var entity = await _context.{Entity}Plural
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure("Kayıt bulunamadı", 404);
        }

        if (entity.UserId != _currentUserService.UserId)
        {
            return Result<Unit>.Failure("Bu kaydı güncelleme yetkiniz yok", 403);
        }

        entity.Title = request.Update{Entity}Dto.Title;
        entity.Description = request.Update{Entity}Dto.Description;

        entity.UpdatedDate = DateTime.UtcNow;
        entity.UpdatedBy = _currentUserService.UserId;

        await _context.SaveChangesAsync(cancellationToken);

        return Result<Unit>.Success("Kayıt başarıyla güncellendi", Unit.Value);
    }
}
```

## Delete Command Template (Soft Delete)

### Command

```csharp
using Application.Core;
using MediatR;

namespace Application.Features.{Feature}.Commands.Delete{Entity};

public class Delete{Entity}Command : IRequest<Result<Unit>>
{
    public required string Id { get; set; }
}
```

### Handler

```csharp
using Application.Core;
using Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.{Feature}.Commands.Delete{Entity};

public class Delete{Entity}CommandHandler : IRequestHandler<Delete{Entity}Command, Result<Unit>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public Delete{Entity}CommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Unit>> Handle(Delete{Entity}Command request, CancellationToken cancellationToken)
    {
        var entity = await _context.{Entity}Plural
            .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure("Kayıt bulunamadı", 404);
        }

        if (entity.UserId != _currentUserService.UserId)
        {
            return Result<Unit>.Failure("Bu kaydı silme yetkiniz yok", 403);
        }

        entity.IsDeleted = true;
        entity.UpdatedDate = DateTime.UtcNow;
        entity.UpdatedBy = _currentUserService.UserId;

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<Unit>.Failure("Kayıt silinemedi", 400);
        }

        return Result<Unit>.Success("Kayıt başarıyla silindi", Unit.Value);
    }
}
```

## Query Templates

### Sort Field Enum

```csharp
using System.ComponentModel;

namespace Application.Features.{Feature}.Queries.Common.Enums;

public enum {Entity}SortField
{
    [Description("title")]
    Title,
    [Description("createddate")]
    CreatedDate
}
```

### List Query (Paginated with Filtering/Sorting)

```csharp
using Application.Core;
using Application.Core.Pagination;
using Application.Features.{Feature}.Queries.Common.DTOs;
using Application.Features.{Feature}.Queries.Common.Enums;
using MediatR;

namespace Application.Features.{Feature}.Queries.Get{Entity}List;

public record Get{Entity}ListQuery : PagedAndSortedQueryBase<{Entity}SortField>,
    IRequest<Result<PaginatedListDto<Get{Entity}Dto>>>
{
    public string? SearchTerm { get; init; }
    public SomeEnum? SomeFilter { get; init; }
}
```

### List Query Handler

```csharp
using Application.Core;
using Application.Core.Pagination;
using Application.Features.{Feature}.Queries.Common.DTOs;
using Application.Features.{Feature}.Queries.Common.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.{Feature}.Queries.Get{Entity}List;

public class Get{Entity}ListQueryHandler :
    IRequestHandler<Get{Entity}ListQuery, Result<PaginatedListDto<Get{Entity}Dto>>>
{
    private readonly IAppDbContext _context;

    public Get{Entity}ListQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PaginatedListDto<Get{Entity}Dto>>> Handle(
        Get{Entity}ListQuery request, CancellationToken cancellationToken)
    {
        var query = _context.{Entity}Plural
            .Where(x => !x.IsDeleted && x.IsActive);

        // Filtering
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(x =>
                x.Title.ToLower().Contains(request.SearchTerm.ToLower()));
        }

        // Sorting (switch expression with default)
        query = request.SortBy switch
        {
            {Entity}SortField.Title => request.SortDescending
                ? query.OrderByDescending(x => x.Title)
                : query.OrderBy(x => x.Title),
            _ => query.OrderByDescending(x => x.CreatedDate)
        };

        var totalCount = await query.CountAsync(cancellationToken);

        // .Select() projection (NOT AutoMapper)
        var items = await query
            .Select(x => new Get{Entity}Dto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                CreatedDate = x.CreatedDate
            })
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var paginatedList = new PaginatedListDto<Get{Entity}Dto>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PaginatedListDto<Get{Entity}Dto>>.Success("Kayıtlar başarıyla getirildi", paginatedList);
    }
}
```

### Details Query Handler (with 404/410/423 status codes)

```csharp
if (entity == null)
{
    return Result<Get{Entity}DetailDto>.Failure("Kayıt bulunamadı", 404);
}

if (entity.IsDeleted)
{
    return Result<Get{Entity}DetailDto>.Failure("Bu kayıt silinmiş", 410);
}

if (!entity.IsActive)
{
    return Result<Get{Entity}DetailDto>.Failure("Bu kayıt aktif değil", 423);
}
```

## AutoMapper Mapping Template

```csharp
// ========== COMMANDS (Write) ==========
// DTO → Entity: Ignore all BaseEntity properties (set by handlers, not from DTOs)
CreateMap<Create{Entity}Dto, {Entity}>()
    .IgnoreAllBaseEntityProperties()
    .ForMember(dest => dest.UserId, opt => opt.Ignore())   // FK set by handler
    .ForMember(dest => dest.User, opt => opt.Ignore())     // navigation property
    .ForMember(dest => dest.Status, opt => opt.Ignore());   // immutable field
```

**Key rule:** Every navigation property AND every property not in the DTO must be explicitly `.Ignore()`d.

## Controller Template

```csharp
using Application.Features.{Feature}.Commands.Create{Entity};
using Application.Features.{Feature}.Commands.Update{Entity};
using Application.Features.{Feature}.Commands.Delete{Entity};
using Application.Features.{Feature}.Queries.Get{Entity}List;
using Application.Features.{Feature}.Queries.Get{Entity}Details;
using API.Controllers.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/{route-prefix}")]
public class {Entity}PluralController : BaseApiController
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult> GetItems([FromQuery] Get{Entity}Request request)
    {
        var query = new Get{Entity}ListQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            SearchTerm = request.SearchTerm
        };
        return HandleResult(await Mediator.Send(query));
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult> GetItemDetails(string id)
    {
        return HandleResult(await Mediator.Send(new Get{Entity}DetailsQuery { Id = id }));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult> CreateItem(Create{Entity}Dto dto)
    {
        return HandleResult(await Mediator.Send(new Create{Entity}Command { Create{Entity}Dto = dto }));
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult> UpdateItem(string id, Update{Entity}Dto dto)
    {
        return HandleResult(await Mediator.Send(new Update{Entity}Command { Id = id, Update{Entity}Dto = dto }));
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult> DeleteItem(string id)
    {
        return HandleResult(await Mediator.Send(new Delete{Entity}Command { Id = id }));
    }
}
```
