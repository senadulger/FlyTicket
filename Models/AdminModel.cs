namespace prgmlab3.Models
{
    public class AdminModel : UserModel
    {
        public AdminModel(int id, string username, string mail)
        {
            _id = id;
            _username = username;
            _mail = mail;
            _role = 1;
        }

        public void AddPlane(string name, int seatCount)
        {
            Execute("INSERT INTO planes (name, seat_count) VALUES (@n, @s)", cmd =>
            {
                cmd.Parameters.AddWithValue("@n", name);
                cmd.Parameters.AddWithValue("@s", seatCount);
            });
        }

        public void RemovePlane(int planeId)
        {
            Execute("DELETE FROM planes WHERE id=@id", cmd =>
            {
                cmd.Parameters.AddWithValue("@id", planeId);
            });
        }

        public void AddFlight(int planeId, int dep, int arr, DateTime depTime, DateTime arrTime, float price)
        {
            Execute(@"INSERT INTO flights 
                (plane_id, departure_location, arrival_location, departure_time, arrival_time, price)
                VALUES (@pid, @dep, @arr, @dtime, @atime, @price)", cmd =>
            {
                cmd.Parameters.AddWithValue("@pid", planeId);
                cmd.Parameters.AddWithValue("@dep", dep);
                cmd.Parameters.AddWithValue("@arr", arr);
                cmd.Parameters.AddWithValue("@dtime", depTime);
                cmd.Parameters.AddWithValue("@atime", arrTime);
                cmd.Parameters.AddWithValue("@price", price);
            });
        }
    }
}
