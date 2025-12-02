using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using prgmlab3.data;
using prgmlab3.Models;
using System.Collections.Generic;

namespace prgmlab3.Controllers
{
    // Admin tarafındaki tüm yönetimsel işlemleri yöneten controller.
    // Uçak, havaalanı, uçuş, koltuk, kullanıcı ve rezervasyon yönetimi burada yapılır.
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // --- USERS ---
        public IActionResult Users()
        {
            var users = SqliteDbHelper.ExecuteQuery("SELECT * FROM users", null);
            return View(users);
        }

        // --- PLANES ---
        public IActionResult Planes()
        {
            var planes = SqliteDbHelper.ExecuteQuery("SELECT * FROM planes", null);
            return View(planes);
        }

        // GET: Admin/AddPlane
        public IActionResult AddPlane()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddPlane([FromForm] PlaneModel model)
        {
            try
            {
                if (model == null)
                {
                    TempData["Error"] = "Geçersiz veri.";
                    return RedirectToAction(nameof(Planes));
                }
                if (string.IsNullOrWhiteSpace(model.Name))
                {
                    TempData["Error"] = "Uçak adı boş olamaz.";
                    return RedirectToAction(nameof(AddPlane));
                }
                if (model.SeatCount <= 0)
                {
                    TempData["Error"] = "Koltuk sayısı pozitif olmalıdır.";
                    return RedirectToAction(nameof(AddPlane));
                }

                var dupPlane = SqliteDbHelper.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM planes WHERE name=@name",
                    cmd => cmd.Parameters.AddWithValue("@name", model.Name.Trim())
                );
                if (dupPlane > 0)
                {
                    TempData["Error"] = "Aynı isimde bir uçak zaten mevcut.";
                    return RedirectToAction(nameof(AddPlane));
                }

                model.Save();
                TempData["Success"] = "Uçak eklendi ve koltuklar oluşturuldu.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Uçak eklenirken hata: " + ex.Message;
            }
            return RedirectToAction(nameof(Planes));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeletePlane(int id)
        {
            try
            {
                // Bu uçağa bağlı uçuş var mı?
                var fcount = SqliteDbHelper.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM flights WHERE plane_id=@id",
                    cmd => cmd.Parameters.AddWithValue("@id", id)
                );
                if (fcount > 0)
                {
                    TempData["Error"] = "Bu uçak için uçuşlar mevcut: silmeden önce uçuşları kaldırın.";
                    return RedirectToAction(nameof(Planes));
                }

                PlaneModel.Delete(id);
                TempData["Success"] = "Uçak silindi.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Uçak silinirken hata: " + ex.Message;
            }
            return RedirectToAction(nameof(Planes));
        }

        // --- AIRPORTS ---
        public IActionResult Airports()
        {
            var airports = SqliteDbHelper.ExecuteQuery("SELECT * FROM airports", null);
            return View(airports);
        }

        // GET: Admin/AddAirport
        public IActionResult AddAirport()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddAirport([FromForm] AirportModel model)
        {
            try
            {
                if (model == null)
                {
                    TempData["Error"] = "Geçersiz veri.";
                    return RedirectToAction(nameof(Airports));
                }
                if (string.IsNullOrWhiteSpace(model.Name) || string.IsNullOrWhiteSpace(model.City))
                {
                    TempData["Error"] = "Havaalanı adı ve şehir gerekli.";
                    return RedirectToAction(nameof(AddAirport));
                }

                // Kod tekrar kontrolü
                var dup = SqliteDbHelper.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM airports WHERE code=@code OR (city=@city AND name=@name)",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@code", model.Code?.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@city", model.City?.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@name", model.Name?.Trim() ?? "");
                    });

                if (dup > 0)
                {
                    TempData["Error"] = "Benzer bir havaalanı zaten mevcut.";
                    return RedirectToAction(nameof(AddAirport));
                }

                SqliteDbHelper.ExecuteNonQuery(
                    "INSERT INTO airports (code, city, name, country) VALUES (@code,@city,@name,@country)",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@code", model.Code?.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@city", model.City?.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@name", model.Name?.Trim() ?? "");
                        cmd.Parameters.AddWithValue("@country", model.Country?.Trim() ?? "");
                    });

                TempData["Success"] = "Havaalanı eklendi.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Havaalanı eklenirken hata: " + ex.Message;
            }
            return RedirectToAction(nameof(Airports));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteAirport(int id)
        {
            try
            {
                // Bu havaalanına bağlı kalkış veya varış var mı?
                var fcount = SqliteDbHelper.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM flights WHERE departure_location=@id OR arrival_location=@id",
                    cmd => cmd.Parameters.AddWithValue("@id", id)
                );
                if (fcount > 0)
                {
                    TempData["Error"] = "Bu havaalanı için uçuşlar mevcut: silmeden önce uçuşları kaldırın.";
                    return RedirectToAction(nameof(Airports));
                }

                SqliteDbHelper.ExecuteNonQuery(
                    "DELETE FROM airports WHERE id=@id",
                    cmd => cmd.Parameters.AddWithValue("@id", id)
                );
                TempData["Success"] = "Havaalanı silindi.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Havaalanı silinirken hata: " + ex.Message;
            }
            return RedirectToAction(nameof(Airports));
        }

        // --- FLIGHTS ---
        public IActionResult Flights()
        {
            var flights = SqliteDbHelper.ExecuteQuery(@"
                SELECT f.id,
                       p.name AS plane_name,
                       a1.city AS departure_city,
                       a2.city AS arrival_city, 
                       f.departure_time,
                       f.arrival_time,
                       f.price
                FROM flights f
                JOIN planes   p  ON f.plane_id = p.id
                JOIN airports a1 ON f.departure_location = a1.id
                JOIN airports a2 ON f.arrival_location   = a2.id
            ", null);
            return View(flights);
        }

        // GET: Admin/AddFlight
        public IActionResult AddFlight()
        {
            var planes = SqliteDbHelper.ExecuteQuery("SELECT * FROM planes", null);
            var airports = SqliteDbHelper.ExecuteQuery("SELECT * FROM airports", null);
            ViewBag.Planes = planes;
            ViewBag.Airports = airports;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddFlight([FromForm] FlightModel model)
        {
            try
            {
                if (model == null)
                {
                    TempData["Error"] = "Geçersiz veri.";
                    return RedirectToAction(nameof(Flights));
                }

                // Temel validasyonlar
                if (model.PlaneId <= 0)
                {
                    TempData["Error"] = "Uçak bilgisi eksik.";
                    return RedirectToAction(nameof(AddFlight));
                }
                if (model.DepartureLocation <= 0 || model.ArrivalLocation <= 0)
                {
                    TempData["Error"] = "Havaalanı bilgileri eksik.";
                    return RedirectToAction(nameof(AddFlight));
                }
                if (model.DepartureLocation == model.ArrivalLocation)
                {
                    TempData["Error"] = "Kalkış ve varış aynı olamaz.";
                    return RedirectToAction(nameof(AddFlight));
                }
                if (model.DepartureTime >= model.ArrivalTime)
                {
                    TempData["Error"] = "Varış zamanı, kalkış zamanından sonra olmalıdır.";
                    return RedirectToAction(nameof(AddFlight));
                }
                if (model.Price <= 0)
                {
                    TempData["Error"] = "Baz fiyat 0'dan büyük olmalıdır.";
                    return RedirectToAction(nameof(AddFlight));
                }

                var plane = SqliteDbHelper.ExecuteQuery(
                    "SELECT id FROM planes WHERE id=@id",
                    cmd => cmd.Parameters.AddWithValue("@id", model.PlaneId)
                );
                if (plane.Count == 0)
                {
                    TempData["Error"] = "Seçilen uçak bulunamadı.";
                    return RedirectToAction(nameof(AddFlight));
                }

                var dep = SqliteDbHelper.ExecuteQuery(
                    "SELECT id FROM airports WHERE id=@id",
                    cmd => cmd.Parameters.AddWithValue("@id", model.DepartureLocation)
                );
                var arr = SqliteDbHelper.ExecuteQuery(
                    "SELECT id FROM airports WHERE id=@id",
                    cmd => cmd.Parameters.AddWithValue("@id", model.ArrivalLocation)
                );
                if (dep.Count == 0 || arr.Count == 0)
                {
                    TempData["Error"] = "Seçilen havaalanı(lar) bulunamadı.";
                    return RedirectToAction(nameof(AddFlight));
                }

                // Aynı uçak için zaman çakışmasını kontrol etme
                var overlaps = SqliteDbHelper.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM flights WHERE plane_id=@pid AND ((departure_time < @arr AND arrival_time > @dep))",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@pid", model.PlaneId);
                        cmd.Parameters.AddWithValue("@dep", model.DepartureTime.ToString("s"));
                        cmd.Parameters.AddWithValue("@arr", model.ArrivalTime.ToString("s"));
                    });

                if (overlaps > 0)
                {
                    TempData["Error"] = "Aynı uçak için zaman çakışması mevcut.";
                    return RedirectToAction(nameof(AddFlight));
                }

                model.Save();
                TempData["Success"] = "Uçuş eklendi.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Ekleme sırasında hata: " + ex.Message;
            }
            return RedirectToAction(nameof(Flights));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditFlight(int id, DateTime departureTime, DateTime arrivalTime)
        {
            try
            {
                var flight = FlightModel.GetById(id);
                if (flight == null)
                {
                    TempData["Error"] = "Uçuş bulunamadı.";
                    return RedirectToAction(nameof(Flights));
                }

                if (departureTime >= arrivalTime)
                {
                    TempData["Error"] = "Varış zamanı, kalkış zamanından sonra olmalıdır.";
                    return RedirectToAction(nameof(Flights));
                }

                // Aynı uçak için çakışma kontrolü
                var overlaps = SqliteDbHelper.ExecuteScalar<int>(
                    @"SELECT COUNT(*) FROM flights
                    WHERE plane_id = @pid
                        AND id <> @fid
                        AND (departure_time < @arr AND arrival_time > @dep)",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@pid", flight.PlaneId);
                        cmd.Parameters.AddWithValue("@fid", flight.Id);
                        cmd.Parameters.AddWithValue("@dep", departureTime.ToString("s"));
                        cmd.Parameters.AddWithValue("@arr", arrivalTime.ToString("s"));
                    });

                if (overlaps > 0)
                {
                    TempData["Error"] = "Bu saatlerde aynı uçak için başka bir uçuş var (zaman çakışması).";
                    return RedirectToAction(nameof(Flights));
                }

                // Saatleri güncelleme
                flight.DepartureTime = departureTime;
                flight.ArrivalTime   = arrivalTime;
                flight.Save();

                TempData["Success"] = "Uçuş saatleri güncellendi.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Güncelleme sırasında hata: " + ex.Message;
            }

            return RedirectToAction(nameof(Flights));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFlight(int id)
        {
            try
            {
                // Aktif rezervasyon var mı?
                var activeCount = SqliteDbHelper.ExecuteScalar<int>(
                    @"SELECT COUNT(*) 
                      FROM reservations 
                      WHERE flight_id=@id 
                        AND (status IS NULL OR UPPER(status) <> 'CANCELLED')",
                    cmd => cmd.Parameters.AddWithValue("@id", id));

                if (activeCount > 0)
                {
                    TempData["Error"] = "Bu uçuş için aktif rezervasyonlar mevcut: silmeden önce bu rezervasyonları iptal edin.";
                    return RedirectToAction(nameof(Flights));
                }

                // Bu uçuşa bağlı bütün rezervasyon satırlarını sil
                SqliteDbHelper.ExecuteNonQuery(
                    "DELETE FROM reservations WHERE flight_id=@id",
                    cmd => cmd.Parameters.AddWithValue("@id", id));

                // Uçuşu sil
                FlightModel.Delete(id);

                TempData["Success"] = "Uçuş ve ilişkili rezervasyon kayıtları silindi.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Silme sırasında hata: " + ex.Message;
            }
            return RedirectToAction(nameof(Flights));
        }

        // --- SEATS ---
        public IActionResult Seats(int? planeId)
        {
            var sql = @"
                SELECT s.id,
                       s.seat_number,
                       s.class,
                       p.name AS plane_name,
                       s.plane_id
                FROM seats s
                JOIN planes p ON s.plane_id = p.id
            ";

            if (planeId.HasValue)
            {
                var seats = SqliteDbHelper.ExecuteQuery(
                    sql + " WHERE s.plane_id = @pid",
                    cmd => cmd.Parameters.AddWithValue("@pid", planeId.Value)
                );

                ViewBag.FilterPlaneId = planeId.Value;
                var planeNameRows = SqliteDbHelper.ExecuteQuery(
                    "SELECT name FROM planes WHERE id=@id",
                    cmd => cmd.Parameters.AddWithValue("@id", planeId.Value)
                );
                if (planeNameRows.Count > 0)
                    ViewBag.FilterPlaneName = planeNameRows[0]["name"];

                return View(seats);
            }

            var seatsAll = SqliteDbHelper.ExecuteQuery(sql, null);
            return View(seatsAll);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddSeat(int planeId, string seatNumber, int seatClass = 0)
        {
            try
            {
                if (planeId <= 0 || string.IsNullOrWhiteSpace(seatNumber))
                {
                    TempData["Error"] = "Eksik veri.";
                    return RedirectToAction(nameof(Seats), new { planeId });
                }

                // Uçak var mı?
                var plane = SqliteDbHelper.ExecuteQuery(
                    "SELECT id FROM planes WHERE id=@id",
                    cmd => cmd.Parameters.AddWithValue("@id", planeId)
                );
                if (plane.Count == 0)
                {
                    TempData["Error"] = "Uçak bulunamadı.";
                    return RedirectToAction(nameof(Seats));
                }

                // Aynı koltuk daha önce tanımlanmış mı?
                var exists = SqliteDbHelper.ExecuteScalar<int>(
                    "SELECT COUNT(*) FROM seats WHERE plane_id=@pid AND seat_number=@sn",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@pid", planeId);
                        cmd.Parameters.AddWithValue("@sn", seatNumber.Trim());
                    });
                if (exists > 0)
                {
                    TempData["Error"] = "Bu koltuk zaten var.";
                    return RedirectToAction(nameof(Seats), new { planeId });
                }

                SqliteDbHelper.ExecuteNonQuery(
                    "INSERT INTO seats (plane_id, seat_number, class) VALUES (@pid, @sn, @c)",
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@pid", planeId);
                        cmd.Parameters.AddWithValue("@sn", seatNumber.Trim());
                        cmd.Parameters.AddWithValue("@c", seatClass);
                    });
                TempData["Success"] = "Koltuk eklendi.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Koltuk eklenirken hata: " + ex.Message;
            }
            return RedirectToAction(nameof(Seats), new { planeId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSeat(int id, int? planeId)
        {
            try
            {
                // Aktif rezervasyon var mı?
                var activeCount = SqliteDbHelper.ExecuteScalar<int>(
                    @"SELECT COUNT(*) 
                      FROM reservations 
                      WHERE seat_id=@id 
                        AND (status IS NULL OR UPPER(status) <> 'CANCELLED')",
                    cmd => cmd.Parameters.AddWithValue("@id", id));

                if (activeCount > 0)
                {
                    TempData["Error"] = "Bu koltuk için aktif rezervasyonlar mevcut: silmeden önce bu rezervasyonları iptal edin.";
                    return RedirectToAction(nameof(Seats), new { planeId });
                }

                // Bu koltuğa bağlı bütün rezervasyon satırlarını sil
                SqliteDbHelper.ExecuteNonQuery(
                    "DELETE FROM reservations WHERE seat_id=@id",
                    cmd => cmd.Parameters.AddWithValue("@id", id));

                // Koltuğu sil
                SqliteDbHelper.ExecuteNonQuery(
                    "DELETE FROM seats WHERE id=@id",
                    cmd => cmd.Parameters.AddWithValue("@id", id));

                TempData["Success"] = "Koltuk ve ilişkili rezervasyon kayıtları silindi.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "Koltuk silinirken hata: " + ex.Message;
            }
            return RedirectToAction(nameof(Seats), new { planeId });
        }

        // --- RESERVATIONS ---
        public IActionResult Reservations()
        {
            var reservations = SqliteDbHelper.ExecuteQuery(@"
                SELECT r.id,
                       u.username,
                       f.id AS flight_id,
                       r.price,
                       r.status,
                       s.seat_number
                FROM reservations r
                JOIN users   u ON r.user_id  = u.id
                JOIN flights f ON r.flight_id = f.id
                JOIN seats   s ON r.seat_id  = s.id
            ", null);
            return View(reservations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelReservation(int id)
        {
            try
            {
                const string sql = @"
                    UPDATE reservations
                    SET status = @st
                    WHERE id = @id AND (status IS NULL OR status <> @st);
                ";

                var affected = SqliteDbHelper.ExecuteNonQuery(
                    sql,
                    cmd =>
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.Parameters.AddWithValue("@st", "CANCELLED");
                    });

                if (affected > 0)
                    TempData["Success"] = "Rezervasyon iptal edildi.";
                else
                    TempData["Info"] = "Kayıt bulunamadı veya zaten iptal.";
            }
            catch (System.Exception ex)
            {
                TempData["Error"] = "İptal sırasında hata: " + ex.Message;
            }

            return RedirectToAction(nameof(Reservations));
        }
    }
}