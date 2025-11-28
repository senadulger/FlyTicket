namespace prgmlab3.Models
{
    public abstract class UserModel : BaseModel
    {
        protected int _id;
        protected string _username = "";
        protected string _mail = "";
        protected string _password = "";
        protected int _role; // 0: Customer, 1: Admin

        public int Id => _id;
        public string Username => _username;
        public string Mail => _mail;
        public int Role => _role;

        public bool VerifyPassword(string pass) => pass == _password;

        public static UserModel? Login(string mail, string password)
        {
            var res = Query("SELECT * FROM users WHERE mail=@mail AND password=@pass", cmd =>
            {
                cmd.Parameters.AddWithValue("@mail", mail);
                cmd.Parameters.AddWithValue("@pass", password);
            });

            if (res.Count == 0) return null;

            var row = res[0];
            int role = Convert.ToInt32(row["role"]);

            if (role == 1)
                return new AdminModel(Convert.ToInt32(row["id"]), (string)row["username"], (string)row["mail"]);
            else
                return new CustomerModel(Convert.ToInt32(row["id"]), (string)row["username"], (string)row["mail"]);
        }

        public static bool Register(string username, string mail, string password)
        {
            // Check if mail already exists
            var existing = Query("SELECT id FROM users WHERE mail=@mail", cmd => cmd.Parameters.AddWithValue("@mail", mail));
            if (existing.Count > 0) return false;

            Execute("INSERT INTO users (username, mail, password, role) VALUES (@u, @m, @p, 0)", cmd =>
            {
                cmd.Parameters.AddWithValue("@u", username);
                cmd.Parameters.AddWithValue("@m", mail);
                cmd.Parameters.AddWithValue("@p", password);
            });
            return true;
        }
    }
}
