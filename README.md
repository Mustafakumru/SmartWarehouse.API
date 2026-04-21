# CALISMA_RAPORU.md
# Akıllı Depo Yönetimi — Smart Warehouse Management System

---

## 1. Projenin Kısa Özeti

Bu proje, bir şirketin depo operasyonlarını dijital ortamda yönetebilmesini sağlayan,
çok kiracılı (multi-tenant) bir **Akıllı Depo Yönetimi** uygulamasıdır.

Temel senaryolar:
- Ürün tanımlama ve kategorilere ayırma
- Depo bölgesi (Zone) ve raf (Rack) yapısı kurma
- Depoya ürün girişi (IN) ve depodan ürün çıkışı (OUT)
- Anlık stok takibi ve stok hareket geçmişi
- Kritik stok seviyesi uyarısı

Uygulama tek sayfalı (SPA) bir arayüze sahiptir. Tüm modüller
(Ürünler, Stok Hareketleri, Kategoriler, Bölgeler, Raflar) sekmeler
halinde tek ekranda sunulmaktadır.

---

## 2. Kullanılan Teknolojiler ve Versiyonları

| Katman | Teknoloji | Versiyon |
|---|---|---|
| Backend Framework | ASP.NET Core Web API | .NET 9.0 |
| ORM | Entity Framework Core | 9.0.0 |
| Veritabanı | Microsoft SQL Server | 2019+ |
| API Dokümantasyonu | Swashbuckle (Swagger) | 6.9.0 |
| Frontend Framework | React | 18.x |
| Dil (Frontend) | TypeScript | 5.x |
| UI Kütüphanesi | Material UI (MUI) | 6.x |
| HTTP İstemcisi | Axios | 1.x |
| Paket Yöneticisi | npm | 10.x |
| Geliştirme Ortamı | Visual Studio / VS Code | — |

---

## 3. Mimari Kararlar ve Nedenleri

### 3.1 Katmanlı Mimari — Controller → Manager → Repository → Entity

```
İstek (HTTP)
    ↓
Controller       → HTTP doğrulama, CompanyId kontrolü, routing
    ↓
Manager          → İş mantığı, validasyon, DTO dönüşümü
    ↓
Repository       → Veri erişim, EF Core sorguları
    ↓
DbContext        → EF Core, SQL Server bağlantısı
    ↓
Veritabanı (MSSQL)
```

**Neden bu mimari?**
- Her katman tek bir sorumluluğa sahiptir (Single Responsibility).
- Controller katmanı iş mantığı içermez; sadece HTTP isteklerini karşılar.
- Manager katmanı validasyon ve iş kurallarını barındırır; test edilebilirliği artırır.
- Repository katmanı veri erişimini soyutlar; ORM değişikliğine karşı esneklik sağlar.

### 3.2 Multi-Tenant Tasarımı

Her entity `BaseEntity` sınıfından türer. `BaseEntity` içindeki `CompanyId` alanı
sayesinde tüm tablolar şirket bazında izole edilmiştir.

