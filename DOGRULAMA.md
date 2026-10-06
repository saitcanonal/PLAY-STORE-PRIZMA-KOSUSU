# V3 doğrulama — 2 Ekim 2026

## Bu güncellemede yapılan kontroller

- Tree-sitter Dart ve C# dil ayrıştırıcılarıyla 14 Dart ve 4 C# dosyası kontrol edildi: sözdizimi hatası bulunmadı. Bu kontrol tip analizi veya derleme değildir.
- `python3 scripts/reference_checks.py` çalıştırıldı. Bu, üretim kodunu çalıştıran test değil, aynı kuralları kullanan bağımsız Python referans modelidir.
- Referans modelinde 40 seed ile hareketsiz koşuların üç can bittiğinde sonlandığı görüldü.
- Referans modelinde 1, 6, 67, 81 ve 99 seed'leriyle kristal rotasını izleyen beş ayrı 15 dakikalık koşu kontrol edildi: 21.220,2 m, sıfır çarpma/düşme, en çok 55 aktif nesne.
- Referans modelinde kristal kaçırma puan kazandırmadı; ilk toplanan kristal 110 puan kazandırdı. Düşme → çarpma → düşme aynı üç canı tüketti.
- Dart ve C# kaynakları aynı hız, PRNG, güvenli rota, platform sınırları, puan ve hasar kurallarıyla güncellendi. C# replay sınırı ve istemci protokolü V3 oldu.
- Yeni Flutter regresyon testleri eklendi: otomatik puanın kaldırılması, beş/iki/tek şerit, geç daralma uyarısı, ortak üç can, tekrar hasar koruması, her elenmede reklamla devam, hız sınırı ve V2 rekor arşivi.

## Bu ortamda çalıştırılamayan kontroller

Flutter/Dart SDK, .NET derleyicisi, Android SDK ve Xcode bu oturumda bulunmuyor. Bu yüzden **V3 için `flutter analyze`, `flutter test`, `dotnet build`, C# self-test, HTTP replay testleri ve native derleme yapılmadı**. Tree-sitter ve Python model kontrolleri bunların yerine geçmez. SDK indirme denemesinde erişim alınamadı.

V3 iPhone/Android simülatöründe veya fiziksel cihazda çalıştırılmadı; yeni görsel tasarımın ekran taşmaları ve performansı bu ortamda doğrulanmadı. Gerçek AdMob gösterimi ve ödül callback'i cihazda kontrol edilmeli. Varsayılan kimlikler test reklamlarıdır; reklam hazır değilse tekrar deneme engellenmez, devam ödülü verilmez.

V2'deki geçmiş kontroller `DOGRULAMA_V2.md` içindedir. V2 test sonuçları ve `v2-preview/` görselleri V3 doğrulaması değildir.

## Mac'te çalıştır

```bash
cd app
flutter pub get
flutter analyze
flutter test
flutter build ios --simulator
# Simülatörde gör
open -a Simulator
flutter devices
flutter run -d IPHONE_SIMULATOR_KIMLIGI
# Sunucuyu doğrula
cd ../server
dotnet build -c Release
dotnet run -c Release -- --self-test
cd ..
python3 scripts/test_api.py
```

Android kontrolü: `app` içinde `flutter build apk --debug` ve `flutter run -d ANDROID_EMULATOR_KIMLIGI`.

Görsel dışa aktarımı: `flutter test --dart-define=CREATE_PREVIEWS=true --update-goldens`. Bu komut `docs/prizma-*.png` dosyalarına V3'ün gerçek Flutter test renderer görüntülerini üretir.
