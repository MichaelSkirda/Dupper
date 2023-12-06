using Dapper;
using Dupper;
using DupperDemo.Models;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace DupperDemo.Controllers
{
	public class UsersController : Controller
	{
		private IDbProvider Db { get; set; }
		private IDbConnectionProvider DbConnectionProvider { get; set; }

		public UsersController(IDbProvider db, IDbConnectionProvider dbProvider)
		{
			Db = db;
			DbConnectionProvider = dbProvider;
		}

		[HttpGet("users/dupper")]
		public async Task<IActionResult> ListDupper()
		{
			string sql = "SELECT u.id, u.name, c.id AS comment_id, c.text FROM users u JOIN comments c ON c.user_id = u.id";

			int users = await Db.QueryFirstOrDefaultAsync<int>(sql);

			return Json(users);
		}

		[HttpGet("users/dapper")]
		public async Task<IActionResult> ListDapper()
		{
			string sql = "SELECT u.id, u.name, c.id AS comment_id, c.text FROM users u JOIN comments c ON c.user_id = u.id";

			using IDbConnection connection = DbConnectionProvider.Connect();
			var rows = new Dictionary<int, User>();

			await connection.QueryAsync<User, Comment, User>(sql,
			(userRow, commentRow) =>
			{
				int key = userRow.Id;
				User? user = default!;
				rows.TryGetValue(key, out user);

				if (user == null)
				{
					user = userRow;
					rows[key] = user;
				}
				user.Comments.Add(commentRow);

				return user;
			}, splitOn: "text");

			IEnumerable<User> users = rows
				.Select(x => new User()
				{ 
					Id = x.Value.Id,
					Name = x.Value.Name,
					Comments = x.Value.Comments
				});

			return Json(users);
		}
	}
}
