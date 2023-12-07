using Dapper;
using Dupper;
using Npgsql;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Threading.Tasks;

namespace DupperDemo.Controllers
{
	public class UsersController
	{
		private IDbProvider Db { get; set; }

		public UsersController(IDbProvider db)
		{
			Db = db;
		}

		public async Task ListDupper()
		{
			string sql = "SELECT u.id, u.name, c.id AS comment_id, c.text FROM users u JOIN comments c ON c.user_id = u.id";

			IDbConnection x = new NpgsqlConnection();
			var y = x.QueryFirstOrDefaultAsync<int>("");
			StringBuilder users = await Db.QueryFirstOrDefaultAsync<StringBuilder>(sql);
		}

	}
}
