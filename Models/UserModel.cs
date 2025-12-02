using System;

namespace prgmlab3.Models
{
    // Sistemdeki kullanıcıların temel bilgilerini tutan soyut model.
    public abstract class UserModel : BaseModel
    {
        public const int RoleCustomer = 0;
        public const int RoleAdmin = 1;

        protected int _id;
        protected string _username = "";
        protected string _mail = "";
        protected string _password = "";
        protected int _role;
        protected string _tcNo = "";
        protected string _firstName = "";
        protected string _lastName = "";
        protected DateTime _birthDate = DateTime.MinValue;
        protected string _phone = "";

        public int Id => _id;
        public string Username => _username;
        public string Mail => _mail;
        public int Role => _role;
        public string TcNo => _tcNo;
        public string FirstName => _firstName;
        public string LastName => _lastName;
        public DateTime BirthDate => _birthDate;
        public string Phone => _phone;

        private static bool HasColumn(System.Collections.Generic.IDictionary<string, object> row, string key)
            => row.ContainsKey(key) && row[key] != null && row[key] != DBNull.Value;


        // DB satırını uygun UserModel nesnesine çevirir.
        private static UserModel MapRowToUser(System.Collections.Generic.Dictionary<string, object> row)
        {
            int id = HasColumn(row, "id") ? Convert.ToInt32(row["id"]) : 0;
            int role = HasColumn(row, "role") ? Convert.ToInt32(row["role"]) : RoleCustomer;

            string mail = HasColumn(row, "mail") ? Convert.ToString(row["mail"]) ?? "" : "";
            string username = HasColumn(row, "username") ? Convert.ToString(row["username"]) ?? "" : "";

            string firstName = HasColumn(row, "first_name") ? Convert.ToString(row["first_name"]) ?? "" : "";
            string lastName = HasColumn(row, "last_name") ? Convert.ToString(row["last_name"]) ?? "" : "";
            string tcNo = HasColumn(row, "tc_no") ? Convert.ToString(row["tc_no"]) ?? "" : "";
            string phone = HasColumn(row, "phone") ? Convert.ToString(row["phone"]) ?? "" : "";

            DateTime birthDate = DateTime.MinValue;
            if (HasColumn(row, "birth_date"))
            {
                DateTime.TryParse(Convert.ToString(row["birth_date"]), out birthDate);
            }

            string dbPass = HasColumn(row, "password") ? Convert.ToString(row["password"]) ?? "" : "";


            if (string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName)))
            {
                username = $"{firstName} {lastName}".Trim();
            }

            UserModel user = role == RoleAdmin
                ? new AdminModel(id, username, mail)
                : new CustomerModel(id, username, mail);

            user._id = id;
            user._role = role;
            user._mail = mail;
            user._username = username;
            user._password = dbPass;

            user._tcNo = tcNo;
            user._firstName = firstName;
            user._lastName = lastName;
            user._birthDate = birthDate;
            user._phone = phone;

            return user;
        }

        // ----- LOGIN -----
        public static UserModel? Login(string mail, string password)
        {
            if (string.IsNullOrWhiteSpace(mail) || string.IsNullOrWhiteSpace(password))
                return null;

            mail = mail.Trim();

            var res = Query(
                "SELECT * FROM users WHERE mail=@mail AND password=@pass",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@mail", mail);
                    cmd.Parameters.AddWithValue("@pass", password);
                });

            if (res.Count == 0)
                return null;

            var row = res[0];
            return MapRowToUser(row);
        }

        public static UserModel? GetById(int id)
        {
            var res = Query(
                "SELECT * FROM users WHERE id=@id",
                cmd => cmd.Parameters.AddWithValue("@id", id));

            if (res.Count == 0)
                return null;

            return MapRowToUser(res[0]);
        }

        // Yeni müşteri kaydı yapar.
        public static bool Register(
            string tcNo,
            string firstName,
            string lastName,
            DateTime birthDate,
            string mail,
            string phone,
            string password)
        {
            // Boşluk kontrolleri
            if (string.IsNullOrWhiteSpace(tcNo) ||
                string.IsNullOrWhiteSpace(firstName) ||
                string.IsNullOrWhiteSpace(lastName) ||
                string.IsNullOrWhiteSpace(mail) ||
                string.IsNullOrWhiteSpace(phone) ||
                string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            tcNo = tcNo.Trim();
            firstName = firstName.Trim();
            lastName = lastName.Trim();
            mail = mail.Trim();
            phone = phone.Trim();

            if (tcNo.Length != 11)
                return false;

            if (password.Length < 4)
                return false;

            // 18 yaş kontrolü
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;
            if (birthDate.Date > today.AddYears(-age)) age--;

            if (age < 18)
                return false;

            // Aynı mail veya TCKN kayıtlı mı?
            var existing = Query(
                "SELECT id FROM users WHERE mail=@mail OR tc_no=@tc",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@mail", mail);
                    cmd.Parameters.AddWithValue("@tc", tcNo);
                });

            if (existing.Count > 0)
                return false;

            // username: Ad + Soyad
            var username = $"{firstName} {lastName}".Trim();

            Execute(
                @"INSERT INTO users 
                  (tc_no, first_name, last_name, birth_date, phone, username, mail, password, role) 
                  VALUES (@tc, @fn, @ln, @bd, @ph, @un, @mail, @pass, @role)",
                cmd =>
                {
                    cmd.Parameters.AddWithValue("@tc", tcNo);
                    cmd.Parameters.AddWithValue("@fn", firstName);
                    cmd.Parameters.AddWithValue("@ln", lastName);
                    cmd.Parameters.AddWithValue("@bd", birthDate.ToString("yyyy-MM-dd"));
                    cmd.Parameters.AddWithValue("@ph", phone);
                    cmd.Parameters.AddWithValue("@un", username);
                    cmd.Parameters.AddWithValue("@mail", mail);
                    cmd.Parameters.AddWithValue("@pass", password);
                    cmd.Parameters.AddWithValue("@role", RoleCustomer); 
                });

            return true;
        }
    }
}