```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public string CompanyId { get; set; }  // Tenant ayrıştırıcı
    public bool IsDeleted { get; set; }    // Soft-delete
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

CompanyId kontrolü iki kademede yapılır:
1. **Controller** katmanında — eksik CompanyId → 400 BadRequest
2. **Manager** katmanında — kayıt başka şirkete aitse → hata mesajı

### 3.3 Soft Delete + Global Query Filter

Fiziksel silme yerine `IsDeleted = true` yaklaşımı benimsendi.
EF Core `HasQueryFilter` ile tüm sorgulara otomatik filtre eklendi:

```csharp
entity.HasQueryFilter(e => !e.IsDeleted);
```

Bu sayede:
- Silinen kayıtlar listelere gelmez.
- Geçmiş veriler korunur (audit trail).
- Yanlış silme durumunda geri alma imkânı vardır.

### 3.4 WarehouseStock Ayrı Tablo Kararı

Anlık stok miktarı `InventoryTransaction` tablosundan SUM() ile hesaplanmak yerine
ayrı bir `WarehouseStock` tablosunda tutulmaktadır.

**Neden?**
- Binlerce hareket kaydı biriktiğinde SUM() sorgusu yavaşlar.
- `WarehouseStock` tablosundan direkt `Quantity` okumak O(1) karmaşıklığındadır.
- `ReservedQuantity` alanı ile ileride sipariş rezervasyonu yapılabilir.
- `StockAfterTransaction` snapshot alanı sayesinde geçmiş tarihteki stok
  durumu, tüm hareketleri toplamadan görülebilir.

### 3.5 HTTP POST ile Güncelleme ve Silme

`PUT` ve `DELETE` HTTP metotları kullanılmamıştır. Tüm yazma işlemleri POST ile yapılır:

```
POST /api/product/update  → Güncelleme
POST /api/product/delete  → Soft-delete
```

`DeleteRequestDto` ortak DTO'su oluşturularak tüm silme işlemleri
{ "id": "...", "companyId": "..." } body formatında standartlaştırılmıştır.

### 3.6 Server-Side Pagination

Ürün listesi ve stok hareketleri listeleri sunucu taraflı sayfalama kullanır.
Tüm filtreleme, arama ve sayfalama işlemleri veritabanında gerçekleşir:

```csharp
var items = await query
    .OrderBy(p => p.Name)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

Frontend sadece parametreleri gönderir, hiçbir zaman tüm veriyi çekmez.

PagedResponseDto standart zarf yapısı tüm sayfalı listeler için kullanılır:

```json
{
  "items": [...],
  "totalCount": 150,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 15,
  "hasPreviousPage": false,
  "hasNextPage": true
}
```

### 3.7 PascalCase ↔ camelCase Dönüşümü

Backend JSON serializasyonu PascalCase olarak yapılandırılmıştır.
Frontend Axios interceptor'ları ile dönüşüm otomatik sağlanır:

- İstek gönderilirken: camelCase → PascalCase
- Yanıt alınırken: PascalCase → camelCase

---

## 4. Özgün Tasarım Yaklaşımı

### 4.1 Hiyerarşik Depo Modeli

Depo yapısı üç kademeli hiyerarşiyle modellendi:

```
WarehouseZone (Bölge)
    └── WarehouseRack (Raf)
            └── WarehouseStock (Stok)
                    └── InventoryTransaction (Hareket)
```

Bu yapı gerçek depo operasyonlarını yansıtır:
- Zone: Farklı sıcaklık veya güvenlik gereksinimli alanlar (Soğuk Depo, Kimyasal Depo)
- Rack: Zone içindeki fiziksel raf konumları
- Stock: Raf bazında anlık miktar takibi
- Transaction: Değişmez hareket kaydı (immutable audit log)

### 4.2 Renk Kodlu Kategori Sistemi

Her `ProductCategory` bir `ColorCode` (HEX) alanına sahiptir.
Frontend'de kategori chip'leri bu renkle gösterilir.

### 4.3 TransactionCode Otomatik Üretimi

Her stok hareketi için benzersiz ve anlamlı bir kod üretilir:

```
TXN-20240115-0001   (Tarih + Günlük Sıra No)
```

### 4.4 Stok Güvenlik Kontrolü

Çıkış işlemlerinde yetersiz stok durumu iş mantığı katmanında engellenir.

### 4.5 Stok Hareket Tipleri

| Tip | Açıklama | Stok Etkisi |
|---|---|---|
| IN | Depoya giriş | Stok + Miktar |
| OUT | Depodan çıkış | Stok - Miktar |
| ADJ | Sayım düzeltmesi | Stok = Miktar |
| TRF | Raf transferi | Stok - Miktar |

---

## 5. Karşılaşılan Sorunlar ve Çözüm Yolları# CALISMA_RAPORU.md
# Akıllı Depo Yönetimi — Smart Warehouse Management System

---

## 1. Projenin Kısa Özeti

