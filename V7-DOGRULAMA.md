# V7 — 3 Ekim 2026

## Güncelleme

V6 üzerine eklenmiştir. İlk kayan blok 13,1 saniyede, açılır kapı 16,52
saniyede, piston 19,75 saniyede gelir. İlk bölümde üç tür de görünür; daha
sonra dar köprü çevresi hariç her altıncı uygun satırda bir tuzak oluşur.
Güvenli kristal şeridi korunur. Kapının alt parçası yukarı kalkar; çerçevesi,
uyarı zemini ve yön oku görünür. Ekranda engelin adı ve geçiş ipucu yazılır.

Desen ve arka iz ayrı seçilir. Jetonla alınabilen ve makbuzla doğrulanan
premium stiller ayrıdır. Renk/efekt puan veya hız avantajı sağlamaz.
Premium paketlerin sunucu allowlist'i ve mobil ürün listesi güncellenmiştir.
Sıralama avatarlarında desenler, bere/taç/atkı vardır. Rakip etiketleri
kullanılır; antrenman liginde sanal rakip açıklaması korunur.

## Yapılan kontroller

- Oyun motoru ve stil kataloğu Dart statik analizi geçti.
- Dart/C# kaynaklarında sözdizimi ayrıştırması geçti; bu Flutter tip testi değildir.
- 40 seed, 40 iki dakikalık güvenli yol, tüm tuzak türleri ve ilk karşılaşma
  zamanları doğrulandı. Kayan blok güncel konumunda can düşürdü; açık kapı ve
  piston hasar vermedi, kapalı durumda bir can düştü.
- Beş 15 dakikalık güvenli koşu sıfır çarpışmayla geçti; bellekte en fazla 53
  etkin nesne tutuldu. 20/60/120 FPS fizik sonuçları aynı kaldı.
- Günlük görev/ödül/kalıcılık kontrolleri geçti.
- C# .NET 8 Roslyn derlemesi uyarılar hata sayılarak geçti; replay self-test geçti.
- MariaDB 10.11.14 / MySqlConnector üzerinde 36 API/aktarım/replay kontrolü geçti.
  Yeni ürünler validator yokken 503 ile reddedildi; bilinmeyen ürün reddedildi.
- 12 bir dakikayı aşan koşuda Dart/C# puan, tick ve mesafe eşleşti.

## Henüz doğrulanmayanlar

Flutter runtime bu oturumda çalıştırılmadı. Yeni desen/iz/mağaza/avatarlara ait
çalışır arayüz görüntüsü, Flutter analyze/test, iOS/Android derlemesi, gerçek
cihaz FPS/ısınma ve ödeme/reklam SDK testleri yapılmadı. appearance_test.dart
ücretin tek kesilmesi, kalıcılık, premium kilit ve kayıt geri yükleme kontrolü
için eklendi; Flutter testleri Mac'te çalıştırılmalıdır.

Gerçek mağaza ürünleri ve receipt validator yapılandırılmadan satın alma
aktif değildir. Kurulum: YAYIN_AYARLARI.md V7 bölümü. Oracle MySQL 8.4/Docker
akışı burada çalıştırılmadı; API kontrolleri MariaDB'de yapıldı.

## Mac'te kontrol

1. Yeni ZIP'i ayrı klasöre çıkar; app içinde flutter pub get.
2. app/ios içinde pod install; Runner.xcworkspace ile Xcode'dan Run.
3. scripts/test.sh çalıştır. İlk 25 saniyede üç tuzak türünü gözle.
4. Küreler sekmesinde desen ve iz seç; koşuya gir, kapat/aç, seçimin korunmasını dene.
5. Sıralama avatarlarını küçük ekran/büyük yazıda kontrol et.
6. Mağaza test hesabıyla desen/iz/prizma paketini satın al ve geri yükle.
7. Mobil uygulama ve C# sunucusunu beraber güncelle: replayVersion 7.
   V6 ve V4 koşu biletlerinin doğrulaması korunur, eski rekorlar silinmez.
