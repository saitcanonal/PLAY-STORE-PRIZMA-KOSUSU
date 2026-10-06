# V9.1 — Normal küçük engeller

Küçük kayan bloklar, hareketli kapı/eziciler, dönen çubuklar, kapanan duvarlar,
lazerler ve çöken küçük platformlar koşu üretiminden kaldırıldı.
Aynı engel konumlarında normal, sabit bloklar üretilir. Küçük engeller artık
hareket oku taşımaz, yer değiştirmez ve açılıp kapanmaz.

Beş şeridi kaplayan dev dünya geçitleri ve içlerindeki hareketli açıklıklar
korunur. Güçlendirmeler, kozmetikler, kristaller, combo ve dünya sistemleri korunur.

Mobil sürüm: 9.0.1+11. Yeni replay sürümü: 10. Sunucu güncellenmelidir.
Eski V9/V8 biletlerini doğrulayan davranışlar sunucuda korunur.

Doğrulama:
- Dart analizi: sorun yok.
- 40 tohumda üç dakikalık koşu: küçük hareketli engel doğmuyor, dev kapılar
  korunuyor ve güvenli kristal koridoru erişilebilir.
- Güçlendirme, combo, dünya ve özel sahne kontrolleri geçti.
- 15 yeni Dart/C# koşusu tam eşleşti.
- Önceki V9'daki 15 koşu eski sürüm doğrulamasıyla tekrar eşleşti.
- Gerçek Flutter test motorundaki mevcut 31 test ve iki hazırlık/temizlik
  adımı geçti. Ekran boyutları, yeniden başlatma ve kayıt kontrolleri dahil.

Xcode/Android native cihaz derlemesi bu Linux ortamında doğrulanmadı.
Mac'te proje klasöründe: `bash scripts/open_xcode.sh`.