Bu proje, bir şirketin depo operasyonlarını dijital ortamda yönetebilmesini sağlayan,
çok kiracılı (multi-tenant) bir **Akıllı Depo Yönetimi** uygulamasıdır.

Temel senaryolar:
- Ürün tanımlama ve kategorilere ayırma
- Depo bölgesi (Zone) ve raf (Rack) yapısı kurma
- Depoya ürün girişi (IN) ve depodan ürün çıkışı (OUT)
- Anlık stok takibi ve stok hareket geçmişi
- Kritik stok seviyesi uyarısı

Uygulama tek sayfalı (SPA) bir arayüze sahiptir. Tüm modüller
(Ürünler, Stok Hareketleri, Kategoriler, Bölgeler, Raflar) sekmeler
halinde tek ekranda sunulmaktadır.

---

## 2. Kullanılan Teknolojiler ve Versiyonları

| Katman | Teknoloji | Versiyon |
|---|---|---|
| Backend Framework | ASP.NET Core Web API | .NET 9.0 |
| ORM | Entity Framework Core | 9.0.0 |
| Veritabanı | Microsoft SQL Server | 2019+ |
| API Dokümantasyonu | Swashbuckle (Swagger) | 6.9.0 |
| Frontend Framework | React | 18.x |
| Dil (Frontend) | TypeScript | 5.x |
| UI Kütüphanesi | Material UI (MUI) | 6.x |
| HTTP İstemcisi | Axios | 1.x |
| Paket Yöneticisi | npm | 10.x |
| Geliştirme Ortamı | Visual Studio / VS Code | — |

---

## 3. Mimari Kararlar ve Nedenleri

### 3.1 Katmanlı Mimari — Controller → Manager → Repository → Entity

```
İstek (HTTP)# CALISMA_RAPORU.md
# Akıllı Depo Yönetimi — Smart Warehouse Management System

---

## 1. Projenin Kısa Özeti

Bu proje, bir şirketin depo operasyonlarını dijital ortamda yönetebilmesini sağlayan,
çok kiracılı (multi-tenant) bir **Akıllı Depo Yönetimi** uygulamasıdır.

Temel senaryolar:
- Ürün tanımlama ve kategorilere ayırma
- Depo bölgesi (Zone) ve raf (Rack) yapısı kurma
- Depoya ürün girişi (IN) ve depodan ürün çıkışı (OUT)
- Anlık stok takibi ve stok hareket geçmişi
- Kritik stok seviyesi uyarısı

Uygulama tek sayfalı (SPA) bir arayüze sahiptir. Tüm modüller
(Ürünler, Stok Hareketleri, Kategoriler, Bölgeler, Raflar) sekmeler
halinde tek ekranda sunulmaktadır.

---

## 2. Kullanılan Teknolojiler ve Versiyonları

| Katman | Teknoloji | Versiyon |
|---|---|---|
| Backend Framework | ASP.NET Core Web API | .NET 9.0 |
| ORM | Entity Framework Core | 9.0.0 |
| Veritabanı | Microsoft SQL Server | 2019+ |
| API Dokümantasyonu | Swashbuckle (Swagger) | 6.9.0 |
| Frontend Framework | React | 18.x |
| Dil (Frontend) | TypeScript | 5.x |
| UI Kütüphanesi | Material UI (MUI) | 6.x |
| HTTP İstemcisi | Axios | 1.x |
| Paket Yöneticisi | npm | 10.x |
| Geliştirme Ortamı | Visual Studio / VS Code | — |

---

## 3. Mimari Kararlar ve Nedenleri

### 3.1 Katmanlı Mimari — Controller → Manager → Repository → Entity

```
İstek (HTTP)
    ↓
Controller       → HTTP doğrulama, CompanyId kontrolü, routing
    ↓
Manager          → İş mantığı, validasyon, DTO dönüşümü
    ↓
Repository       → Veri erişim, EF Core sorguları
    ↓
DbContext        → EF Core, SQL Server bağlantısı
    ↓
