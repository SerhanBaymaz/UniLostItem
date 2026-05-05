# Validator Patterns — UniLostItem

FluentValidation patterns matching the actual codebase from `LostItems` and `ItemClaims` features.

## Command Validator Structure

All validators follow the same pattern:
1. Validate DTO is not null
2. Use `When(x => x.Dto != null, () => { ... })` to scope property validations
3. Turkish error messages
4. No `OverridePropertyName` — use default property names

## Common Validation Rules

### Required String

```csharp
RuleFor(x => x.Dto.Title)
    .NotEmpty().WithMessage("Başlık boş olamaz")
    .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir")
    .When(x => x.Dto != null);
```

### Optional String (nullable, only validate length when present)

```csharp
RuleFor(x => x.Dto.ImageUrl)
    .MaximumLength(500).WithMessage("Resim URL en fazla 500 karakter olabilir")
    .When(x => x.Dto != null && !string.IsNullOrEmpty(x.Dto.ImageUrl));
```

### Enum

```csharp
RuleFor(x => x.Dto.Category)
    .IsInEnum().WithMessage("Geçersiz kategori")
    .When(x => x.Dto != null);
```

### DateTime (past or present only)

```csharp
RuleFor(x => x.Dto.IncidentDate)
    .NotEmpty().WithMessage("Olay tarihi boş olamaz")
    .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Olay tarihi gelecekte olamaz")
    .When(x => x.Dto != null);
```

### Numeric Range

```csharp
RuleFor(x => x.Dto.Latitude)
    .InclusiveBetween(-90, 90).WithMessage("Enlem -90 ile 90 arasında olmalıdır")
    .When(x => x.Dto != null);

RuleFor(x => x.Dto.Longitude)
    .InclusiveBetween(-180, 180).WithMessage("Boylam -180 ile 180 arasında olamaz")
    .When(x => x.Dto != null);
```

### Boolean (always valid, no rule needed)

### Id Validation (for Update/Delete commands)

```csharp
RuleFor(x => x.Id)
    .NotEmpty().WithMessage("ID gereklidir");
```

### Comment/Note (optional string with max length)

```csharp
RuleFor(x => x.Dto.Comment)
    .MaximumLength(500).WithMessage("Yorum en fazla 500 karakter olabilir")
    .When(x => x.Dto != null);
```

## Complete Examples

### CreateLostItemCommandValidator

```csharp
using FluentValidation;

namespace Application.Features.LostItems.Commands.CreateLostItem;

public class CreateLostItemCommandValidator : AbstractValidator<CreateLostItemCommand>
{
    public CreateLostItemCommandValidator()
    {
        RuleFor(x => x.CreateLostItemDto)
            .NotNull().WithMessage("Item bilgileri gereklidir");

        When(x => x.CreateLostItemDto != null, () =>
        {
            RuleFor(x => x.CreateLostItemDto!.Title)
                .NotEmpty().WithMessage("Başlık boş olamaz")
                .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir");

            RuleFor(x => x.CreateLostItemDto!.Description)
                .NotEmpty().WithMessage("Açıklama boş olamaz")
                .MaximumLength(2000).WithMessage("Açıklama en fazla 2000 karakter olabilir");

            RuleFor(x => x.CreateLostItemDto!.Category)
                .IsInEnum().WithMessage("Geçersiz kategori");

            RuleFor(x => x.CreateLostItemDto!.ItemType)
                .IsInEnum().WithMessage("Geçersiz item tipi");

            RuleFor(x => x.CreateLostItemDto!.IncidentDate)
                .NotEmpty().WithMessage("Olay tarihi boş olamaz")
                .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Olay tarihi gelecekte olamaz");

            RuleFor(x => x.CreateLostItemDto!.ImageUrl)
                .MaximumLength(500).WithMessage("Resim URL en fazla 500 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.CreateLostItemDto!.ImageUrl));

            RuleFor(x => x.CreateLostItemDto!.ContactInfo)
                .MaximumLength(300).WithMessage("İletişim bilgisi en fazla 300 karakter olabilir")
                .When(x => !string.IsNullOrEmpty(x.CreateLostItemDto!.ContactInfo));

            RuleFor(x => x.CreateLostItemDto!.LocationLabel)
                .NotEmpty().WithMessage("Konum açıklaması boş olamaz")
                .MaximumLength(300).WithMessage("Konum açıklaması en fazla 300 karakter olabilir");

            RuleFor(x => x.CreateLostItemDto!.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Enlem -90 ile 90 arasında olmalıdır");

            RuleFor(x => x.CreateLostItemDto!.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Boylam -180 ile 180 arasında olmalıdır");
        });
    }
}
```

### CreateItemClaimCommandValidator (simple DTO)

```csharp
using FluentValidation;

namespace Application.Features.ItemClaims.Commands.CreateItemClaim;

public class CreateItemClaimCommandValidator : AbstractValidator<CreateItemClaimCommand>
{
    public CreateItemClaimCommandValidator()
    {
        RuleFor(x => x.CreateItemClaimDto)
            .NotNull().WithMessage("Talep bilgileri gereklidir");

        When(x => x.CreateItemClaimDto != null, () =>
        {
            RuleFor(x => x.CreateItemClaimDto!.LostItemId)
                .NotEmpty().WithMessage("İlan ID gereklidir");

            RuleFor(x => x.CreateItemClaimDto!.Description)
                .NotEmpty().WithMessage("Açıklama gereklidir")
                .MaximumLength(1000).WithMessage("Açıklama en fazla 1000 karakter olabilir");
        });
    }
}
```

### RespondToClaimCommandValidator (Id + DTO with optional comment)

```csharp
using FluentValidation;

namespace Application.Features.ItemClaims.Commands.RespondToClaim;

public class RespondToClaimCommandValidator : AbstractValidator<RespondToClaimCommand>
{
    public RespondToClaimCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Talep ID gereklidir");

        RuleFor(x => x.RespondToClaimDto).NotNull().WithMessage("Yanıt bilgileri gereklidir");

        When(x => x.RespondToClaimDto != null, () =>
        {
            RuleFor(x => x.RespondToClaimDto!.Comment)
                .MaximumLength(500).WithMessage("Yorum en fazla 500 karakter olabilir");
        });
    }
}
```

### Paginated Query Validator

```csharp
using Application.Core.Pagination;
using Application.Features.{Feature}.Queries.Common.Enums;
using FluentValidation;

namespace Application.Features.{Feature}.Queries.Get{Entity}List;

public class Get{Entity}ListValidator : PagedAndSortedQueryValidator<Get{Entity}ListQuery, {Entity}SortField>
{
    public Get{Entity}ListValidator()
    {
        // Base class handles PageNumber, PageSize, SortBy, SortDescending
        // Add feature-specific rules here if needed
    }
}
```
