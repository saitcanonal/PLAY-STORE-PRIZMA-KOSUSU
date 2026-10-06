# V8 doğrulama — 3 Ekim 2026

Küçük hareketli engellerin üretimi kaldırıldı. Beş şeridi (6 dünya birimi)
kaplayan büyük kapı çerçevesi, yatay kayan geçiş açıklığı ve dikey hareket eden
geçiş açıklığı eklendi. Kapılar yaklaşık dünya geçiş bölgelerinde, geniş
zeminde dönüşümlü gelir. Uyarı okları ve yaklaşan tuzak yazıları kaldırıldı.

Kontrol değişmedi: oyuncu sadece sağa-sola sürükler. Dikey geçidin hareketi
topun varışında açıklığı ulaşılabilir bırakır; aksi halde zıplama/eğilme olmadan
kaçınılmaz can kaybı olurdu. Yatay panelde açıklık dışına çarpmak bir can düşürür.
Geçit hareketi dünya mesafesine bağlıdır; duraklatmada donar. Mobil ve C# aynı
hesabı kullanır. Eski sürümlerin 4/6/7 biletleri eski davranışla doğrulanır.

Yapılanlar:
- engine.dart statik analizi geçti.
- 40 adet üç dakikalık güvenli koşu: sıfır çarpışma, iki kapı türü oluştu,
  küçük hareketli engel oluşmadı, kapılar beş şeritlik zeminde kaldı.
- Yatay açıklık dışına çarpma testi bir can düşürdü; açıklık içinden geçiş ve
  dikey açıklığın varışta ulaşılabilirliği doğrulandı.
- Beş 15 dakikalık güvenli koşu, 40 seed ve 20/60/120 FPS kontrolleri geçti.
- C# .NET 8 Roslyn derlemesi ve replay self-test geçti.
- MariaDB/MySqlConnector üzerinde 36 API/aktarım/replay kontrolü geçti.
- On iki bir dakikayı aşan Dart/C# koşusunda puan, tick ve mesafe eşleşti.
- Dart/C# kaynak sözdizimi ayrıştırması geçti.

Yapılmayanlar:
Flutter analyze/test, yeni arayüzün renderi, iOS/Android build, cihaz
performansı ve mağaza/reklam SDK kontrolleri bu ortamda yapılmadı. Sözdizimi
kontrolü Flutter tip analizi yerine geçmez. Eski beyaz ekranın nedeni kullanıcı
cihazı açılış logu olmadan belirlenmedi; bu paket o sorunun çözüldüğü iddiasında
bulunmaz. Gerçek satın alma için mevcut mağaza/validator kurulumu gereklidir.

Mac'te yeni paketi ayrı klasöre çıkar. İlk dünya geçişine kadar oyna ve büyük
geçidi gözle; sonraki geçitte dikey açıklığı gör. Duraklat/devam, çarpışma,
üç can, desen/iz seçimi, mağaza ve sıralama akışlarını cihazda doğrula.
