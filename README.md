# TemplateDeneme - .NET 9 Clean Architecture Projesi

Bu proje, **Clean Architecture** prensiplerine uygun olarak geliştirilmiş bir .NET 9 Web API uygulamasıdır. Proje, **CQRS pattern**, **MediatR**, **Entity Framework Core** ve modern .NET teknolojilerini kullanarak ölçeklenebilir ve sürdürülebilir bir yapı sunmaktadır.

## 📋 İçindekiler

- [Proje Yapısı](#proje-yapısı)
- [Kullanılan Teknolojiler](#kullanılan-teknolojiler)
- [Kullanılan Kütüphaneler](#kullanılan-kütüphaneler)
- [Kurulum](#kurulum)
- [Kullanım](#kullanım)
- [Mimari](#mimari)

## 🏗️ Proje Yapısı

Proje, Clean Architecture prensiplerine göre 4 katmana ayrılmıştır; ayrıca testler için bir `Tests/` proje dizini bulunmaktadır.

```
TemplateDeneme/
├── API/                      # Presentation Layer - Web API
│   ├── Controllers/          # API Controller'ları
│   │   ├── BaseApiController.cs
│   │   └── SerhanKitaplarController.cs
│   ├── Middleware/           # Custom middleware'ler
│   │   └── ExceptionMiddleware.cs
│   ├── Responses/            # API response modelleri
│   │   ├── AppProblemDetails.cs
│   │   └── StandardApiResponse.cs
│   ├── Properties/
│   │   └── launchSettings.json
│   ├── appsettings.Development.json
│   └── Program.cs            # Uygulama başlangıç noktası
├── Application/              # Application Layer - İş mantığı
│   ├── Core/                 # Ortak yapılar (Result, Validation, Mapping)
│   │   ├── MappingProfiles.cs       # AutoMapper profilleri
│   │   ├── Result.cs                # Result pattern implementasyonu
│   │   └── ValidationBehavior.cs    # MediatR validation pipeline
│   └── Features/
│       └── SerhanKitaplar/   # SerhanKitap feature modülü
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
│               │   └── DTOs/
│               │       └── GetSerhanKitapDto.cs
│               ├── GetSerhanKitapList/
│               │   ├── GetSerhanKitapListQuery.cs
│               │   └── GetSerhanKitapListQueryHandler.cs
│               └── GetSerhanKitapDetails/
│                   ├── GetSerhanKitapDetailsQuery.cs
│                   └── GetSerhanKitapDetailsQueryHandler.cs
├── Domain/                   # Domain Layer - Domain modelleri
│   └── SerhanKitap.cs        # Domain entity
├── Persistence/              # Infrastructure Layer - Data erişimi
│   ├── AppDbContext.cs       # Entity Framework DbContext
│   ├── IAppDbContext.cs      # DbContext interface
│   ├── DbInitializer.cs      # Veritabanı seed data
│   └── Migrations/           # EF Core migration'ları
└── Tests/                    # Unit and integration tests project
    ├── Features/             # Feature-specific tests
    │   └── SerhanKitaplar/
    │       ├── Commands/
    │       │   ├── CreateSerhanKitap/
    │       │   │   ├── CreateSerhanKitapCommandHandlerTests.cs
    │       │   │   └── CreateSerhanKitapCommandValidatorTests.cs
    │       │   ├── EditSerhanKitap/
    │       │   │   ├── EditSerhanKitapCommandHandlerTests.cs
    │       │   │   └── EditSerhanKitapCommandValidatorTests.cs
    │       │   └── DeleteSerhanKitap/
    │       │       └── DeleteSerhanKitapCommandHandlerTests.cs
    │       └── Queries/
    │           ├── GetSerhanKitapList/
    │           │   └── GetSerhanKitapListQueryHandlerTests.cs
    │           └── GetSerhanKitapDetails/
    │               └── GetSerhanKitapDetailsQueryHandlerTests.cs
## 🚀 Kullanılan Teknolojiler

- **.NET 9.0** - .NET framework versiyonu
- **ASP.NET Core Web API** - RESTful API geliştirme
- **Entity Framework Core 9.0** - ORM ve veritabanı işlemleri
- **PostgreSQL** - Güçlü, açık kaynaklı ilişkisel veritabanı
- **Docker & Docker Compose** - Container orchestration ve deployment
- **Seq** - Structured logging ve log yönetimi
- **C# 13** - Modern C# özellikleri ile geliştirme

## 📦 Kullanılan Kütüphaneler

### API Katmanı
| Kütüphane | Versiyon | Açıklama |
|-----------|----------|----------|
| **Swashbuckle.AspNetCore** | 6.5.0 | Swagger/OpenAPI dokümantasyonu |
| **DotNetEnv** | 3.1.1 | Loads environment variables from .env files |
| **Microsoft.EntityFrameworkCore.Design** | 9.0.0 | EF Core design-time araçları |
| **Ben.Demystifier** | 0.4.1 | Gelişmiş exception stack trace formatlaması |
| **Serilog** | 4.3.0 | Structured logging and enrichment |
| **Serilog.Sinks.Seq** | 9.0.0 | Seq sink for centralized structured logging |

### Application Katmanı
| Kütüphane | Versiyon | Açıklama |
|-----------|----------|----------|
| **MediatR** | 12.4.1 | CQRS pattern implementasyonu |
| **AutoMapper** | 13.0.1 | Object-to-object mapping |
| **FluentValidation.DependencyInjectionExtensions** | 11.11.0 | Validation kuralları ve DI entegrasyonu |

### Persistence Katmanı
| Kütüphane | Versiyon | Açıklama |
|-----------|----------|----------|
| **Npgsql.EntityFrameworkCore.PostgreSQL** | 9.0.3 | PostgreSQL veritabanı sağlayıcısı |

### Domain Katmanı
- Harici bağımlılık içermez (Clean Architecture prensibi)

### Tests Katmanı
| Kütüphane | Versiyon | Açıklama |
|-----------|----------|----------|
| **xUnit** | 2.9.2 | Test framework |
| **xunit.runner.visualstudio** | 2.8.2 | Visual Studio test runner |
| **FluentAssertions** | 8.8.0 | Readable test assertions |
| **Moq** | 4.20.72 | Mocking framework |
| **Microsoft.EntityFrameworkCore.InMemory** | 9.0.0 | In-memory database for testing |
| **coverlet.collector** | 6.0.2 | Code coverage collection |
| **Microsoft.NET.Test.Sdk** | 17.12.0 | .NET Test SDK |

### Code Quality & Analyzers (Directory.Build.props)
| Kütüphane | Versiyon | Açıklama |
|-----------|----------|----------|
| **Microsoft.CodeAnalysis.NetAnalyzers** | 9.0.0 | .NET kod analizi ve best practice kuralları |
| **SonarAnalyzer.CSharp** | 10.16.1.129956 | SonarQube/SonarCloud kod kalitesi analizi |
| **SonarLint** | - | IDE-level static analysis (recommended for developers) |

Bu projede ayrıca SonarQube ile merkezi kod kalite taramaları entegre edilebilir; `SonarAnalyzer.CSharp` sunucu/CI analizleri için yapılandırılmıştır.

> **Not:** `TreatWarningsAsErrors` özelliği aktiftir - tüm uyarılar hata olarak kabul edilir.

## 🔧 Kurulum

### Gereksinimler
- .NET 9.0 SDK
- Docker & Docker Compose
- Visual Studio 2022 / Visual Studio Code / JetBrains Rider (opsiyonel)

### Docker Compose ile Kurulum (Önerilen)

#### Development Ortamı

1. **Projeyi klonlayın:**
```bash
git clone <repository-url>
cd TemplateDeneme
```

2. **Environment dosyasını oluşturun:**
```bash
cp example.dev.env dev.env
```

> **Not:** `dev.env` dosyasını ihtiyaçlarınıza göre düzenleyebilirsiniz.

3. **Docker Compose ile servisleri başlatın:**
```bash
docker-compose -f docker-compose.dev.yml up -d
```

Bu komut şu servisleri başlatır:
- **PostgreSQL** (port: 5432) - Veritabanı
- **Seq** (port: 8088 UI, 5348 ingestion) - Log yönetimi
- **API** (port: 8089 HTTP, 5009 Debug) - .NET API

4. **Servislerin durumunu kontrol edin:**
```bash
docker-compose -f docker-compose.dev.yml ps
```

5. **API'ye erişin:**
- Swagger UI: `http://localhost:8089/swagger`
- Seq UI: `http://localhost:8088` (Kullanıcı: `admindev`, Şifre: `admindev`)

6. **Servisleri durdurmak için:**
```bash
docker-compose -f docker-compose.dev.yml down
```

#### Production Ortamı

1. **Production environment dosyasını oluşturun:**
```bash
cp example.prod.env prod.env
```

> **ÖNEMLİ:** `prod.env` dosyasındaki şifreleri mutlaka değiştirin!

2. **Production servisleri başlatın:**
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

3. **Environment dosyası oluşturun:**
```bash
cp example.dev.env dev.env
```

4. **Connection string'i güncelleyin:**
`dev.env` dosyasında PostgreSQL bağlantı bilgilerinizi düzenleyin.

5. **Veritabanı migration'larını uygulayın:**
```bash
dotnet ef database update -p Persistence -s API
```

> **Not:** Migration'lar uygulama başlangıcında otomatik olarak çalışır.

6. **Projeyi çalıştırın:**
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

# Seq Ayarları
SeqServerUrl=http://seq:5341
ACCEPT_EULA=Y
SEQ_FIRSTRUN_ADMINUSERNAME=admindev
SEQ_FIRSTRUN_ADMINPASSWORD=admindev
```

## 💻 Kullanım

### API Endpoints

API, `SerhanKitap` (Kitap) entity'si üzerinde CRUD işlemleri gerçekleştirir:

- **GET** `/api/serhankitaplar` - Tüm kitapları listele
- **GET** `/api/serhankitaplar/{id}` - Belirli bir kitabı getir
- **POST** `/api/serhankitaplar` - Yeni kitap ekle
- **PUT** `/api/serhankitaplar/{id}` - Kitap bilgilerini güncelle
- **DELETE** `/api/serhankitaplar/{id}` - Kitap sil

### Örnek Request (POST)
```json
{
  "kitapName": "1984",
  "kitapYazar": "George Orwell",
  "kitapSayfaSayisi": 328
}
```

## 🏛️ Mimari

### Clean Architecture Katmanları

**1. Domain Layer (Domain/)**
- En içteki katman
- İş kurallarını ve entity'leri içerir
- Dış katmanlara bağımlılığı yoktur

**2. Application Layer (Application/)**
- Use case'leri ve iş mantığını içerir
- CQRS pattern ile Commands ve Queries
- MediatR ile request/response pipeline
- FluentValidation ile input validasyonu
- AutoMapper ile DTO dönüşümleri

**3. Infrastructure Layer (Persistence/)**
- Veritabanı işlemleri
- Entity Framework Core implementasyonu
- Repository pattern (DbContext üzerinden)

**4. Presentation Layer (API/)**
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

✅ **Clean Architecture** ile katmanlı mimari
✅ **CQRS Pattern** ile command/query ayrımı
✅ **MediatR** ile request handling
✅ **FluentValidation** ile güçlü validation
✅ **AutoMapper** ile nesne dönüşümleri
✅ **Entity Framework Core** ile ORM
✅ **PostgreSQL** veritabanı desteği
✅ **Docker & Docker Compose** ile containerization
✅ **Seq** ile structured logging
✅ **Environment-based configuration** (.env dosyaları)
✅ **Otomatik migration** ve seed data
✅ **Custom Exception Middleware** ile merkezi hata yönetimi
✅ **Swagger/OpenAPI** dokümantasyonu
✅ **CORS** desteği
✅ **Standardize API Responses** yapısı
✅ **Code Analyzers** ile kod kalitesi kontrolü (Microsoft & SonarAnalyzer)
✅ **TreatWarningsAsErrors** ile sıkı kod standartları
✅ **Hot reload** desteği (development ortamında)

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
dotnet build
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
