# UniLostItem

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=SerhanBaymaz_UniLostItem&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=SerhanBaymaz_UniLostItem)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=SerhanBaymaz_UniLostItem&metric=bugs)](https://sonarcloud.io/summary/new_code?id=SerhanBaymaz_UniLostItem)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=SerhanBaymaz_UniLostItem&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=SerhanBaymaz_UniLostItem)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=SerhanBaymaz_UniLostItem&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=SerhanBaymaz_UniLostItem)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=SerhanBaymaz_UniLostItem&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=SerhanBaymaz_UniLostItem)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=SerhanBaymaz_UniLostItem&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=SerhanBaymaz_UniLostItem)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=SerhanBaymaz_UniLostItem&metric=coverage)](https://sonarcloud.io/summary/new_code?id=SerhanBaymaz_UniLostItem)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=SerhanBaymaz_UniLostItem&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=SerhanBaymaz_UniLostItem)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=SerhanBaymaz_UniLostItem&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=SerhanBaymaz_UniLostItem)

A Lost & Found platform for university campuses. Built with .NET 9 Clean Architecture and CQRS.

## Features

- **LostItems** — Create, update, soft-delete lost/found item postings with filtering, sorting, and pagination
- **ItemClaims** — Claim workflow: create → approve/reject → item Resolved, deadline extension (max 2x), Admin escalation
- **Auth** — JWT Bearer, register/login, refresh token, profile management (ASP.NET Core Identity)

## Tech Stack

| Layer          | Technology                                           |
| -------------- | ---------------------------------------------------- |
| API            | ASP.NET Core 9, Swagger, Health Checks, Seq logging  |
| Application    | MediatR (CQRS), FluentValidation, AutoMapper 16      |
| Persistence    | EF Core 9, PostgreSQL (Npgsql)                       |
| Infrastructure | JWT Bearer, ICurrentUserService                      |
| Domain         | BaseEntity audit trail, enums, navigation properties |
| Tests          | xUnit, FluentAssertions, Moq, SQLite in-memory (391) |
| CI/CD          | GitHub Actions, Docker, GHCR, SonarCloud             |

## Project Structure

```text
UniLostItem/
├── API/                          # Controllers, Middleware, Extensions
│   └── Controllers/
│       ├── AuthController.cs             # /api/v1/auth
│       ├── LostItemsController.cs        # /api/v1/items
│       └── ItemClaimsController.cs       # /api/v1/claims
├── Application/
│   ├── Core/                    # Result<T>, ValidationBehavior, MappingProfiles
│   └── Features/
│       ├── Auth/                # Login, Register, RefreshToken, Profile
│       ├── LostItems/           # Commands (CRUD) + Queries (List, Details, MyItems)
│       ├── ItemClaims/          # Commands (Create, Cancel, Respond, Extend, AdminReview)
│       │                       # Queries (ByItem, MyClaims, Details, Pending)
├── Domain/
│   ├── ApplicationUser.cs       # IdentityUser (does not inherit BaseEntity)
│   ├── LostItem.cs              # BaseEntity → Title, Description, Category, ItemType, Status, Location
│   ├── ItemClaim.cs             # BaseEntity → Description, Status, ExpiresAt, ExtensionCount
│   └── Common/Enums/            # ItemType, ItemStatus, ItemCategory, ClaimStatus
├── Infrastructure/              # JwtService, CurrentUserService
├── Persistence/                 # AppDbContext, IAppDbContext, Migrations
└── Tests/                       # 554 unit tests (SQLite in-memory)
```

## API Endpoints

### Auth — `/api/v1/auth`

| Method | Endpoint         | Auth          | Description          |
| ------ | ---------------- | ------------- | -------------------- |
| POST   | `/register`      | Anonymous     | Register             |
| POST   | `/login`         | Anonymous     | Login                |
| POST   | `/refresh-token` | Anonymous     | Refresh access token |
| GET    | `/profile`       | `[Authorize]` | Get current profile  |
| PUT    | `/profile`       | `[Authorize]` | Update profile       |

### LostItems — `/api/v1/items`

| Method | Endpoint    | Auth                  | Description                             |
| ------ | ----------- | --------------------- | --------------------------------------- |
| GET    | `/`         | Anonymous             | List active items (paginated, filtered) |
| GET    | `/{id}`     | Anonymous             | Item details                            |
| POST   | `/`         | `[Authorize]`         | Create item                             |
| PUT    | `/{id}`     | `[Authorize]` (owner) | Update item                             |
| DELETE | `/{id}`     | `[Authorize]` (owner) | Soft delete                             |
| GET    | `/my-items` | `[Authorize]`         | List own items                          |

**Filters (GET `/`):** category, itemType, searchTerm, location, sortBy (Title/Category/IncidentDate/CreatedDate), sortDescending, pageNumber, pageSize

### ItemClaims — `/api/v1/claims`

| Method | Endpoint                | Auth                           | Description                           |
| ------ | ----------------------- | ------------------------------ | ------------------------------------- |
| POST   | `/`                     | `[Authorize]`                  | Create claim                          |
| GET    | `/{id}`                 | `[Authorize]` (claimant/owner) | Claim details                         |
| GET    | `/my-claims`            | `[Authorize]`                  | List my claims                        |
| PUT    | `/{id}/cancel`          | `[Authorize]` (claimant)       | Cancel claim                          |
| GET    | `/by-item/{lostItemId}` | `[Authorize]` (item owner)     | List claims for an item               |
| PUT    | `/{id}/respond`         | `[Authorize]` (item owner)     | Approve or reject claim               |
| PUT    | `/{id}/extend`          | `[Authorize]` (item owner)     | Extend deadline (+2 days, max 2x)     |
| PUT    | `/{id}/admin-review`    | `[Authorize(Roles="Admin")]`   | Admin approve/reject                  |
| GET    | `/pending`              | `[Authorize(Roles="Admin")]`   | List pending claims (Admin dashboard) |

### Claim Flow

```
Create Claim → Pending (ExpiresAt = +2 days)
  ├─ Owner APPROVES   → ApprovedByOwner, Item Resolved
  ├─ Owner REJECTS    → RejectedByOwner
  ├─ Owner EXTENDS    → +2 days (max 2 extensions = 6 days total)
  ├─ Admin REVIEWS    → ApprovedByAdmin / RejectedByAdmin
  └─ Claimant CANCELS → Cancelled
```

## Setup

### Docker Compose (Recommended)

```bash
git clone <repo-url> && cd UniLostItem
cp example.dev.env dev.env          # edit connection string & JWT config
docker-compose -f docker-compose.dev.yml up -d
# Swagger: http://localhost:8089/swagger
# Seq:     http://localhost:8088
```

### Manual

```bash
dotnet restore
cp example.dev.env dev.env          # update connection string
dotnet ef database update -p Persistence -s API
dotnet run --project API
```

## Development

```bash
dotnet build UniLostItem.sln                     # Build
dotnet test                                     # Run 391 tests
dotnet ef migrations add <Name> -p Persistence -s API   # Add migration
dotnet ef database update -p Persistence -s API         # Apply migration
```

## Architecture

**Clean Architecture** — API → Application → Persistence → Domain ← Infrastructure

- **CQRS:** MediatR command/query separation
- **Result Pattern:** `Result<T>.Success(message, value)` / `Result<T>.Failure(message, code)`
- **Soft Delete:** `IsDeleted = true`, no physical removal
- **Ownership:** Update/Delete checks `entity.UserId != currentUserId` → 403
- **BaseEntity:** Id, CreatedDate/By, UpdatedDate/By, IsDeleted, IsActive
- **Query Projection:** `.Select()` for computed fields (UserFullName, ClaimCount), not AutoMapper
- **AutoMapper:** Only for Create DTO → Entity mapping

## CI/CD

| Pipeline | Trigger                    | Output                               |
| -------- | -------------------------- | ------------------------------------ |
| ci-dev   | push to main/develop, PR   | Build → Test → Sonar → Docker → GHCR |
| ci-prod  | push to main, version tags | Build → Test → Docker → GHCR         |

## License

MIT
