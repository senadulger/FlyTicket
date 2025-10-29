using Microsoft.AspNetCore.Mvc;
using prgmlab3.data;
using prgmlab3.Models;
using System.Collections.Generic;

namespace prgmlab3.Controllers
{
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

        // --- AIRPORTS ---
        public IActionResult Airports()
        {
            var airports = SqliteDbHelper.ExecuteQuery("SELECT * FROM airports", null);
            return View(airports);
        }

        // --- FLIGHTS ---
        public IActionResult Flights()
        {
            var flights = SqliteDbHelper.ExecuteQuery(@"
                SELECT f.id, p.name AS plane_name, a1.city AS departure_city, a2.city AS arrival_city, 
                       f.departure_time, f.arrival_time, f.price
                FROM flights f
                JOIN planes p ON f.plane_id = p.id
                JOIN airports a1 ON f.departure_location = a1.id
                JOIN airports a2 ON f.arrival_location = a2.id
            ", null);
            return View(flights);
        }

        // --- SEATS ---
        public IActionResult Seats()
        {
            var seats = SqliteDbHelper.ExecuteQuery(@"
                SELECT s.id, s.seat_number, s.class, p.name AS plane_name
                FROM seats s
                JOIN planes p ON s.plane_id = p.id
            ", null);
            return View(seats);
        }

        // --- RESERVATIONS ---
        public IActionResult Reservations()
        {
            var reservations = SqliteDbHelper.ExecuteQuery(@"
                SELECT r.id, u.username, f.id AS flight_id, r.price, r.status, s.seat_number
                FROM reservations r
                JOIN users u ON r.user_id = u.id
                JOIN flights f ON r.flight_id = f.id
                JOIN seats s ON r.seat_id = s.id
            ", null);
            return View(reservations);
        }
    }
}
