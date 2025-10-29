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
                return new AdminModel((int)row["id"], (string)row["username"], (string)row["mail"]);
            else
                return new CustomerModel((int)row["id"], (string)row["username"], (string)row["mail"]);
        }
    }
}
