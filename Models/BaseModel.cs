using prgmlab3.data;
using System.Collections.Generic;

namespace prgmlab3.Models
{
    // Tüm modeller için ortak veritabanı işlemlerini sağlayan soyut temel sınıf.
    public abstract class BaseModel
    {
        public static List<Dictionary<string, object>> Query(string sql, Action<Microsoft.Data.Sqlite.SqliteCommand> binder)
        {
            return SqliteDbHelper.ExecuteQuery(sql, binder);
        }

        protected static void Execute(string sql, Action<Microsoft.Data.Sqlite.SqliteCommand> binder)
        {
            SqliteDbHelper.ExecuteNonQuery(sql, binder);
        }
    }
}