using System;

namespace prgmlab3.Models
{
    // Sistemdeki yönetici (admin) kullanıcıları temsil eden model.
    public class AdminModel : UserModel
    {
        public AdminModel(int id, string username, string mail)
        {
            _id = id;
            _username = username;
            _mail = mail;
            _role = 1; 
        }

        // Sisteme yeni uçak ekler.
        public void AddPlane(string name, int seatCount)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Uçak adı boş olamaz.", nameof(name));

            if (seatCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(seatCount), "Koltuk sayısı 0'dan büyük olmalıdır.");

            Execute("INSERT INTO planes (name, seat_count) VALUES (@n, @s)", cmd =>
            {
                cmd.Parameters.AddWithValue("@n", name.Trim());
                cmd.Parameters.AddWithValue("@s", seatCount);
            });
        }

        // Var olan bir uçağın bilgilerini günceller.
        public void UpdatePlane(int planeId, string name, int seatCount)
        {
            if (planeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(planeId), "Geçersiz uçak ID.");

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Uçak adı boş olamaz.", nameof(name));

            if (seatCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(seatCount), "Koltuk sayısı 0'dan büyük olmalıdır.");

            Execute("UPDATE planes SET name=@n, seat_count=@s WHERE id=@id", cmd =>
            {
                cmd.Parameters.AddWithValue("@id", planeId);
                cmd.Parameters.AddWithValue("@n", name.Trim());
                cmd.Parameters.AddWithValue("@s", seatCount);
            });
        }

        // Sistemdeki bir uçağı siler.
        public void RemovePlane(int planeId)
        {
            if (planeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(planeId), "Geçersiz uçak ID.");

            Execute("DELETE FROM planes WHERE id=@id", cmd =>
            {
                cmd.Parameters.AddWithValue("@id", planeId);
            });
        }

        // Yeni uçuş ekler.
        public void AddFlight(
            int planeId,
            int dep,
            int arr,
            DateTime depTime,
            DateTime arrTime,
            decimal basePrice)
        {
            if (planeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(planeId), "Geçersiz uçak ID.");

            if (dep <= 0 || arr <= 0)
                throw new ArgumentOutOfRangeException(nameof(dep), "Kalkış/varış lokasyonu geçersiz.");

            if (depTime >= arrTime)
                throw new ArgumentException("Kalkış saati varış saatinden sonra olamaz.");

            if (basePrice <= 0)
                throw new ArgumentOutOfRangeException(nameof(basePrice), "Fiyat 0'dan büyük olmalıdır.");

            Execute(@"
                INSERT INTO flights 
                (plane_id, departure_location, arrival_location, departure_time, arrival_time, price)
                VALUES (@pid, @dep, @arr, @dtime, @atime, @price)", cmd =>
            {
                cmd.Parameters.AddWithValue("@pid", planeId);
                cmd.Parameters.AddWithValue("@dep", dep);
                cmd.Parameters.AddWithValue("@arr", arr);
                cmd.Parameters.AddWithValue("@dtime", depTime);
                cmd.Parameters.AddWithValue("@atime", arrTime);
                cmd.Parameters.AddWithValue("@price", basePrice); 
            });
        }

        // Var olan bir uçuşu günceller.
        public void UpdateFlight(
            int flightId,
            int planeId,
            int dep,
            int arr,
            DateTime depTime,
            DateTime arrTime,
            decimal basePrice)
        {
            if (flightId <= 0)
                throw new ArgumentOutOfRangeException(nameof(flightId), "Geçersiz uçuş ID.");
            if (planeId <= 0)
                throw new ArgumentOutOfRangeException(nameof(planeId), "Geçersiz uçak ID.");
            if (dep <= 0 || arr <= 0)
                throw new ArgumentOutOfRangeException(nameof(dep), "Kalkış/varış lokasyonu geçersiz.");
            if (depTime >= arrTime)
                throw new ArgumentException("Kalkış saati varış saatinden sonra olamaz.");
            if (basePrice <= 0)
                throw new ArgumentOutOfRangeException(nameof(basePrice), "Fiyat 0'dan büyük olmalıdır.");

            Execute(@"
                UPDATE flights SET
                    plane_id = @pid,
                    departure_location = @dep,
                    arrival_location = @arr,
                    departure_time = @dtime,
                    arrival_time = @atime,
                    price = @price
                WHERE id = @fid", cmd =>
            {
                cmd.Parameters.AddWithValue("@fid", flightId);
                cmd.Parameters.AddWithValue("@pid", planeId);
                cmd.Parameters.AddWithValue("@dep", dep);
                cmd.Parameters.AddWithValue("@arr", arr);
                cmd.Parameters.AddWithValue("@dtime", depTime);
                cmd.Parameters.AddWithValue("@atime", arrTime);
                cmd.Parameters.AddWithValue("@price", basePrice);
            });
        }

        // Uçuşu siler.
        public void RemoveFlight(int flightId)
        {
            if (flightId <= 0)
                throw new ArgumentOutOfRangeException(nameof(flightId), "Geçersiz uçuş ID.");

            Execute("DELETE FROM flights WHERE id=@id", cmd =>
            {
                cmd.Parameters.AddWithValue("@id", flightId);
            });
        }

        // Admin tarafından bir rezervasyonu iptal eder.
        public void CancelReservation(int reservationId)
        {
            if (reservationId <= 0)
                throw new ArgumentOutOfRangeException(nameof(reservationId), "Geçersiz rezervasyon ID.");
                
            Execute("UPDATE reservations SET status = 'CANCELLED' WHERE id=@id", cmd =>
            {
                cmd.Parameters.AddWithValue("@id", reservationId);
            });
        }


    }
}