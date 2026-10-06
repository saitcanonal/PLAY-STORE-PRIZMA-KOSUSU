# Yayın bağlantıları

## AdMob

Bu proje test kimlikleriyle teslim edilir. Test reklamları kazanç üretmez. Canlı reklamları yalnız kendi AdMob uygulamalarını ve reklam birimlerini kurduktan sonra etkinleştir.

1. AdMob'da ayrı Android/iOS uygulamaları ve her biri için interstitial + rewarded reklam birimleri oluştur.
2. `app/android/app/src/main/AndroidManifest.xml` içindeki `com.google.android.gms.ads.APPLICATION_ID` değerini Android uygulama kimliğinle değiştir.
3. `app/ios/Runner/Info.plist` içindeki `GADApplicationIdentifier` değerini iOS uygulama kimliğinle değiştir.
4. AdMob Privacy & messaging bölümünde gereken UMP mesajlarını yayınla. iOS'ta güncel SKAdNetwork kimlikleri ve mağaza gizlilik beyanlarını resmî dokümana göre yapılandır.
5. Aşağıdaki parametrelerin **tamamını kendi reklam birimi kimliklerinle** kullan:

```bash
flutter build appbundle --release \
  --dart-define=LIVE_ADS=true \
  --dart-define=ANDROID_INTERSTITIAL_ID=KENDI_ANDROID_INTERSTITIAL_KIMLIGIN \
  --dart-define=ANDROID_REWARDED_ID=KENDI_ANDROID_REWARDED_KIMLIGIN \
  --dart-define=IOS_INTERSTITIAL_ID=KENDI_IOS_INTERSTITIAL_KIMLIGIN \
  --dart-define=IOS_REWARDED_ID=KENDI_IOS_REWARDED_KIMLIGIN \
  --dart-define=PRISMA_API=https://KENDI_SUNUCUN
```

Geliştirirken test kimlikleri kullan. Canlı reklama test amacıyla tıklama. Reklam sıklığını ölçerek ayarla; V4 her tekrar deneme basışında hazır reklam gösterir. Reklam yoksa yeni koşu engellenmez; devam ödülü yalnız SDK ödül callback’iyle verilir.

## Sıralama sunucusu

.NET 8 sunucuyu HTTPS ters proxy arkasında tek süreç olarak çalıştır. Başka sunucuyla aynı JSON dosyasına eşzamanlı yazma.

Ortam değişkenleri:

- `ASPNETCORE_URLS`: sunucunun dinleme adresi; varsayılan 127.0.0.1:5080.
- `AllowedHosts`: yayın alan adın; geliştirme varsayılanında localhost, 127.0.0.1 ve 10.0.2.2 vardır.
- `PRISMA_MYSQL`: sunucu MySQL bağlantısı. Parolayı yalnız sunucuda tut; TLS ve yedekleme yapılandır. `docs/MYSQL-KURULUM.md` dosyasını izle.

Mobil sürümde `PRISMA_API=https://...` kullan. Sunucu URL'si secret değildir. Veritabanını ve yedeklerini herkese açık klasörde tutma.

API:

- `POST /api/players {name}` → anonim kimlik ve token.
- `POST /api/runs` → seed ve koşu kimliği; bearer token gerekir.
- `POST /api/runs/{id}/finish {ticks,events:[{tick,value}]}` → sunucunun hesapladığı puan.
- `GET /api/ranks` → ilk 100, kendi sıran ve hemen üstündeki rakip.
- `GET /api/entitlements` → doğrulanmış ürün hakları.
- `POST /api/purchases/verify` → makbuz doğrulama geçidi.

V4 kurtarma kodu, aynı sunucu adresindeki oyun kimliğine dönmeyi sağlar. Kullanıcı kodu oyun dışında saklamalıdır. Sunucu kurulu değilken kodun içindeki yerel anlık kayıt kullanılır. Bir makbuz birden fazla oyun kimliğine hak vermez. `PUT /api/backup` ilerlemeyi saklar; `GET /api/backup` yalnız ilgili token sahibine döner. Ücretli haklar yedekten doğrudan açılmaz.

