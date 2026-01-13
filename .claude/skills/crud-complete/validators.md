# Validator Templates

This file contains FluentValidation validator patterns for CRUD operations.

## Create Command Validator

**File:** `Application/Features/{EntityName}Plural/Commands/Create{EntityName}/Create{EntityName}CommandValidator.cs`

```csharp
using FluentValidation;
using Application.Features.{EntityName}Plural.Commands.Create{EntityName};

namespace Application.Features.{EntityName}Plural.Validators;

public class Create{EntityName}CommandValidator : AbstractValidator<Create{EntityName}Command>
{
    public Create{EntityName}CommandValidator()
    {
        RuleFor(x => x.Create{EntityName}Dto)
            .NotNull().WithMessage("DTO is required")
            .OverridePropertyName("{EntityName}");

        // Validate nested properties when DTO is not null
        RuleFor(x => x.Create{EntityName}Dto.Property1)
            .NotEmpty().WithMessage("Property1 is required")
            .When(x => x.Create{EntityName}Dto != null)
            .OverridePropertyName("Property1");

        RuleFor(x => x.Create{EntityName}Dto.Property2)
            .NotEmpty().WithMessage("Property2 is required")
            .When(x => x.Create{EntityName}Dto != null)
            .OverridePropertyName("Property2");

        RuleFor(x => x.Create{EntityName}Dto.Property3)
            .NotEmpty().WithMessage("Property3 is required")
            .GreaterThan(0).WithMessage("Property3 must be greater than 0")
            .When(x => x.Create{EntityName}Dto != null)
            .OverridePropertyName("Property3");
    }
}
```

## Edit Command Validator

**File:** `Application/Features/{EntityName}Plural/Commands/Edit{EntityName}/Edit{EntityName}CommandValidator.cs`

```csharp
using FluentValidation;
using Application.Features.{EntityName}Plural.Commands.Edit{EntityName};

namespace Application.Features.{EntityName}Plural.Validators;

public class Edit{EntityName}CommandValidator : AbstractValidator<Edit{EntityName}Command>
{
    public Edit{EntityName}CommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ID is required")
            .OverridePropertyName("Id");

        RuleFor(x => x.Edit{EntityName}Dto)
            .NotNull().WithMessage("DTO is required")
            .OverridePropertyName("{EntityName}");

        // Validate nested properties when DTO is not null
        RuleFor(x => x.Edit{EntityName}Dto.Property1)
            .NotEmpty().WithMessage("Property1 is required")
            .When(x => x.Edit{EntityName}Dto != null)
            .OverridePropertyName("Property1");

        RuleFor(x => x.Edit{EntityName}Dto.Property2)
            .NotEmpty().WithMessage("Property2 is required")
            .When(x => x.Edit{EntityName}Dto != null)
            .OverridePropertyName("Property2");

        RuleFor(x => x.Edit{EntityName}Dto.Property3)
            .NotEmpty().WithMessage("Property3 is required")
            .GreaterThan(0).WithMessage("Property3 must be greater than 0")
            .When(x => x.Edit{EntityName}Dto != null)
            .OverridePropertyName("Property3");
    }
}
```

---

## Common Validation Patterns

## String Validations

### Required String

```csharp
RuleFor(x => x.Property)
    .NotEmpty().WithMessage("Property is required");
```

### String Length

```csharp
RuleFor(x => x.Property)
    .MaximumLength(100).WithMessage("Property cannot exceed 100 characters");

RuleFor(x => x.Property)
    .Length(5, 50).WithMessage("Property must be between 5 and 50 characters");
```

### Email Validation

```csharp
RuleFor(x => x.Email)
    .EmailAddress().WithMessage("Invalid email format");
```

### Phone Number (Basic)

```csharp
RuleFor(x => x.Phone)
    .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format");
```

### URL Validation

```csharp
RuleFor(x => x.Website)
    .Must(BeAValidUrl).WithMessage("Invalid URL format");

private bool BeAValidUrl(string? url)
{
    return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
        && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
}
```

## Numeric Validations

### Required Number

```csharp
RuleFor(x => x.Amount)
    .NotEmpty().WithMessage("Amount is required")
    .GreaterThan(0).WithMessage("Amount must be greater than 0");
```

### Range Validation

```csharp
RuleFor(x => x.Rating)
    .InclusiveBetween(1, 5).WithMessage("Rating must be between 1 and 5");

RuleFor(x => x.Price)
    .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative");
```

### Decimal Precision

```csharp
RuleFor(x => x.Price)
    .ScalePrecision(2, 10).WithMessage("Price cannot have more than 2 decimal places");
```

## Date/Time Validations

### Required Date

```csharp
RuleFor(x => x.BirthDate)
    .NotEmpty().WithMessage("Birth date is required");
```

### Date Range

```csharp
RuleFor(x => x.StartDate)
    .LessThan(x => x.EndDate).WithMessage("Start date must be before end date");

RuleFor(x => x.EventDate)
    .Must(BeAFutureDate).WithMessage("Event date must be in the future");

private bool BeAFutureDate(DateTime date)
{
    return date > DateTime.UtcNow;
}
```

### Age Validation

```csharp
RuleFor(x => x.BirthDate)
    .Must(BeAtLeast18YearsOld).WithMessage("Must be at least 18 years old");

private bool BeAtLeast18YearsOld(DateTime birthDate)
{
    var today = DateTime.Today;
    var age = today.Year - birthDate.Year;
    if (birthDate.Date > today.AddYears(-age)) age--;
    return age >= 18;
}
```

