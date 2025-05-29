# Stok Takip Programı

<p>Programın çalışır halde bulunan video: www.youtube.com/watch?v=YL2aCuQvhyo
 
# Proje Veritabanının Kurulumu ve Yapılandırılması
<p>Uygulamanın veritabanını kendi SQL Server Management Studio ortamınıza eklemek için aşağıdaki adımları takip ediniz:</p>
1. SQL Server Management Studio'yu açınız. <br>
2. Sol kısımdaki Databases bölümüne sağ tıklayarak Import Data-tier Application seçeneğine tıklayınız.<br>
3. Açılan sihirbazda Next butonuna tıklayarak devam ediniz.<br>
4. Proje klasörüne gidip database klasörünü açınız.<br>
5. Bu klasörde yer alan stok_takip.bacpac dosyasını seçerek içeri aktarım işlemini tamamlayınız.<br>
6. Ardından Visual Studio ortamında SqlConnectionCls isimli sınıf dosyasını açınız.<br>
7. Bu sınıf içerisindeki Constring (bağlantı dizesi) alanını, kendi veritabanınızın bağlantı yoluna göre güncelleyiniz.

Belirtilen adımları eksiksiz uyguladığınız takdirde proje sorunsuz bir şekilde çalışacaktır.

## Giriş Ekranı Sayfası

![img](StokTakipUygulamasi/ekranFotograflari/f_login.png)

## Ürün Ekleme Sayfası

![img](StokTakipUygulamasi/ekranFotograflari/f_product_add.png)

## Ürün Kontrollerinin Sağlandığı Sayfa

![img](StokTakipUygulamasi/ekranFotograflari/f_product_control.png)

## Kategori Ekleme Sayfası

![img](StokTakipUygulamasi/ekranFotograflari/f_category.png)

## Marka Ekleme Sayfası

![img](StokTakipUygulamasi/ekranFotograflari/f_brands.png)

## Ürün Arama Sayfası

![img](StokTakipUygulamasi/ekranFotograflari/f_search_product.png)

## Satış Sayfası

![img](StokTakipUygulamasi/ekranFotograflari/f_sales.png)

## Satış Detayı Sayfası

![img](StokTakipUygulamasi/ekranFotograflari/f_sales_detail.png)

## Yapılan Satışları Listeleme Sayfası

![img](StokTakipUygulamasi/ekranFotograflari/f_list_sales.png)
