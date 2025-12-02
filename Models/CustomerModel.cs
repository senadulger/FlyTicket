namespace prgmlab3.Models
{
    public class CustomerModel : UserModel
    {
        public CustomerModel(int id, string username, string mail)
        {
            _id = id;
            _username = username;
            _mail = mail;
            _role = 0;
        }

        public ReservationModel? CreateReservation(int flightId, int seatId)
        {
            var flight = FlightModel.GetById(flightId);
            if (flight == null) return null;

            ReservationModel reservation = new ReservationModel(_id, flightId, (float)flight.Price, seatId);
            reservation.Save();
            return reservation;
        }
    }
}