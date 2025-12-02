using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using prgmlab3.data;

namespace prgmlab3.Models
{
    // Uçak bilgisini temsil eden model.
    // Admin tarafından uçak ekleme, listeleme ve koltuk üretimi için kullanılır.
    public class PlaneModel : BaseModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Uçak adı zorunludur.")]
        [Display(Name = "Uçak Adı")]
        [StringLength(100, ErrorMessage = "Uçak adı en fazla 100 karakter olabilir.")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "Koltuk sayısı zorunludur.")]
        [Range(1, int.MaxValue, ErrorMessage = "Koltuk sayısı en az 1 olmalıdır.")]
        [Display(Name = "Koltuk Sayısı")]
        public int SeatCount { get; set; }

        /// Tüm uçakları döndürür.
        public static List<PlaneModel> GetAll()
        {
            var res = Query("SELECT * FROM planes", cmd => { });
            List<PlaneModel> planes = new();

            foreach (var row in res)
            {
                planes.Add(new PlaneModel
                {
                    Id = Convert.ToInt32(row["id"]),
                    Name = Convert.ToString(row["name"]) ?? "",
                    SeatCount = Convert.ToInt32(row["seat_count"])
                });
            }

            return planes;
        }

        // Yeni uçak ekler veya mevcut uçağı günceller.
        // Yeni uçak için otomatik koltuk kayıtları oluşturulur.
        public void Save()
        {
            Name = (Name ?? "").Trim();

            if (string.IsNullOrWhiteSpace(Name))
                throw new ArgumentException("Uçak adı boş olamaz.", nameof(Name));

            if (SeatCount < 1)
                throw new ArgumentOutOfRangeException(nameof(SeatCount), "Koltuk sayısı en az 1 olmalıdır.");

            if (Id == 0)
            {
                // Yeni uçak ekle
                SqliteDbHelper.ExecuteNonQuery(
                    "INSERT INTO planes (name, seat_count) VALUES (@name, @seat)",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@name", Name);
                        cmd.Parameters.AddWithValue("@seat", SeatCount);
                    });

                var id = SqliteDbHelper.ExecuteScalar<long>("SELECT last_insert_rowid();", null);
                Id = (int)id;

                // Bu uçak için otomatik koltuk oluştur
                for (int i = 1; i <= SeatCount; i++)
                {
                    var seatNumber = i.ToString();
                    SqliteDbHelper.ExecuteNonQuery(
                        "INSERT INTO seats (plane_id, seat_number, class) VALUES (@pid, @sn, @class)",
                        cmd =>
                        {
                            cmd.Parameters.AddWithValue("@pid", Id);
                            cmd.Parameters.AddWithValue("@sn", seatNumber);
                            cmd.Parameters.AddWithValue("@class", 0); 
                        });
                }
            }
            else
            {
                // Mevcut uçağı güncelle
                SqliteDbHelper.ExecuteNonQuery(
                    "UPDATE planes SET name=@name, seat_count=@seat WHERE id=@id",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@id", Id);
                        cmd.Parameters.AddWithValue("@name", Name);
                        cmd.Parameters.AddWithValue("@seat", SeatCount);
                    });

                // NOT: Gerekirse burada koltuk ekleme/silme mantığı eklenebilir.
            }
        }

        // Önce uçağa bağlı koltukları, ardından uçağı siler.
        public static void Delete(int id)
        {
            // Önce koltukları sil
            SqliteDbHelper.ExecuteNonQuery(
                "DELETE FROM seats WHERE plane_id=@id",
                cmd => cmd.Parameters.AddWithValue("@id", id)
            );

            // Sonra uçağı sil
            SqliteDbHelper.ExecuteNonQuery(
                "DELETE FROM planes WHERE id=@id",
                cmd => cmd.Parameters.AddWithValue("@id", id)
            );
        }
    }
}

