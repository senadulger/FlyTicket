namespace prgmlab3.Models;

using prgmlab3.data;

class RezervationModel
{
    public RezervationModel(int user_id, int flight_id, float price, int seat_id)
    {
        SqliteDbHelper.ExecuteNonQuery("Insert INTO rezervation (user_id,flight_id,price,seat_id,status) VALUES (@userid,@flight_id,@price,@seat_id,@status)",
        cmd =>
        {
            cmd.Parameters.AddWithValue("@userid", user_id);
            cmd.Parameters.AddWithValue("@flight_id", flight_id);
            cmd.Parameters.AddWithValue("@price", price);
            cmd.Parameters.AddWithValue("@seat_id", seat_id);
            cmd.Parameters.AddWithValue("@status","OK");
        });
    }

}