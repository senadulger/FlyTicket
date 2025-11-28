using prgmlab3.data;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;
using System.Runtime.CompilerServices;

namespace prgmlab3.Models
{
    public class FlightModel :BaseModel
    {
        public int Id { get; set; }
        public int PlaneId { get; set; }
        public int DepartureLocation { get; set; }
        public int ArrivalLocation { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
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
                    DepartureTime = DateTime.TryParse(Convert.ToString(r["departure_time"]), out var dttmp) ? dttmp : DateTime.MinValue,
                    ArrivalTime = DateTime.TryParse(Convert.ToString(r["arrival_time"]), out var attmp) ? attmp : DateTime.MinValue,
                    Price = Convert.ToDouble(r["price"])
                });
            }
            return flights;
        }

        public static FlightModel? GetById(int id)
        {
            var rows = SqliteDbHelper.ExecuteQuery("SELECT * FROM flights WHERE id=@id", cmd =>
            {
                cmd.Parameters.AddWithValue("@id", id);
            });
            if (rows.Count == 0) return null;
            var r = rows[0];
            return new FlightModel
            {
                Id = Convert.ToInt32(r["id"]),
                PlaneId = Convert.ToInt32(r["plane_id"]),
                DepartureLocation = Convert.ToInt32(r["departure_location"]),
                ArrivalLocation = Convert.ToInt32(r["arrival_location"]),
                DepartureTime = DateTime.TryParse(Convert.ToString(r["departure_time"]), out var dttmp2) ? dttmp2 : DateTime.MinValue,
                ArrivalTime = DateTime.TryParse(Convert.ToString(r["arrival_time"]), out var attmp2) ? attmp2 : DateTime.MinValue,
                Price = Convert.ToDouble(r["price"])
            };
        }

        public void Save()
        {
            if (Id == 0)
            {
                SqliteDbHelper.ExecuteNonQuery("INSERT INTO flights (plane_id, departure_location, arrival_location, departure_time, arrival_time, price) VALUES (@p, @d, @a, @dt, @at, @pr)", cmd =>
                {
                    cmd.Parameters.AddWithValue("@p", PlaneId);
                    cmd.Parameters.AddWithValue("@d", DepartureLocation);
                    cmd.Parameters.AddWithValue("@a", ArrivalLocation);
                    cmd.Parameters.AddWithValue("@dt", DepartureTime.ToString("s"));
                    cmd.Parameters.AddWithValue("@at", ArrivalTime.ToString("s"));
                    cmd.Parameters.AddWithValue("@pr", Price);
                });
            }
            else
            {
                // Update logic if needed
            }
        }

        public double CalculatePrice()
        {
            var timeToDeparture = DepartureTime - DateTime.Now;
            var days = timeToDeparture.TotalDays;

            if (days < 0) return Price;

            if (days >= 30)
            {
                return Price * 0.60;
            }
            else
            {
                double factor = 1.0 - (days / 30.0) * 0.4;
                return Price * factor;
            }
        }

        public static List<Dictionary<string, object>> SearchFlights(string departuateLocation,string ArrivalLocation)
        {
            return Query(@"SELECT 
        f.id,
        a1.name AS departure_name,
        a2.name AS arrival_name,
        f.plane_id,
        f.departure_time,
        f.arrival_time
        FROM flights f
        JOIN airports a1 ON f.departure_location = a1.id
        JOIN airports a2 ON f.arrival_location = a2.id
        WHERE a1.name = @departure_name
        AND a2.name = @arrival_name;",
            cmd =>
            {
                cmd.Parameters.AddWithValue("@departure_name", departuateLocation);
                cmd.Parameters.AddWithValue("@arrival_name", ArrivalLocation);
            });
        }
        

        public static void Delete(int id)
        {
            SqliteDbHelper.ExecuteNonQuery("DELETE FROM flights WHERE id=@id", cmd =>
            {
                cmd.Parameters.AddWithValue("@id", id);
            });
        }
    }
}
