namespace prgmlab3.Models
{
    public class SeatModel : BaseModel
    {
        public int Id { get; set; }
        public int PlaneId { get; set; }
        public string SeatNumber { get; set; } = "";
        public int Class { get; set; } // 0: Economy, 1: Business

        public static List<SeatModel> GetByPlane(int planeId)
        {
            var res = Query("SELECT * FROM seats WHERE plane_id=@id", c => c.Parameters.AddWithValue("@id", planeId));
            List<SeatModel> list = new();
            foreach (var row in res)
            {
                list.Add(new SeatModel
                {
                    Id = Convert.ToInt32(row["id"]),
                    PlaneId = Convert.ToInt32(row["plane_id"]),
                    SeatNumber = (string)row["seat_number"],
                    Class = Convert.ToInt32(row["class"])
                });
            }
            return list;
        }
    }
}
