# V9 doğrulama — 3 Ekim 2026

## Geçen kontroller

- Dart kod analizi: `lib` ve `test` için hata/uyarı yok.
- Gerçek Flutter 3.41.0 Linux test motorunda 31 test + 2 suite hazırlık/temizlik
  adımı: tamamı geçti. Motor, profil, satın alma hakları, yedekleme, karakter
  görselleri ve ekran etkileşimleri dahil.
- 320×640 ve 390×844 ekranlarında ana sayfa, dünyalar, mağaza ve oyun kontrolleri.
- Oyun başlatma, kaydırma, duraklatma/devam, çıkış, sonuç kaydı ve tekrar başlatma.
- Geç yanıt veren yönlendirme kanalında başlangıç ekranının açılması.
- 14 dünyayı gerçek Flutter Canvas ile rasterleştirme, tüm engel ağları ve 24 desen.
- Yeni kozmetiklerin yeniden yüklemede korunması, bir kez ücret alınması,
  eski premium hakların korunması; backup ile premium hak kazanılamaması.
- V9 güçlendirme testleri: yedi türü toplama ve sürenin dolması, tek kullanımlık
  kalkan, hayaletin engeli geçip kenardan düşmesi, mıknatıs, iki x2 gücünün
  birleşimi, küçülme/çarpışma, yakın geçiş +50, x5 combo ve kaçırınca sıfırlanması.
- 40 tohumda üç dakikalık güvenli yol: dev kapılar, tüm yeni hareket tipleri,
  boş bırakılan kristal koridoru ve temiz köprü girişleri.
- 40 tohum ve beş uzun koşuyla önceki motor kontrolleri; 20/60/120 FPS eşleşmesi.
- V9 tek uzun koşuda 14 dünya, dört özel sahne, yedi güç ve tüm engel tipleri.
- C# replay/self-test derlenip çalıştırıldı. Beş tohum × üç davranış = 15
  Dart/C# karşılaştırması: puan, kristal, mesafe, elenme tick'i eşleşti.
  Güçlerin tamamı ve boss kapıları dahil uzun koşular karşılaştırıldı.
- Bekleyen V8 biletleri için eski replay kontrolü geçti.

## Düzeltme

Yeni ölüm animasyonunun bitişinden önce sonuç ekranından tekrar başlatma,
aktif ticker'ı tekrar başlatabiliyordu. Ekran testi bunu yakaladı; yeni koşu ve
reklamla devam öncesinde önce ticker durdurularak düzeltildi.

## Bu ortamda tamamlanmayan kontroller

- macOS/Xcode ve iOS simülatör derlemesi.
- Android APK/AAB derlemesi ve fiziksel cihaz GPU/ısınma/FPS ölçümü.
- Swift/Kotlin native sesin cihaz üzerinde çalması ve yaşam döngüsü.
- Gerçek reklam servisi, App Store/Google Play ödeme ve sunucu makbuz doğrulaması.
- Canlı MySQL veritabanıyla HTTP uçtan uca API testi; mevcut API/DB yapısı korunur.

Flutter'ın standart bağımlılık indirmesi otomatik güvenlik denetimiyle
engellendi; gereksiz bulut instance metadata erişimine izin verilmedi.
Kontroller sabit sürümlü Flutter test motoru, yerel derleyici, çevrimdışı analiz
ve gerekli açık kaynak bağımlılıklarıyla yapıldı. Paket normal kullanıcı
makinesinde kendi `flutter pub get` çözümlemesini kullanır; test ortamının yerel
paket yolları ZIP'e eklenmez. Tam mobil derleme geçtiği iddia edilmez.

Mac'te `bash scripts/open_xcode.sh`, kapsamlı kontrol için `bash scripts/test.sh`.
