# Prizma Koşusu V12 — App Store / Google Play yayın yolu

Bu kaynak paket mağazaya otomatik olarak yüklenmedi. AAB/IPA henüz imzalanmadı.
Mağaza onayı garanti edilmez. V12, eski README ve YAYIN_AYARLARI.md içindeki reklam/yayın komutlarının yerine geçer.

## Bu sürümün kapsamı

- Sonsuz koşu, 14 dünya, 6 şerit, günlük parkur, süreli modlar, jetonlar ve kazanılan kozmetikler korunur.
- Reklam: yalnızca oyuncunun açıkça seçtiği ödüllü AdMob reklamı. Tekrar dene, çıkış ve oyun başlangıcında otomatik reklam yoktur.
- Bir koşuda en fazla bir ek can. Jetonları ikiye katlama da aynı koşu için bir defadır. SDK ödül olayı gelmeden ödül verilmez.
- Sunucu zorunlu değildir. İlk yayın çevrimdışıdır; sıralama sanal rakiplerle antrenman ligidir ve böyle etiketlenir.
- Gerçek para ile ürün satışı yayın sürümünde kapalıdır. Ürün kartları ve çalışmayan paket bağlantıları gizlenir. Jetonla alışveriş devam eder.
- C# / MySQL prototipi korunur. Üretim sunucusu kurulup güvenlik, veri silme, yük ve satın alma doğrulaması test edilmeden ilk mağaza paketine bağlanamaz.

## 1. Gerekli hesaplar ve bilgiler

Apple Developer ve Google Play Console hesabınızda uygulamayı oluşturun.
AdMob'da Android ve iOS uygulamalarını ayrı ayrı oluşturup her biri için **ödüllü reklam birimi** açın.
App ID `ca-app-pub-…~…`, reklam birimi ID `ca-app-pub-…/…` biçimindedir; birbirinin yerine kullanmayın.
Bu kimlikler parolalar değildir. Apple parolası, sertifika özel anahtarı, Android keystore ve AdMob hesap parolasını projeye koymayın.

| Alan | Açıklama |
|---|---|
| ANDROID_ADMOB_APP_ID | AdMob Android uygulama kimliği |
| ANDROID_REWARDED_ID | Android ödüllü reklam birimi |
| IOS_ADMOB_APP_ID | AdMob iOS uygulama kimliği |
| IOS_REWARDED_ID | iOS ödüllü reklam birimi |
| PRIVACY_URL | Giriş istemeyen, erişilebilir HTTPS gizlilik politikası |
| SUPPORT_URL | Çalışan destek / reklam bildirimi sayfası |
| SUPPORT_EMAIL | Düzenli kontrol ettiğiniz e-posta |

`app/config/store.example.json` dosyasını `app/config/store.json` adıyla kopyalayıp doldurun.
STORE_READY=true, ADS_ENABLED=true, LIVE_ADS=true, IAP_ENABLED=false, PRISMA_API="" olmalı.
Reklamsız çıkmak isterseniz ADS_ENABLED=false yapabilirsiniz; reklam düğmeleri görünmez ve SDK uygulama kodundan başlatılmaz.
SDK bağımlılığı yine pakettedir: no-ads derlemesinde gerçek cihaz ağ trafiğini kontrol ederek gizlilik beyanını hazırlayın.
Örnekte kimliklerin boş bırakılması kasıtlıdır. Başkasının reklam kimliği veya uydurma kimlik kullanılmaz.

Mevcut Android uygulama kimliği: `com.codevacore.prisma_kosusu`
Mevcut iOS Bundle ID: `com.codevacore.prismaKosusu`
Bunları kendi hesaplarınızda kaydedin; kayıtlı uygulamanız varsa mevcut kimliğini koruyun.
İlk yayın sürümü 12.0.0, build 16. Daha yüksek numarayla yüklemeniz olduysa pubspec.yaml içindeki build değerini artırın.

## 2. AdMob konsolu

