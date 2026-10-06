# V12 iOS reklam modülü düzeltmesi

Ekran görüntüsündeki iki hata google_mobile_ads içindeki FLTAd_Internal / FLTAdPreloader başlıklarından kaynaklanıyor: GoogleMobileAds_Beta.h non-modular include.
Podfile post_install içine yalnız google_mobile_ads hedefinde CLANG_ALLOW_NON_MODULAR_INCLUDES_IN_FRAMEWORK_MODULES=YES eklendi.
Diğer Pod hedeflerinin veya tüm projenin uyarıları kapatılmadı. Reklam sürümü değiştirilmedi.

Xcode'u kapatın. Proje kökünden:

```bash
cd app
flutter pub get
cd ios
pod install
open Runner.xcworkspace
```

Xcode: Product > Clean Build Folder (Shift+Command+K), sonra iPhone simülatörünü seçip Run.
Mevcut projenizi kullanıyorsanız bu paketteki app/ios/Podfile dosyasını kendi app/ios/Podfile dosyanızın yerine koyup aynı adımları uygulayın. Pods içindeki Google başlıklarını elle değiştirmeyin.

Bu ayar gösterilen derleyici hatasını hedefler. Bu ortamda Xcode olmadığından native build başarıyla tamamlandı iddiası yoktur; yeniden derleme sonucu Mac üzerinde doğrulanmalıdır.
Kaynak: https://developer.apple.com/documentation/xcode/build-settings-reference
