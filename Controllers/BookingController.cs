using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using prgmlab3.Models;

namespace prgmlab3.Controllers
{
    public class BookingController : Controller
    {

        //  UÇUŞ ARAMA / LİSTELEME
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Index(string? from, string? to, DateTime? startDate, DateTime? endDate)
        {
            var now = DateTime.Now;

            var all = FlightModel.GetAll()
                                 .Where(f => f.DepartureTime > now)
                                 .ToList();

            var airports = AirportModel.Query("SELECT * FROM airports", null);
            var airportMap = new Dictionary<int, AirportModel>();
            foreach (var a in airports)
            {
                var am = new AirportModel
                {
                    Id = Convert.ToInt32(a["id"]),
                    City = Convert.ToString(a["city"]) ?? "",
                    Name = Convert.ToString(a["name"]) ?? ""
                };
                airportMap[am.Id] = am;
            }

            List<FlightModel> flights;

            if (!string.IsNullOrEmpty(from) ||
                !string.IsNullOrEmpty(to) ||
                startDate.HasValue ||
                endDate.HasValue)
            {
                flights = all.Where(f =>
                {
                    bool matchFrom = true;
                    bool matchTo = true;
                    bool matchDate = true;

                    if (!string.IsNullOrEmpty(from))
                    {
                        if (!airportMap.ContainsKey(f.DepartureLocation))
                        {
                            matchFrom = false;
                        }
                        else
                        {
                            var dep = airportMap[f.DepartureLocation];
                            matchFrom =
                                dep.City.Contains(from, StringComparison.CurrentCultureIgnoreCase) ||
                                dep.Name.Contains(from, StringComparison.CurrentCultureIgnoreCase);
                        }
                    }

                    if (!string.IsNullOrEmpty(to))
                    {
                        if (!airportMap.ContainsKey(f.ArrivalLocation))
                        {
                            matchTo = false;
                        }
                        else
                        {
                            var arr = airportMap[f.ArrivalLocation];
                            matchTo =
                                arr.City.Contains(to, StringComparison.CurrentCultureIgnoreCase) ||
                                arr.Name.Contains(to, StringComparison.CurrentCultureIgnoreCase);
                        }
                    }

                    if (startDate.HasValue && f.DepartureTime.Date < startDate.Value.Date)
                        matchDate = false;
                    if (endDate.HasValue && f.DepartureTime.Date > endDate.Value.Date)
                        matchDate = false;

                    return matchFrom && matchTo && matchDate;
                }).ToList();
            }
            else
            {
                flights = all;
            }

            // Dinamik fiyat 
            foreach (var f in flights)
            {
                var reservedSeatIds = ReservationModel.GetReservedSeatIds(f.Id);
                var allSeats = SeatModel.GetByPlane(f.PlaneId);
                int totalSeats = allSeats.Count == 0 ? 1 : allSeats.Count;  
                double occupancy = (double)reservedSeatIds.Count / totalSeats;

                f.Price = f.CalculateDynamicPrice(
                    seatClass: 0,
                    occupancyRatio: occupancy,
                    nowOverride: now
                );
            }

            var airportNameMap = new Dictionary<int, string>();
            foreach (var kvp in airportMap)
            {
                airportNameMap[kvp.Key] = $"{kvp.Value.City} ({kvp.Value.Name})";
            }
            ViewBag.AirportMap = airportNameMap;

            return View(flights);
        }