- Uygulamaları doğru mağaza kimlikleriyle ilişkilendirin. Mağaza bağlantıları oluşunca AdMob'a ekleyin.
- Privacy & messaging bölümünde dağıtılacak bölgeler için gerekli mesajları hazırlayıp yayımlayın. UMP bunları uygulama kimliğinden alır.
- EEA/UK/İsviçre ve ABD eyaletleri gibi bölgelerde gerekli seçenekleri test edin. Kullanıcı kararını beklerken reklam yüklenmez.
- Uygulama G içerik derecesi ister ve nonPersonalizedAds=true gönderir. Bu ayar tek başına çocuklara yönelik uygulama uygunluğu veya tüm gizlilik yükümlülükleri anlamına gelmez.
- Gerçek hedef yaş kitlesini mağazalarda dürüstçe belirtin. Çocukları hedefleyecekseniz Families / Kids, çocuklara uygun SDK, yaş akışı ve reklam gereklilikleri ayrıca uygulanmadan bu paketi çocuk uygulaması olarak göndermeyin.
- Bu sürüm Android reklam kimliği iznini kaldırır; iOS ATT izni istemez ve IDFA yetkisi talep etmez. Takip/personalizasyon veya mediation eklerseniz izin, manifest ve beyanları yeniden düzenleyin.
- Destek e-postasına gelen uygunsuz reklamları inceleyin; Ad Review Center'dan reklamveren/kreatifi engelleyin. Uygulama rapora reklam response ID ve zamanı ekler; otomatik e-posta göndermez.
- Geliştirme sırasında gerçek reklamlara tıklamayın. Debug test reklamları için aşağıdaki komutu kullanın. Konsoldaki test cihazı/UMP coğrafya testlerini mağaza gönderiminden önce kaldırın.
- Reklam geliri; AdMob uygulama incelemesi, ödeme hesabı, doğrulama ve gerçek reklam doluluğuna bağlıdır. app-ads.txt satırını AdMob'dan alıp mağazada beyan ettiğiniz geliştirici sitesinin köküne yerleştirin; bu dosyada yayıncı ID'si uydurulmadı.

## 3. Yerel çalışma (Mac Terminal)

Flutter 3.47.6 ile Dart analizi ve widget testleri çalıştırıldı. Flutter, Python 3, Android SDK 36 ve JDK 17; iOS için macOS, Xcode 26+ ve CocoaPods gerekir.

```bash
cd prisma_kosusu/app
flutter pub get
flutter run
```

Debug test reklamı:

```bash
flutter run --dart-define=ADS_ENABLED=true --dart-define=LIVE_ADS=false
```

Debug iOS varsayılan Ads.xcconfig Google test App ID'sidir. Canlı iOS derlemesi bu dosyayı günceller. Sonra debug testine dönecekseniz Ads.xcconfig içindeki ID'yi `ca-app-pub-3940256099942544~1458002511` yapın.

## 4. Android: gerçek imza ve AAB

API target/compile=36, minimum Android=24 (Android 7), AGP=8.13.1, Gradle=8.13, Java=17.
AdMob 9.1.0'ın Android minimumu nedeniyle eski minimum 23 yükseltildi.
Android Studio SDK Manager'dan gerekli SDK/NDK araçlarını kurun ve lisansları `flutter doctor --android-licenses` ile kabul edin.

Proje dışında bir upload key oluşturun (parolayı komuta eklemeyin):

```bash
keytool -genkeypair -v -keystore "$HOME/prizma-upload.jks" -alias upload -keyalg RSA -keysize 2048 -validity 10000
```

`app/android/key.properties.example` dosyasını `key.properties` olarak kopyalayın. Gerçek dosya yolunu, alias ve parolaları yalnız kendi bilgisayarınızda doldurun.
Anahtarı ve parolasını güvenli biçimde yedekleyin. ZIP/Git içine koymayın.

Proje kökünden:

```bash
python3 scripts/release.py android app/config/store.json --check
python3 scripts/release.py android app/config/store.json
```

Çıktı: `app/build/app/outputs/bundle/release/app-release.aab`
Debug anahtarıyla yayın imzası artık yapılmaz. Önce Play Console internal testing'e yükleyin.
Play App Signing'i yapılandırın. Hesabınıza özel kapalı test / üretim erişimi koşullarını Console'dan tamamlayın.

## 5. iOS: Xcode ve IPA

```bash
cd prisma_kosusu/app
flutter pub get
cd ios
pod install
open Runner.xcworkspace
```

Xcode > Runner > Signing & Capabilities: kendi Team'inizi ve hesabınızda kayıtlı Bundle ID'yi seçin.
App Store Connect uygulamasını aynı Bundle ID ile açın. Simulator `.app` dosyası mağazaya yüklenmez.
Proje kökünden:

```bash
python3 scripts/release.py ios app/config/store.json --check
python3 scripts/release.py ios app/config/store.json
```

