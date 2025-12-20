using System;
using System.Collections.Generic;
using prgmlab3.data;

namespace prgmlab3.Models
{
    public class CouponModel : BaseModel
    {
        public int Id { get; set; }
        public string Code { get; set; } = "";
        public int DiscountPercent { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public static void EnsureTableExists()
        {
            SqliteDbHelper.ExecuteNonQuery(@"
                CREATE TABLE IF NOT EXISTS coupons (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    code TEXT UNIQUE NOT NULL,
                    discount_percent INTEGER NOT NULL,
                    expiration_date TEXT
                );
            ");

        // Kupon oluşturma
            var count = SqliteDbHelper.ExecuteScalar<long>("SELECT COUNT(*) FROM coupons;", null);
            if (count == 0)
            {
                SqliteDbHelper.ExecuteNonQuery(@"
                    INSERT INTO coupons (code, discount_percent, expiration_date) 
                    VALUES ('YENIYIL20', 20, '2025-12-31');
                ");

                SqliteDbHelper.ExecuteNonQuery(@"
                    INSERT INTO coupons (code, discount_percent, expiration_date) 
                    VALUES ('INDIRIM10', 10, '2026-06-01');
                ");
            }
        }

        public static CouponModel? GetByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            var rows = SqliteDbHelper.ExecuteQuery(
                "SELECT * FROM coupons WHERE code=@code COLLATE NOCASE", // Case insensitive match
                cmd => cmd.Parameters.AddWithValue("@code", code.Trim())
            );

            if (rows.Count == 0) return null;

            var r = rows[0];
            return new CouponModel
            {
                Id = Convert.ToInt32(r["id"]),
                Code = Convert.ToString(r["code"]) ?? "",
                DiscountPercent = Convert.ToInt32(r["discount_percent"]),
                ExpirationDate = DateTime.TryParse(Convert.ToString(r["expiration_date"]), out var dt) ? dt : null
            };
        }

        public bool IsValid()
        {
            if (ExpirationDate.HasValue && ExpirationDate.Value < DateTime.Now)
                return false;
            return true;
        }
    }
}
