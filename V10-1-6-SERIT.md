# V10.1 — 6 şerit ve oyuna odaklanan ekran

- Yol 5 yerine 6 şerit. Engel, kristal, dar köprü, büyük kapı ve dokunmatik kontrol yerleşimleri buna göre güncellendi.
- “Sonsuz koşu”, parkur parçası ve dünya mekanik açıklaması oyun başında 1,5 saniye içinde solar ve kaybolur. Sürükleme ipucu da kısa süre görünür.
- Süreli modlarda kalan süre skorun yanında görünmeye devam eder. Can, skor ve güçlendirmeler korunur.
- Uygulama sürümü: 10.1.0+13. Yeni koşularda sunucu replay sürümü: 12. Eski biletlerin 5 şeritli doğrulaması korunur. Çevrimiçi kullanımda uygulamayla birlikte bu paketteki C# sunucuyu da güncelleyin.

## Doğrulama

40 Flutter testi geçti; açıklamaların kaybolması ve sayacın görünür kalması test edildi. Dart/C# arasında 21 koşu kaydı aynı sonucu verdi. Sunucu öz testleri ve uzun süreli güvenli yol kontrolleri geçti.

iOS derlemesi bu Linux ortamında çalıştırılmadı. Mac üzerinde proje kökünde `bash scripts/open_xcode.sh` çalıştırın; Xcode’da Runner hedefini ve imzalama takımınızı seçin.
