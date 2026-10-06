# V4 doğrulama ve kurulum

Bu pakette `README.md` V4 davranışını anlatır; önceki doğrulama belgeleri geçmiş sürümlere aittir.

## Burada çalıştırılan kontroller

- Gerçek Dart motoru analiz edildi: hata yok. `dart scripts/engine_checks.dart` çalıştı: 40 seed, beş 15 dakikalık rota, dar köprülerin engelsizliği ve 20/60/120 FPS deterministik sonuç kontrolü geçti.
- Gerçek Dart motoru örnek koşu üretip C# replay ile HTTP testinde karşılaştırıldı: puan ve süre eşleşti.
- C# kaynakları .NET 8 Roslyn ile uyarıları hata kabul ederek derlendi. SQLite bağımlılığı 8.0.27.
- C# self-test: 40 seed, geçersiz input, düşme, kalıcılık, aynı puanda sıra.
- HTTP testleri: 26 kontrol; yetkisiz erişim, başka oyuncunun koşusu, sahte makbuz, yalnız ilgili kullanıcının yedeğine erişim, SQLite üzerinden sunucu yeniden başlatma ve kayıtların korunması.
- Python referans model: 40 başlangıç ve beş 15 dakikalık güvenli rota. Her rotada 24.435 metre, sıfır can kaybı; en fazla 54 aktif nesne. Referans model gerçek mobil çalıştırmanın yerine geçmez.
- Dart ve C# dosyalarının sözdizimi tarandı; hata yok.

## Burada tamamlanamayanlar

Flutter çalıştırması otomatik onay denetimi tarafından, başlatma sırasında hassas bulut metadata adresine erişim girişimi nedeniyle engellendi. Flutter widget testleri, mobil SQLite plugin testi, iOS/Android build ve V4 ekran görüntüsü doğrulanmış değildir. Standart MSBuild de bu ortamın süreç bilgisi erişim kısıtı nedeniyle başlatılamadı; C# derleme ve testleri doğrudan Roslyn/runtime ile yapıldı. Xcode ve Android cihaz testleri ayrıca gerekir.

## Mac'te doğrulama

ZIP'i yeni klasöre çıkart. `app` klasöründe:

```bash
flutter pub get
flutter analyze
flutter test
open -a Simulator
flutter devices
flutter run -d IPHONE_SIMULATOR_KIMLIGI
```

SQLite yeni bağımlılıktır. İlk kurulumda `flutter pub get` sonrası `app/ios` içinde `pod install` çalıştır; Xcode'da `Runner.xcworkspace` aç. Paket kimliği önceki sürümle aynıdır; önce silmeden güncelleme denemesi yap. Mevcut isim, rekor, jeton ve kozmetikleri kontrol et.

`server` klasöründe:

```bash
dotnet build -c Release
dotnet run --no-build -c Release -- --self-test
dotnet run -c Release
```

Gerçek API için sunucuyu HTTPS üzerinde çalıştır ve uygulamayı `--dart-define=PRISMA_API=https://SUNUCU_ADRESIN` ile başlat. Sunucu pakette hazır kod olarak bulunur; internete dağıtılmış canlı hizmet değildir. iOS simülatöründeki yerel deneme için `http://127.0.0.1:5080` kullanılabilir. `/health` yanıtında `replayVersion: 4` olmalıdır.

## Cihazda kayıt ve kurtarma senaryosu

1. V3 üstüne aynı paket kimliğiyle V4 kur: eski ilerleme görünmeli; SQLite'a geçmeli.
2. Koşuyu bitir, jeton kazan, küre değiştir, uygulamayı kapat/aç: ilerleme korunmalı.
3. Ayarlardan kurtarma kodunu kopyalayıp oyun dışında sakla.
4. Sunucusuz senaryoda oyunu sil/kur, açılışta kodu yapıştır: kod anındaki kimlik ve ilerleme geri gelmeli.
5. Sunuculu senaryoda kod aldıktan sonra bir koşu daha yap; yedek gönderimi tamamlanınca sil/kur. Aynı API adresiyle kodu yapıştır: en son sunucu yedeği ve aynı token kimliği geri gelmeli.
6. Kod olmadan, otomatik yeniden kurulum kurtarması vaat edilmez. Apple/Google hesabıyla otomatik hesap kurtarma bu sürümde yoktur.
7. Ödüllü devam ve tekrar dene reklamını test kimlikleriyle dene; reklam ödülü gelmeden can verilmemeli. Canlı reklam kimlikleri kullanıcı tarafından yapılandırılmalıdır.
8. 320px genişlikte sol üst panel ve duraklatma düğmesini; 450m sonrası tek şerit köprüyü kontrol et.
9. Türkiye gece yarısından sonra sıralamayı yenile: bot puanları artmalı. Bot etiketi ve kendi çevrendeki sıra numaraları görünmeli.

Eklenen Flutter testleri `rank_recovery_test.dart` içinde günlük bot ligi, yerel kimlik taşıma ve bozuk yedeğin kaydı ezmemesini kapsar. Bu testler burada çalıştırılamadı; yerel SQLite plugin doğrulaması cihaz senaryosuyla yapılmalıdır.