Veritabanı (MSSQL)
```

**Neden bu mimari?**# CALISMA_RAPORU.md
# Akıllı Depo Yönetimi — Smart Warehouse Management System

---

## 1. Projenin Kısa Özeti

Bu proje, bir şirketin depo operasyonlarını dijital ortamda yönetebilmesini sağlayan,
çok kiracılı (multi-tenant) bir **Akıllı Depo Yönetimi** uygulamasıdır.

Temel senaryolar:
- Ürün tanımlama ve kategorilere ayırma
- Depo bölgesi (Zone) ve raf (Rack) yapısı kurma
- Depoya ürün girişi (IN) ve depodan ürün çıkışı (OUT)
- Anlık stok takibi ve stok hareket geçmişi
- Kritik stok seviyesi uyarısı

Uygulama tek sayfalı (SPA) bir arayüze sahiptir. Tüm modüller
(Ürünler, Stok Hareketleri, Kategoriler, Bölgeler, Raflar) sekmeler
halinde tek ekranda sunulmaktadır.

---

## 2. Kullanılan Teknolojiler ve Versiyonları

| Katman | Teknoloji | Versiyon |
|---|---|---|
| Backend Framework | ASP.NET Core Web API | .NET 9.0 |
| ORM | Entity Framework Core | 9.0.0 |
| Veritabanı | Microsoft SQL Server | 2019+ |
| API Dokümantasyonu | Swashbuckle (Swagger) | 6.9.0 |
| Frontend Framework | React | 18.x |
| Dil (Frontend) | TypeScript | 5.x |
| UI Kütüphanesi | Material UI (MUI) | 6.x |
| HTTP İstemcisi | Axios | 1.x |
| Paket Yöneticisi | npm | 10.x |
| Geliştirme Ortamı | Visual Studio / VS Code | — |

---

## 3. Mimari Kararlar ve Nedenleri

### 3.1 Katmanlı Mimari — Controller → Manager → Repository → Entity

```
İstek (HTTP)
    ↓
Controller       → HTTP doğrulama, CompanyId kontrolü, routing
    ↓
Manager          → İş mantığı, validasyon, DTO dönüşümü
    ↓
Repository       → Veri erişim, EF Core sorguları
    ↓
DbContext        → EF Core, SQL Server bağlantısı
    ↓
Veritabanı (MSSQL)
```

**Neden bu mimari?**# CALISMA_RAPORU.md
# Akıllı Depo Yönetimi — Smart Warehouse Management System

---

## 1. Projenin Kısa Özeti

Bu proje, bir şirketin depo operasyonlarını dijital ortamda yönetebilmesini sağlayan,
çok kiracılı (multi-tenant) bir **Akıllı Depo Yönetimi** uygulamasıdır.

Temel senaryolar:
- Ürün tanımlama ve kategorilere ayırma
- Depo bölgesi (Zone) ve raf (Rack) yapısı kurma
- Depoya ürün girişi (IN) ve depodan ürün çıkışı (OUT)
- Anlık stok takibi ve stok hareket geçmişi
- Kritik stok seviyesi uyarısı

Uygulama tek sayfalı (SPA) bir arayüze sahiptir. Tüm modüller
(Ürünler, Stok Hareketleri, Kategoriler, Bölgeler, Raflar) sekmeler
halinde tek ekranda sunulmaktadır.

---

## 2. Kullanılan Teknolojiler ve Versiyonları

| Katman | Teknoloji | Versiyon |
|---|---|---|
| Backend Framework | ASP.NET Core Web API | .NET 9.0 |
| ORM | Entity Framework Core | 9.0.0 |
| Veritabanı | Microsoft SQL Server | 2019+ |
| API Dokümantasyonu | Swashbuckle (Swagger) | 6.9.0 |
| Frontend Framework | React | 18.x |
| Dil (Frontend) | TypeScript | 5.x |
| UI Kütüphanesi | Material UI (MUI) | 6.x |
| HTTP İstemcisi | Axios | 1.x |
| Paket Yöneticisi | npm | 10.x |
| Geliştirme Ortamı | Visual Studio / VS Code | — |

---

## 3. Mimari Kararlar ve Nedenleri

### 3.1 Katmanlı Mimari — Controller → Manager → Repository → Entity

```
İstek (HTTP)
    ↓