        //  KOLTUK SEÇİMİ
        [HttpGet]
        [Authorize]
        public IActionResult SelectSeat(int flightId, string? couponCode = null)
        {
            var flight = FlightModel.GetById(flightId);
            if (flight == null)
                return NotFound();

            var now = DateTime.Now;

            if (flight.DepartureTime <= now)
            {
                TempData["Error"] = "Geçmiş uçuşlar için işlem yapılamaz.";
                return RedirectToAction(nameof(Index));
            }

            var seats = SeatModel.GetByPlane(flight.PlaneId);
            var reservedSeatIds = ReservationModel.GetReservedSeatIds(flightId);

            int totalSeats = seats.Count == 0 ? 1 : seats.Count;
            double occupancy = (double)reservedSeatIds.Count / totalSeats;
            // 0=Economy, 1=Business
            double priceEconomy = flight.CalculateDynamicPrice(0, occupancy, now);
            double priceBusiness = flight.CalculateDynamicPrice(1, occupancy, now);

            // Kupon kontrolü
            CouponModel.EnsureTableExists(); 

            if (!string.IsNullOrEmpty(couponCode))
            {
                var coupon = CouponModel.GetByCode(couponCode);
                if (coupon != null && coupon.IsValid())
                {
                    double discountRate = coupon.DiscountPercent / 100.0;
                    
                    priceEconomy -= (priceEconomy * discountRate);
                    priceBusiness -= (priceBusiness * discountRate);

                    ViewBag.CouponCode = coupon.Code;
                    ViewBag.IsCouponValid = true;
                    ViewBag.CouponMessage = $"%{coupon.DiscountPercent} indirim uygulandı!";
                }
                else
                {
                    ViewBag.IsCouponValid = false;
                    ViewBag.CouponMessage = "Geçersiz veya süresi dolmuş kupon kodu.";
                }
            }
            priceEconomy = Math.Round(priceEconomy, 2);
            priceBusiness = Math.Round(priceBusiness, 2);

            flight.Price = priceEconomy; 
            ViewBag.PriceEconomy = priceEconomy;
            ViewBag.PriceBusiness = priceBusiness;
            
            var planeRows = PlaneModel.Query(
                "SELECT * FROM planes WHERE id=@id",
                c => c.Parameters.AddWithValue("@id", flight.PlaneId)
            );
            if (planeRows.Count == 0)
                return NotFound();

            var planeName = Convert.ToString(planeRows[0]["name"]) ?? "?";

            ViewBag.Flight = flight;
            ViewBag.PlaneName = planeName;
            ViewBag.ReservedSeatIds = reservedSeatIds;

            var allAirports = AirportModel.Query("SELECT * FROM airports", null);
            var airportNameMap = new Dictionary<int, string>();
            foreach (var a in allAirports)
            {
                var id = Convert.ToInt32(a["id"]);
                var city = Convert.ToString(a["city"]) ?? "";
                var name = Convert.ToString(a["name"]) ?? "";
                airportNameMap[id] = $"{city} ({name})";
            }
            ViewBag.AirportMap = airportNameMap;

            return View(seats);
        }

        //  REZERVASYON OLUŞTUR
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult BookSeat(int flightId, int seatId, string? couponCode = null)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var flight = FlightModel.GetById(flightId);
            if (flight == null)
                return NotFound();

            var now = DateTime.Now;

            if (flight.DepartureTime <= now)
            {
                TempData["Error"] = "Geçmiş uçuşlar rezerve edilemez.";
                return RedirectToAction(nameof(Index));
            }

            var reserved = ReservationModel.GetReservedSeatIds(flightId);
            if (reserved.Contains(seatId))
            {
                TempData["Error"] = "Bu koltuk maalesef az önce doldu.";
                return RedirectToAction(nameof(SelectSeat), new { flightId });
            }

            var seatRows = SeatModel.Query(
                "SELECT * FROM seats WHERE id=@id",
                c => c.Parameters.AddWithValue("@id", seatId)
            );
            if (seatRows.Count == 0)
            {
                TempData["Error"] = "Koltuk bulunamadı.";
                return RedirectToAction(nameof(SelectSeat), new { flightId });
            }
            int seatClass = Convert.ToInt32(seatRows[0]["class"]);

            var allSeats = SeatModel.GetByPlane(flight.PlaneId);
            int totalSeats = allSeats.Count == 0 ? 1 : allSeats.Count;
            double occupancyRatio = (double)reserved.Count / totalSeats;

            double currentPrice = flight.CalculateDynamicPrice(
                seatClass: seatClass,
                occupancyRatio: occupancyRatio,
                nowOverride: now
            );

            // FİNAL İÇİN EKLENDİ: Kupon indirimi
            if (!string.IsNullOrEmpty(couponCode))
            {
                 var coupon = CouponModel.GetByCode(couponCode);
                 if (coupon != null && coupon.IsValid())
                 {
                     double discount = currentPrice * (coupon.DiscountPercent / 100.0);
                     currentPrice -= discount;
                 }
            }

            var res = new ReservationModel(userId, flightId, (float)currentPrice, seatId);
            res.Save();

            TempData["Success"] = "Rezervasyonunuz başarıyla oluşturuldu!";
            return RedirectToAction(nameof(MyReservations));
        }

