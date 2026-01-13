# CRUD Code Templates

This file contains complete code templates for all CRUD components.

## Table of Contents

1. [Domain Entity](#domain-entity)
2. [Create Command](#create-command)
3. [Edit Command](#edit-command)
4. [Delete Command](#delete-command)
5. [Get List Query](#get-list-query)
6. [Get Details Query](#get-details-query)
7. [DTOs](#dtos)
8. [API Controller](#api-controller)
9. [MappingProfiles](#mappingprofiles)

---

## Domain Entity

**File:** `Domain/{EntityName}.cs`

```csharp
using System;

namespace Domain;

public class {EntityName}
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string Property1 { get; set; }
    public required string Property2 { get; set; }
    public int Property3 { get; set; }
    public bool IsActive { get; set; } = true;
}
```

---

## Create Command

**File:** `Application/Features/{EntityName}Plural/Commands/Create{EntityName}/Create{EntityName}Command.cs`

```csharp
using Application.Core;
using MediatR;

namespace Application.Features.{EntityName}Plural.Commands.Create{EntityName};

public class Create{EntityName}Command : IRequest<Result<string>>
{
    public required Create{EntityName}Dto Create{EntityName}Dto { get; set; }
}
```

**File:** `Application/Features/{EntityName}Plural/Commands/Create{EntityName}/Create{EntityName}Dto.cs`

```csharp
namespace Application.Features.{EntityName}Plural.Commands.Create{EntityName};

public class Create{EntityName}Dto
{
    public required string Property1 { get; set; }
    public required string Property2 { get; set; }
    public required int Property3 { get; set; }
}
```

**File:** `Application/Features/{EntityName}Plural/Commands/Create{EntityName}/Create{EntityName}CommandHandler.cs`

```csharp
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.Features.{EntityName}Plural.Commands.Create{EntityName};

public class Create{EntityName}CommandHandler : IRequestHandler<Create{EntityName}Command, Result<string>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public Create{EntityName}CommandHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<string>> Handle(Create{EntityName}Command request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<{EntityName}>(request.Create{EntityName}Dto);

        _context.{EntityName}Plural.Add(entity);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<string>.Failure("Failed to create the {entityNameLower}", 400);
        }

        return Result<string>.Success("{EntityName} created successfully", entity.Id);
    }
}
```

---

## Edit Command

**File:** `Application/Features/{EntityName}Plural/Commands/Edit{EntityName}/Edit{EntityName}Command.cs`

```csharp
using Application.Core;
using MediatR;

namespace Application.Features.{EntityName}Plural.Commands.Edit{EntityName};

public class Edit{EntityName}Command : IRequest<Result<Unit>>
{
    public string Id { get; set; } = string.Empty;
    public required Edit{EntityName}Dto Edit{EntityName}Dto { get; set; }
}
```

**File:** `Application/Features/{EntityName}Plural/Commands/Edit{EntityName}/Edit{EntityName}Dto.cs`

```csharp
namespace Application.Features.{EntityName}Plural.Commands.Edit{EntityName};

public class Edit{EntityName}Dto
{
    public required string Property1 { get; set; }
    public required string Property2 { get; set; }
    public required int Property3 { get; set; }
}
```

**File:** `Application/Features/{EntityName}Plural/Commands/Edit{EntityName}/Edit{EntityName}CommandHandler.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.{EntityName}Plural.Commands.Edit{EntityName};

public class Edit{EntityName}CommandHandler : IRequestHandler<Edit{EntityName}Command, Result<Unit>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public Edit{EntityName}CommandHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<Unit>> Handle(Edit{EntityName}Command request, CancellationToken cancellationToken)
    {
        var entity = await _context.{EntityName}Plural
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure("{EntityName} not found", 404);
        }

        _mapper.Map(request.Edit{EntityName}Dto, entity);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<Unit>.Failure("Failed to update the {entityNameLower}", 400);
        }

        return Result<Unit>.Success("{EntityName} updated successfully", Unit.Value);
    }
}
```

---

## Delete Command

**File:** `Application/Features/{EntityName}Plural/Commands/Delete{EntityName}/Delete{EntityName}Command.cs`

```csharp
using Application.Core;
using MediatR;

namespace Application.Features.{EntityName}Plural.Commands.Delete{EntityName};

public class Delete{EntityName}Command : IRequest<Result<Unit>>
{
    public string Id { get; set; } = string.Empty;
}
```

**File:** `Application/Features/{EntityName}Plural/Commands/Delete{EntityName}/Delete{EntityName}CommandHandler.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.{EntityName}Plural.Commands.Delete{EntityName};

public class Delete{EntityName}CommandHandler : IRequestHandler<Delete{EntityName}Command, Result<Unit>>
{
    private readonly IAppDbContext _context;

    public Delete{EntityName}CommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<Unit>> Handle(Delete{EntityName}Command request, CancellationToken cancellationToken)
    {
        var entity = await _context.{EntityName}Plural
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<Unit>.Failure("{EntityName} not found", 404);
        }

        _context.{EntityName}Plural.Remove(entity);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<Unit>.Failure("Failed to delete the {entityNameLower}", 400);
        }

        return Result<Unit>.Success("{EntityName} deleted successfully", Unit.Value);
    }
}
```

---

## Get List Query

**File:** `Application/Features/{EntityName}Plural/Queries/Get{EntityName}List/Get{EntityName}ListQuery.cs`

```csharp
using Application.Core;
using MediatR;

namespace Application.Features.{EntityName}Plural.Queries.Get{EntityName}List;

public class Get{EntityName}ListQuery : IRequest<Result<List<Get{EntityName}Dto>>>
{
}
```

**File:** `Application/Features/{EntityName}Plural/Queries/Get{EntityName}List/Get{EntityName}ListQueryHandler.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.{EntityName}Plural.Queries.Get{EntityName}List;

public class Get{EntityName}ListQueryHandler : IRequestHandler<Get{EntityName}ListQuery, Result<List<Get{EntityName}Dto>>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public Get{EntityName}ListQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<List<Get{EntityName}Dto>>> Handle(Get{EntityName}ListQuery request, CancellationToken cancellationToken)
    {
        var entities = await _context.{EntityName}Plural
            .ToListAsync(cancellationToken);

        var dtos = _mapper.Map<List<Get{EntityName}Dto>>(entities);

        return Result<List<Get{EntityName}Dto>>.Success("Entities retrieved successfully", dtos);
    }
}
```

---

## Get Details Query

**File:** `Application/Features/{EntityName}Plural/Queries/Get{EntityName}Details/Get{EntityName}DetailsQuery.cs`

```csharp
using Application.Core;
using MediatR;

namespace Application.Features.{EntityName}Plural.Queries.Get{EntityName}Details;

public class Get{EntityName}DetailsQuery : IRequest<Result<Get{EntityName}Dto>>
{
    public string Id { get; set; } = string.Empty;
}
```

**File:** `Application/Features/{EntityName}Plural/Queries/Get{EntityName}Details/Get{EntityName}DetailsQueryHandler.cs`

```csharp
using System;
using System.Threading;
using System.Threading.Tasks;
using Application.Core;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Persistence;

namespace Application.Features.{EntityName}Plural.Queries.Get{EntityName}Details;

public class Get{EntityName}DetailsQueryHandler : IRequestHandler<Get{EntityName}DetailsQuery, Result<Get{EntityName}Dto>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;

    public Get{EntityName}DetailsQueryHandler(IAppDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<Result<Get{EntityName}Dto>> Handle(Get{EntityName}DetailsQuery request, CancellationToken cancellationToken)
    {
        var entity = await _context.{EntityName}Plural
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
        {
            return Result<Get{EntityName}Dto>.Failure("{EntityName} not found", 404);
        }

        var dto = _mapper.Map<Get{EntityName}Dto>(entity);

        return Result<Get{EntityName}Dto>.Success("Entity retrieved successfully", dto);
    }
}
```

---

## DTOs

**File:** `Application/Features/{EntityName}Plural/Queries/Common/DTOs/Get{EntityName}Dto.cs`

```csharp
namespace Application.Features.{EntityName}Plural.Queries.Common.DTOs;

public class Get{EntityName}Dto
{
    public string Id { get; set; } = string.Empty;
    public required string Property1 { get; set; }
    public required string Property2 { get; set; }
    public int Property3 { get; set; }
    public bool IsActive { get; set; }
}
```

---

## API Controller

**File:** `API/Controllers/{EntityName}PluralController.cs`

```csharp
using API.Responses;
using Application.Features.{EntityName}Plural.Commands.Create{EntityName};
using Application.Features.{EntityName}Plural.Commands.Delete{EntityName};
using Application.Features.{EntityName}Plural.Commands.Edit{EntityName};
using Application.Features.{EntityName}Plural.Queries.Common.DTOs;
using Application.Features.{EntityName}Plural.Queries.Get{EntityName}Details;
using Application.Features.{EntityName}Plural.Queries.Get{EntityName}List;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/v1/{route-prefix}")]
public class {EntityName}PluralController : BaseApiController
{
    [HttpGet]
    public async Task<ActionResult<StandardApiResponse<List<Get{EntityName}Dto>>>> Get{EntityName}Plural()
    {
        return HandleResult(await Mediator.Send(new Get{EntityName}ListQuery()));
    }

    [HttpGet("{{id}}")]
    public async Task<ActionResult<StandardApiResponse<Get{EntityName}Dto>>> Get{EntityName}Detail(string id)
    {
        return HandleResult(await Mediator.Send(new Get{EntityName}DetailsQuery { Id = id }));
    }

    [HttpPost]
    public async Task<ActionResult<StandardApiResponse<string>>> Create{EntityName}([FromBody] Create{EntityName}Dto create{EntityName}Dto)
    {
        var command = new Create{EntityName}Command { Create{EntityName}Dto = create{EntityName}Dto };
        return HandleResult(await Mediator.Send(command));
    }

    [HttpPut("{{id}}")]
    public async Task<ActionResult<StandardApiResponse<Unit>>> Edit{EntityName}(string id, [FromBody] Edit{EntityName}Dto edit{EntityName}Dto)
    {
        var command = new Edit{EntityName}Command { Id = id, Edit{EntityName}Dto = edit{EntityName}Dto };
        return HandleResult(await Mediator.Send(command));
    }

    [HttpDelete("{{id}}")]
    public async Task<ActionResult<StandardApiResponse<Unit>>> Delete{EntityName}(string id)
    {
        return HandleResult(await Mediator.Send(new Delete{EntityName}Command { Id = id }));
    }
}
```

---

## MappingProfiles

**File:** `Application/Core/MappingProfiles.cs`

Add these mappings to the `MappingProfiles` class:

```csharp
// ========== QUERIES (Read) ==========
// Entity → DTO
CreateMap<{EntityName}, Get{EntityName}Dto>();

// ========== COMMANDS (Write) ==========
// DTO → Entity (Id is ignored for creation and editing)
CreateMap<Create{EntityName}Dto, {EntityName}>()
    .ForMember(dest => dest.Id, opt => opt.Ignore());

CreateMap<Edit{EntityName}Dto, {EntityName}>()
    .ForMember(dest => dest.Id, opt => opt.Ignore());
```

---

## Template Placeholders

Replace these placeholders when generating code:

| Placeholder | Description | Example |
| --- | --- | --- |
| `{EntityName}` | PascalCase entity name (singular) | `Product`, `Customer` |
| `{EntityName}Plural` | Plural form of entity name | `Products`, `Customers` |
| `{entityNameLower}` | Lowercase entity name | `product`, `customer` |
| `{route-prefix}` | URL-friendly kebab-case plural | `products`, `customers` |
| `{Property1}`, etc. | Entity property names | `Name`, `Price`, `Stock` |
| `Property1`, etc. | Property name without prefix | `Name`, `Price`, `Stock` |
| `property1`, etc. | Lowercase property name | `name`, `price`, `stock` |
