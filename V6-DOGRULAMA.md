# V6 doğrulama — 3 Ekim 2026

## Değişiklikler

V5 üzerine üç hareketli engel eklenmiştir: kayan blok, kapı ve zeminden çıkan
piston. Özel engeller 430 metreden önce üretilmez; ilk görünüşü parkur düzenine
bağlı olarak daha geç olabilir. Her altıncı uygun satırda en fazla bir özel
engel vardır. Engelin iki şeritlik hareket alanı kristalin güvenli şeridine
uzanmaz; dar köprü yakınlarında üretilmez. Açık kapı/piston çarpışma üretmez.
Yön okları, uyarı zemini ve yaklaşan engel yazısı eklenmiştir. Top yaklaşık
%15 büyütülmüştür; kontrol ve çarpışma toleransı V5 ile aynıdır.

## Yapılan kontroller

- Üretim Dart engine.dart statik analizi: hata yok.
- 21 Dart ve 5 C# kaynak dosyasında sözdizimi ayrıştırması geçti. Bu, Flutter
  tip analizi veya arayüz render testi değildir.
- 40 seed ile koşunun üç can sonrası bitmesi ve 20/60/120 FPS adım tutarlılığı.
- Beş ayrı 15 dakikalık güvenli yol: 24.435 metre, sıfır çarpışma, en fazla
  53 etkin nesne. Dar köprüler açık kaldı.
- 40 ayrı iki dakikalık hareketli engel testi: üç tür de oluştu, güvenli şerit
  hareket döngüsü boyunca açık kaldı, köprü mesafeleri korundu.
- Kayan bloğun hareket eden konumunda can kaybı; kapı ve piston açıkken hasar
  olmaması, kapalıyken bir can düşmesi geçti.
- Günlük görev, devam sonrası ödül farkı, çift ödül engeli, kayıt ve gün değişimi
  kontrolleri geçti.
- C# .NET 8 Roslyn derlemesi uyarılar hata sayılarak geçti; replay self-test geçti.
- On iki ayrı koşu önce 60 saniye güvenli kristal yolunu izledi, sonra düşerek
  bitti. Dart ve C# puan, tick ve mesafeyi aynı hesapladı.
- MySqlConnector ile MariaDB 10.11.14 üzerinde 32 API/aktarım/replay kontrolü
  geçti. Eski kayıt aktarımı, yetkisiz erişim, sahte satın alma reddi, yeniden
  başlatma sonrası kalıcılık ve bir dakikayı aşan Dart/C# koşusu doğrulandı.
  Örnek: 25.750 puan, 3.821 tick.

## Yapılmayan kontroller

Bu oturumda Flutter runtime çalıştırılmadı. Flutter analyze/test, yeni arayüz
renderi, iOS/Android build, gerçek cihaz FPS/ısınma, reklam ve ödeme SDK'larının
cihaz testi yapılmadı. Önceki paketin Flutter çalıştırma engeli dolanılmadı.
Oracle MySQL 8.4 ve Docker Compose akışı çalıştırılmadı; veritabanı testi
MySQL protokolü kullanan MariaDB üzerinde yapıldı.

## Mac'te kabul kontrolü

1. app klasöründe flutter pub get; scripts/test.sh ile tam kontroller.
2. app/ios içinde pod install; Runner.xcworkspace ile iOS 15+ simülatör.
3. Geniş yolda 60 saniyeden uzun oyna; kayan blok, kapı ve piston gör.
4. Açık/kapalı kapı geçişini, kayan blok çarpışmasını ve üç can akışını dene.
5. Duraklat/devam et, arka plana al/geri dön; hareketlerin donup sürmesini dene.
6. Düşük efekt, küçük ekran ve büyük yazı ayarında uyarıların okunurluğunu dene.
7. Tekrar dene ve ödüllü devam, günlük görev, kozmetik, kurtarma ve lig akışını dene.
8. Canlı uygulama ve C# sunucusunu birlikte V6'ya güncelle. Puan protokolü 6'dır.

## Kayıt uyumluluğu

Profil anahtarları, bundle kimlikleri, rekorlar ve kozmetik kayıtları korunur.
Uygulamayı silmeden güncelle. MySQL tickets tablosuna replay_version sütunu
otomatik eklenir; eski biletler 4, yeni biletler 6 ile doğrulanır. Eski rekorlar
korunur. ALTER izni gerekir; gerçek veritabanında güncelleme öncesi yedek al.