Controller       → HTTP doğrulama, CompanyId kontrolü, routing
    ↓
Manager          → İş mantığı, validasyon, DTO dönüşümü
    ↓
Repository       → Veri erişim, EF Core sorguları
    ↓
DbContext        → EF Core, SQL Server bağlantısı
    ↓
Veritabanı (MSSQL)
```

**Neden bu mimari?**# CALISMA_RAPORU.md
# Akıllı Depo Yönetimi — Smart Warehouse Management System

---

## 1. Projenin Kısa Özeti

Bu proje, bir şirketin depo operasyonlarını dijital ortamda yönetebilmesini sağlayan,
çok kiracılı (multi-tenant) bir **Akıllı Depo Yönetimi** uygulamasıdır.

Temel senaryolar:
- Ürün tanımlama ve kategorilere ayırma
- Depo bölgesi (Zone) ve raf (Rack) yapısı kurma
- Depoya ürün girişi (IN) ve depodan ürün çıkışı (OUT)
- Anlık stok takibi ve stok hareket geçmişi
- Kritik stok seviyesi uyarısı

Uygulama tek sayfalı (SPA) bir arayüze sahiptir. Tüm modüller
(Ürünler, Stok Hareketleri, Kategoriler, Bölgeler, Raflar) sekmeler
halinde tek ekranda sunulmaktadır.

---

## 2. Kullanılan Teknolojiler ve Versiyonları

| Katman | Teknoloji | Versiyon |
|---|---|---|
| Backend Framework | ASP.NET Core Web API | .NET 9.0 |
| ORM | Entity Framework Core | 9.0.0 |
| Veritabanı | Microsoft SQL Server | 2019+ |
| API Dokümantasyonu | Swashbuckle (Swagger) | 6.9.0 |
| Frontend Framework | React | 18.x |
| Dil (Frontend) | TypeScript | 5.x |
| UI Kütüphanesi | Material UI (MUI) | 6.x |
| HTTP İstemcisi | Axios | 1.x |
| Paket Yöneticisi | npm | 10.x |
| Geliştirme Ortamı | Visual Studio / VS Code | — |

---

## 3. Mimari Kararlar ve Nedenleri

### 3.1 Katmanlı Mimari — Controller → Manager → Repository → Entity

```
İstek (HTTP)
    ↓
Controller       → HTTP doğrulama, CompanyId kontrolü, routing
    ↓
Manager          → İş mantığı, validasyon, DTO dönüşümü
    ↓
Repository       → Veri erişim, EF Core sorguları
    ↓
DbContext        → EF Core, SQL Server bağlantısı
    ↓
Veritabanı (MSSQL)
```

**Neden bu mimari?**
    ↓
Controller       → HTTP doğrulama, CompanyId kontrolü, routing
    ↓
Manager          → İş mantığı, validasyon, DTO dönüşümü
    ↓
Repository       → Veri erişim, EF Core sorguları
    ↓
DbContext        → EF Core, SQL Server bağlantısı
    ↓
Veritabanı (MSSQL)
```

**Neden bu mimari?**

