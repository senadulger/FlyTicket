using prgmlab3.data;
using System.Collections.Generic;
using System;

namespace prgmlab3.Models
{
    public class FlightModel :BaseModel
    {
        public int Id { get; set; }
        public int PlaneId { get; set; }
        public string From { get; set; } = "";
        public string To { get; set; } = "";
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public double Price { get; set; }

        public static List<FlightModel> GetAll()
        {
            var flights = new List<FlightModel>();
            var rows = SqliteDbHelper.ExecuteQuery("SELECT * FROM Flights");
            foreach (var r in rows)
            {
                flights.Add(new FlightModel
                {
                    Id = (int)r["Id"],
                    PlaneId = (int)r["PlaneId"],
                    From = (string)r["From"],
                    To = (string)r["To"],
                    DepartureTime = DateTime.Parse((string)r["DepartureTime"]),
                    ArrivalTime = DateTime.Parse((string)r["ArrivalTime"]),
                    Price = Convert.ToDouble(r["Price"])
                });
            }
            return flights;
        }

        public static FlightModel? GetById(int id)
        {
            var rows = SqliteDbHelper.ExecuteQuery("SELECT * FROM Flights WHERE Id=@id", cmd =>
            {
                cmd.Parameters.AddWithValue("@id", id);
            });
            if (rows.Count == 0) return null;
            var r = rows[0];
            return new FlightModel
            {
                Id = (int)r["Id"],
                PlaneId = (int)r["PlaneId"],
                From = (string)r["From"],
                To = (string)r["To"],
                DepartureTime = DateTime.Parse((string)r["DepartureTime"]),
                ArrivalTime = DateTime.Parse((string)r["ArrivalTime"]),
                Price = Convert.ToDouble(r["Price"])
            };
        }

        public void Save()
        {
            if (Id == 0)
            {
                SqliteDbHelper.ExecuteNonQuery(
                    "INSERT INTO Flights (PlaneId, From, To, DepartureTime, ArrivalTime, Price) VALUES (@p,@f,@t,@d,@a,@pr)",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@p", PlaneId);
                        cmd.Parameters.AddWithValue("@f", From);
                        cmd.Parameters.AddWithValue("@t", To);
                        cmd.Parameters.AddWithValue("@d", DepartureTime.ToString("s"));
                        cmd.Parameters.AddWithValue("@a", ArrivalTime.ToString("s"));
                        cmd.Parameters.AddWithValue("@pr", Price);
                    });
            }
            else
            {
                SqliteDbHelper.ExecuteNonQuery(
                    "UPDATE Flights SET PlaneId=@p, From=@f, To=@t, DepartureTime=@d, ArrivalTime=@a, Price=@pr WHERE Id=@id",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@id", Id);
                        cmd.Parameters.AddWithValue("@p", PlaneId);
                        cmd.Parameters.AddWithValue("@f", From);
                        cmd.Parameters.AddWithValue("@t", To);
                        cmd.Parameters.AddWithValue("@d", DepartureTime.ToString("s"));
                        cmd.Parameters.AddWithValue("@a", ArrivalTime.ToString("s"));
                        cmd.Parameters.AddWithValue("@pr", Price);
                    });
            }
        }

        public static void Delete(int id)
        {
            SqliteDbHelper.ExecuteNonQuery("DELETE FROM Flights WHERE Id=@id", cmd =>
            {
                cmd.Parameters.AddWithValue("@id", id);
            });
        }
    }
}
