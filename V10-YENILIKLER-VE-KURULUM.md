# Prizma Koşusu V10 — 10.0.0+12

## Oynamak için

Sunucu veya hesap açmadan oynanabilir. İlk ekrandan **Misafir olarak oyna** seçilebilir.
Ana ekran: büyük OYNA, seçili top, sıradaki görünüm, üç ek mod, ustalık ve günlük görevler.

| Mod | Kural | Sıralama |
|---|---|---|
| Sonsuz koşu | Üç ortak can; reklamla bir kez ek can | Yardımsız tek koşu / haftalık lig |
| Günün parkuru | Türkiye gününe göre aynı seed; 90 saniye, üç can | O günün doğrulanmış puanı |
| Kristal meydan okuması | 60 saniye, üç can | Haftalık kristal modu puanı |
| Tek can | Çarpışma veya boşluğa düşmede biter | Haftalık mesafe |

Günlük ve kısa modlarda ek can yoktur. Modların yerel rekorları ayrı tutulur.
Günün değişmesi 00:00 Türkiye saatine göredir; gece yarısını aşan koşu başladığı güne yazılır.
Parkur rastgele sıradan engeller yerine zikzak, düz kristal hattı, ödül yolu ve dinlenme parçalarını birleştirir. Dar köprüler dünyaya göre değişir; kristal dünyalarında seriler sıklaşır, kapı dünyalarında tam yol genişliğindeki geçitler bulunur. Küçük engeller normal sabit bloktur.
Hız 24 m/s, turbo ile 30 m/s sınırına sahiptir. Satırlar 8–15 m aralığına açılır. Köprü çevresindeki bütün 49 nokta kontrol edilir; yalnız uçlara bakılmaz. Her sırada erişilebilir güvenli hat korunur.

## Görünüm, ustalık ve kontrol

- Mağaza: top + aksesuar + desen + iz birlikte hareketli önizlenir. Altı favori kombinasyon kaydedilir.
- Ustalık: toplam mesafe, hasarsız tamamlanan koşular, keşfedilen dünyalar ve kristal serisi. Tekrar kayıt aynı koşuyu ikinci kez saymaz.
- Kaşif (1.000 m): neon profil çerçevesi. Maratoncu (10.000 m): krom desen. Kusursuz (3 hasarsız biten koşu): taç. Dünya gezgini (5 dünya): galaksi çerçevesi. Kristal ustası (30 seri): yıldız tozu izi.
- Ayarlar: 0,5–2× hassasiyet, ekranın alt %28'inden kontrol ve yüksek kontrast tehlike işaretleri.
- Sonuç ekranı: moda uygun rekor farkı, görünüm için gereken jeton, toplam mesafe, rozet sayısı ve hazır günlük görevler.
- Bütün kozmetikler herkese açıktır; ücretli görünüm hız, can veya puan artırmaz.

## Xcode ile açma

Mac Terminal'de açılmış ZIP klasörüne gir:

```bash
cd prisma_kosusu
bash scripts/open_xcode.sh
```

Flutter 3.41+ ve CocoaPods gerekir. Script paketleri indirir, Pod kurar, simülatör build'i alır ve `app/ios/Runner.xcworkspace` dosyasını açar. Xcode'da Runner şemasını ve iPhone simülatörünü seçip ▶ tuşuna bas. `Runner.xcodeproj` yerine workspace kullan.
Linux'ta iOS build/Keychain veya Android Keystore cihaz çalıştırması yapılamadı; bu native kontroller Mac/telefon üzerinde tamamlanmalıdır. Önceki açılış ekranı ve hata durumunda yeniden deneme korunur.

## C# + MySQL backend

Mevcut veritabanına `progression` tablosu eklenir. Oyuncular, eski biletler ve kayıtlar korunur. Mobil uygulama ve sunucu birlikte güncellenmelidir: **replayVersion 11**. Eski replay sürümleri server'da tutulur.

```bash
cd server
# .env içerisine MYSQL_ROOT_PASSWORD ve MYSQL_PASSWORD değerlerini kendin yaz.
# Yönetim için ayrıca en az 32 karakter rastgele PRISMA_ADMIN_TOKEN belirle.
docker compose up --build -d
```

Mobil geliştirme:

```bash
cd app
flutter run --dart-define=PRISMA_API=http://127.0.0.1:5080
```

Bu adres iOS simülatöründe Mac'i gösterir; Android emülatöründe `10.0.2.2:5080` kullanılır. Fiziksel telefon / yayın için HTTPS API adresi gerekir. MySQL şifresi ve yönetici anahtarı mobil uygulamaya konulmaz.

### İki cüzdanın kapsamı

Cihaz jetonları çevrimdışı koşular, günlük hediyeler ve mevcut kozmetik mağazası içindir. Jeton geçmişi artık her kazanım/harcamanın nedeni ve bakiyesini saklar. Yeni sürümden önceki işlemler geriye dönük uydurulmaz.

Çevrimiçi cüzdan yalnız server'da doğrulanmış koşu ve canlı görev ödüllerini kabul eder. Ana ekrandaki **Çevrimiçi cüzdan** üzerinden işlem geçmişi görülebilir ve sunucu fiyatlarıyla top alınabilir. Aynı koşu, görev veya top ikinci kez ödeme yapmaz. İki bakiye ayrı gösterilir: düzenlenebilir yerel yedekten çevrimiçi para üretilemez. Yerel kozmetikler oyun gücü değiştirmediği için mevcut mağaza çevrimdışıyken çalışır.

