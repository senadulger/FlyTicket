namespace prgmlab3.Models;
using prgmlab3.data;

class CustomerModel : UserModel
{

    public CustomerModel(string mail,string pass) {
        var a = SqliteDbHelper.ExecuteQuery("Select * FROM USERS WHERE mail = @mail AND WHERE password = @pass ",
        cmd =>
        {
            cmd.Parameters.AddWithValue("@mail", mail);
            cmd.Parameters.AddWithValue("@pass", pass);
        });
        if (a.Count == 0)
        {
        // yoksa buraya bişi bulmak lazım nasıl engellicez
        }
        else
        {
            this._id = (int)a[0]["id"];
            this._username = (string)a[0]["username"];
            this._mail = (string)a[0]["mail"];
        }
    }

}