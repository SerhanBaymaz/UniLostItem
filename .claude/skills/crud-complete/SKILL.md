---
name: crud-complete
description: Generates complete CRUD operations following UniLostItem's .NET 9 Clean Architecture with CQRS, MediatR, EF Core, and FluentValidation. Use when user asks to create CRUD for an entity, add CRUD operations, or scaffold entity features.
allowed-tools: Read, Write, Edit, Glob, Grep, Bash
---

# CRUD Complete — UniLostItem Pattern Generator

## Overview

Generates complete CRUD operations following the project's established patterns from `LostItems` and `ItemClaims` features.

**Generated files:**
- Domain Entity (inherits `BaseEntity`)
- Application: Commands, Queries, Handlers, Validators, DTOs
- API: Controller with RESTful endpoints + Request DTO
- MappingProfiles update
- Tests: Unit tests using SQLite in-memory via `TestDbContextFactory`

## Workflow

### Step 1: Ask for Entity Details

Ask the user for:
1. **Entity Name** (singular, PascalCase) — e.g., `Product`, `Notification`
2. **Properties** with types — e.g., `Name (string), Price (decimal), IsActive (bool)`
3. **API Route Prefix** (optional, defaults to kebab-case plural)
4. **Auth requirements** — anonymous, [Authorize], owner-only, admin-only

### Step 2: Generate Domain Entity

**File:** `Domain/{EntityName}.cs`

```csharp
using Domain.Common;

namespace Domain;

public class {EntityName} : BaseEntity
{
    public required string Property1 { get; set; }
    public required string Property2 { get; set; }
    // max length hints from implementation plan should be followed
    public string? OptionalProperty { get; set; } // nullable
}
```

**Rules:**
- Always inherit from `BaseEntity` (provides Id, CreatedDate, CreatedBy, UpdatedDate, UpdatedBy, IsDeleted, IsActive)
- Use `required` for non-nullable reference types
- Navigation properties use `= null!` syntax

### Step 3: Generate Application Layer — Commands

#### Create Command

**File:** `Application/Features/{FeatureName}/Commands/Create{EntityName}/Create{EntityName}Dto.cs`
```csharp
namespace Application.Features.{FeatureName}.Commands.Create{EntityName};

public class Create{EntityName}Dto
{
    public required string Property1 { get; set; }
    // ... entity properties minus BaseEntity fields and immutable fields
}
```

**File:** `Application/Features/{FeatureName}/Commands/Create{EntityName}/Create{EntityName}Command.cs`
```csharp
using Application.Core;
using MediatR;

namespace Application.Features.{FeatureName}.Commands.Create{EntityName};

public class Create{EntityName}Command : IRequest<Result<string>>
{
    public required Create{EntityName}Dto Create{EntityName}Dto { get; set; }
}
```

**File:** `Application/Features/{FeatureName}/Commands/Create{EntityName}/Create{EntityName}CommandValidator.cs`
```csharp
using FluentValidation;

namespace Application.Features.{FeatureName}.Commands.Create{EntityName};

public class Create{EntityName}CommandValidator : AbstractValidator<Create{EntityName}Command>
{
    public Create{EntityName}CommandValidator()
    {
        RuleFor(x => x.Create{EntityName}Dto)
            .NotNull().WithMessage("{EntityName} bilgileri gereklidir");

        When(x => x.Create{EntityName}Dto != null, () =>
        {
            RuleFor(x => x.Create{EntityName}Dto!.Property1)
                .NotEmpty().WithMessage("Property1 boş olamaz")
                .MaximumLength(200).WithMessage("Property1 en fazla 200 karakter olabilir");

            // string: NotEmpty + MaximumLength
            // int/decimal: GreaterThan(0) or InclusiveBetween(min, max)
            // DateTime: NotEmpty + LessThanOrEqualTo(DateTime.UtcNow)
            // enum: IsInEnum()
            // optional strings: MaximumLength only with .When(x => x.Create{EntityName}Dto != null && !string.IsNullOrEmpty(x.Create{EntityName}Dto!.OptionalProp))
        });
    }
}
```

**File:** `Application/Features/{FeatureName}/Commands/Create{EntityName}/Create{EntityName}CommandHandler.cs`
```csharp
using Application.Core;
using Application.Interfaces;
using AutoMapper;
using Domain;
using MediatR;
using Persistence;

namespace Application.Features.{FeatureName}.Commands.Create{EntityName};

public class Create{EntityName}CommandHandler : IRequestHandler<Create{EntityName}Command, Result<string>>
{
    private readonly IAppDbContext _context;
    private readonly IMapper _mapper;
    private readonly ICurrentUserService _currentUserService;

    public Create{EntityName}CommandHandler(IAppDbContext context, IMapper mapper, ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _currentUserService = currentUserService;
    }

    public async Task<Result<string>> Handle(Create{EntityName}Command request, CancellationToken cancellationToken)
    {
        var entity = _mapper.Map<{EntityName}>(request.Create{EntityName}Dto);

        entity.CreatedDate = DateTime.UtcNow;
        entity.CreatedBy = _currentUserService.UserId;
        // Set FK properties: entity.UserId = _currentUserService.UserId!; (if owned by user)

        _context.{EntityName}Plural.Add(entity);

        var result = await _context.SaveChangesAsync(cancellationToken) > 0;

        if (!result)
        {
            return Result<string>.Failure("Kayıt oluşturulamadı", 400);
        }

        return Result<string>.Success("Kayıt başarıyla oluşturuldu", entity.Id);
    }
}
```

