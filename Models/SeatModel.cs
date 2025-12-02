using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using prgmlab3.data;

namespace prgmlab3.Models
{
    // Uçak koltuğu modelidir.
    // 0: Ekonomi, 1: Business
    public class SeatModel : BaseModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Uçak")]
        public int PlaneId { get; set; }

        [Required(ErrorMessage = "Koltuk numarası zorunludur.")]
        [Display(Name = "Koltuk No")]
        [StringLength(10, ErrorMessage = "Koltuk numarası en fazla 10 karakter olabilir.")]
        public string SeatNumber { get; set; } = "";

        [Range(0, 1, ErrorMessage = "Koltuk sınıfı 0 (Economy) veya 1 (Business) olmalıdır.")]
        [Display(Name = "Sınıf")]
        public int Class { get; set; }

        // Belirli bir uçağa ait tüm koltukları getirir.
        public static List<SeatModel> GetByPlane(int planeId)
        {
            var res = Query(
                "SELECT * FROM seats WHERE plane_id=@id",
                c => c.Parameters.AddWithValue("@id", planeId)
            );

            List<SeatModel> list = new();
            foreach (var row in res)
            {
                list.Add(new SeatModel
                {
                    Id         = Convert.ToInt32(row["id"]),
                    PlaneId    = Convert.ToInt32(row["plane_id"]),
                    SeatNumber = Convert.ToString(row["seat_number"]) ?? "",
                    Class      = Convert.ToInt32(row["class"])
                });
            }

            return list;
        }

        // Id'ye göre tek koltuk getirir.
        public static SeatModel? GetById(int id)
        {
            var res = Query(
                "SELECT * FROM seats WHERE id=@id",
                c => c.Parameters.AddWithValue("@id", id)
            );

            if (res.Count == 0)
                return null;

            var row = res[0];

            return new SeatModel
            {
                Id         = Convert.ToInt32(row["id"]),
                PlaneId    = Convert.ToInt32(row["plane_id"]),
                SeatNumber = Convert.ToString(row["seat_number"]) ?? "",
                Class      = Convert.ToInt32(row["class"])
            };
        }

        // Yeni koltuk ekler.
        public void Save()
        {
            SeatNumber = (SeatNumber ?? "").Trim();

            if (string.IsNullOrWhiteSpace(SeatNumber))
                throw new ArgumentException("Koltuk numarası boş olamaz.", nameof(SeatNumber));

            if (PlaneId <= 0)
                throw new ArgumentOutOfRangeException(nameof(PlaneId), "Geçersiz uçak ID.");

            if (Class is < 0 or > 1)
                throw new ArgumentOutOfRangeException(nameof(Class), "Koltuk sınıfı 0 veya 1 olmalıdır.");

            if (Id == 0)
            {
                // INSERT
                SqliteDbHelper.ExecuteNonQuery(
                    "INSERT INTO seats (plane_id, seat_number, class) VALUES (@pid, @sn, @c)",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@pid", PlaneId);
                        cmd.Parameters.AddWithValue("@sn", SeatNumber);
                        cmd.Parameters.AddWithValue("@c", Class);
                    });

                // Son eklenen id
                var newId = SqliteDbHelper.ExecuteScalar<long>("SELECT last_insert_rowid();", null);
                Id = (int)newId;
            }
            // Koltuk güncelleme özelliği daha sonra eklenebilir.
        }

        // Koltuğu siler
        public static void Delete(int id)
        {
            SqliteDbHelper.ExecuteNonQuery(
                "DELETE FROM seats WHERE id=@id",
                cmd => cmd.Parameters.AddWithValue("@id", id)
            );
        }
    }
}

