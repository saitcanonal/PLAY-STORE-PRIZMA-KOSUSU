# V12 doğrulama ve teslim durumu

## Çalıştırılan kontroller

- Flutter 3.47.6 / Dart 3.13.5 ile 48 birim/widget testi geçti.
- Oyun motoru: 40 başlangıç tohumu, 5 ayrı 15 dakikalık rota ve 20/60/120 FPS tutarlılığı geçti.
- Uzun rotalarda hız 60 üstüne çıktı; ekrandaki nesne sayısı 63–82 aralığında kaldı.
- 40 ayrı 7 dakikalık rota, kapı açıklıkları ve statik küçük engel kontrolleri geçti.
- Güçlendirmeler, kalkan, hayalet, mıknatıs, çarpan, 14 dünya ve dört kilometre taşı kontrolleri geçti.
- Günlük görev, ödül tekilleştirme ve kalıcılık kontrolleri geçti.
- Python yayın denetiminin 5 testi geçti: test reklamı, eksik canlı kimlik, eksik destek bağlantısı, doğrulanmamış sunucu ve satın alma engeli.
- Android XML, iOS plist ve privacy manifest sözdizimi kontrol edildi.
- Gerçek Flutter render'ından ana ekran/günlük oyun önizlemeleri docs/v12-preview içinde.

## Testlerden çıkan düzeltmeler

1. GlassCard Material olarak düzenlendi: ListTile / ExpansionTile dokunma efektlerinin görünmesini engelleyen renkli Container sorunu giderildi.
2. Canlandırma motor tarafından koşu başına bir kez ile sınırlandı. Süre limitini dolduran koşu da tekrar canlandırılamaz.
3. Sınırsız canlandırma ve eski hız tavanını varsayan testler yeni davranışı doğrulayacak şekilde güncellendi.
4. Rota test sürücüsü kapıya yönelmeden önce aradaki engel satırını geçecek şekilde düzeltildi. Üretim parkuru bu test için değiştirilmedi.
5. Desktop profil testinin Android veritabanı kanalı kullanmasına neden olan platform kurulumu düzeltildi.

## Başlıca yayın değişiklikleri

- google_mobile_ads 6.0.0 → 9.1.0; UMP kararından önce reklam talebi yok.
- Ödül için SDK callback'i zorunlu; erken kapatma / gösterim hatası ödül kazandırmaz.
- Otomatik interstitial kaldırıldı. Eksik veya test kimlikleriyle canlı yayın engellenir.
- Rıza değişince önbellekteki reklamlar atılır. Eski callback'ler yeni tercihi ezemez.
- Önbellek 50 dakikada geçersizleşir; yükleme hatasında sınırlı tekrar deneme vardır.
- Android debug imzası kaldırıldı. Gerçek key.properties zorunlu.
- Android target/compile 36, minimum 24, AGP 8.13.1 / Gradle 8.13 / JDK 17.
- iOS App ID ayarı, release kontrolü, privacy manifest ve SKAdNetwork listesi eklendi.
- Gizlilik, destek, reklam bildirimi, lisanslar, yerel veri silme ve optional backend DELETE eklendi.
- Gerçek para ürünleri ve sunucu bağlantısı ilk yayın sürümünde kapalı; çalışmayan satış kartları gizlenir.
- Gerçek isim/soyadı harfi zorunlu değil. Sanal ve gerçek sıralamalar karıştırılmaz.

## Tamamlanması gereken dış kontroller

Bu paket imzalanmış AAB/IPA değildir. Apple, Google veya AdMob hesaplarına erişilmedi; mağazalara gönderilmedi.
Xcode, Android SDK ve gerçek cihaz bulunmadığından native build, imzalama, cihaz üzerinde reklam, UMP bölge senaryoları ve 16 KB binary testi yapılmadı.
48 Flutter testi ana değişikliklerden sonra geçti. Ardından in_app_purchase 3.3.1 / Android alt paketi 0.5.3 bağımlılık çözümlemesi, privacy hata yakalama koruması ve native sürüm ayarları tamamlandı. Son oturumda Flutter çalışma ortamı mevcut olmadığından bu son değişikliklerden sonra tam testler tekrar çalıştırılamadı. Yayın betiği her build öncesinde analyze ve test'i zorunlu çalıştırır.
C# / MySQL silme değişiklikleri kaynak üzerinden incelendi; .NET/MySQL çalışma testi yapılmadı. Sunucu bu nedenle yayın sürümünde engellidir.
Gerçek reklam kimlikleri, yayıncı bilgileri, politikaların yayımlanması, mağaza beyanları ve cihaz testleri tamamlanmalıdır.

İlk okunacak dosya: STORE-RELEASE.md.
