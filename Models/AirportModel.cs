using prgmlab3.data;
namespace prgmlab3.Models
{
    public class AirportModel : BaseModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public string City { get; set; } = "";
        public string Name { get; set; } = "";
        public string Country { get; set; } = "";

        public static AirportModel? GetById(int id)
        {
            var res = Query("SELECT * FROM airports WHERE id=@id", c => c.Parameters.AddWithValue("@id", id));
            if (res.Count == 0) return null;
            var r = res[0];
            return new AirportModel
            {
                Id = (int)r["id"],
                Code = (string)r["code"],
                City = (string)r["city"],
                Name = (string)r["name"],
                Country = (string)r["country"]
            };
        }

        public void Save()
        {
            if (Id == 0)
            {
                SqliteDbHelper.ExecuteNonQuery("INSERT INTO airports (code, city, name, country) VALUES (@code, @city, @name, @country)", cmd =>
                {
                    cmd.Parameters.AddWithValue("@code", Code ?? "");
                    cmd.Parameters.AddWithValue("@city", City ?? "");
                    cmd.Parameters.AddWithValue("@name", Name ?? "");
                    cmd.Parameters.AddWithValue("@country", Country ?? "");
                });
                var id = SqliteDbHelper.ExecuteScalar<long>("SELECT last_insert_rowid();", null);
                Id = (int)id;
            }
            else
            {
                SqliteDbHelper.ExecuteNonQuery("UPDATE airports SET code=@code, city=@city, name=@name, country=@country WHERE id=@id", cmd =>
                {
                    cmd.Parameters.AddWithValue("@id", Id);
                    cmd.Parameters.AddWithValue("@code", Code ?? "");
                    cmd.Parameters.AddWithValue("@city", City ?? "");
                    cmd.Parameters.AddWithValue("@name", Name ?? "");
                    cmd.Parameters.AddWithValue("@country", Country ?? "");
                });
            }
        }

        public static void Delete(int id)
        {
            SqliteDbHelper.ExecuteNonQuery("DELETE FROM airports WHERE id=@id", cmd => cmd.Parameters.AddWithValue("@id", id));
        }
    }
}