### Hesap kurtarma ve gizlilik

Misafir oynamak için e-posta / sosyal giriş gerekmez. Sunucu varsa mevcut misafir kimliğiyle bulut yedeği kullanılır. Kurtarma ekranında verilen **PKR1** kodu server'da sadece hash olarak saklanır. Kod tek kullanımlıktır; geri yükleme eski oturumu iptal eder, yeni bir token üretir. Yeni kurtarma kodu üretmek önceki kodu iptal eder. iOS Keychain / Android Keystore token'ı saklar; önceki düz metin cihaz kaydı başarılı taşımadan sonra silinir. Çevrimdışı **PK4** yedeği desteklenir. Apple/Google/e-posta ile hesap bağlama bu sürümün kapsamına eklenmedi; bulut kurtarma kod üzerinden yapılır.

Ölçümler varsayılan kapalıdır. Kullanıcı açarsa çarpışma, koşu sonu, bırakma, reklam dönüşü ve Flutter hata türü gönderilir. Hata metni, isim ve cihaz kimliği gönderilmez; işletim sistemi etiketi tutulur. Olaylar en fazla 30 gün / 20.000 kayıt tutulur. Crashlytics/Sentry hizmeti bağlanmadı; kendi API'nizde ölçüm ve hata olayı altyapısı vardır.

### Yönetim ve izleme

`http://127.0.0.1:5080/admin` adresini aç. Yönetici anahtarını gir. Görünüm fiyatları, günlük kristal görevleri ve tarihli etkinlikler form üzerinden yönetilir. Koşular onaylanabilir, bekletilebilir veya reddedilebilir. Reddetme dönem sıralamasını ve V10 tüm zamanlar rekorunu yeniden hesaplar. V10 öncesi kayıtlar inceleme tablosunda yoktur ve ayrı başlangıç rekoru olarak korunur. Görev hedefleri yalnız doğrulanmış kristallerle karşılanır.

`/health` API durumu, `/health/ready` veritabanı bağlantısı içindir. `scripts/check_health.py` bunları kontrol eder; servis sağlıksızsa sıfırdan farklı çıkış kodu verir. Hosting'in izleme hizmetinde bunu zamanlayabilirsin. Sunucu exception middleware'i standart hata ve traceId döndürür; ayrıntı sunucu logunda kalır.

### Yedek ve geri yükleme

```bash
python3 -m pip install pymysql
# PRISMA_MYSQL bağlantısı ortam değişkeninde bulunmalı.
python3 scripts/backup_mysql.py export prisma-backup.json
# Boş bir hedef veritabanı oluştur, PRISMA_MYSQL'i bu hedefe değiştir.
python3 scripts/backup_mysql.py restore prisma-backup.json
```

Yedek tutarlı transaction snapshot'ıdır; SHA-256 dosyasıyla kontrol edilir. Restore boş veritabanı şartını ve tablo satır sayılarını denetler. Boş hedef şartı gerçek kayıtların üzerine yanlışlıkla yazmayı önler. Yedek dosyalarını kaynak kod deposuna koyma. Otomatik yedek takvimi ve harici depolama sunucu kurulumunda ayarlanmalıdır.

## Doğrulama

- Dart analyzer: hata/uyarı yok.
- Flutter motorunda 40 test: küçük ekranlar, mod açılışı, kaydetme, alışveriş, kurtarma, tekrar ödül, hassasiyet ve yeni alanlar.
- 40 rastgele seed; beş seed'de 15 dakika, güvenli kristal hattını izleyen bot 0 hasar; 20/60/120 fps eşdeğerliği.
- 21 gerçek Dart/C# replay: bütün modlarda skor, kristal, mesafe ve bitiş tick'i aynı.
- C# kaynakları .NET 8 compiler ile uyarılar hata sayılarak derlendi; replay self-test geçti.
- Canlı MariaDB 10.11 + MySqlConnector üzerinde 60 API/persistence kontrolü; gerçek yedek dosyası boş veritabanına geri yüklendi ve API yeni veritabanında başlatılıp kimlik, cüzdan, etkinlik tekrar doğrulandı. Docker MySQL 8.4 ve native iOS/Android build burada çalıştırılmadı.
- `docs/v10-preview` gerçek Flutter home / günlük koşu ekran çıktıları içerir.

Standart ortamda `bash scripts/test.sh`. HTTP testleri için ayrı test veritabanı yetkisiyle `PRISMA_MYSQL_TEST` ayarla ve `python3 scripts/test_api.py` çalıştır; script yalnız `prisma_test_` ile başlayan geçici veritabanını oluşturup siler.

Mevcut backend tek API instance ve bellekte çalışma modeli kullanır; çok sunuculu/milyonlarca oyunculu yayın için dağıtık kilit, ayrı indeksli ledger/telemetry tabloları ve kuyruğa taşıma ayrıca gerekir. Reklam ve IAP mağaza kimlikleri yayın ortamında yapılandırılmalıdır.
