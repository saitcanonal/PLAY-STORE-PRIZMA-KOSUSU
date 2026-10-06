# V2 doğrulama — 2 Ekim 2026

## Geçen kontroller

- Flutter 3.35.5 / Dart 3.9.2 ile `flutter analyze`: **No issues found**.
- `flutter test --dart-define=CREATE_PREVIEWS=true --update-goldens`: **16 test geçti** (15 işlevsel test ve 1 görsel dışa aktarım testi).
- 20, 60 ve 120 FPS'de aynı seed'in bitiş tick'i ve puanı aynı.
- Güvenli rotayı izleyen **15 dakikalık**, 54.000 tick'lik sonsuz koşu tamamlandı; parkur nesneleri 90'ın altında kaldı, hız 17.5'te sınırlı kaldı, dünya döngüsü birden çok kez geçti.
- Hareket replay'i mobil simülasyonla aynı puan ve mesafeyi üretti.
- Tek devam hakkı, can/kalkan/koruma, ilk elenmedeki sıralama puanının korunması kontrol edildi.
- İsim ekranı, 320×640 ve 390×844 telefon boyutları, dört sekme, günlük hediye, sürükleme, duraklatma, çıkış, elenme ve tekrar deneme kontrol edildi.
- Dar ekrandaki puan satırı taşması düzeltildi. Puan ve kristal metinleri gerektiğinde ölçeklenir.
- Reklam hazır olmadığında sahte ödül veya devam hakkı verilmediği kontrol edildi.
- V1 kayıt migration'ı, tekrarlanan sonuç callback'i, ek can jeton farkı, ödülün iki kez verilmemesi, günlük hediye Türkiye gece yarısı ve sabit kozmetik fiyatları kontrol edildi.
- C# kaynakları .NET 8 Roslyn derleyicisiyle derlendi; replay/bozuk input/persistence/eşit puan self-test'leri geçti. 40 farklı seed kullanıldı.
- Çalışan HTTP API'ye iki oyuncuyla **22 doğrulama** yapıldı: yetkisiz erişim, geçersiz isim, başka oyuncunun koşusuna müdahale, aşırı/bozuk tick, null hareket, sahte puanın yok sayılması, idempotent gönderim, rakip sırası, token gizliliği, satın alma doğrulama hizmeti yokken ürünün reddi ve yeniden açılışta kalıcılık.
- Dart tarafından üretilen gerçek hareket kaydı C# sunucusunda aynı puanla yeniden hesaplandı.

Görseller `prizma-home.png`, `prizma-worlds.png`, `prizma-play.png`, `prizma-result.png` dosyalarıdır; tasarım maketi değil Flutter test renderer çıktılarıdır.

## Bu ortamda tamamlanamayan kontroller

- `flutter build apk --debug --no-pub` denendi. **Android SDK bulunmadığı için native APK derlemesi tamamlanamadı.** APK teslim edildiği veya Android emülatöründe çalıştırıldığı iddia edilmez.
- Linux ortamında Xcode/iOS simülatörü yoktur; iOS native derlemesi ve simülatör kontrolü yapılmadı.
- .NET CLI/MSBuild burada süreç bilgilerini okuyamadığı için normal `dotnet build` yolu kullanılamadı. Aynı .NET 8 referans assembly'leriyle doğrudan Roslyn derlemesi yapıldı; çıkan sunucu çalıştırılarak self-test ve HTTP kontrolleri geçti.
- AdMob/UMP'nin native reklam gösterimi ve mağaza sandbox ödemeleri gerçek Android/iOS cihazda doğrulanmadı. Test reklam kimlikleri ve plugin callback bağlantıları vardır; gerçek hesaplar bağlı değildir.
- Ücretli makbuz doğrulama hizmeti bu pakette sağlanmaz; doğrulama geçidi vardır ve yapılandırılmadan hak vermez. Gerçek oyuncu sunucusu internet üzerinde yayınlanmadı.

## Kendi bilgisayarında tekrar çalıştır

```bash
cd app
flutter pub get
flutter analyze
flutter test
flutter build apk --debug
# Mac'te
flutter build ios --simulator
cd ../server
dotnet build -c Release
dotnet run -c Release -- --self-test
cd ..
python3 scripts/test_api.py
```

`test_api.py` ayrı 5088 portunda geçici bir sunucu başlatıp kapatır; kendi geçici veritabanını kullanır. `dotnet` ve `dart` PATH'te olmalıdır. `DOTNET_BIN`, `DART_BIN` ve `PRISMA_TEST_DLL` ile yollar değiştirilebilir. Makbuz doğrulayıcı tanımlanmayan test ortamını kullanır.