## Satın alma: tamamlanması gereken hesap bağlantıları

Bu ZIP, Apple/Google hesap anahtarları olmayan bir ödeme altyapısı teslimidir. Makbuz doğrulama geçidi vardır; Apple/Google ile bağımsız doğrulama hizmetinin kendisi bu pakette yoktur. Ücretli ürünler bu yüzden varsayılan olarak kapalıdır.

1. Google Play Console / App Store Connect'te **non-consumable** ürünler oluştur:
   - `prisma_remove_ads`
   - `prisma_prism_pack`
2. Paket kimliği, fiyatlar, sandbox test hesabı ve mağaza sözleşmelerini yapılandır.
3. Sunucuda, Google Play Developer API / App Store Server API ile makbuzun uygulama kimliğini, ürünü, ödeme durumunu, iptal/iade durumunu ve transaction kimliğini gerçekten doğrulayan güvenilir bir hizmet kur.
4. Sunucu ortamında `RECEIPT_VALIDATOR_URL=https://...` ve `RECEIPT_VALIDATOR_SECRET` tanımla. Secret mobil uygulamaya eklenmez.
5. Bu doğrulayıcı aşağıdaki JSON'u alır (Authorization: Bearer secret):

```json
{"productId":"prisma_remove_ads","purchaseId":"transaction-id","source":"google_play","receipt":"store-verification-data"}
```

Gerçek doğrulama başarılıysa:

```json
{"valid":true,"productId":"prisma_remove_ads","transactionId":"unique-verified-store-transaction"}
```

Yanlış/iptal/iade edilmiş makbuz için `valid:false` döndür. İstemcinin `purchaseId` değerine güvenme; transactionId mağazadan doğrulanmış olmalıdır. SK2 JWS veya Google purchase token doğrulaması kullanılan platformun resmî sunucu API'sine göre yapılmalıdır.

6. Bunlar çalıştıktan sonra `--dart-define=IAP_ENABLED=true` ile mağazayı aç. Sunucu bağlantısı da gereklidir.
7. Gerçek cihazda satın alma, kullanıcı iptali, ödeme bekleme, ağ kesintisi ve geri yüklemeyi sandbox ortamında test et. Ürün hakkı sunucu onayından önce verilmez; `completePurchase` başarılı doğrulamadan sonra çağrılır.

## Native yayın

Android release yapılandırmasında mevcut debug signing örneğini kendi keystore ve release imzanla değiştir. iOS Signing & Capabilities bölümünde Team ve benzersiz bundle identifier seç. Android/iOS simülatörü ve fiziksel cihazda son kontroller bu Linux ortamının dışındadır.

Resmî kaynaklar:
- https://developers.google.com/admob/flutter/quick-start
- https://developers.google.com/admob/flutter/interstitial
- https://developers.google.com/admob/flutter/rewarded
- https://developers.google.com/admob/flutter/privacy
- https://pub.dev/packages/in_app_purchase


## V7 stil ürünleri

App Store Connect ve Google Play Console'da şu non-consumable ürünleri oluştur:

- prisma_patterns_pack: Galaksi top deseni.
- prisma_trails_pack: Gökkuşağı ve Kuyruklu yıldız izleri.
- prisma_prism_pack: Mevcut paket; özel Prizma küresi + yukarıdaki desen/izler.
- prisma_remove_ads: Mevcut reklam kaldırma ürünü.

Mobil uygulamayı IAP_ENABLED=true ve V7 HTTPS API adresiyle derle. Mevcut
receipt validator yeni productId değerlerini de desteklemeli. Fiyatlar SDK'dan
alınır; yerel sabit fiyat kullanılmaz. Gerçek makbuz doğrulanmadan premium
stil açılmaz. Ürünler yapılandırılmadan gerçek satın alma çalışmaz; mağaza
hazır değil mesajı gösterilir. Ödeme ve geri yükleme Sandbox/License Tester
ile cihazda test edilmelidir. Önceki Prizma paket sahiplerinin ek stilleri
load/grantEntitlement sırasında açılır.
