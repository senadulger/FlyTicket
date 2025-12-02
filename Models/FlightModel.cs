using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using prgmlab3.data;

namespace prgmlab3.Models
{
    // Uçuş bilgilerini temsil eden model.
    public class FlightModel : BaseModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Uçak seçimi zorunludur.")]
        [Display(Name = "Uçak")]
        public int PlaneId { get; set; }

        [Required(ErrorMessage = "Kalkış havaalanı zorunludur.")]
        [Display(Name = "Kalkış Havaalanı")]
        public int DepartureLocation { get; set; }

        [Required(ErrorMessage = "Varış havaalanı zorunludur.")]
        [Display(Name = "Varış Havaalanı")]
        public int ArrivalLocation { get; set; }

        [Required(ErrorMessage = "Kalkış zamanı zorunludur.")]
        [Display(Name = "Kalkış Zamanı")]
        public DateTime DepartureTime { get; set; }

        [Required(ErrorMessage = "Varış zamanı zorunludur.")]
        [Display(Name = "Varış Zamanı")]
        public DateTime ArrivalTime { get; set; }

        // Baz fiyat
        [Required(ErrorMessage = "Baz fiyat zorunludur.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Baz fiyat 0'dan büyük olmalıdır.")]
        [Display(Name = "Baz Fiyat")]
        public double Price { get; set; }



        public static List<FlightModel> GetAll()
        {
            var flights = new List<FlightModel>();
            var rows = SqliteDbHelper.ExecuteQuery("SELECT * FROM flights");

            foreach (var r in rows)
            {
                flights.Add(new FlightModel
                {
                    Id = Convert.ToInt32(r["id"]),
                    PlaneId = Convert.ToInt32(r["plane_id"]),
                    DepartureLocation = Convert.ToInt32(r["departure_location"]),
                    ArrivalLocation = Convert.ToInt32(r["arrival_location"]),
                    DepartureTime = DateTime.TryParse(Convert.ToString(r["departure_time"]), out var dttmp)
                        ? dttmp
                        : DateTime.MinValue,
                    ArrivalTime = DateTime.TryParse(Convert.ToString(r["arrival_time"]), out var attmp)
                        ? attmp
                        : DateTime.MinValue,
                    Price = Convert.ToDouble(r["price"])
                });
            }

            return flights;
        }

        public static FlightModel? GetById(int id)
        {
            var rows = SqliteDbHelper.ExecuteQuery(
                "SELECT * FROM flights WHERE id=@id",
                cmd => cmd.Parameters.AddWithValue("@id", id)
            );

            if (rows.Count == 0)
                return null;

            var r = rows[0];

            return new FlightModel
            {
                Id = Convert.ToInt32(r["id"]),
                PlaneId = Convert.ToInt32(r["plane_id"]),
                DepartureLocation = Convert.ToInt32(r["departure_location"]),
                ArrivalLocation = Convert.ToInt32(r["arrival_location"]),
                DepartureTime = DateTime.TryParse(Convert.ToString(r["departure_time"]), out var dttmp2)
                    ? dttmp2
                    : DateTime.MinValue,
                ArrivalTime = DateTime.TryParse(Convert.ToString(r["arrival_time"]), out var attmp2)
                    ? attmp2
                    : DateTime.MinValue,
                Price = Convert.ToDouble(r["price"])
            };
        }

        // Id = 0 ise yeni uçuş ekler, aksi halde mevcut uçuşu günceller.
        public void Save()
        {
            if (DepartureLocation == ArrivalLocation)
                throw new ArgumentException("Kalkış ve varış havaalanı aynı olamaz.");

            if (DepartureTime >= ArrivalTime)
                throw new ArgumentException("Varış zamanı, kalkış zamanından sonra olmalıdır.");

            if (Price <= 0)
                throw new ArgumentOutOfRangeException(nameof(Price), "Baz fiyat 0'dan büyük olmalıdır.");

            if (Id == 0)
            {
                SqliteDbHelper.ExecuteNonQuery(
                    @"INSERT INTO flights 
                      (plane_id, departure_location, arrival_location, departure_time, arrival_time, price) 
                      VALUES (@p, @d, @a, @dt, @at, @pr)",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@p", PlaneId);
                        cmd.Parameters.AddWithValue("@d", DepartureLocation);
                        cmd.Parameters.AddWithValue("@a", ArrivalLocation);
                        cmd.Parameters.AddWithValue("@dt", DepartureTime.ToString("s"));
                        cmd.Parameters.AddWithValue("@at", ArrivalTime.ToString("s"));
                        cmd.Parameters.AddWithValue("@pr", Price);
                    });

