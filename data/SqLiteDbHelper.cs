// SqliteDbHelper.cs
using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace prgmlab3.data
{
    public static class SqliteDbHelper
    {
        private static string _connectionString = "Data Source=prgmlab3.db";

        public static void SetConnectionString(string conn) => _connectionString = conn;

        public static SqliteConnection GetConnection()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys = ON;";
            cmd.ExecuteNonQuery();
            return conn;
        }

        // Execute non-query (INSERT/UPDATE/DELETE)
        public static int ExecuteNonQuery(string sql, Action<SqliteCommand> addParams = null)
        {
            using var conn = GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            addParams?.Invoke(cmd);
            return cmd.ExecuteNonQuery();
        }

        // Execute scalar (last_insert_rowid, COUNT, etc.)
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

        // Execute query (SELECT)
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

        // Parametre ekleme helper
        public static void AddParameter(SqliteCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }

    }
}
