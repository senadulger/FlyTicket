using System;
using System.ComponentModel.DataAnnotations;
using prgmlab3.data;

namespace prgmlab3.Models
{
    // Havaalanı bilgisini temsil eden model.
    public class AirportModel : BaseModel
    {
        public int Id { get; set; }

        [Display(Name = "Kod")]
        [StringLength(10, ErrorMessage = "Kod en fazla 10 karakter olabilir.")]
        public string Code { get; set; } = "";

        [Required(ErrorMessage = "Şehir bilgisi zorunludur.")]
        [Display(Name = "Şehir")]
        [StringLength(100, ErrorMessage = "Şehir adı en fazla 100 karakter olabilir.")]
        public string City { get; set; } = "";

        [Required(ErrorMessage = "Havaalanı adı zorunludur.")]
        [Display(Name = "Havaalanı Adı")]
        [StringLength(150, ErrorMessage = "Havaalanı adı en fazla 150 karakter olabilir.")]
        public string Name { get; set; } = "";

        [Display(Name = "Ülke")]
        [StringLength(100, ErrorMessage = "Ülke adı en fazla 100 karakter olabilir.")]
        public string Country { get; set; } = "";

        public static AirportModel? GetById(int id)
        {
            var res = Query(
                "SELECT * FROM airports WHERE id=@id",
                c => c.Parameters.AddWithValue("@id", id)
            );

            if (res.Count == 0) 
                return null;

            var r = res[0];

            return new AirportModel
            {
                Id      = Convert.ToInt32(r["id"]),
                Code    = Convert.ToString(r["code"])    ?? "",
                City    = Convert.ToString(r["city"])    ?? "",
                Name    = Convert.ToString(r["name"])    ?? "",
                Country = Convert.ToString(r["country"]) ?? ""
            };
        }

        // Yeni kayıt ekler.
        // Id = 0 ise insert
        public void Save()
        {
            Code    = (Code    ?? "").Trim();
            City    = (City    ?? "").Trim();
            Name    = (Name    ?? "").Trim();
            Country = (Country ?? "").Trim();

            if (!string.IsNullOrEmpty(Code))
                Code = Code.ToUpperInvariant();

            if (Id == 0)
            {
                // Yeni kayıt
                SqliteDbHelper.ExecuteNonQuery(
                    "INSERT INTO airports (code, city, name, country) VALUES (@code, @city, @name, @country)",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@code", Code);
                        cmd.Parameters.AddWithValue("@city", City);
                        cmd.Parameters.AddWithValue("@name", Name);
                        cmd.Parameters.AddWithValue("@country", Country);
                    });

                var id = SqliteDbHelper.ExecuteScalar<long>("SELECT last_insert_rowid();", null);
                Id = (int)id;
            }

            // Daha sonra güncelleme özelliği getirilebilir.
        }

                    // Id'ye göre havaalanı kaydını siler.
            public static void Delete(int id)
            {
                SqliteDbHelper.ExecuteNonQuery(
                    "DELETE FROM airports WHERE id=@id",
                    cmd => cmd.Parameters.AddWithValue("@id", id)
                );
            }
    }
}