# Project Rename: CqrsCleanLayerDotNET9Template → UniLostItem

## Dosya Yeniden Adlandırma

| Eski | Yeni |
|---|---|
| `CqrsCleanLayerDotNET9Template.sln` | `UniLostItem.sln` |

## Yapılan Değişiklikler (15 dosya)

### Docker & Container İsimleri (`cqrs-clean-net9` → `uni-lost-item`)
- `docker-compose.local.yml` — compose name, container name'ler
- `docker-compose.dev.yml` — compose name, container name'ler, image name
- `docker-compose.prod.yml` — compose name, container name'ler, image name
- `Dockerfile.dev` — solution dosya referansı
- `Dockerfile.prod` — solution dosya referansı

### CI/CD Pipeline'ları
- `.github/workflows/ci-dev.yml` — solution adı, Docker image adları (`dev-cqrs-clean-net9` → `dev-uni-lost-item`)
- `.github/workflows/ci-prod.yml` — solution adı, Docker image adları (`prod-cqrs-clean-net9` → `prod-uni-lost-item`)

### Environment Dosyaları (JWT & DB)
- `dev.env` — `POSTGRES_DB`, `Jwt__Issuer`, `Jwt__Audience`
- `prod.env` — `POSTGRES_DB`, `Jwt__Issuer`, `Jwt__Audience`
- `example.dev.env` — `POSTGRES_DB`, `Jwt__Issuer`, `Jwt__Audience`
- `example.prod.env` — `POSTGRES_DB`, `Jwt__Issuer`, `Jwt__Audience`

### VS Code
- `.vscode/launch.json` — container name (`api-cqrs-clean-net9-dev` → `api-uni-lost-item-dev`)

### Dokümantasyon
- `CLAUDE.md` — proje adı, solution adı, Docker image referansları
- `README.md` — başlık, proje yapısı, JWT örnekleri, Docker komutları, CI badge'leri

## Değişen String Eşlemeleri

| Eski | Yeni |
|---|---|
| `CqrsCleanLayerDotNET9Template` | `UniLostItem` |
| `CqrsCleanLayerDotNET9TemplateAPI` | `UniLostItemAPI` |
| `CqrsCleanLayerDotNET9TemplateClient` | `UniLostItemClient` |
| `cqrscleanlayernet9template_dev` | `unilostitem_dev` |
| `cqrscleanlayernet9template_prod` | `unilostitem_prod` |
| `cqrs-clean-net9` | `uni-lost-item` |
| `dev-cqrs-clean-net9` | `dev-uni-lost-item` |
| `prod-cqrs-clean-net9` | `prod-uni-lost-item` |
| `api-cqrs-clean-net9-dev` | `api-uni-lost-item-dev` |

## Manuel Yapılması Gerekenler

1. **GitHub repo adını değiştir** — Ayarlar → Repository name (GitHub'da otomatik redirect sağlar)
2. **Docker volume temizliği** — DB adı değiştiği için mevcut veriler korunmaz: `docker-compose -f docker-compose.local.yml down -v`
3. **GitHub Secrets güncelle** — `JWT_ISSUER_DEV`, `JWT_AUDIENCE_DEV` gibi secret'lar eski adı içeriyorsa güncelle

## Doğrulama

- Build: **0 Warning, 0 Error**
- Testler: **455 passed, 0 failed**
- Kalan eski referans taraması: **0 sonuç** (case-insensitive)
