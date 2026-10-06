# Prizma Koşusu V11 — parkurlar, meteorlar ve ilerleme

## Eklenenler

- Sekizer satırlık 20 elle tanımlanmış parkur. Bir döngüde 20 parça kullanılır; aynı parça art arda gelmez. Parça sırası koşunun seed değerinden türetilir; günlük parkur sabit kalır.
- Güvenli ana kristal hattına ek olarak yoğun, pembe kristalli isteğe bağlı ödül yolu. Dar yan koridorlarda üçlü kristal grupları ve x2 kristal bulunur. Lavda boşlukla ayrılmış tek şeritli ödül adacığına, adacıktan önceki geniş bağlantıdan girilir; bitişte tekrar geniş yola bağlanır. Ana rota her zaman korunur.
- Uzayda kristal kavisleri, ormanda kök slalomu, buzda iki şeritli kristal köprüleri ve lavda ödül adacıkları. Kontrol hassasiyeti dünya değişince değişmez.
- İlk oynayış için metin penceresi yerine etkileşimli deneme: sürükle → üç kristal topla → engelden kaç. Olağan tamamlanma yaklaşık 15–20 saniye; oyuncu yapana kadar adımlar bekler. Can, cüzdan ve sıralama etkilenmez. Ardından asıl koşu temiz motor ve geri sayımla başlar.
- Günlük üç görev, sekiz farklı görevden deterministik seçilir. Kalkanla çarpışmayı önleme, hasarsız mesafe ve yeni dünyaya ulaşma dahil. Gün Türkiye saatine göre döner. Tekrarlanan bitiş/kurtarma kayıtları ilerlemeyi veya ödülü çoğaltmaz.
- İlk üç koşudan sonra Benekli desen ücretsiz açılır. Ana ekranda hedef desen seçilir; koşu sonunda hedefin jeton ilerlemesi görünür. Yeterli jeton varsa ana ekrandan açılıp kullanılabilir. Hedef seçmek satın alma yapmaz.
- Günün parkurunda önceki en iyi koşunun isteğe bağlı, şeffaf hayaleti. Hareket kayıtları gerçek motorla yeniden oynatılır. Farklı tarih, seed veya motor sürümündeki kayıtlar kullanılmaz. Ayarlardan kapatılır; fiziksel çarpışması veya ödülü yoktur.
- İlk üç yeniden deneme otomatik reklamsızdır. Sonraki tam ekran reklamlar için en az üç biten koşu ve iki dakika aralık gerekir; ilk otomatik reklam dördüncü koşudan önce çıkmaz. Ödüllü reklam oyuncunun seçimine ve SDK ödül geri çağrısına bağlı kalır.
- Orijinal sentezlenmiş dört WAV efekt: kristal notası, tok çarpışma, cam benzeri kalkan kırılması, dünya/portal geçişi. iOS AVAudioPlayer ve Android SoundPool bağlantıları hazırdır.

## Görsellik ve mekanik düzeltmeler

Meteor yağmuru artık ön uyarı halkaları, renkli parlak çekirdekler, uzun incelen kuyruklar, kıvılcımlar ve çarpmada yayılan halkalar/parçacıklar içerir. Meteorlar dekoratiftir, can eksiltmez; inişler yolun dışında kalır. Düşük efekt ayarında sayı azaltılır. Dünya geçişi gösterisi ile üst üste bindirilmez.

Uzak engellerin tabanları ve işaretleri belirginleştirildi. Normal kristal toplamanın küçük parıltıları, seri eşiği/hasar vurgularından ayrıldı. Olay başlıkları kısa süre görünür; sabit açıklama eklenmedi. Altı şerit korundu.

Büyük dikey kapı artık yaklaşma mesafesinden değil 60 Hz simülasyon saatinden hareket alır. Geniş açıklık gerçekten kapanabilir; tek şeritlik sabit kaçış açıklığı hiçbir zaman kapanmaz. Çizim ve çarpışma aynı fazı kullanır. Yatay kapı da aynı saatle çalışır.

Prizma Muhafızı 20.000 puanda üç gerçek engel dalgası oluşturur. Dalgalarda belirgin renk ve açık kaçış şeridi vardır. Henüz görünmeyen mesafeden başlayarak yaklaşırlar. Küçük normal engeller sabit kalır.

İsteğe bağlı kristalleri kaçırmak seriyi bozmaz; seri hasarda/kalkan darbesinde sıfırlanır. Puan hâlâ yalnızca kristal ve mevcut kıl payı bonusundan gelir; yürünmüş mesafe kendiliğinden puan vermez.

## Sunucu

Dart üretimi ile C# ReplayEngine eşleştirildi. Yeni biletler protokol 13 kullanır; eski doğrulayıcı dalları korundu. Mobil uygulama ve sunucu birlikte güncellenmelidir. Mevcut C# / MySQL, cüzdan ve kurtarma yapısı devam eder.

İsteğe bağlı analitiğe parkur parçası, dünya, mesafe ve son hasar nedeni eklendi. Yönetici ekranı parça/dünya bazında çarpışma, bırakma, bitiş sayısı ve ortalama çıkış mesafesini gösterir. Ölçümler ayarlardaki izin açıkken gönderilir. Bunlar istemci ölçümleridir, hileye dayanıklı finansal kanıt değildir.

Satın alma makbuzu doğrulaması hâlâ harici doğrulama hizmeti ister. Bu sürüm App Store/Play yayını veya canlı ödeme doğrulaması yapmaz.

## Doğrulama

- Linux Flutter 3.41.5 / Dart 3.11.3 SDK ile analiz ve çevrimdışı Flutter test çalıştırıcısı.
- Oynanabilir öğretici, görevlerde tekrar ödül engeli, hayalet uyumu, koleksiyon kayıtları, reklam aralığı, motor ve arayüz regresyonları.
- 320 ve 390 piksel telefon genişliklerinde meteor/az efekt/hayalet çizimi; dar ekran taşma kontrolleri.
- Beş seed için 15'er dakikalık güvenli ana rota: sıfır hasar, sınırlı nesne sayısı ve hız.
- 40 seed için yedi dakikalık büyük kapı kontrolleri; 20/60/120 FPS deterministik simülasyon.
- 21 yeni ve 21 eski sürüm Dart/C# koşu eşleştirmesi; C# geçersiz giriş testleri ve tüm sunucu kaynaklarının derleme kontrolü.

Standart Flutter hazırlığı bu ortamın otomatik güvenlik kontrolünde engellendiğinden açık paket arşivleri ve doğrudan çevrimdışı SDK araçları kullanıldı. Native iOS/Android derlemesi, cihazda ses/haptik performansı, gerçek reklam ve ödeme akışları, canlı MySQL entegrasyonu burada çalıştırılmadı. Mac'te aşağıdaki kontrolü tamamla.

## Mac'te açma

```bash
cd prisma_kosusu
bash scripts/open_xcode.sh
```

Script Flutter bağımlılıklarını, CocoaPods'u ve iOS simülatör build'ini hazırlar; `app/ios/Runner.xcworkspace` açılır. Xcode'da bir iPhone simülatörü seçip Run düğmesine bas. `.xcodeproj` yerine `.xcworkspace` kullan.

Yerel tam test için: `bash scripts/test.sh`. Flutter 3.41 veya üstü, .NET 8 ve iOS için Xcode/CocoaPods gerekir.
