# V5 MySQL kurulumu

Telefon → HTTPS API → MySQL. Telefonda çevrimdışı çalışmak için SQLite kalır. MySQL şifresi Flutter içine yazılmaz. ZIP canlı sunucu içermez; sunucu adresin ayarlanana kadar bot ligi ve yerel oyun çalışır.

## Docker ile yerel geliştirme

`server/.env.example` dosyasını `server/.env` olarak kopyala, iki örnek parolayı güçlü ve farklı parolalarla değiştir. Parolalarda yerel örneğin bağlantı metni ayracı `;` kullanma. `.env` dosyasını paylaşma.

```bash
cd server
docker compose up --build -d
```

API `http://127.0.0.1:5080/health`. MySQL portu dışarı açılmaz; veri `mysql-data` hacminde tutulur. `docker compose down -v` veriyi siler: kullanma. Docker akışı bu ortamda çalıştırılmadı.

## Var olan MySQL ile

Önce UTF-8 veritabanı ve yalnız bu veritabanına erişen kullanıcı oluştur. İlk başlatmada tablolar otomatik oluşturulur; kullanıcının CREATE/INDEX/REFERENCES ve okuma/yazma izinleri gerekir. Uygulama tek API örneğini destekler. MySQL 8.4 hedeflenir; API testleri MySQL protokolünü kullanan MariaDB 10.11 üzerinde yapıldı.

```bash
export PRISMA_MYSQL='Server=127.0.0.1;Port=3306;Database=prisma;User ID=prisma_app;Password=KENDI_PAROLAN;SslMode=Required'
cd server
dotnet run
```

`SslMode=Required` yalnız şifreleme ister; üretimde sunucunun CA/hostname doğrulaması için `SslMode=VerifyFull` ve sağlayıcının CA ayarını kullan. Yerel Docker ağı `SslMode=None` kullanır; bunu uzak üretim bağlantısına kopyalama. Üretimde API için HTTPS ve gerçek hostname ile AllowedHosts ayarı gerekir. Sunucu .NET 8 gerektirir.

Flutter'da yalnız API adresini ayarla:

```bash
flutter run --dart-define=PRISMA_API=https://SENIN-SUNUCUN.example
```

## V4 SQLite sunucu kaydını taşıma

Eski API'yi durdur, `league.sqlite` dosyasını yedekle. MySQL hedefi boş olmalı. PRISMA_MYSQL ayarlıyken:

```bash
dotnet run -- --import-sqlite /TAM/YOL/league.sqlite
```

Oyuncu kimliği, token hash, rekor, bilet, satın alma sahipliği ve ilerleme yedeği aktarılır. Eski SQLite dosyasına yazılmaz. Dolu hedefe aktarım reddedilir. Yalnız V4 replay biçimi kabul edilir; replay sürümü oyun fiziği değişmediği için 4 kalır. Ardından API'yi normal başlat. Aynı API adresini kullanmak mevcut kurtarma kodlarının bağlantısını korur.

## Yedek ve geri dönüş

MySQL sağlayıcısının otomatik yedeğini veya mysqldump kullan; geri yüklemeyi ayrı veritabanında dene. Kullanıcı kodunu gizli saklamalı. Oyunu silip yeniden yüklemek cihazdaki SQLite kaydını siler. Eski profile geri dönüş hâlâ kurtarma koduyla yapılır; otomatik Apple/Google hesap girişi bu sürümde yoktur. Görev jetonları yerel oyun ödülüdür; sunucu doğrulaması gerektiren para veya sıralama puanı değildir.

## Entegrasyon testi

```bash
python -m pip install pymysql
export PRISMA_MYSQL_TEST='Server=127.0.0.1;Port=3306;Database=prisma_test_local;User ID=TEST_KULLANICISI;Password=TEST_PAROLASI;SslMode=None'
python scripts/test_api.py
```

Test kullanıcı hesabının test veritabanı oluşturma/silme izni olmalı. Script `prisma_test_` ön ekini zorunlu tutar; rastgele yeni test veritabanı oluşturup sonunda yalnız bu veritabanını siler. Canlı kimlik bilgilerini verme. DLL önce `dotnet build server -c Release` ile hazırlanmalıdır; DOTNET_BIN, DART_BIN ve PRISMA_TEST_DLL değişkenleriyle araç yolları seçilebilir.