                var id = SqliteDbHelper.ExecuteScalar<long>("SELECT last_insert_rowid();", null);
                Id = (int)id;
            }
            else
            {
                SqliteDbHelper.ExecuteNonQuery(
                    @"UPDATE flights SET 
                          plane_id = @p,
                          departure_location = @d,
                          arrival_location = @a,
                          departure_time = @dt,
                          arrival_time = @at,
                          price = @pr
                      WHERE id = @id",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@id", Id);
                        cmd.Parameters.AddWithValue("@p", PlaneId);
                        cmd.Parameters.AddWithValue("@d", DepartureLocation);
                        cmd.Parameters.AddWithValue("@a", ArrivalLocation);
                        cmd.Parameters.AddWithValue("@dt", DepartureTime.ToString("s"));
                        cmd.Parameters.AddWithValue("@at", ArrivalTime.ToString("s"));
                        cmd.Parameters.AddWithValue("@pr", Price);
                    });
            }
        }

        public static void Delete(int id)
        {
            SqliteDbHelper.ExecuteNonQuery(
                "DELETE FROM flights WHERE id=@id",
                cmd => cmd.Parameters.AddWithValue("@id", id)
            );
        }




        // Uçuşa kalan gün + doluluk + koltuk tipi + sezon
        // occupancyRatio : 0.0–1.0 arası doluluk oranı
        public double CalculateDynamicPrice(int seatClass, double occupancyRatio, DateTime? nowOverride = null)
        {
            var now = nowOverride ?? DateTime.Now;

            // 1) Zaman faktörü
            var timeToDeparture = DepartureTime - now;
            var days = timeToDeparture.TotalDays;

            double timeFactor;
            if (days < 0)
            {
                // Geçmiş uçuş, zaman faktörü 1 ve direkt baz fiyat
                timeFactor = 1.0;
            }
            else if (days >= 30)
            {
                // 30+ gün varsa %40 indirim
                timeFactor = 0.60;
            }
            else
            {
                // 0–30 gün arasında lineer olarak 0.6 => 1.0
                // days=30 => 0.6, days=0 => 1.0
                timeFactor = 1.0 - (days / 30.0) * 0.4;
            }

            // 2) Doluluk faktörü
            // 0–30%  : 0.9   (indirim)
            // 30–70% : 1.0   (normal)
            // 70–90% : 1.2   (artış)
            // 90–100%: 1.4   (yüksek artış)
            double occ = Math.Clamp(occupancyRatio, 0.0, 1.0);
            double occupancyFactor;
            if (occ < 0.30)
                occupancyFactor = 0.90;
            else if (occ < 0.70)
                occupancyFactor = 1.00;
            else if (occ < 0.90)
                occupancyFactor = 1.20;
            else
                occupancyFactor = 1.40;

            // 3) Koltuk tipi faktörü
            // Economy : 1.0
            // Business: 1.5
            double seatFactor = seatClass == 1 ? 1.50 : 1.00;

            // 4) Sezon faktörü (ay bazlı)
            // Yüksek sezon: Haziran–Eylül  (6–9)        => 1.20
            // Orta sezon  : Mayıs, Ekim, Aralık (5,10,12)=> 1.05
            // Düşük sezon : diğer aylar                  => 0.95
            int m = now.Month;
            double seasonFactor;
            if (m >= 6 && m <= 9)
            {
                seasonFactor = 1.20;
            }
            else if (m == 5 || m == 10 || m == 12)
            {
                seasonFactor = 1.05;
            }
            else
            {
                seasonFactor = 0.95;
            }

            double finalFactor = timeFactor * occupancyFactor * seatFactor * seasonFactor;
            double result = Price * finalFactor;

            return Math.Round(result, 2, MidpointRounding.AwayFromZero);
        }



        public static List<Dictionary<string, object>> SearchFlights(string departureLocationName, string arrivalLocationName)
        {
            return Query(@"
                SELECT 
                    f.id,
                    a1.name AS departure_name,
                    a2.name AS arrival_name,
                    f.plane_id,
                    f.departure_time,
                    f.arrival_time,
                    f.price
                FROM flights f
                JOIN airports a1 ON f.departure_location = a1.id
                JOIN airports a2 ON f.arrival_location = a2.id
                WHERE a1.name = @departure_name
                  AND a2.name = @arrival_name;",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@departure_name", departureLocationName);
                    cmd.Parameters.AddWithValue("@arrival_name", arrivalLocationName);
                });
        }


    }
}