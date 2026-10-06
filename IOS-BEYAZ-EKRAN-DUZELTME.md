# V8.1 — iOS açılış düzeltmesi

Xcode görüntülerindeki hata, oyun çiziminden önce UIKit'in uygulamayı durdurmasıdır:
`__UIApplicationEvaluateRuntimeIssueForNonSceneLifecycleAdoption_block_invoke`,
`Thread 1: EXC_BREAKPOINT`. Yeni SDK, UIScene yaşam döngüsü istiyor.

## Düzeltilen kod

- Info.plist içine UIScene manifesti, FlutterSceneDelegate ve Main storyboard bağlantısı eklendi.
- AppDelegate, FlutterImplicitEngineDelegate kullanıyor. Eklentiler artık didInitializeImplicitFlutterEngine içinde kaydediliyor.
- Flutter alt sınırı 3.41.0. Eski Flutter ile bu native açılış kullanılmamalı.
- İlk Flutter ekranı, native yön kilidi yanıtını beklemeden çiziliyor.
- Profil yüklemesi 15 saniyede yanıt alamazsa tekrar deneme ekranı gösteriliyor.
- Eksik SQLite bağımlılıkları kilit dosyasına eklendi; native ve Flutter açılış rengi eşitlendi.
- Tüm Dart kaynaklarındaki analiz uyarıları düzeltildi.

Oyun, beş şeritlik geçitler, desenler, izler, mağaza, sıralama ve kayıt sistemi korunmuştur.
Oyun/sunucu tekrar protokolü 8 olarak kalır.

## Mac'te çalıştırma

Eski Xcode oturumunu kapat, bu ZIP'i yeni klasöre çıkar. Terminal'de yeni paketin
app klasörünü aç ve çalıştır:

```bash
flutter upgrade
bash ../scripts/open_xcode.sh
```

Betik eski derleme çıktılarını temizler, paketleri ve CocoaPods'u kurar, iOS simülatör
uygulamasını derler ve Runner.xcworkspace dosyasını açar. Xcode'da Runner ve bir iPhone
simülatörü seçip ▶ düğmesine bas. Uygulamayı silmeye gerek yoktur.

## Doğrulama

- Tüm uygulama ve test kodu: Dart analizinde hata veya uyarı yok.
- Flutter ana uygulamasının kernel derlemesi başarılı.
- Gerçek Flutter test motorunda 29 test geçti, 0 başarısız: 10 oyun motoru, 7 profil,
  3 sıralama/kurtarma, 4 görünüm ve 5 arayüz testi.
- Native yön kilidi yanıt vermediğinde bile açılış ekranının çizilmesi test edildi.
- Dar ve standart ekranlar, mağaza, oyun başlatma, sürükleme, duraklatma, ölüm ve
  ödülsüz reklam akışı arayüz testlerinden geçti.
- Bu ortamda Xcode yok. iOS 27 native derleme ve simülatör açılışı Mac üzerinde
  doğrulanmalı. Flutter arayüz testleri Linux motorunda çalıştırıldı.

Native değişiklikler resmi Flutter UIScene geçiş belgesini takip eder:
https://docs.flutter.dev/release/breaking-changes/uiscenedelegate
