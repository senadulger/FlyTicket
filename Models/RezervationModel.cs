namespace prgmlab3.Models
{
    public class ReservationModel : BaseModel
    {
        public const string StatusPending = "Pending";
        public const string StatusCancelled = "Cancelled";
        public const string StatusCheckedIn = "CheckedIn";
        public int Id { get; set; }
        public int UserId { get; set; }
        public int FlightId { get; set; }
        public float Price { get; set; }
        public int SeatId { get; set; }
        public string Status { get; set; } = StatusPending;

        public ReservationModel(int userId, int flightId, float price, int seatId)
        {
            UserId = userId;
            FlightId = flightId;
            Price = price;
            SeatId = seatId;
        }

        public void Save()
        {
            Execute("INSERT INTO reservations (user_id, flight_id, price, seat_id, status) VALUES (@u, @f, @p, @s, @st)", cmd =>
            {
                cmd.Parameters.AddWithValue("@u", UserId);
                cmd.Parameters.AddWithValue("@f", FlightId);
                cmd.Parameters.AddWithValue("@p", Price);
                cmd.Parameters.AddWithValue("@s", SeatId);
                cmd.Parameters.AddWithValue("@st", Status);
            });
        }

        public static void CancelById(int id)
        {
            Execute(@"
                UPDATE reservations
                SET status = @st
                WHERE id = @id
                  AND (status IS NULL OR status <> @st);
            ", cmd =>
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@st", StatusCancelled);
            });
        }

        public static void CheckInById(int id)
        {
            Execute(@"
                UPDATE reservations
                SET status = @st
                WHERE id = @id;
            ", cmd =>
            {
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@st", StatusCheckedIn);
            });
        }

        public void Cancel()
        {
            if (Id <= 0)
                throw new InvalidOperationException("Cancel() için geçerli bir Id gerekli.");

            CancelById(Id);
            Status = StatusCancelled; 
        }

        public static List<int> GetReservedSeatIds(int flightId)
        {
            var list = new List<int>();
            var rows = Query("SELECT seat_id FROM reservations WHERE flight_id=@fid AND (status IS NULL OR status <> @st)", cmd =>
            {
                cmd.Parameters.AddWithValue("@fid", flightId);
                cmd.Parameters.AddWithValue("@st", StatusCancelled);
            });

            foreach (var r in rows)
            {
                list.Add(Convert.ToInt32(r["seat_id"]));
            }
            return list;
        }

        public static List<ReservationModel> GetByUserId(int userId)
        {
            var list = new List<ReservationModel>();
            var rows = Query("SELECT * FROM reservations WHERE user_id=@uid", cmd => cmd.Parameters.AddWithValue("@uid", userId));
            foreach (var r in rows)
            {
                list.Add(new ReservationModel(
                    Convert.ToInt32(r["user_id"]),
                    Convert.ToInt32(r["flight_id"]),
                    Convert.ToSingle(r["price"]),
                    Convert.ToInt32(r["seat_id"])
                )
                {
                    Id = Convert.ToInt32(r["id"]),
                    Status = (r["status"] as string) ?? StatusPending
                });
            }
            return list;
        }
    }
}