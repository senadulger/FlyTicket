using prgmlab3.data;
namespace prgmlab3.Models
{
    public class PlaneModel : BaseModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public int SeatCount { get; set; }

        public static List<PlaneModel> GetAll()
        {
            var res = Query("SELECT * FROM planes", _ => { });
            List<PlaneModel> planes = new();
            foreach (var row in res)
            {
                planes.Add(new PlaneModel
                {
                    Id = Convert.ToInt32(row["id"]),
                    Name = (string)row["name"],
                    SeatCount = Convert.ToInt32(row["seat_count"])
                });
            }
            return planes;
        }

        public void Save()
        {
            if (Id == 0)
            {
                SqliteDbHelper.ExecuteNonQuery("INSERT INTO planes (name, seat_count) VALUES (@name, @seat)", cmd =>
                {
                    cmd.Parameters.AddWithValue("@name", Name);
                    cmd.Parameters.AddWithValue("@seat", SeatCount);
                });
                // Get last insert id
                var id = SqliteDbHelper.ExecuteScalar<long>("SELECT last_insert_rowid();", null);
                Id = (int)id;

                // Create seats for this plane
                for (int i = 1; i <= SeatCount; i++)
                {
                    var seatNumber = i.ToString();
                    SqliteDbHelper.ExecuteNonQuery("INSERT INTO seats (plane_id, seat_number, class) VALUES (@pid, @sn, @class)", cmd =>
                    {
                        cmd.Parameters.AddWithValue("@pid", Id);
                        cmd.Parameters.AddWithValue("@sn", seatNumber);
                        cmd.Parameters.AddWithValue("@class", 0);
                    });
                }
            }
            else
            {
                SqliteDbHelper.ExecuteNonQuery("UPDATE planes SET name=@name, seat_count=@seat WHERE id=@id", cmd =>
                {
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.Parameters.AddWithValue("@name", Name);
                    cmd.Parameters.AddWithValue("@seat", SeatCount);
                });
                // Note: updating seat_count won't automatically add/remove seats; this is a TODO for seat management.
            }
        }

        public static void Delete(int id)
        {
            // Remove seats first
            SqliteDbHelper.ExecuteNonQuery("DELETE FROM seats WHERE plane_id=@id", cmd => cmd.Parameters.AddWithValue("@id", id));
            // Then remove plane
            SqliteDbHelper.ExecuteNonQuery("DELETE FROM planes WHERE id=@id", cmd => cmd.Parameters.AddWithValue("@id", id));
        }
    }
}