| # | Sorun | Neden Oluştu | Çözüm |
|---|---|---|---|
| 1 | `.HasDatabaseName()` derleme hatası | EF Core 9'da metod davranışı değişti | Metod kaldırıldı, EF otomatik isim üretir |
| 2 | `AddSwaggerGen` tanımsız hatası | .NET 9'da Swagger otomatik gelmiyor | `Swashbuckle.AspNetCore 6.9.0` paketi kuruldu |
| 3 | ConnectionString başlatılmadı hatası | appsettings.json yanlış instance adı | `MKUMRU\\MSSQLSERVER01` formatı kullanıldı |
| 4 | Frontend port uyuşmazlığı | api.ts yanlış port tanımlıydı | BASE_URL → `https://localhost:7235/api` güncellendi |
| 5 | MUI Grid `item` prop hatası | MUI v6'da Grid API değişti | `Grid2` bileşeni ve `size` prop kullanıldı |
| 6 | TypeScript import path hataları | Dosya yolları hatalı yazıldı | `../../types` relative path formatı kullanıldı |
| 7 | DbContext threading hatası | `Task.WhenAll` paralel DB sorgusu açtı | `foreach + await` sıralı döngüye geçildi |
| 8 | Kategori ürün sayısı 0 görünüyordu | Repository'de `Include(Products)` eksikti | `GetActiveCategoriesAsync` metoduna Include eklendi |
| 9 | Bölge raf sayısı 0 görünüyordu | Repository'de `Include(Racks)` eksikti | `GetAllAsync` metoduna Include eklendi |
| 10 | CORS hatası | Middleware sırası yanlıştı | UseRouting → UseCors → UseAuthorization sırası düzeltildi |

---


### Frontend UI/UX (%15)

- MUI v6 ile tutarlı ve modern arayüz.
- Dashboard özet kartları anlık durum görüntüsü sunar.
- Kritik stok uyarısı görsel olarak öne çıkar (uyarı ikonu + turuncu renk).
- Silme işlemlerinde onay dialogu kullanıcı hatasını önler.
- CompanyId AppBar'dan değiştirilebilir; çoklu tenant testi kolaylaşır.
- Tüm listeler yükleme durumu gösterir.
- Sayfalama kontrolleri Türkçe etiketlenmiştir.

### Server-Side Pagination (%10)

```
Ürün Listesi:
  GET /api/product/paged?companyId=COMP001&pageNumber=1&pageSize=10&searchTerm=laptop

Stok Hareketleri:
  GET /api/inventorytransaction/paged?companyId=COMP001&pageNumber=1&pageSize=10

Her ikisinde de:
  - Filtreleme DB'de (WHERE)
  - Sıralama DB'de (ORDER BY)
  - Sayfalama DB'de (SKIP / TAKE)
  - Frontend sadece parametre gönderir
```


## 6. Proje Dosya Yapısı

```
SmartWarehouse/
├── SmartWarehouse.API/                  ← .NET 9.0 Backend
│   ├── Controllers/                     ← 5 controller, 25 endpoint
│   ├── Managers/
│   │   ├── Interfaces/                  ← 5 interface
│   │   └── Implementations/             ← 5 manager
│   ├── Repositories/
│   │   ├── Interfaces/                  ← 6 interface
│   │   └── Implementations/             ← 6 repository
│   ├── DTOs/                            ← 20+ DTO sınıfı
│   ├── Entities/                        ← 7 entity (1 abstract base)
│   ├── Data/
│   │   ├── SmartWarehouseDbContext.cs
│   │   └── Migrations/
│   ├── appsettings.json
│   └── Program.cs
│
└── smart-warehouse-ui/                  ← React 18 Frontend
    └── src/
        ├── types/index.ts               ← Tüm TypeScript tipleri
        ├── services/                    ← 5 servis + Axios instance
        ├── context/CompanyContext.tsx   ← Global tenant yönetimi
        ├── components/                  ← 10 bileşen (5 Tab + 5 Modal)
        ├── pages/WarehousePage.tsx      ← Ana SPA sayfası
        └── App.tsx                      ← MUI Theme + Provider
```

---

## 9. Kurulum Talimatları

### Backend

```bash
cd SmartWarehouse.API

# appsettings.json içinde connection string güncellenir:
# "Server=SUNUCU_ADI\\INSTANCE;Database=SmartWarehouseDb;
#  Trusted_Connection=True;TrustServerCertificate=True;"

dotnet ef migrations add InitialCreate --output-dir Data/Migrations
dotnet ef database update
dotnet run
# Swagger UI → https://localhost:7235
```

### Frontend

```bash
cd smart-warehouse-ui
npm install
npm start
# Uygulama → http://localhost:3000
```

---

*Rapor Tarihi: Nisan 2025*
*Proje: Smart Warehouse Management System v1.0.0*