        //  KULLANICININ REZERVASYONLARI
        [HttpGet]
        [Authorize]
        public IActionResult MyReservations()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var now = DateTime.Now;
            var reservations = ReservationModel.GetByUserId(userId);

            var viewModels = new List<Dictionary<string, object>>();

            foreach (var r in reservations)
            {
                var flight = FlightModel.GetById(r.FlightId);

                var seatRows = SeatModel.Query(
                    "SELECT seat_number FROM seats WHERE id=@id",
                    c => c.Parameters.AddWithValue("@id", r.SeatId)
                );
                string seatNum = seatRows.Count > 0
                    ? Convert.ToString(seatRows[0]["seat_number"]) ?? "?"
                    : "?";

                string depCity = "?";
                string arrCity = "?";
                bool canCancel = false;
                bool canCheckIn = false;
                string cancelReason = "";

                if (flight != null)
                {
                    var dep = AirportModel.GetById(flight.DepartureLocation);
                    var arr = AirportModel.GetById(flight.ArrivalLocation);
                    depCity = dep?.City ?? "?";
                    arrCity = arr?.City ?? "?";

                    // FİNAL İÇİN EKLENDİ: Uçuşa 24 saatten az kaldıysa iptal edilemez.
                    if ((flight.DepartureTime - now).TotalHours >= 24)
                    {
                        canCancel = true;
                    }
                    else if (flight.DepartureTime > now)
                    {
                        // Gelecek uçuş ama 24 saatten az kalmış
                        cancelReason = "Uçuşunuza 24 saatten az kaldığı için iptal edilemez.";
                    }
                    else
                    {
                        // Geçmiş uçuş
                        cancelReason = "Geçmiş Uçuş";
                    }

                    var status = r.Status ?? "Pending";
                    if (flight.DepartureTime > now &&
                        !string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase) &&
                        !string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
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
                    { "status", r.Status ?? "Pending" },
                    { "can_cancel", canCancel },
                    { "cancel_reason", cancelReason },
                    { "can_checkin", canCheckIn }
                });
            }

            return View(viewModels);
        }

        //  CHECK-IN
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult CheckIn(int id)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            // Kullanıcıya ait mi?
            var res = ReservationModel.Query(
                "SELECT * FROM reservations WHERE id=@id",
                c => c.Parameters.AddWithValue("@id", id)
            );
            if (res.Count == 0 || Convert.ToInt32(res[0]["user_id"]) != userId)
            {
                TempData["Error"] = "Rezervasyon bulunamadı veya size ait değil.";
                return RedirectToAction(nameof(MyReservations));
            }

            string status = Convert.ToString(res[0]["status"]) ?? "Pending";

            if (string.Equals(status, "Cancelled", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(status, "CANCELLED", StringComparison.OrdinalIgnoreCase))
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

        //  REZERVASYON İPTAL
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public IActionResult CancelReservation(int id)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            // Kullanıcıya ait mi?
            var res = ReservationModel.Query(
                "SELECT * FROM reservations WHERE id=@id",
                c => c.Parameters.AddWithValue("@id", id)
            );
            if (res.Count == 0 || Convert.ToInt32(res[0]["user_id"]) != userId)
            {
                TempData["Error"] = "Rezervasyon bulunamadı veya size ait değil.";
                return RedirectToAction(nameof(MyReservations));
            }

            int flightId = Convert.ToInt32(res[0]["flight_id"]);
            var flight = FlightModel.GetById(flightId);
            var now = DateTime.Now;

            if (flight == null)
            {
                 TempData["Error"] = "Uçuş bulunamadı.";
                 return RedirectToAction(nameof(MyReservations));
            }

            // Geçmiş uçuş mu?
            if (flight.DepartureTime <= now)
            {
                TempData["Error"] = "Geçmiş uçuş olduğu için iptal edilemez.";
                return RedirectToAction(nameof(MyReservations));
            }

            // 24 saat kuralı
            if ((flight.DepartureTime - now).TotalHours < 24)
            {
                TempData["Error"] = "Uçuşunuza 24 saatten az kaldığı için iptal edilemez.";
                return RedirectToAction(nameof(MyReservations));
            }

            ReservationModel.CancelById(id);
            TempData["Success"] = "Rezervasyon iptal edildi.";
            return RedirectToAction(nameof(MyReservations));
        }
    }
}