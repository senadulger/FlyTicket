using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace prgmlab3.data
{
    public static class SqliteDbHelper
    {
        private static string _connectionString = "Data Source=prgmlab3.db";

        public static void SetConnectionString(string conn) => _connectionString = conn;


        public static void Initialize()
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
    CREATE TABLE IF NOT EXISTS users (
        id INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
        username VARCHAR NOT NULL,
        password VARCHAR NOT NULL,
        role SMALLINT NOT NULL,
        mail VARCHAR,
        tc_no VARCHAR,
        first_name VARCHAR,
        last_name VARCHAR,
        birth_date DATETIME,
        phone VARCHAR
    );

    CREATE TABLE IF NOT EXISTS planes (
        id INTEGER PRIMARY KEY NOT NULL,
        name VARCHAR,
        seat_count INTEGER
    );

    CREATE TABLE IF NOT EXISTS airports (
        id INTEGER PRIMARY KEY NOT NULL,
        code VARCHAR,
        city VARCHAR,
        name VARCHAR,
        country VARCHAR
    );

    CREATE TABLE IF NOT EXISTS flights (
        id INTEGER PRIMARY KEY NOT NULL,
        plane_id INTEGER NOT NULL,
        departure_time DATETIME,
        arrival_time DATETIME,
        price FLOAT,
        departure_location INTEGER,
        arrival_location INTEGER,
        FOREIGN KEY(plane_id) REFERENCES planes(id),
        FOREIGN KEY(departure_location) REFERENCES airports(id),
        FOREIGN KEY(arrival_location) REFERENCES airports(id)
    );

    CREATE TABLE IF NOT EXISTS seats (
        id INTEGER PRIMARY KEY NOT NULL,
        plane_id INTEGER,
        seat_number VARCHAR,
        class SMALLINT,
        FOREIGN KEY(plane_id) REFERENCES planes(id)
    );

    CREATE TABLE IF NOT EXISTS reservations (
        id INTEGER PRIMARY KEY NOT NULL,
        user_id INTEGER,
        flight_id INTEGER,
        price FLOAT,
        seat_id INTEGER,
        status VARCHAR,
        FOREIGN KEY(user_id) REFERENCES users(id),
        FOREIGN KEY(flight_id) REFERENCES flights(id),
        FOREIGN KEY(seat_id) REFERENCES seats(id)
    );
    ";
            cmd.ExecuteNonQuery();
            SeedAdmin();
        }

        public static SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();
            return conn;
        }

        public static int Execute(string sql, Action<SqliteCommand> addParams = null)
        {
            using var conn = GetConnection();
            using var tx = conn.BeginTransaction();
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = sql;
            addParams?.Invoke(cmd);
            var affected = cmd.ExecuteNonQuery();
            tx.Commit();
            return affected;
        }

        public static int ExecuteNonQuery(string sql, Action<SqliteCommand> addParams = null)
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            addParams?.Invoke(cmd);
            return cmd.ExecuteNonQuery();
        }

        public static T ExecuteScalar<T>(string sql, Action<SqliteCommand> addParams = null)
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            addParams?.Invoke(cmd);
            var res = cmd.ExecuteScalar();
            if (res == null || res == DBNull.Value) return default;
            return (T)Convert.ChangeType(res, typeof(T));
        }

        public static List<Dictionary<string, object>> ExecuteQuery(string sql, Action<SqliteCommand> addParams = null)
        {
            var rows = new List<Dictionary<string, object>>();
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            addParams?.Invoke(cmd);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }
            return rows;
        }

        public static void AddParameter(SqliteCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }

        public static void SeedAdmin()
        {
            var count = ExecuteScalar<long>("SELECT COUNT(*) FROM users WHERE username = @un", cmd =>
            {
                cmd.Parameters.AddWithValue("@un", "Admin Admin");
            });

            if (count == 0)
            {
                Execute(@"
                    INSERT INTO users (username, password, role, mail, first_name, last_name, tc_no, phone, birth_date)
                    VALUES (@un, @pass, @role, @mail, @fn, @ln, @tc, @ph, @bd)", cmd =>
                {
                    cmd.Parameters.AddWithValue("@un", "Admin Admin");
                    cmd.Parameters.AddWithValue("@pass", "admin");
                    cmd.Parameters.AddWithValue("@role", 1); 
                    cmd.Parameters.AddWithValue("@mail", "admin@admin.com");
                    cmd.Parameters.AddWithValue("@fn", "Admin");
                    cmd.Parameters.AddWithValue("@ln", "Admin");
                    cmd.Parameters.AddWithValue("@tc", "11111111111");
                    cmd.Parameters.AddWithValue("@ph", "5555555555");
                    cmd.Parameters.AddWithValue("@bd", DateTime.Now.ToString("yyyy-MM-dd"));
                });
            }
        }
    }
}