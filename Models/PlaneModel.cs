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
    }
}
