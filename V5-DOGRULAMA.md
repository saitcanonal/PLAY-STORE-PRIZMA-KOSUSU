# V5 doğrulama — 3 Ekim 2026

## Çalıştırılan kontroller

- Üretim Dart oyun motorunda 40 seed; beş adet 15 dakikalık güvenli yol: her biri 24.435 metre, 0 çarpışma, en fazla 54 etkin nesne. Köprü açıklıkları ve 20/60/120 FPS adımlarında sonuç tutarlılığı geçti.
- Günlük görevler: koşu tekrarı, devam sonrası yalnız ek kristaller, tek ödül, serileştirme, ertesi gün sıfırlama ve geçersiz sayılar geçti.
- engine.dart ve challenges.dart Dart statik analizinde hata yok.
- 20 Dart ve 5 C# kaynak dosyası sözdizimi ayrıştırması geçti. Bu kontrol Flutter tip analizi yerine geçmez.
- C# kaynakları .NET 8 Roslyn ile uyarılar hata sayılarak derlendi; replay self-test geçti.
- MySqlConnector 2.4.0 ile MariaDB 10.11.14 üzerinde 32 aktarım/HTTP/replay doğrulaması geçti: SQLite V4 kimlik ve rekor aktarımı, token ile eski yedeğe erişim, oyuncu izolasyonu, geçersiz giriş, sahte satın alma reddi, yetkisiz koşu erişimi, sunucuda puan hesaplama, tekrar gönderim ve yeniden başlatma sonrası kalıcılık.
- Dart ile C# aynı koşuda aynı puanı hesapladı. Oyun fiziği/replay sürümü 4 kalır.

## Çalıştırılamayan kontroller

Flutter'ın önceki çalıştırması ortamın otomatik güvenlik incelemesince reddedildi; hassas bulut metadata adresine erişim denemesi belirtilmişti. Bu engel dolanılmadı. Bu nedenle Flutter analyze/test, yeni arayüzün çalışır görüntüsü, iOS/Android derlemesi ve gerçek cihaz FPS/ısınma ölçümü yapılmadı. Standart dotnet build bu ortamın /proc kısıtına takıldığı için doğrudan C# derleyicisi kullanıldı.

Oracle MySQL 8.4 ve Docker Compose akışı burada çalıştırılmadı; test veritabanı MySQL protokolü kullanan MariaDB idi. ZIP içinde canlı sunucu, gerçek reklam hesabı veya mağaza onayı yok.

## Mac'te son kabul

1. app klasöründe flutter pub get ve scripts/test.sh.
2. ios klasöründe pod install; Runner.xcworkspace dosyasını Xcode ile aç, iOS 15+ cihaz seç.
3. İlk oyun öğretimini tamamla, dar köprüden geç, duraklat/devam et.
4. Günlük hedef ödülünü al, uygulamayı kapat/aç, ödülün tekrar verilmediğini kontrol et.
5. Düşük efekt açık/kapalı karşılaştır; 320 piksel genişlikte ve büyük yazı ayarında taşma kontrolü yap.
6. Kendi test MySQL sunucunla scripts/test_api.py; ardından HTTPS API adresini uygulamaya tanımla.
7. Reklam hazırlanmamışken buton, ödüllü devam ve tekrar dene akışını gerçek test reklamıyla kontrol et.

Yeni paket önceki dosyaların üstüne parça parça kopyalanmamalı. Uygulama bundle kimliği korunursa normal güncellemede yerel profil korunur; silip yeniden kurulumda kurtarma kodu gerekir.
