# BaseEntity Implementation Plan

**Project:** TemplateDeneme (.NET 9 Clean Architecture)
**Goal:** Add BaseEntity with full audit trail to enable soft delete and audit tracking
**Entity:** SerhanKitap (ApplicationUser already extends IdentityUser, so it won't inherit from BaseEntity)

---

## Overview

The `BaseEntity` class will provide common audit and tracking properties for all domain entities that need:

1. **Identity tracking** - Unique ID as string GUID
2. **Creation audit** - When and by whom a record was created
3. **Update audit** - When and by whom a record was last modified
4. **Soft delete capability** - Mark records as deleted without physical removal
5. **Active status tracking** - Enable/disable records without deletion

---

## BaseEntity Properties

```csharp
public abstract class BaseEntity
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsActive { get; set; } = true;
}
```

### Property Details

| Property      | Type      | Default                     | Description                            |
| ------------- | --------- | --------------------------- | -------------------------------------- |
| `Id`          | string    | `Guid.NewGuid().ToString()` | Unique identifier for the entity       |
| `CreatedDate` | DateTime  | `DateTime.UtcNow`           | Timestamp when entity was created      |
| `CreatedBy`   | string?   | null                        | User ID who created the entity         |
| `UpdatedDate` | DateTime? | null                        | Timestamp when entity was last updated |
| `UpdatedBy`   | string?   | null                        | User ID who last updated the entity    |
| `IsDeleted`   | bool      | false                       | Soft delete flag (true = deleted)      |
| `IsActive`    | bool      | true                        | Active status flag (false = inactive)  |

---

## Step-by-Step Implementation

### Step 1: Create BaseEntity Class

**Tasks:**

- [x] Create `Domain/Common/BaseEntity.cs`
- [x] Add all audit properties with appropriate defaults
- [x] Make class abstract (should not be instantiated directly)

**Commit Message:**

```text
feat: add BaseEntity with full audit trail properties

- Created BaseEntity abstract class in Domain layer
- Added Id (string GUID), CreatedDate, CreatedBy, UpdatedDate, UpdatedBy
- Added IsDeleted (soft delete) and IsActive flags
- Set appropriate default values: CreatedDate=UTC, IsDeleted=false, IsActive=true
```

**Tests:**

- Create `Tests/Domain_Tests/Common/BaseEntityTests.cs`
- Test 1: Verify Id is auto-generated as valid GUID string
- Test 2: Verify CreatedDate defaults to current UTC time
- Test 3: Verify IsDeleted defaults to false
- Test 4: Verify IsActive defaults to true
- Test 5: Verify nullable properties (CreatedBy, UpdatedDate, UpdatedBy) are null by default

**Run:** `dotnet test --filter "FullyQualifiedName~BaseEntityTests"`

---

### Step 2: Update SerhanKitap Entity

**Tasks:**

- [x] Update `Domain/SerhanKitap.cs` to inherit from `BaseEntity`
- [x] Remove duplicate `Id` property (now inherited)
- [x] Keep existing properties: KitapName, KitapYazar, KitapSayfaSayisi
- [x] Create `AutoMapperExtensions` with reusable methods for BaseEntity mapping
- [x] Update `MappingProfiles.cs` to use extension methods

**Before:**

```csharp
public class SerhanKitap
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public required string KitapName { get; set; }
    public required string KitapYazar { get; set; }
    public int KitapSayfaSayisi { get; set; }
}
```

**After:**

```csharp
public class SerhanKitap : BaseEntity
{
    public required string KitapName { get; set; }
    public required string KitapYazar { get; set; }
    public int KitapSayfaSayisi { get; set; }
}
```

**Commit Message:**

```text
refactor: make SerhanKitap inherit from BaseEntity

- SerhanKitap now extends BaseEntity for audit tracking
- Removed duplicate Id property (inherited from BaseEntity)
- Inherits: CreatedDate, CreatedBy, UpdatedDate, UpdatedBy, IsDeleted, IsActive
```

**Tests:**

- Create or update `Tests/Domain_Tests/SerhanKitapTests.cs`
- Test 1: Verify SerhanKitap inherits all BaseEntity properties
- Test 2: Verify KitapName, KitapYazar, KitapSayfaSayisi are still accessible
- Test 3: Verify all audit properties are available on SerhanKitap instance

**Run:** `dotnet test --filter "FullyQualifiedName~SerhanKitapTests"`

---

### Step 3: Create EF Core Migration

**Tasks:**

- [x] Run migration command: `dotnet ef migrations add AddBaseEntityAuditFields -p Persistence -s API`
- [x] Review generated migration in `Persistence/Migrations/`
- [x] Verify migration adds new columns:
  - `CreatedBy` (nullable string)
  - `UpdatedDate` (nullable DateTime)
  - `UpdatedBy` (nullable string)
  - `IsDeleted` (bool, default false)
  - `IsActive` (bool, default true)
- [ ] Verify `CreatedDate` constraint is already present (added in AddIdentitySchema migration)

**Migration Preview:**

```csharp
migrationBuilder.AddColumn<string>(
    name: "CreatedBy",
    table: "SerhanKitaplar",
    type: "text",
    nullable: true);

migrationBuilder.AddColumn<DateTime>(
    name: "UpdatedDate",
    table: "SerhanKitaplar",
    type: "timestamp with time zone",
    nullable: true);

migrationBuilder.AddColumn<string>(
    name: "UpdatedBy",
    table: "SerhanKitaplar",
    type: "text",
    nullable: true);

migrationBuilder.AddColumn<bool>(
    name: "IsDeleted",
    table: "SerhanKitaplar",
    type: "boolean",
    nullable: false,
    defaultValue: false);

migrationBuilder.AddColumn<bool>(
    name: "IsActive",
    table: "SerhanKitaplar",
    type: "boolean",
    nullable: false,
    defaultValue: true);
```

**Commit Message:**

```text
feat: add migration for BaseEntity audit fields to SerhanKitap

- Created migration: AddBaseEntityAuditFields
- Added columns: CreatedBy, UpdatedDate, UpdatedBy, IsDeleted, IsActive
- Set defaults: IsDeleted=false, IsActive=true
- Note: CreatedDate column already exists from previous migration
```

**Tests:**

- Create `Tests/Persistence_Tests/Migrations/AddBaseEntityAuditFieldsTests.cs`
- Test 1: Verify migration can be applied to fresh in-memory database
- Test 2: Verify new columns are created with correct types
- Test 3: Verify default values are correctly applied (IsDeleted=false, IsActive=true)
- Test 4: Verify existing records get default values for new columns

**Run:** `dotnet test --filter "FullyQualifiedName~AddBaseEntityAuditFieldsTests"`

---

### Step 4: Update CreateSerhanKitapCommandHandler

**Tasks:**

- [x] Open `Application/Features/SerhanKitaplar/Commands/CreateSerhanKitap/CreateSerhanKitapCommandHandler.cs`
- [x] Inject `ICurrentUserService` if not already injected
- [x] After creating SerhanKitap, set:
  - `CreatedDate = DateTime.UtcNow` (though it has default, set explicitly for clarity)
  - `CreatedBy = _currentUserService.UserId` (may be null for anonymous users)

**Handler Update:**

```csharp
public class CreateSerhanKitapCommandHandler : IRequestHandler<CreateSerhanKitapCommand, Result<string>>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUserService;

    public CreateSerhanKitapCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }

    public async Task<Result<string>> Handle(CreateSerhanKitapCommand request, CancellationToken cancellationToken)
    {
        var kitap = new SerhanKitap
        {
            KitapName = request.Dto.KitapName,
            KitapYazar = request.Dto.KitapYazar,
            KitapSayfaSayisi = request.Dto.KitapSayfaSayisi,
            // Explicitly set audit fields
            CreatedDate = DateTime.UtcNow,
            CreatedBy = _currentUserService.UserId
        };

        _context.SerhanKitaplar.Add(kitap);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<string>.Success("Kitap başarıyla oluşturuldu.", kitap.Id);
    }
}
```

**Commit Message:**

```text
feat: set CreatedDate and CreatedBy on SerhanKitap creation

- Injected ICurrentUserService into CreateSerhanKitapCommandHandler
- Set CreatedDate to UTC timestamp on entity creation
- Set CreatedBy from current user context (null for unauthenticated)
- Preserves existing behavior for anonymous users
```

**Tests:**

- Update `Tests/Application_Tests/Features/SerhanKitaplar/Commands/CreateSerhanKitap/CreateSerhanKitapCommandHandlerTests.cs`
- Test 1: Verify CreatedDate is set to recent UTC time when creating SerhanKitap
- Test 2: Verify CreatedBy is set to user ID when user is authenticated
- Test 3: Verify CreatedBy is null when no user context (unauthenticated)
- Test 4: Verify successful creation returns the correct Id

**Run:** `dotnet test --filter "FullyQualifiedName~CreateSerhanKitapCommandHandlerTests"`

---

### Step 5: Update EditSerhanKitapCommandHandler

**Tasks:**

- [x] Open `Application/Features/SerhanKitaplar/Commands/EditSerhanKitap/EditSerhanKitapCommandHandler.cs`
- [x] Inject `ICurrentUserService` if not already injected
- [x] Before `SaveChangesAsync`, update:
  - `UpdatedDate = DateTime.UtcNow`
  - `UpdatedBy = _currentUserService.UserId`
- [x] Ensure `CreatedDate` and `CreatedBy` are NOT modified

**Handler Update:**

```csharp
public async Task<Result<string>> Handle(EditSerhanKitapCommand request, CancellationToken cancellationToken)
{
    var kitap = await _context.SerhanKitaplar.FindAsync(request.Id);

    if (kitap == null)
        return Result<string>.Failure("Kitap bulunamadı.", "404");

    kitap.KitapName = request.Dto.KitapName;
    kitap.KitapYazar = request.Dto.KitapYazar;
    kitap.KitapSayfaSayisi = request.Dto.KitapSayfaSayisi;

    // Update audit fields
    kitap.UpdatedDate = DateTime.UtcNow;
    kitap.UpdatedBy = _currentUserService.UserId;

    await _context.SaveChangesAsync(cancellationToken);

    return Result<string>.Success("Kitap başarıyla güncellendi.", kitap.Id);
}
```

**Commit Message:**

```text
feat: set UpdatedDate and UpdatedBy on SerhanKitap update

- Injected ICurrentUserService into EditSerhanKitapCommandHandler
- Set UpdatedDate to UTC timestamp on entity update
- Set UpdatedBy from current user context (null for unauthenticated)
- Preserves original CreatedDate and CreatedBy values
```

**Tests:**

- Update `Tests/Application_Tests/Features/SerhanKitaplar/Commands/EditSerhanKitap/EditSerhanKitapCommandHandlerTests.cs`
- Test 1: Verify UpdatedDate is set to recent UTC time when updating SerhanKitap
- Test 2: Verify UpdatedBy is set to user ID when user is authenticated
- Test 3: Verify UpdatedBy is null when no user context (unauthenticated)
- Test 4: Verify CreatedDate remains unchanged after update
- Test 5: Verify CreatedBy remains unchanged after update
- Test 6: Verify multiple updates correctly update UpdatedDate (latest value)

**Run:** `dotnet test --filter "FullyQualifiedName~EditSerhanKitapCommandHandlerTests"`

---

### Step 6: Update DeleteSerhanKitapCommandHandler (Soft Delete)

**Tasks:**

- [x] Open `Application/Features/SerhanKitaplar/Commands/DeleteSerhanKitap/DeleteSerhanKitapCommandHandler.cs`
- [x] Inject `ICurrentUserService` if not already injected
- [x] Replace `_context.Remove(kitap)` with soft delete:
  - Set `IsDeleted = true`
  - Set `UpdatedDate = DateTime.UtcNow`
  - Set `UpdatedBy = _currentUserService.UserId`
- [x] Keep `SaveChangesAsync` call

**Handler Update:**

```csharp
public async Task<Result<string>> Handle(DeleteSerhanKitapCommand request, CancellationToken cancellationToken)
{
    var kitap = await _context.SerhanKitaplar.FindAsync(request.Id);

    if (kitap == null)
        return Result<string>.Failure("Kitap bulunamadı.", "404");

    // Soft delete instead of physical removal
    kitap.IsDeleted = true;
    kitap.UpdatedDate = DateTime.UtcNow;
    kitap.UpdatedBy = _currentUserService.UserId;

    await _context.SaveChangesAsync(cancellationToken);

    return Result<string>.Success("Kitap başarıyla silindi.", kitap.Id);
}
```

**Commit Message:**

```text
feat: implement soft delete for SerhanKitap using IsDeleted flag

- Changed DeleteSerhanKitapCommandHandler to use soft delete
- Set IsDeleted=true instead of removing entity from database
- Set UpdatedDate and UpdatedBy on deletion for audit trail
- Preserves data integrity and allows recovery if needed
```

**Tests:**

- Update `Tests/Application_Tests/Features/SerhanKitaplar/Commands/DeleteSerhanKitap/DeleteSerhanKitapCommandHandlerTests.cs`
- Test 1: Verify entity is NOT physically removed from database after delete
- Test 2: Verify IsDeleted is set to true after delete operation
- Test 3: Verify UpdatedDate is set on soft delete
- Test 4: Verify UpdatedBy is set when user is authenticated
- Test 5: Verify existing CreatedDate and CreatedBy are preserved
- Test 6: Verify deleting already-deleted entity returns success (idempotent)

**Run:** `dotnet test --filter "FullyQualifiedName~DeleteSerhanKitapCommandHandlerTests"`

---

### Step 7: Update GetSerhanKitapListQueryHandler

**Tasks:**

- [x] Open `Application/Features/SerhanKitaplar/Queries/GetSerhanKitapList/GetSerhanKitapListQueryHandler.cs`
- [x] Add `.Where(x => !x.IsDeleted)` filter to the query
- [x] Optionally add `.Where(x => x.IsActive)` filter if active status should be checked

**Handler Update:**

```csharp
var query = _context.SerhanKitaplar
    .Where(x => !x.IsDeleted);  // Exclude soft-deleted records

// Apply filters if needed
if (!string.IsNullOrWhiteSpace(request.SearchTerm))
{
    query = query.Where(x =>
        x.KitapName.Contains(request.SearchTerm) ||
        x.KitapYazar.Contains(request.SearchTerm));
}

// ... rest of the query
```

**Commit Message:**

```text
feat: filter out soft-deleted SerhanKitap entities in list query

- Updated GetSerhanKitapListQueryHandler to exclude IsDeleted=true records
- Added Where(x => !x.IsDeleted) filter to base query
- Soft-deleted entities no longer appear in list results
```

**Tests:**

- Update `Tests/Application_Tests/Features/SerhanKitaplar/Queries/GetSerhanKitapList/GetSerhanKitapListQueryHandlerTests.cs`
- Test 1: Verify soft-deleted items are excluded from list results
- Test 2: Verify non-deleted items are still included in list
- Test 3: Verify totalCount excludes soft-deleted items
- Test 4: Verify pagination works correctly when some items are deleted
- Test 5: Verify filter/search still works correctly with soft delete filter

**Run:** `dotnet test --filter "FullyQualifiedName~GetSerhanKitapListQueryHandlerTests"`

---

### Step 8: Update GetSerhanKitapDetailsQueryHandler

**Tasks:**

- [ ] Open `Application/Features/SerhanKitaplar/Queries/GetSerhanKitapDetails/GetSerhanKitapDetailsQueryHandler.cs`
- [ ] After finding entity, check if `IsDeleted` is true
- [ ] Check if `IsActive` is false
- [ ] If deleted or inactive, return failure response
- [ ] If not deleted and active, proceed with normal flow

**Handler Update:**

```csharp
public async Task<Result<GetSerhanKitapDetailsDto>> Handle(GetSerhanKitapDetailsQuery request, CancellationToken cancellationToken)
{
    var kitap = await _context.SerhanKitaplar.FindAsync(request.Id);

    if (kitap == null)
        return Result<GetSerhanKitapDetailsDto>.Failure("Kitap bulunamadı.", "404");

    if (kitap.IsDeleted)
        return Result<GetSerhanKitapDetailsDto>.Failure("Kitap silinmiş.", "410");

    if (!kitap.IsActive)
        return Result<GetSerhanKitapDetailsDto>.Failure("Kitap aktif değil.", "423");

    // Mapping logic continues...
}
```

**Commit Message:**

```text
feat: return failure when requesting soft-deleted or inactive SerhanKitap details

- Updated GetSerhanKitapDetailsQueryHandler to check IsDeleted and IsActive flags
- Returns failure (410 Gone) when requesting deleted entity details
- Returns failure (423 Locked) when requesting inactive entity details
- Prevents access to soft-deleted or inactive entity data
```

**Tests:**

- Update `Tests/Application_Tests/Features/SerhanKitaplar/Queries/GetSerhanKitapDetails/GetSerhanKitapDetailsQueryHandlerTests.cs`
- Test 1: Verify returns failure when requesting soft-deleted entity
- Test 2: Verify failure has correct message and status code (410) for deleted
- Test 3: Verify returns failure when requesting inactive entity
- Test 4: Verify failure has correct message and status code (423) for inactive
- Test 5: Verify returns success for active and non-deleted entity
- Test 6: Verify returns 404 for non-existent entity (not deleted)
- Test 7: Verify failure status codes are different for deleted (410), inactive (423), and not found (404)

**Run:** `dotnet test --filter "FullyQualifiedName~GetSerhanKitapDetailsQueryHandlerTests"`

---

### Step 9: Update MappingProfiles

**Tasks:**

- [ ] Open `Application/Core/MappingProfiles.cs`
- [ ] Check if `GetSerhanKitapDetailsDto` or other DTOs need audit fields
- [ ] Add mappings for audit fields if they should be included in DTOs
- [ ] Create separate DTOs (e.g., `GetSerhanKitapWithAuditDto`) if audit fields should be exposed separately

**Example Mapping Update:**

```csharp
// If audit fields should be exposed in details DTO
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Existing mappings...

        // SerhanKitap mappings
        CreateMap<SerhanKitap, GetSerhanKitapDetailsDto>()
            .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => src.CreatedDate))
            .ForMember(dest => dest.CreatedBy, opt => opt.MapFrom(src => src.CreatedBy))
            .ForMember(dest => dest.UpdatedDate, opt => opt.MapFrom(src => src.UpdatedDate))
            .ForMember(dest => dest.UpdatedBy, opt => opt.MapFrom(src => src.UpdatedBy))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive));
    }
}
```

**Commit Message:**

```text
feat: add audit field mappings in AutoMapper profiles

- Added audit field mappings in MappingProfiles
- Maps CreatedDate, CreatedBy, UpdatedDate, UpdatedBy, IsDeleted, IsActive
- Audit fields now available in DTOs for API responses
```

**Tests:**

- Update `Tests/Application_Tests/Core/MappingProfilesTests.cs`
- Test 1: Verify audit fields are mapped correctly from entity to DTO
- Test 2: Verify nullable fields (CreatedBy, UpdatedDate, UpdatedBy) map correctly when null
- Test 3: Verify audit fields map correctly when they have values
- Test 4: Verify IsDeleted and IsActive flags map correctly
- Test 5: Verify existing non-audit fields still map correctly

**Run:** `dotnet test --filter "FullyQualifiedName~MappingProfilesTests"`

---

### Step 10: Run Full Test Suite

**Tasks:**

- [ ] Run all tests to verify implementation: `dotnet test --verbosity detailed`
- [ ] Check for any failing tests
- [ ] Fix any issues found
- [ ] Run again to ensure all tests pass

**Commit Message:**

```text
test: verify all tests pass after BaseEntity implementation

- Ran full test suite with detailed output
- All tests passing successfully
- BaseEntity audit trail implementation complete
- No regressions in existing functionality
```

### Unit Test Coverage by Step

| Step | Test File                                   | Test Count | Key Scenarios                        |
| ---- | ------------------------------------------- | ---------- | ------------------------------------ |
| 1    | `Common/BaseEntityTests.cs`                 | 5          | Property initialization, defaults    |
| 2    | `SerhanKitapTests.cs`                       | 3          | Inheritance, property access         |
| 3    | `AddBaseEntityAuditFieldsTests.cs`          | 4          | Migration, column creation, defaults |
| 4    | `CreateSerhanKitapCommandHandlerTests.cs`   | 4          | Creation audit, user context         |
| 5    | `EditSerhanKitapCommandHandlerTests.cs`     | 6          | Update audit, preserve created       |
| 6    | `DeleteSerhanKitapCommandHandlerTests.cs`   | 6          | Soft delete behavior                 |
| 7    | `GetSerhanKitapListQueryHandlerTests.cs`    | 5          | Filter soft-deleted, pagination      |
| 8    | `GetSerhanKitapDetailsQueryHandlerTests.cs` | 5          | Deleted entity handling              |
| 9    | `MappingProfilesTests.cs`                   | 5          | Audit field mappings                 |
| 10   | Full Suite                                  | All        | Regression testing field mappings    |
| 10   | Full Suite                                  | All        | Regression testing                   |

**Total New/Updated Tests:** ~43 tests

---

## Important Considerations

### Why ApplicationUser Won't Inherit from BaseEntity

1. **Already extends IdentityUser**: `ApplicationUser` inherits from `IdentityUser`, which has its own `Id` property (string)
2. **Different Id type**: IdentityUser uses string GUID but with different generation logic
3. **Partial overlap**: ApplicationUser already has `CreatedDate`, adding other audit fields would require migration on Identity tables
4. **Design decision**: Keeping Identity tables separate from business entities is a common pattern

### Soft Delete vs Hard Delete

- **Soft Delete Benefits**:
  - Preserves data integrity
  - Allows recovery
  - Maintains audit trail
  - No data loss

- **Hard Delete** (not implemented):
  - Physical removal from database
  - Cannot recover
  - Clean database but loses history

### Authentication Context Behavior

- **Authenticated endpoints** (`[Authorize]`): `ICurrentUserService.UserId` will be set
- **Anonymous endpoints**: `ICurrentService.UserId` will be null
- **Result**: CreatedBy/UpdatedBy will be null for unauthenticated operations

### DateTime Consistency

- All timestamps use `DateTime.UtcNow`
- Avoids timezone issues
- Consistent across application layers

---

## Migration Strategy for Production

If deploying to production with existing data:

1. **Backup database** before migration
2. **Migration sets defaults**:
   - `IsDeleted = false` (existing records not deleted)
   - `IsActive = true` (existing records active)
   - `CreatedBy = null` (no user history available)
   - `UpdatedDate = null` (no update history available)
   - `UpdatedBy = null` (no user history available)
3. **Existing records remain functional** with minimal impact
4. **New operations** will populate audit fields correctly

---

## Future Enhancements

After completing this implementation, consider:

1. **Global Query Filter**: Add `.Where(x => !x.IsDeleted)` as global filter in `AppDbContext`
2. **Recovery Endpoint**: Add endpoint to restore soft-deleted entities
3. **Audit Log Entity**: Create separate entity for full audit trail history
4. **Batch Operations**: Soft delete multiple entities at once
5. **Expiration**: Add `DeletedDate` to track when soft delete occurred

---

## Completion Checklist

- [x] Step 1: BaseEntity created with tests ✓
- [x] Step 2: SerhanKitap inherits from BaseEntity ✓
- [x] Step 3: EF Core migration created ✓
- [x] Step 4: Create handler sets CreatedDate/CreatedBy ✓
- [x] Step 5: Edit handler sets UpdatedDate/UpdatedBy ✓
- [x] Step 6: Delete handler uses soft delete ✓
- [x] Step 7: List handler filters deleted entities ✓
- [ ] Step 8: Details handler rejects deleted entities
- [ ] Step 9: AutoMapper profiles updated
- [ ] Step 10: All tests passing

**Total Estimated Time:** ~2-3 hours (including testing)
**Total Commits:** 10
**Total Tests:** ~43 new/updated tests

---

## References

- Clean Architecture pattern in project
- CQRS with MediatR
- Entity Framework Core migrations
- ASP.NET Core Identity (for ApplicationUser)
- AutoMapper profiles
- FluentAssertions for tests
