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
    }
}