IPA: `app/build/ios/ipa/`. Önce TestFlight'a yükleyin. Gerekirse Xcode Organizer'dan dağıtın.
Release build phase yapılandırmayı denetler; direkt Xcode Archive da eksik/test reklam ayarlarıyla geçmez.
PrivacyInfo.xcprivacy, sistem şifrelemesi beyanı ve Google'ın 5 Ekim 2026 iOS quick-start listesindeki 50 SKAdNetwork kaydı eklendi.
Archive içinden Privacy Report üretin; Google SDK ve diğer plugin manifestlerinin birleştiğini denetleyin. Uygulama manifesti, üçüncü taraf SDK veri beyanlarının yerine geçmez.

## 6. Zorunlu gönderim kontrolü

- Gerçek iPhone ve Android: temiz kurulum, eski V11 üstüne güncelleme, oyun açılışı, küçük ekran, tablet/katlanabilir, arka plana geçiş, ses, düşük efekt.
- Günlük parkur/saat değişimi, kayıt-kapat-aç, kurtarma kodu, veri silme, silip yeni oyuncu oluşturma.
- Reklam bulunamaması, bağlantının kesilmesi, rızanın reddi/geri alınması, normal kapatma, erken kapatma, ödül callback'i, art arda düğmeye basma, reklamdan dışarı açılıp geri dönme. Ödül yalnızca bir kez.
- API 36 büyük ekranlarda orientation değişimlerini test edin. Flutter widget testleri telefon genişliklerini kapsar; native tablet testi yapılmış sayılmaz.
- Android App Bundle Explorer / cihaz testinde 16 KB sayfa boyutu ve native kütüphaneleri doğrulayın. SDK sürümü tek başına binary uygunluğunu kanıtlamaz.
- Kamuya açık PRIVACY_URL / SUPPORT_URL ve e-posta gerçek ve çalışır olmalı. `docs/store/PRIVACY-TEMPLATE.md` taslağını yayıncı bilgileriyle tamamlayın.
- Data Safety ve App Privacy beyanlarını yayınlanacak binary + SDK davranışı ile karşılaştırın. Reklamlı sürüm için “hiç veri toplanmıyor” seçmeyin.
- Reklam içerir beyanı, gerçek hedef kitle, içerik derecelendirmesi ve bölge bilgilerini doldurun. Gerçek para ürünleri bu sürümde yok.
- Mağaza ekran görüntüleri gerçek sürümden alınmalı. docs/v12-preview görüntüleri QA önizlemesidir; mağazanın istediği cihaz çözünürlüklerini karşılamaz.
- Apple inceleme notları: giriş gerekmiyor, antrenman rakipleri sanal, reklamlar isteğe bağlı, destek/gizlilik ve silme ayarlarda; inceleyiciye ödüllü reklama ulaşma adımlarını yazın.
- Mağaza onayından sonra kademeli dağıtın; crash/ANR ve reklam bildirimlerini takip edin.

## Çevrimiçi özellikler daha sonra açılırsa

Bu paketin release denetimi PRISMA_API ve IAP_ENABLED ayarlarını engeller.
C# sunucusu tek süreç / bellekte durum tutma mimarisindedir; yatay ölçeklemeye hazır değildir.
MySQL'deki DELETE ve veri silme uç noktaları eklendi ama bu ortamda .NET/MySQL entegrasyon testi çalıştırılmadı.
Canlıya alma öncesi bağımsız makbuz doğrulama servisi, hesap sahipliği, iadeler/iptaller, yeniden yükleme ve iade edilen hakların geri alınması tamamlanmalıdır.
Yedeklerin saklama/silme süresi, TLS reverse proxy ve rate limiting, izleme, silme sonrası eşzamanlı istekler test edilmelidir.
Play'de çevrimiçi hesap oluşturulacaksa uygulama içi silmeye ek olarak halka açık web silme talep yolu sağlayın. Bu pakette çalışan web silme servisi yoktur.

## Resmî kaynaklar (5 Ekim 2026 kontrolü)

- Apple App Review: https://developer.apple.com/app-store/review/guidelines/
- Xcode/SDK şartı: https://developer.apple.com/news/upcoming-requirements/?id=04282026a
- Play reklam politikası: https://support.google.com/googleplay/android-developer/answer/9857753?hl=en
- Play target API: https://support.google.com/googleplay/android-developer/answer/11926878?hl=en
- AdMob ödüllü reklam: https://developers.google.com/admob/flutter/rewarded
- UMP: https://developers.google.com/admob/flutter/privacy
- iOS reklam ayarları: https://developers.google.com/admob/ios/quick-start
- Google SDK sürümleri: https://pub.dev/packages/google_mobile_ads/changelog
- Android sayfa boyutu: https://developer.android.com/guide/practices/page-sizes
