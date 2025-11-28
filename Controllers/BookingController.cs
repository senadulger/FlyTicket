using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using prgmlab3.Models;
using System.Security.Claims;

namespace prgmlab3.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        public IActionResult Index(string? from, string? to, DateTime? startDate, DateTime? endDate)
        {
            List<FlightModel> flights;
            var all = FlightModel.GetAll();
            
            // Filter out past flights first
            all = all.Where(f => f.DepartureTime > DateTime.Now).ToList();

            // Fetch airports map
            var airports = AirportModel.Query("SELECT * FROM airports", null);
            var airportMap = new Dictionary<int, AirportModel>();
            foreach(var a in airports)
            {
                var am = new AirportModel
                {
                    Id = Convert.ToInt32(a["id"]),
                    City = (string)a["city"],
                    Name = (string)a["name"]
                };
                airportMap[am.Id] = am;
            }

            if (!string.IsNullOrEmpty(from) || !string.IsNullOrEmpty(to) || startDate.HasValue || endDate.HasValue)
            {
                flights = all.Where(f => 
                {
                    bool matchFrom = true;
                    bool matchTo = true;
                    bool matchDate = true;

                    if (!string.IsNullOrEmpty(from))
                    {
                        if (!airportMap.ContainsKey(f.DepartureLocation)) matchFrom = false;
                        else
                        {
                            var dep = airportMap[f.DepartureLocation];
                            matchFrom = dep.City.Contains(from, StringComparison.CurrentCultureIgnoreCase) || dep.Name.Contains(from, StringComparison.CurrentCultureIgnoreCase);
                        }
                    }

                    if (!string.IsNullOrEmpty(to))
                    {
                        if (!airportMap.ContainsKey(f.ArrivalLocation)) matchTo = false;
                        else
                        {
                            var arr = airportMap[f.ArrivalLocation];
                            matchTo = arr.City.Contains(to, StringComparison.CurrentCultureIgnoreCase) || arr.Name.Contains(to, StringComparison.CurrentCultureIgnoreCase);
                        }
                    }

                    if (startDate.HasValue && f.DepartureTime.Date < startDate.Value.Date) matchDate = false;
                    if (endDate.HasValue && f.DepartureTime.Date > endDate.Value.Date) matchDate = false;

                    return matchFrom && matchTo && matchDate;
                }).ToList();
            }
            else
            {
                flights = all;
            }

            // Update prices dynamically
            foreach (var f in flights)
            {
                f.Price = f.CalculatePrice();
            }

            // Populate ViewBag with Airport names for display
            var airportNameMap = new Dictionary<int, string>();
            foreach(var kvp in airportMap)
            {
                airportNameMap[kvp.Key] = $"{kvp.Value.City} ({kvp.Value.Name})";
            }
            ViewBag.AirportMap = airportNameMap;

            return View(flights);
        }

        public IActionResult SelectSeat(int flightId)
        {
            var flight = FlightModel.GetById(flightId);
            if (flight == null) return NotFound();

            // Check if flight is in the past
            if (flight.DepartureTime <= DateTime.Now)
            {
                TempData["Error"] = "Geçmiş uçuşlar için işlem yapılamaz.";
                return RedirectToAction(nameof(Index));
            }

            // Update price dynamically
            flight.Price = flight.CalculatePrice();

            var plane = PlaneModel.Query("SELECT * FROM planes WHERE id=@id", c => c.Parameters.AddWithValue("@id", flight.PlaneId));
            if (plane.Count == 0) return NotFound();
            
            var planeName = (string)plane[0]["name"];
            var seats = SeatModel.GetByPlane(flight.PlaneId);
            var reservedSeatIds = ReservationModel.GetReservedSeatIds(flightId);

            ViewBag.Flight = flight;
            ViewBag.PlaneName = planeName;
            ViewBag.ReservedSeatIds = reservedSeatIds;

            var allAirports = AirportModel.Query("SELECT * FROM airports", null);
            var airportNameMap = new Dictionary<int, string>();
            foreach(var a in allAirports)
            {
                airportNameMap[Convert.ToInt32(a["id"])] = $"{a["city"]} ({a["name"]})";
            }
            ViewBag.AirportMap = airportNameMap;

            return View(seats);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BookSeat(int flightId, int seatId)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var flight = FlightModel.GetById(flightId);
            if (flight == null) return NotFound();

            // Check if flight is in the past
            if (flight.DepartureTime <= DateTime.Now)
            {
                TempData["Error"] = "Geçmiş uçuşlar rezerve edilemez.";
                return RedirectToAction(nameof(Index));
            }

            // Check if seat is already reserved
            var reserved = ReservationModel.GetReservedSeatIds(flightId);
            if (reserved.Contains(seatId))
            {
                TempData["Error"] = "Bu koltuk maalesef az önce doldu.";
                return RedirectToAction(nameof(SelectSeat), new { flightId });
            }

            // Use dynamic price
            double currentPrice = flight.CalculatePrice();

            // Create reservation
            var res = new ReservationModel(userId, flightId, (float)currentPrice, seatId);
            res.Save();

            TempData["Success"] = "Rezervasyonunuz başarıyla oluşturuldu!";
            return RedirectToAction(nameof(MyReservations));
        }

        public IActionResult MyReservations()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            var reservations = ReservationModel.GetByUserId(userId);
            
            // Enrich data for view
            var viewModels = new List<Dictionary<string, object>>();
            foreach (var r in reservations)
            {
                var flight = FlightModel.GetById(r.FlightId);
                var seat = SeatModel.Query("SELECT seat_number FROM seats WHERE id=@id", c => c.Parameters.AddWithValue("@id", r.SeatId));
                string seatNum = seat.Count > 0 ? (string)seat[0]["seat_number"] : "?";
                
                string depCity = "?", arrCity = "?";
                bool canCancel = false;
                bool canCheckIn = false;

                if (flight != null)
                {
                    var dep = AirportModel.GetById(flight.DepartureLocation);
                    var arr = AirportModel.GetById(flight.ArrivalLocation);
                    depCity = dep?.City ?? "?";
                    arrCity = arr?.City ?? "?";
                    
                    // Can only cancel if flight is in the future
                    if (flight.DepartureTime > DateTime.Now)
                    {
                        canCancel = true;
                    }

                    // Check-in logic: e.g., within 24 hours of departure
                    // For simplicity, let's say check-in is allowed if flight is in future and not cancelled
                    if (flight.DepartureTime > DateTime.Now && !string.Equals(r.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                    {
                        canCheckIn = true;
                    }
                }

                viewModels.Add(new Dictionary<string, object>
                {
                    { "id", r.Id },
                    { "flight_info", $"{depCity} -> {arrCity}" },
                    { "date", flight?.DepartureTime.ToString("g") ?? "?" },
                    { "seat", seatNum },
                    { "price", r.Price },
                    { "status", r.Status },
                    { "can_cancel", canCancel },
                    { "can_checkin", canCheckIn }
                });
            }

            return View(viewModels);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CheckIn(int id)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            // Verify ownership
            var res = ReservationModel.Query("SELECT * FROM reservations WHERE id=@id", c => c.Parameters.AddWithValue("@id", id));
            if (res.Count == 0 || Convert.ToInt32(res[0]["user_id"]) != userId)
            {
                TempData["Error"] = "Rezervasyon bulunamadı veya size ait değil.";
                return RedirectToAction(nameof(MyReservations));
            }

            // Verify status
            string status = (res[0]["status"] as string) ?? "Pending";
            if (string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                 TempData["Error"] = "İptal edilmiş rezervasyon için check-in yapılamaz.";
                 return RedirectToAction(nameof(MyReservations));
            }
            if (string.Equals(status, "CheckedIn", StringComparison.OrdinalIgnoreCase))
            {
                 TempData["Info"] = "Zaten check-in yapılmış.";
                 return RedirectToAction(nameof(MyReservations));
            }

            ReservationModel.CheckInById(id);
            TempData["Success"] = "Check-in başarılı!";
            return RedirectToAction(nameof(MyReservations));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelReservation(int id)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();
            int userId = int.Parse(userIdStr);

            // Verify ownership and flight date
            var res = ReservationModel.Query("SELECT * FROM reservations WHERE id=@id", c => c.Parameters.AddWithValue("@id", id));
            if (res.Count == 0 || Convert.ToInt32(res[0]["user_id"]) != userId)
            {
                TempData["Error"] = "Rezervasyon bulunamadı veya size ait değil.";
                return RedirectToAction(nameof(MyReservations));
            }

            int flightId = Convert.ToInt32(res[0]["flight_id"]);
            var flight = FlightModel.GetById(flightId);
            if (flight == null || flight.DepartureTime <= DateTime.Now)
            {
                TempData["Error"] = "Geçmiş uçuşlar iptal edilemez.";
                return RedirectToAction(nameof(MyReservations));
            }

            ReservationModel.CancelById(id);
            TempData["Success"] = "Rezervasyon iptal edildi.";
            return RedirectToAction(nameof(MyReservations));
        }
    }
}
