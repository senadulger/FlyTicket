# FlyTicket – Uçuş Bileti Rezervasyon Sistemi 

## 1. Proje Hakkında
Bu proje, **"Nesneye Yönelimli Programlama"** konusu kapsamında geliştirilmiş bir Uçuş Rezervasyon Sistemidir. Temel amaç; .NET ekosistemini kullanarak Model-View-Controller (MVC) mimarisine uygun, ilişkisel veritabanı yönetimi içeren ve kullanıcı dostu arayüze sahip kapsamlı bir web uygulaması geliştirmektir.

## 2. Özellikler
*   **Kimlik Doğrulama & Yetkilendirme:** Cookie tabanlı güvenli giriş ve kayıt sistemi.
*   **Kullanıcı Rolleri:**
    *   **Standart Üye (User):** Uçuş arayabilir, koltuk seçimi yapabilir, rezervasyon oluşturabilir ve geçmiş seyahatlerini görüntüleyebilir.
    *   **Yönetici (Admin):** Uçuşları, uçakları, havalimanlarını ve rezervasyonları yönetebilir.
*   **Uçuş Yönetimi:** Kalkış/Varış noktalarına göre uçuş planlama ve güncelleme.
*   **Rezervasyon Sistemi:** Dinamik koltuk seçimi ve rezervasyon iptal/onay süreçleri.
*   **Dashboard:** Admin paneli üzerinden sistemdeki toplam uçuş, kullanıcı ve rezervasyon istatistiklerinin takibi.

## 3. Teknoloji Yığını

| Kategori | Teknoloji | Açıklama |
| :--- | :--- | :--- |
| **Frontend** | HTML5 / CSS3 | Modern ve responsive kullanıcı arayüzü |
| | Razor Views | Server-side rendering ile dinamik sayfa oluşturma |
| **Backend** | C# .NET 8.0 | Ana programlama dili ve framework |
| | ASP.NET Core MVC | Web uygulama mimarisi |
| **Veritabanı** | SQLite | Hafif ve taşınabilir ilişkisel veritabanı |
| **Araçlar** | Visual Studio / VS Code | Geliştirme ortamı |
| | NuGet | Paket yönetimi |

## 4. Nesne Yönelimli Programlama (OOP) Yaklaşımı
Proje, Nesne Yönelimli Programlama prensipleri temel alınarak geliştirilmiştir. Kullanılan temel OOP kavramları şunlardır:

*   **Kapsülleme (Encapsulation):** `FlightModel`, `AirportModel`, `RezervationModel` gibi sınıflar, verileri private alanlarda (fields) saklayarak ve public özellikler (properties) aracılığıyla dışarı açarak veri bütünlüğünü korur.
*   **Kalıtım (Inheritance):** Tüm model sınıfları (örneğin `FlightModel`), ortak veritabanı işlemlerini barındıran soyut `BaseModel` sınıfından türetilmiştir. Bu sayede kod tekrarı önlenmiş ve merkezi bir yapı kurulmuştur.
*   **Soyutlama (Abstraction):** `BaseModel` sınıfı, veritabanı bağlantı ve sorgu işlemlerini soyutlayarak, alt sınıfların sadece kendi iş mantıklarına odaklanmasını sağlar. Karmaşık SQL işlemleri bu katmanda gizlenmiştir.
*   **Sorumlulukların Ayrılması (Separation of Concerns):** MVC mimarisi sayesinde veri erişimi (Model), iş mantığı (Controller) ve sunum katmanı (View) birbirinden ayrılarak daha sürdürülebilir bir yapı oluşturulmuştur.

## 5. Proje Yapısı ve Mantığı
*   **MVC Mimarisi:** Proje, sorumlulukların ayrılması ilkesine uygun olarak Model, View ve Controller katmanlarından oluşur.
*   **Veritabanı Yönetimi:** `SqliteDbHelper` sınıfı ile veritabanı bağlantıları ve CRUD işlemleri yönetilir.
*   **Modüler Yapı:** Uçuş (Flight), Havalimanı (Airport), Rezervasyon (Rezervation) gibi varlıklar ayrı modeller ve controller'lar ile modüler hale getirilmiştir.

## 6. Proje Nasıl Çalıştırılır?

### Ön Gereksinimler
*   .NET SDK 8.0 kurulu olmalı
*   Visual Studio veya VS Code kurulu olmalı

### Kurulum

1.  **Depoyu Klonlayın:**
    ```bash
    git clone [https://github.com/SerhatCanBakir/Lab3_Project.git]
    cd Lab3_Project
    ```

2.  **Bağımlılıkları Yükleyin:**
    Terminalde proje dizinindeyken aşağıdaki komutu çalıştırın:
    ```bash
    dotnet restore
    ```

3.  **Uygulamayı Çalıştırın:**
    ```bash
    dotnet run
    ```
    Uygulama varsayılan adreste çalışacaktır.

## 7. Proje Demo Videosu
Uygulamanın temel özelliklerini ve genel işleyişini görmek için aşağıdaki demo videosunu izleyebilirsiniz:

https://github.com/user-attachments/assets/1e4da47f-ff28-42d6-880c-dc867becb016

## 8. Proje Raporu
Teknik detaylar, diyagramlar (ER, Class vb.) ve ekran görüntülerini içeren detaylı inceleme için depodaki [VizeRapor.pdf](VizeRapor.pdf) ve [FinalRapor.pdf](FinalRapor.pdf) dosyalarına göz atabilirsiniz.

## 9. Yazarlar

| Geliştirici | Profil |
| :--- | :--- |
| [Sena Nur Dülger](https://github.com/senadulger) | [<img src="https://upload.wikimedia.org/wikipedia/commons/c/ca/LinkedIn_logo_initials.png" alt="LinkedIn" width="20"/>](https://www.linkedin.com/in/senadulger/) |
| [Serhat Can Bakır](https://github.com/SerhatCanBakir) | [<img src="https://upload.wikimedia.org/wikipedia/commons/c/ca/LinkedIn_logo_initials.png" alt="LinkedIn" width="20"/>](https://www.linkedin.com/in/serhat-can-bak%C4%B1r-53850125a/) |
