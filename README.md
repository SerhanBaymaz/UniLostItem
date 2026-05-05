# UniLostItem - .NET 9 Clean Architecture Projesi

Bu proje, **Clean Architecture** prensiplerine uygun olarak geliştirilmiş bir .NET 9 Web API uygulamasıdır. Proje, **CQRS pattern**, **MediatR**, **Entity Framework Core** ve modern .NET teknolojilerini kullanarak ölçeklenebilir ve sürdürülebilir bir yapı sunmaktadır.

## 📋 İçindekiler

- [Proje Yapısı](#proje-yapısı)
- [Kullanılan Teknolojiler](#kullanılan-teknolojiler)
- [Kullanılan Kütüphaneler](#kullanılan-kütüphaneler)
- [Kurulum](#kurulum)
- [Kullanım](#kullanım)
- [Mimari](#mimari)

## Proje Yapısı

Proje, Clean Architecture prensiplerine göre 4 katmana ayrılmıştır; ayrıca testler için bir `Tests/` proje dizini bulunmaktadır.

```text
UniLostItem/
├── API/                      # Presentation Layer - Web API
│   ├── Controllers/          # API Controller'ları
│   │   ├── BaseApiController.cs
│   │   ├── AuthController.cs
│   │   └── SerhanKitaplarController.cs
│   ├── Extensions/           # Extension Methods - Program.cs yapılandırması
│   │   ├── ApiExtensions.cs           # Controller & ModelState config
│   │   ├── ApplicationExtensions.cs   # MediatR, AutoMapper, Validation
│   │   ├── ConfigurationExtensions.cs # .env dosyası yönetimi
│   │   ├── DatabaseExtensions.cs      # DbContext & Migration
│   │   ├── LoggingExtensions.cs       # Serilog configuration
│   │   └── SwaggerExtensions.cs       # Swagger/OpenAPI docs
│   ├── Middleware/           # Custom middleware'ler
│   │   └── ExceptionMiddleware.cs
│   ├── Responses/            # API response modelleri
│   │   ├── AppProblemDetails.cs
│   │   └── StandardApiResponse.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.Development.json
│   └── Program.cs            # Uygulama başlangıç noktası (Extensions method ile Temiz ve okunabilir yapılandırma)
├── Application/              # Application Layer - İş mantığı
│   ├── Core/                 # Ortak yapılar (Result, Validation, Mapping)
│   │   ├── MappingProfiles.cs       # AutoMapper profilleri
│   │   ├── Result.cs                # Result pattern implementasyonu
│   │   └── ValidationBehavior.cs    # MediatR validation pipeline
│   └── Features/
│       ├── Auth/             # Authentication & Identity module
│       │   ├── Commands/
│       │   │   ├── Login/
│       │   │   │   ├── LoginCommand.cs
│       │   │   │   ├── LoginCommandHandler.cs
│       │   │   │   ├── LoginCommandValidator.cs
│       │   │   │   └── LoginDto.cs
│       │   │   ├── Register/
│       │   │   │   ├── RegisterCommand.cs
│       │   │   │   ├── RegisterCommandHandler.cs
│       │   │   │   ├── RegisterCommandValidator.cs
│       │   │   │   └── RegisterDto.cs
│       │   │   ├── RefreshToken/
│       │   │   │   ├── RefreshTokenCommand.cs
│       │   │   │   ├── RefreshTokenCommandHandler.cs
│       │   │   │   ├── RefreshTokenCommandValidator.cs
│       │   │   │   └── RefreshTokenDto.cs
│       │   │   └── UpdateUserProfile/
│       │   │       ├── UpdateUserProfileCommand.cs
│       │   │       ├── UpdateUserProfileCommandHandler.cs
│       │   │       ├── UpdateUserProfileCommandValidator.cs
│       │   │       └── UpdateUserProfileDto.cs
│       │   ├── Queries/
│       │   │   └── GetCurrentUser/
│       │   │       ├── GetCurrentUserQuery.cs
│       │   │       ├── GetCurrentUserQueryHandler.cs
│       │   │       └── CurrentUserDto.cs
│       │   └── Common/
│       │       └── DTOs/
│       │           └── UserDto.cs
│       └── SerhanKitaplar/    # SerhanKitap feature modülü
│           ├── Commands/
│           │   ├── CreateSerhanKitap/
│           │   │   ├── CreateSerhanKitapCommand.cs
│           │   │   ├── CreateSerhanKitapCommandHandler.cs
│           │   │   ├── CreateSerhanKitapCommandValidator.cs
│           │   │   └── CreateSerhanKitapDto.cs
│           │   ├── EditSerhanKitap/
│           │   │   ├── EditSerhanKitapCommand.cs
│           │   │   ├── EditSerhanKitapCommandHandler.cs
│           │   │   ├── EditSerhanKitapCommandValidator.cs
│           │   │   └── EditSerhanKitapDto.cs
│           │   └── DeleteSerhanKitap/
│           │       ├── DeleteSerhanKitapCommand.cs
│           │       └── DeleteSerhanKitapCommandHandler.cs
│           └── Queries/
│               ├── Common/
│               │   ├── DTOs/
│               │   │   └── GetSerhanKitapDto.cs
│               │   └── Enums/
│               │       └── KitapSortField.cs
│               ├── GetSerhanKitapList/
│               │   ├── GetSerhanKitapListQuery.cs
│               │   ├── GetSerhanKitapListQueryHandler.cs
│               │   └── GetSerhanKitapListValidator.cs
│               └── GetSerhanKitapDetails/
│                   ├── GetSerhanKitapDetailsQuery.cs
│                   └── GetSerhanKitapDetailsQueryHandler.cs
├── Domain/                   # Domain Layer - Domain modelleri
│   ├── Common/               # Ortak domain sınıfları
│   │   └── BaseEntity.cs     # Base entity (audit trail: Id, CreatedDate, CreatedBy, UpdatedDate, UpdatedBy, IsDeleted, IsActive)
│   ├── ApplicationUser.cs    # Identity user entity (extended, BaseEntity miras almaz)
│   └── SerhanKitap.cs        # Domain entity (BaseEntity miras alır)
├── Infrastructure/           # Infrastructure Layer - Dış servisler (JWT, Security)
│   ├── Security/             # Güvenlik implementasyonları
│   │   └── JwtService.cs
│   ├── Services/             # Infrastructure servisleri
│   │   └── CurrentUserService.cs
│   └── InfrastructureServiceExtensions.cs
├── Persistence/              # Persistence Layer - Data erişimi
│   ├── AppDbContext.cs       # Entity Framework DbContext
│   ├── IAppDbContext.cs      # DbContext interface
│   ├── DbInitializer.cs      # Veritabanı seed data
│   └── Migrations/           # EF Core migration'ları
└── Tests/                    # Unit and integration tests project
    ├── API_Tests/            # API layer tests
    │   ├── Controllers/
    │   │   ├── AuthControllerTests.cs
    │   │   ├── BaseApiControllerTests.cs
    │   │   ├── SerhanKitaplarControllerTests.cs
    │   │   └── SerhanKitaplarControllerAuthorizationTests.cs
    │   ├── Extensions/        # Extension method tests
    │   ├── Helpers/           # Helper class tests
    │   ├── Middleware/        # Middleware tests
    │   └── Responses/         # Response model tests
    ├── Application_Tests/    # Application layer tests
    │   └── Features/
    │       ├── Auth/         # Auth feature tests
    │       └── SerhanKitaplar/ # SerhanKitap feature tests
    │           ├── Commands/
    │           │   ├── CreateSerhanKitap/
    │           │   │   ├── CreateSerhanKitapCommandHandlerTests.cs
    │           │   │   └── CreateSerhanKitapCommandValidatorTests.cs
    │           │   ├── EditSerhanKitap/
    │           │   │   ├── EditSerhanKitapCommandHandlerTests.cs
    │           │   │   └── EditSerhanKitapCommandValidatorTests.cs
    │           │   └── DeleteSerhanKitap/
    │           │       └── DeleteSerhanKitapCommandHandlerTests.cs
    │           └── Queries/
    │               ├── GetSerhanKitapList/
    │               │   ├── GetSerhanKitapListQueryHandlerTests.cs
    │               │   └── GetSerhanKitapListValidatorTests.cs
    │               └── GetSerhanKitapDetails/
    │                   └── GetSerhanKitapDetailsQueryHandlerTests.cs
    └── Domain_Tests/         # Domain layer tests
```

## Kullanılan Teknolojiler

- **.NET 9.0** - .NET framework versiyonu
- **ASP.NET Core Web API** - RESTful API geliştirme
- **ASP.NET Core Identity** - Kullanıcı kimlik doğrulama ve yönetimi
- **Entity Framework Core 9.0** - ORM ve veritabanı işlemleri
- **PostgreSQL** - Güçlü, açık kaynaklı ilişkisel veritabanı
- **Docker & Docker Compose** - Container orchestration ve deployment
- **Seq** - Structured logging ve log yönetimi
- **C# 13** - Modern C# özellikleri ile geliştirme
- **GitHub Actions** - CI/CD pipeline automation
- **SonarCloud** - Code quality and security analysis
- **GitHub Container Registry (GHCR)** - Docker image hosting

## Kullanılan Kütüphaneler

### API Katmanı

| Kütüphane                                         | Versiyon | Açıklama                                    |
| ------------------------------------------------- | -------- | ------------------------------------------- |
| **Swashbuckle.AspNetCore**                        | 6.5.0    | Swagger/OpenAPI dokümantasyonu              |
| **DotNetEnv**                                     | 3.1.1    | Loads environment variables from .env files |
| **Microsoft.EntityFrameworkCore.Design**          | 9.0.0    | EF Core design-time araçları                |
| **Ben.Demystifier**                               | 0.4.1    | Gelişmiş exception stack trace formatlaması |
| **Serilog**                                       | 4.3.0    | Structured logging and enrichment           |
| **Serilog.Sinks.Seq**                             | 9.0.0    | Seq sink for centralized structured logging |
| **AspNetCore.HealthChecks.NpgSql**                | 9.0.0    | PostgreSQL health check                     |
| **AspNetCore.HealthChecks.Network**               | 9.0.0    | Network (TCP) health check                  |
| **AspNetCore.HealthChecks.UI.Client**             | 9.0.0    | Health check UI response writer             |
| **AspNetCore.HealthChecks.Uris**                  | 9.0.0    | URI health check                            |
| **Microsoft.AspNetCore.Authentication.JwtBearer** | 9.0.0    | JWT bearer authentication                   |
| **Microsoft.Extensions.Diagnostics.HealthChecks** | 9.0.9    | Health checks abstraction                   |

### Application Katmanı

| Kütüphane                                          | Versiyon | Açıklama                                |
| -------------------------------------------------- | -------- | --------------------------------------- |
| **MediatR**                                        | 12.4.1   | CQRS pattern implementasyonu            |
| **AutoMapper**                                     | 16.1.1   | Object-to-object mapping                |
| **FluentValidation.DependencyInjectionExtensions** | 11.11.0  | Validation kuralları ve DI entegrasyonu |
| **System.IdentityModel.Tokens.Jwt**                | 8.15.0   | JWT token types ve claims               |

### Infrastructure Katmanı

| Kütüphane                                           | Versiyon | Açıklama                       |
| --------------------------------------------------- | -------- | ------------------------------ |
| **System.IdentityModel.Tokens.Jwt**                 | 8.15.0   | JWT oluşturma ve doğrulama     |
| **Microsoft.Extensions.Configuration.Abstractions** | 10.0.1   | Konfigürasyon okuma arayüzleri |

### Persistence Katmanı

| Kütüphane                                             | Versiyon | Açıklama                           |
| ----------------------------------------------------- | -------- | ---------------------------------- |
| **Npgsql.EntityFrameworkCore.PostgreSQL**             | 9.0.4    | PostgreSQL veritabanı sağlayıcısı  |
| **Microsoft.AspNetCore.Identity.EntityFrameworkCore** | 9.0.0    | ASP.NET Core Identity entegrasyonu |

### Domain Katmanı

| Kütüphane                                | Versiyon | Açıklama                       |
| ---------------------------------------- | -------- | ------------------------------ |
| **Microsoft.Extensions.Identity.Stores** | 10.0.1   | Identity entity tanımları için |

### Tests Katmanı

| Kütüphane                                  | Versiyon | Açıklama                       |
| ------------------------------------------ | -------- | ------------------------------ |
| **xUnit**                                  | 2.9.2    | Test framework                 |
| **xunit.runner.visualstudio**              | 2.8.2    | Visual Studio test runner      |
| **FluentAssertions**                       | 8.8.0    | Readable test assertions       |
| **Moq**                                    | 4.20.72  | Mocking framework              |
| **Microsoft.EntityFrameworkCore.InMemory** | 9.0.0    | In-memory database for testing |
| **Microsoft.AspNetCore.Mvc.Testing**       | 9.0.0    | Integration test helpers       |
| **coverlet.collector**                     | 6.0.2    | Code coverage collection       |
| **coverlet.msbuild**                       | 6.0.2    | Code coverage MSBuild task     |
| **Microsoft.NET.Test.Sdk**                 | 17.12.0  | .NET Test SDK                  |

### Code Quality & Analyzers (Directory.Build.props)

| Kütüphane                               | Versiyon       | Açıklama                                               |
| --------------------------------------- | -------------- | ------------------------------------------------------ |
| **Microsoft.CodeAnalysis.NetAnalyzers** | 9.0.0          | .NET kod analizi ve best practice kuralları            |
| **SonarAnalyzer.CSharp**                | 10.16.1.129956 | SonarQube/SonarCloud kod kalitesi analizi              |
| **SonarLint**                           | -              | IDE-level static analysis (recommended for developers) |

Bu projede ayrıca SonarQube ile merkezi kod kalite taramaları entegre edilebilir; `SonarAnalyzer.CSharp` sunucu/CI analizleri için yapılandırılmıştır.

> **Not:** `TreatWarningsAsErrors` özelliği aktiftir - tüm uyarılar hata olarak kabul edilir.

## Kurulum

### Gereksinimler

- .NET 9.0 SDK
- Docker & Docker Compose
- Visual Studio 2022 / Visual Studio Code / JetBrains Rider (opsiyonel)

### Docker Compose ile Kurulum (Önerilen)

#### Development Ortamı

1. **Projeyi klonlayın:**

```bash
git clone <repository-url>
cd UniLostItem
```

1. **Environment dosyasını oluşturun:**

```bash
cp example.dev.env dev.env
```

> **Not:** `dev.env` dosyasını ihtiyaçlarınıza göre düzenleyebilirsiniz.

1. **Docker Compose ile servisleri başlatın:**

```bash
docker-compose -f docker-compose.dev.yml up -d
```

Bu komut şu servisleri başlatır:

- **PostgreSQL** (port: 5432) - Veritabanı
- **Seq** (port: 8088 UI, 5348 ingestion) - Log yönetimi
- **API** (port: 8089 HTTP, 5009 Debug) - .NET API

1. **Servislerin durumunu kontrol edin:**

```bash
docker-compose -f docker-compose.dev.yml ps
```

1. **API'ye erişin:**

- Swagger UI: `http://localhost:8089/swagger`
- Seq UI: `http://localhost:8088` (Kullanıcı: `admindev`, Şifre: `admindev`)

1. **Servisleri durdurmak için:**

```bash
docker-compose -f docker-compose.dev.yml down
```

#### Production Ortamı

1. **Production environment dosyasını oluşturun:**

```bash
cp example.prod.env prod.env
```

> **ÖNEMLİ:** `prod.env` dosyasındaki şifreleri mutlaka değiştirin!

1. **Production servisleri başlatın:**

```bash
docker-compose -f docker-compose.prod.yml up -d
```

Production servisleri:

- **PostgreSQL** (port: 5433) - Veritabanı
- **Seq** (port: 5341 ingestion, 8081 UI) - Log yönetimi
- **API** (port: 8080) - .NET API

### Manuel Kurulum (Docker Olmadan)

1. **PostgreSQL'i yerel olarak kurun ve çalıştırın**

2. **Bağımlılıkları yükleyin:**

```bash
dotnet restore
```

1. **Environment dosyası oluşturun:**

```bash
cp example.dev.env dev.env
```

1. **Connection string'i güncelleyin:**
   `dev.env` dosyasında PostgreSQL bağlantı bilgilerinizi düzenleyin.

2. **Veritabanı migration'larını uygulayın:**

```bash
dotnet ef database update -p Persistence -s API
```

> **Not:** Migration'lar uygulama başlangıcında otomatik olarak çalışır.

1. **Projeyi çalıştırın:**

```bash
dotnet run --project API
```

### Environment Dosyaları

Proje, farklı ortamlar için environment dosyaları kullanır:

- `dev.env` - Development ortamı için (Docker ve local)
- `prod.env` - Production ortamı için
- `example.dev.env` - Development örnek dosyası
- `example.prod.env` - Production örnek dosyası

**Environment Değişkenleri:**

```bash
# PostgreSQL Ayarları
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=unilostitem_dev
POSTGRES_PORT=5432

# Connection String (otomatik oluşur)
DefaultConnection=Host=postgres;Port=${POSTGRES_PORT};Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}

# JWT Ayarları
Jwt__SecretKey=<your-64-byte-secret-key>
Jwt__Issuer=UniLostItemAPI
Jwt__Audience=UniLostItemClient
Jwt__AccessTokenExpirationMinutes=60
Jwt__RefreshTokenExpirationDays=7

# Seq Ayarları
SeqServerUrl=http://seq:5341
ACCEPT_EULA=Y
SEQ_FIRSTRUN_ADMINUSERNAME=admindev
SEQ_FIRSTRUN_ADMINPASSWORD=admindev
```

## Kullanım

### API Endpoints

#### Authentication Endpoints (`/api/v1/auth`)

- **POST** `/api/v1/auth/register` - Yeni kullanıcı kaydı
- **POST** `/api/v1/auth/login` - Kullanıcı girişi
- **POST** `/api/v1/auth/refresh-token` - Access token yenileme
- **GET** `/api/v1/auth/profile` - Mevcut kullanıcı profilini getir (JWT gerektirir)
- **PUT** `/api/v1/auth/profile` - Mevcut kullanıcı profilini güncelle (JWT gerektirir)

#### SerhanKitap Endpoints (`/api/v1/serhan-kitaplar`) - **JWT Authentication Required**

Tüm endpoint'ler JWT Bearer token gerektirir (`[Authorize]`):

- **GET** `/api/v1/serhan-kitaplar` - Kitapları listele (Pagination, Filtering, Sorting desteği)
- **GET** `/api/v1/serhan-kitaplar/{id}` - Belirli bir kitabı getir
- **POST** `/api/v1/serhan-kitaplar` - Yeni kitap ekle
- **PUT** `/api/v1/serhan-kitaplar/{id}` - Kitap bilgilerini güncelle
- **DELETE** `/api/v1/serhan-kitaplar/{id}` - Kitap sil

> **Not:** Swagger UI'da test etmek için önce `/api/v1/auth/login` endpoint'i ile token almalı ve "Authorize" butonuna tıklayarak token girmelisiniz.

##### Pagination, Filtering & Sorting

GET `/api/v1/serhan-kitaplar` endpoint'i gelişmiş sorgulama özellikleri sunar:

**Query Parameters:**

| Parameter        | Type    | Default | Description                                              |
| ---------------- | ------- | ------- | -------------------------------------------------------- |
| `pageNumber`     | int     | 1       | Sayfa numarası (min: 1)                                  |
| `pageSize`       | int     | 10      | Sayfa boyutu (min: 1, max: 100)                          |
| `sortBy`         | enum    | null    | Sıralama alanı (KitapName, KitapYazar, KitapSayfaSayisi) |
| `sortDescending` | bool    | false   | Azalan sıralama                                          |
| `searchTerm`     | string? | null    | Kitap adı ve yazarında arama                             |
| `kitapName`      | string? | null    | Kitap adına göre filtrele (contains)                     |
| `kitapYazar`     | string? | null    | Yazar adına göre filtrele (contains)                     |
| `minPageCount`   | int?    | null    | Minimum sayfa sayısı                                     |
| `maxPageCount`   | int?    | null    | Maksimum sayfa sayısı                                    |

**Örnek Request'ler:**

```bash
# İlk 10 kitap (default)
GET /api/v1/serhan-kitaplar

# 2. sayfa, 20 kayıt
GET /api/v1/serhan-kitaplar?pageNumber=2&pageSize=20

# "Orwell" içeren kitapları ara, sayfa sayısına göre sırala
GET /api/v1/serhan-kitaplar?searchTerm=Orwell&sortBy=2&sortDescending=true

# 100-500 sayfa aralığındaki kitapları filtrele
GET /api/v1/serhan-kitaplar?minPageCount=100&maxPageCount=500

# Kitap adına göre filtrele ve yazar adına göre sırala
GET /api/v1/serhan-kitaplar?kitapName=1984&sortBy=1
```

**Response Format:**

```json
{
  "success": true,
  "message": "Books retrieved successfully",
  "data": [
    {
      "id": "1",
      "kitapName": "1984",
      "kitapYazar": "George Orwell",
      "kitapSayfaSayisi": 328,
      "createdDate": "2025-01-15T10:30:00Z",
      "createdBy": "user-id-123",
      "updatedDate": "2025-01-16T14:22:00Z",
      "updatedBy": "user-id-456",
      "isDeleted": false,
      "isActive": true
    },
    {
      "id": "2",
      "kitapName": "Animal Farm",
      "kitapYazar": "George Orwell",
      "kitapSayfaSayisi": 112,
      "createdDate": "2025-01-14T09:15:00Z",
      "createdBy": "user-id-123",
      "updatedDate": null,
      "updatedBy": null,
      "isDeleted": false,
      "isActive": true
    }
  ],
  "metadata": {
    "totalCount": 5,
    "pageNumber": 1,
    "pageSize": 10,
    "totalPages": 1,
    "hasNext": false,
    "hasPrevious": false
  }
}
```

### BaseEntity & Audit Trail

**BaseEntity Özellikleri:**

Tüm business entity'ler `BaseEntity` sınıfından miras aldığında otomatik olarak audit trail özellikleri kazanır:

| Property      | Type      | Default                     | Description                            |
| ------------- | --------- | --------------------------- | -------------------------------------- |
| `Id`          | string    | `Guid.NewGuid().ToString()` | Entity'nin benzersiz tanımlayıcısı     |
| `CreatedDate` | DateTime  | `DateTime.UtcNow`           | Entity oluşturulma zaman damgası       |
| `CreatedBy`   | string?   | null                        | Entity'yi oluşturan kullanıcı ID'si    |
| `UpdatedDate` | DateTime? | null                        | Entity son güncelleme zaman damgası    |
| `UpdatedBy`   | string?   | null                        | Entity'yi güncelleyen kullanıcı ID'si  |
| `IsDeleted`   | bool      | false                       | Soft delete bayrağı (true = silinmiş)  |
| `IsActive`    | bool      | true                        | Aktiflik bayrağı (false = aktif değil) |

**Soft Delete Pattern:**

- Silme işlemleri `IsDeleted = true` olarak ayarlar, fiziksel olarak kaydı silmez
- List sorguları otomatik olarak `IsDeleted = true` kayıtları filtreler
- Detay sorguları soft-delete edilmiş entity'ler için **410 Gone** döner
- Detay sorguları inaktif (`IsActive = false`) entity'ler için **423 Locked** döner
- Bu pattern sayesinde veri bütünlüğü korunur ve gerekirse geri yükleme yapılabilir

**Önemli Notlar:**

- `ApplicationUser` `BaseEntity`'den miras almaz (zaten `IdentityUser`'dan miras alıyor)
- Handler'lar `CreatedDate`, `CreatedBy`, `UpdatedDate`, `UpdatedBy` alanlarını otomatik olarak ayarlar
- Audit alanları Query DTO'larına dahil edilir, ancak Command DTO'lardan hariç tutulur (AutoMapper `IgnoreAllBaseEntityProperties()` extension method'u ile)

### Health Check Endpoints

- **GET** `/health/api` - **Liveness Check**: Sadece API uygulamasının ayakta olup olmadığını kontrol eder. Bağımlılıkları (DB, Seq) kontrol etmez.
- **GET** `/health/all` - **Readiness Check**: API ve tüm bağımlılıkların (PostgreSQL, Seq) durumunu kontrol eder.

### Örnek Request (POST)

```json
{
  "kitapName": "1984",
  "kitapYazar": "George Orwell",
  "kitapSayfaSayisi": 328
}
```

## Mimari

### Clean Architecture Katmanları

#### 1. Domain Layer (Domain/)

- En içteki katman
- İş kurallarını ve entity'leri içerir
- Dış katmanlara bağımlılığı yoktur

#### 2. Application Layer (Application/)

- Use case'leri ve iş mantığını içerir
- CQRS pattern ile Commands ve Queries
- MediatR ile request/response pipeline
- FluentValidation ile input validasyonu
- AutoMapper ile DTO dönüşümleri

#### 3. Infrastructure Layer (Infrastructure/)

- Dış dünya ile iletişim (3rd party services)
- JWT Token üretimi ve güvenliği
- Email, Dosya sistemi gibi servisler

#### 4. Persistence Layer (Persistence/)

- Veritabanı işlemleri
- Entity Framework Core implementasyonu
- Repository pattern (DbContext üzerinden)

#### 5. Presentation Layer (API/)

- HTTP isteklerini karşılar
- Controller'lar ve middleware'ler
- Swagger dokümantasyonu
- Exception handling

### Kullanılan Design Pattern'ler

- **CQRS (Command Query Responsibility Segregation)** - Okuma ve yazma işlemlerinin ayrılması
- **Mediator Pattern** - MediatR ile loosely coupled iletişim
- **Repository Pattern** - EF Core DbContext ile veri erişimi
- **Dependency Injection** - ASP.NET Core built-in DI container
- **Pipeline Behavior** - Validation ve cross-cutting concerns
- **Result Pattern** - İş mantığı sonuçlarının standart şekilde dönülmesi

### Özellikler

- **Clean Architecture** ile katmanlı mimari
- **CQRS Pattern** ile command/query ayrımı
- **MediatR** ile request handling
- **FluentValidation** ile güçlü validation
- **AutoMapper** ile nesne dönüşümleri
- **Entity Framework Core** ile ORM
- **ASP.NET Core Identity** ile kullanıcı kimlik yönetimi
- **JWT Authentication** ile token-based authentication
- **PostgreSQL** veritabanı desteği
- **Docker & Docker Compose** ile containerization
- **Seq** ile structured logging
- **Environment-based configuration** (.env dosyaları)
- **Advanced Health Checks** - Liveness (`/health/api`) & Readiness (`/health/all`) endpoints with Seq integration
- **Otomatik migration** ve seed data
- **Custom Exception Middleware** ile merkezi hata yönetimi
- **Swagger/OpenAPI** dokümantasyonu (JWT auth ile)
- **CORS** desteği
- **Standardize API Responses** yapısı
- **Extension Methods** ile temiz ve modüler Program.cs yapısı
- **Code Analyzers** ile kod kalitesi kontrolü (Microsoft & SonarAnalyzer)
- **TreatWarningsAsErrors** ile sıkı kod standartları
- **Hot reload** desteği (development ortamında)
- **CI/CD Pipelines** (GitHub Actions)
- **Pagination** - Sayfalı listeler için `PaginatedListDto<T>` ile metadata desteği
- **Filtering** - Esnek filtreleme (arama, aralık filtreleri, contains vb.)
- **BaseEntity Audit Trail** - Tüm entity'ler için otomatik audit alanları (CreatedDate, CreatedBy, UpdatedDate, UpdatedBy, IsDeleted, IsActive)
- **Soft Delete** - Veri bütünlüğünü koruyan soft delete pattern (fiziksel silme yerine `IsDeleted` bayrağı)
- **Sorting** - Enum tabanlı type-safe sıralama desteği
- **Request DTO Pattern** - Controller tarafında temiz parametre yönetimi için Data Annotations
- **Docker Image Registry** - GitHub Container Registry entegrasyonu
- **Automated Docker Tagging** - Git SHA ve latest tags

## 🚀 CI/CD Pipelines

### Development Pipeline (ci-dev.yml)

Tetikleyiciler:

- `main` ve `develop` branch'lerine push
- Pull request'ler (main ve develop'a)
- Manuel tetikleme

**Pipeline Adımları:**

1. ✅ **Build & Test** - .NET 9 ile build ve test çalıştırma
2. ✅ **Code Coverage** - Coverlet ile test coverage toplama (OpenCover format)
3. ✅ **SonarCloud Analysis** - Kod kalitesi ve güvenlik analizi
4. ✅ **Docker Build** - Development image build (`dev-uni-lost-item`)
5. ✅ **Docker Test** - Container health check testi (`/health/api` endpoint)
6. ✅ **Docker Push** - GHCR'ye image push (sadece push event'lerinde, test başarılı ise)

**Docker Tags:**

```text
ghcr.io/serhanbaymaz/dev-uni-lost-item:latest
ghcr.io/serhanbaymaz/dev-uni-lost-item:sha-<git-sha>
```

**Gerekli Secrets:**

**SonarCloud:**

- `SONAR_TOKEN_DEV` - SonarCloud authentication token
- `SONAR_PROJECT_KEY_DEV` - SonarCloud project key
- `SONAR_ORGANIZATION_DEV` - SonarCloud organization
- `SONAR_HOST_URL_DEV` - SonarCloud URL (<https://sonarcloud.io>)

**JWT Configuration:**

- `JWT_SECRET_KEY_DEV` - JWT secret key (en az 64 byte, `openssl rand -base64 64` ile oluşturulabilir)
- `JWT_ISSUER_DEV` - JWT issuer (örn: `UniLostItemApi`)
- `JWT_AUDIENCE_DEV` - JWT audience (örn: `UniLostItemClient`)
- `JWT_ACCESS_TOKEN_EXPIRATION_MINUTES_DEV` - Access token süresi (dakika, örn: `60`)
- `JWT_REFRESH_TOKEN_EXPIRATION_DAYS_DEV` - Refresh token süresi (gün, örn: `7`)

### Production Pipeline (ci-prod.yml)

Tetikleyiciler:

- `main` branch'e push
- Version tags (`v*.*.*`)
- Manuel tetikleme

**Pipeline Adımları:**

1. ✅ **Build & Test** - .NET 9 ile build ve test çalıştırma
2. ✅ **Code Coverage** - Coverlet ile test coverage toplama (90 gün saklama)
3. ✅ **Docker Build** - Production image build (`prod-uni-lost-item`)
4. ✅ **Docker Test** - Container health check testi (`/health/api` endpoint)
5. ✅ **Docker Push** - GHCR'ye image push (test başarılı ise)

**Docker Tags:**

```text
ghcr.io/serhanbaymaz/prod-uni-lost-item:latest
ghcr.io/serhanbaymaz/prod-uni-lost-item:sha-<git-sha>
```

**Gerekli Secrets:**

**JWT Configuration:**

- `JWT_SECRET_KEY_PROD` - JWT secret key (en az 64 byte, güçlü ve production-ready olmalı)
- `JWT_ISSUER_PROD` - JWT issuer (örn: `UniLostItemApi`)
- `JWT_AUDIENCE_PROD` - JWT audience (örn: `UniLostItemClient`)
- `JWT_ACCESS_TOKEN_EXPIRATION_MINUTES_PROD` - Access token süresi (dakika, production için önerilen: `15`)
- `JWT_REFRESH_TOKEN_EXPIRATION_DAYS_PROD` - Refresh token süresi (gün, örn: `7`)

> **Önemli:** Production için JWT secret key oluşturmak:
>
> ```bash
> # PowerShell
> [Convert]::ToBase64String((1..64 | ForEach-Object { Get-Random -Minimum 0 -Maximum 256 }))
>
> # Linux/Mac
> openssl rand -base64 64
> ```

### Docker Image Kullanımı

**Development image'ı çekmek:**

```bash
docker pull ghcr.io/serhanbaymaz/dev-uni-lost-item:latest
# veya specific version
docker pull ghcr.io/serhanbaymaz/dev-uni-lost-item:sha-abc1234
```

**Production image'ı çekmek:**

```bash
docker pull ghcr.io/serhanbaymaz/prod-uni-lost-item:latest
# veya specific version
docker pull ghcr.io/serhanbaymaz/prod-uni-lost-item:sha-xyz5678
```

**Image'ı çalıştırmak:**

```bash
docker run -p 8080:8080 ghcr.io/serhanbaymaz/prod-uni-lost-item:latest
```

### Pipeline Özellikleri

- 🚀 **Otomatik build ve test** her commit'te
- 📊 **SonarCloud entegrasyonu** (development)
- 🐳 **Docker image build ve push** (GHCR)
- 📦 **Multi-tag stratejisi** (latest + git SHA)
- 🔒 **Job-level permissions** (least privilege)
- 💾 **Caching** - NuGet, SonarCloud scanner
- 📈 **Coverage artifacts** - 30/90 gün saklama
- ✅ **Container testing** - Smoke tests

### GitHub Actions Badge

[![CI-Dev Pipeline](https://github.com/SerhanBaymaz/UniLostItem/actions/workflows/ci-dev.yml/badge.svg)](https://github.com/SerhanBaymaz/UniLostItem/actions/workflows/ci-dev.yml)
[![CI-Prod Pipeline](https://github.com/SerhanBaymaz/UniLostItem/actions/workflows/ci-prod.yml/badge.svg)](https://github.com/SerhanBaymaz/UniLostItem/actions/workflows/ci-prod.yml)

## 🛠️ Geliştirme Komutları

### Entity Framework Migrations

Yeni bir migration oluşturmak için:

```bash
dotnet ef migrations add <MigrationCommandMessage> -p Persistence -s API
```

Veritabanını güncellemek için:

```bash
dotnet ef database update -p Persistence -s API
```

Migration'ı geri almak için:

```bash
dotnet ef migrations remove -p Persistence -s API
```

### Build & Run

Projeyi build etmek:

```bash
dotnet build UniLostItem.sln
```

Projeyi çalıştırmak:

```bash
dotnet run --project API
```

Watch mode ile çalıştırmak (hot reload):

```bash
dotnet watch run --project API
```

### Test Komutları

Testleri çalıştırmak için:

```bash
dotnet test
```

Kod kapsamı (coverage) raporu ile:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

Belirli bir test projesini çalıştırmak:

```bash
dotnet test Tests/Tests.csproj
```

Verbose output ile test çalıştırmak:

```bash
dotnet test --verbosity detailed
```

## 📝 Notlar

- Proje **PostgreSQL** veritabanı kullanmaktadır.
- **Docker Compose** ile hem development hem production ortamları hazır durumdadır.
- **Migration'lar** uygulama başlangıcında otomatik olarak çalışır (`Program.cs`).
- **Seed data** otomatik olarak veritabanına eklenir.
- CORS yapılandırması development ortamı için `localhost:3000` portuna izin vermektedir.
- Nullable referans türleri aktif durumdadır (C# 8.0+).
- Implicit usings özelliği etkinleştirilmiştir.

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'feat: Add amazing feature'`)
4. Branch'inizi push edin (`git push origin feature/amazing-feature`)
5. Pull Request oluşturun

## 📄 Lisans

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır.

---

**Geliştirici:** Serhan Baymaz
**Tarih:** Aralık 2025