## Boolean Validations

### Required Boolean

```csharp
RuleFor(x => x.AcceptTerms)
    .Equal(true).WithMessage("You must accept the terms and conditions");
```

## Enum Validations

### Required Enum

```csharp
RuleFor(x => x.Status)
    .IsInEnum().WithMessage("Invalid status value")
    .NotEqual(Status.None).WithMessage("Status must be specified");
```

## Collection Validations

### Required Collection

```csharp
RuleFor(x => x.Items)
    .NotNull().WithMessage("Items are required")
    .NotEmpty().WithMessage("At least one item is required");
```

### Collection Size

```csharp
RuleFor(x => x.Tags)
    .Must(tags => tags == null || tags.Count <= 5).WithMessage("Cannot have more than 5 tags");
```

## Conditional Validation

### Validate When Another Property Has Value

```csharp
RuleFor(x => x.ShippingAddress)
    .NotEmpty().WithMessage("Shipping address is required")
    .When(x => x.RequiresShipping)
    .OverridePropertyName("ShippingAddress");
```

### Validate Based on Property Value

```csharp
RuleFor(x => x.BusinessName)
    .NotEmpty().WithMessage("Business name is required")
    .When(x => x.AccountType == AccountType.Business)
    .OverridePropertyName("BusinessName");
```

## Custom Validation

### Must Method

```csharp
RuleFor(x => x.Username)
    .Must(BeUniqueUsername).WithMessage("Username already exists")
    .When(x => !string.IsNullOrEmpty(x.Username));

private bool BeUniqueUsername(string username)
{
    // Custom validation logic
    return !_context.Users.Any(u => u.Username == username);
}
```

### Custom Async Validation

```csharp
RuleFor(x => x.Email)
    .MustAsync(BeUniqueEmail).WithMessage("Email already registered")
    .When(x => !string.IsNullOrEmpty(x.Email));

private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
{
    return !await _context.Users.AnyAsync(u => u.Email == email, cancellationToken);
}
```

---

## Validator Best Practices

1. **Always use `OverridePropertyName`**: This ensures the error response shows the correct field name
2. **Use `.When()` for null checks**: Validate nested properties only when parent is not null
3. **Keep validators focused**: Each validator should handle one concern
4. **Use descriptive error messages**: Messages should be clear and actionable
5. **Language considerations**: Use Turkish messages for Turkish projects, English for English projects

---

## Common Validator Examples

## Product Entity Validator

```csharp
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.CreateProductDto)
            .NotNull().WithMessage("Ürün bilgileri gereklidir")
            .OverridePropertyName("Product");

        RuleFor(x => x.CreateProductDto.Name)
            .NotEmpty().WithMessage("Ürün adı boş olamaz")
            .MaximumLength(100).WithMessage("Ürün adı 100 karakteri geçemez")
            .When(x => x.CreateProductDto != null)
            .OverridePropertyName("Name");

        RuleFor(x => x.CreateProductDto.Price)
            .NotEmpty().WithMessage("Fiyat boş olamaz")
            .GreaterThan(0).WithMessage("Fiyat 0'dan büyük olmalıdır")
            .When(x => x.CreateProductDto != null)
            .OverridePropertyName("Price");

        RuleFor(x => x.CreateProductDto.Stock)
            .NotEmpty().WithMessage("Stok miktarı boş olamaz")
            .GreaterThanOrEqualTo(0).WithMessage("Stok miktarı negatif olamaz")
            .When(x => x.CreateProductDto != null)
            .OverridePropertyName("Stock");

        RuleFor(x => x.CreateProductDto.Category)
            .IsInEnum().WithMessage("Geçersiz kategori")
            .When(x => x.CreateProductDto != null)
            .OverridePropertyName("Category");
    }
}
```

## Customer Entity Validator

```csharp
public class CreateCustomerCommandValidator : AbstractValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(x => x.CreateCustomerDto)
            .NotNull().WithMessage("Müşteri bilgileri gereklidir")
            .OverridePropertyName("Customer");

        RuleFor(x => x.CreateCustomerDto.FirstName)
            .NotEmpty().WithMessage("İsim boş olamaz")
            .MaximumLength(50).WithMessage("İsim 50 karakteri geçemez")
            .When(x => x.CreateCustomerDto != null)
            .OverridePropertyName("FirstName");

        RuleFor(x => x.CreateCustomerDto.LastName)
            .NotEmpty().WithMessage("Soyisim boş olamaz")
            .MaximumLength(50).WithMessage("Soyisim 50 karakteri geçemez")
            .When(x => x.CreateCustomerDto != null)
            .OverridePropertyName("LastName");

        RuleFor(x => x.CreateCustomerDto.Email)
            .NotEmpty().WithMessage("E-posta boş olamaz")
            .EmailAddress().WithMessage("Geçersiz e-posta formatı")
            .When(x => x.CreateCustomerDto != null)
            .OverridePropertyName("Email");

        RuleFor(x => x.CreateCustomerDto.Phone)
            .Matches(@"^[0-9]{10,15}$").WithMessage("Geçersiz telefon numarası")
            .When(x => x.CreateCustomerDto != null && !string.IsNullOrEmpty(x.CreateCustomerDto.Phone))
            .OverridePropertyName("Phone");
    }
}
```
