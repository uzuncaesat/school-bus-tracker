# 🚌 Okyanus Servis — Okul Servisi Yönetim & Canlı Takip Sistemi

Okul servis taşımacılığı yapan bir işletme için **uçtan uca** geliştirilmiş; veli kaydı, sözleşme/belge saklama, ödeme takibi, güzergah yönetimi ve **GPS ile canlı araç takibi + yaklaşım bildirimi** sağlayan tam kapsamlı bir web uygulaması.

> Gerçek bir müşteri (Okyanus Koleji servis operasyonu) için tasarlandı, geliştirildi ve **canlıya alındı** — sabit alan adı, HTTPS ve Windows servisleri ile 7/24 çalışır durumda.

---

## ✨ Öne Çıkan Özellikler

### Kayıt & Veli
- **Online servis kayıt formu** — öğrenci/veli bilgileri, TC, adres, sağlık notu
- **Haritadan adres seçimi** — yazılan adresi otomatik haritada işaretleme (Leaflet + OpenStreetMap Nominatim geocoding)
- **Sözleşme / belge yükleme** — her veliye ait imzalı sözleşmeler veritabanında saklanır; veli portalından görüntüleme/indirme
- **Veli portalı** — her veli kendi kullanıcı adı/şifresiyle girer, yalnızca kendi çocuğunu görür
- **Kardeş / aile ilişkisi** — kardeşleri gruplama, tek veli girişiyle tüm çocukları görme

### Ödeme Takibi
- **Bölünebilir ödeme planı** — peşin/tek çekim + taksit ayrı ayrı (tutar, yöntem, banka)
- **Aile bazlı ödeme** — kardeşlerin ödemesi tek aile üzerinden takip edilir
- **Tahsilat paneli** — toplam ücret / tahsil edilen / kalan, borçlular, **bankaya göre dağılım**
- Nakit, Elden, Kredi Kartı, Havale/EFT, Taksit yöntemleri

### Güzergah & Operasyon
- **Araç–öğrenci ataması** ve **güzergah sırası** (öğrenci alınış sırası, sürükle-sırala mantığı)
- **İkinci araç** desteği (bir öğrenci iki güzergaha atanabilir)
- **Yazdırılabilir güzergah çıktısı** — şoför/hostes için adres + iletişim + sınıf bazlı otomatik notlar
- Excel/CSV dışa aktarma

### 🛰️ Canlı GPS Takibi (Arvento Entegrasyonu)
- **Arvento araç takip web servisi** (SOAP/XML) entegrasyonu
- Veli portalında **canlı harita** — çocuğun servisinin gerçek konumu, ~20 sn'de bir güncellenir (yalnızca servis saatlerinde)
- **Yaklaşım bildirimi** — servis eve belirlenen mesafeye yaklaşınca veliye otomatik bildirim
- Arka planda çalışan **`BackgroundService`** ile periyodik konum sorgulama + haversine mesafe hesabı
- Gerçek/mock veri arasında tek anahtarla geçiş (Arvento erişimi olmadan da geliştirme/test)

### 🔔 Bildirimler
- **Web Push (VAPID)** — service worker + PWA ile telefona bildirim (uygulama kapalıyken bile)
- Uygulama içi bildirim listesi, okunmadı rozeti
- (Planlı) Netgsm ile toplu SMS

---

## 🧱 Teknoloji Yığını

| Katman | Teknoloji |
|---|---|
| **Backend** | ASP.NET Core 9 Web API (controller tabanlı) |
| **Frontend** | Blazor WebAssembly (PWA) |
| **Veritabanı** | SQL Server (LocalDB → SQL Server Express), EF Core 9 (Code First, Migrations) |
| **Kimlik** | ASP.NET Core Identity (rol bazlı: Admin / Driver / Veli), token tabanlı |
| **Harita** | Leaflet + OpenStreetMap / CARTO döşemeleri, Nominatim geocoding |
| **GPS** | Arvento Filo Yönetim Web Servisleri (XML) |
| **Bildirim** | Web Push (VAPID), Service Worker |
| **Dağıtım** | Windows Service (Kestrel), Cloudflare Tunnel (sabit HTTPS alan adı) |

---

## 🏗️ Mimari

Katmanlı ve sade bir yapı benimsendi:

```
Entity  →  DTO  →  Mapper  →  Controller        (Backend, service katmanı olmadan)
Components (.razor)  →  ApiClient  →  API        (Frontend)
```

- **Tek origin dağıtımı:** Blazor WASM, API tarafından `UseBlazorFrameworkFiles` ile servis edilir — tek uygulama, tek adres, CORS'suz.
- **Entegrasyon soyutlaması:** `IArventoClient` arayüzü sayesinde gerçek Arvento servisi ile sahte (mock) konum kaynağı arasında tek yapılandırma değeriyle geçiş.
- **Arka plan işleri:** `ApproachEngine : BackgroundService` servis saatlerinde konumları çeker, mesafeyi hesaplar, bildirim üretir (sefer başına tekrar koruması ile).

### Proje Yapısı
```
OkyanusServis.sln
├── OkyanusServis.Api/          # ASP.NET Core Web API + Blazor host
│   ├── Controllers/            # Registrations, Vehicles, Documents, Config, Tracking, Push...
│   ├── Entities/               # EF Core varlıkları
│   ├── Dtos/  Mappers/         # Elle mapping (DTO ↔ Entity)
│   ├── Data/                   # DbContext, seeders, migrations
│   └── Integrations/Arvento/   # GPS istemcisi, yaklaşım motoru, web push
└── OkyanusServis.Web/          # Blazor WebAssembly (PWA)
    ├── Pages/  Layout/  Components/
    ├── Services/               # ApiClient, AuthService, AuthState
    └── wwwroot/                # theme.css, js (harita/push/dosya), manifest, service worker
```

---

## 🚀 Çalıştırma

Gereksinimler: **.NET 9 SDK**, **SQL Server LocalDB veya Express**.

```bash
# API + Blazor (tek uygulama)
cd OkyanusServis.Api
dotnet run
# → https://localhost:7060  /  http://localhost:5165
```

Veritabanı açılışta otomatik oluşturulur (EF Core migrate + seed). Varsayılan yönetici hesabı seed edilir.

> Canlı ortamda uygulama **Windows Service** olarak yayınlanır ve **Cloudflare Tunnel** ile sabit HTTPS alan adına bağlanır (repodaki `kur-servis.bat`, `guncelle.bat`, `tunel-onar.ps1` betikleri bu süreci otomatikleştirir).

---

## 📸 Ekran Görüntüleri

> _(Ekran görüntüleri buraya eklenecek: kayıt paneli, veli portalı canlı harita, güzergah yönetimi, tahsilat paneli.)_

---

## 🎯 Bu Projede Ele Alınan Konular

- Gerçek bir işletme sürecinin **uçtan uca dijitalleştirilmesi** ve canlıya alınması
- **3. parti SOAP/XML web servisi** (Arvento GPS) entegrasyonu, savunmacı ayrıştırma, hata teşhisi
- **Gerçek zamanlıya yakın takip**: arka plan servisi, periyodik sorgulama, coğrafi mesafe hesabı
- **Web Push / PWA** ile mobil bildirim
- **Rol bazlı yetkilendirme** ve veri izolasyonu (veli yalnızca kendi verisini görür)
- **EF Core Code First** ile evrimleşen şema (migration'lar)
- **DevOps**: Windows Service dağıtımı, Cloudflare Tunnel ile HTTPS, tek tıkla güncelleme betikleri

---

## 📄 Lisans

Bu depo bir portföy/gösterim projesidir. Tüm hakları saklıdır.