#### Update Command

**Key differences from Create:**
- DTO does NOT include immutable fields (e.g., ItemType, Status)
- Handler does NOT use AutoMapper — property-by-property mapping instead
- Handler checks ownership (403 if not owner)
- Handler checks soft delete filter (`!x.IsDeleted`)

```csharp
public async Task<Result<Unit>> Handle(Update{EntityName}Command request, CancellationToken cancellationToken)
{
    var entity = await _context.{EntityName}Plural
        .FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken);

    if (entity == null)
    {
        return Result<Unit>.Failure("Kayıt bulunamadı", 404);
    }

    // Ownership check if entity has UserId:
    if (entity.UserId != _currentUserService.UserId)
    {
        return Result<Unit>.Failure("Bu kaydı güncelleme yetkiniz yok", 403);
    }

    // Property-by-property mapping (NOT AutoMapper)
    entity.Property1 = request.Update{EntityName}Dto.Property1;
    entity.Property2 = request.Update{EntityName}Dto.Property2;

    entity.UpdatedDate = DateTime.UtcNow;
    entity.UpdatedBy = _currentUserService.UserId;

    await _context.SaveChangesAsync(cancellationToken);

    return Result<Unit>.Success("Kayıt başarıyla güncellendi", Unit.Value);
}
```

#### Delete Command

- Soft delete pattern: `entity.IsDeleted = true` (NOT `_context.Remove()`)
- Ownership check (403) or admin override
- No DTO needed

```csharp
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
```

### Step 4: Generate Application Layer — Queries

#### Sort Field Enum

**File:** `Application/Features/{FeatureName}/Queries/Common/Enums/{EntityName}SortField.cs`

```csharp
using System.ComponentModel;

namespace Application.Features.{FeatureName}.Queries.Common.Enums;

public enum {EntityName}SortField
{
    [Description("createddate")]
    CreatedDate,
    [Description("property1")]
    Property1
}
```

#### List Query (Paginated)

**File:** `Application/Features/{FeatureName}/Queries/Get{EntityName}List/Get{EntityName}ListQuery.cs`

```csharp
using Application.Core;
using Application.Core.Pagination;
using Application.Features.{FeatureName}.Queries.Common.DTOs;
using Application.Features.{FeatureName}.Queries.Common.Enums;
using MediatR;

namespace Application.Features.{FeatureName}.Queries.Get{EntityName}List;

public record Get{EntityName}ListQuery : PagedAndSortedQueryBase<{EntityName}SortField>,
    IRequest<Result<PaginatedListDto<Get{EntityName}Dto>>>
{
    public string? SearchTerm { get; init; }
    // Add filter properties as needed
}
```

**Handler key patterns:**
- Always filter `!x.IsDeleted && x.IsActive`
- Use `.Select()` projection (NOT AutoMapper) for computed fields
- Sorting via `switch` expression with default fallback
- Paginate with `Skip/Take` + `PaginatedListDto<T>`

#### Details Query

- Return 404 if not found, 410 if soft-deleted, 423 if inactive

### Step 5: Update MappingProfiles

**File:** `Application/Core/MappingProfiles.cs`

```csharp
// ========== COMMANDS (Write) ==========
CreateMap<Create{EntityName}Dto, {EntityName}>()
    .IgnoreAllBaseEntityProperties()
    .ForMember(dest => dest.UserId, opt => opt.Ignore()) // if entity has UserId
    .ForMember(dest => dest.User, opt => opt.Ignore());  // ignore all navigation properties
```

**Important:** Each navigation property must be explicitly `.Ignore()`d. Query DTOs use `.Select()` projection, not AutoMapper.

### Step 6: Create API Controller

**File:** `API/Controllers/{EntityName}PluralController.cs`

- Route: `api/v1/{route-prefix}`
- Inherits `BaseApiController`
- Request DTO in `API/Controllers/Requests/` with Data Annotations
- Use `HandleResult()` from base controller
- Anonymous endpoints: `[AllowAnonymous]`
- Auth endpoints: `[Authorize]`

### Step 7: Database Migration

```bash
dotnet ef migrations add Add{EntityName} -p Persistence -s API
dotnet ef database update -p Persistence -s API
```

### Step 8: Build & Test

```bash
dotnet build UniLostItem.sln
dotnet test
```

## Code Style Rules

- **Always use braces `{ }`** on if/else, even single-line bodies
- **Turkish error messages** in handlers and validators
- **`required` keyword** on command DTO properties and command Id
- **`DateTime.UtcNow`** for all timestamps
- **No comments** unless the WHY is non-obvious

## Important Notes

1. **BaseEntity inheritance** — All business entities inherit from `BaseEntity`; `ApplicationUser` does NOT
2. **Soft delete** — Delete sets `IsDeleted = true`, NOT physical removal
3. **Ownership** — Update/Delete check `entity.UserId != currentUserId` → 403
4. **No AutoMapper for Update** — Property-by-property mapping instead
5. **Query projection** — Use `.Select()` with computed fields, not AutoMapper
6. **Navigation properties** — Always `.Ignore()` in AutoMapper, always `.Include()` in EF queries when accessing nav props
7. **FK constraints in tests** — Must seed referenced entities in SQLite in-memory DB
