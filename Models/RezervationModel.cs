namespace prgmlab3.Models
{
    public class ReservationModel : BaseModel
    {
        public const string StatusPending = "Pending";
        public const string StatusCancelled = "Cancelled";
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

        public void Cancel()
        {
            if (Id <= 0)
                throw new InvalidOperationException("Cancel() için geçerli bir Id gerekli.");

            CancelById(Id);
            Status = StatusCancelled; // local durumunu da güncelle
        }
    }
}
