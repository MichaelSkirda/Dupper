namespace DupperDemo.Models
{
	public class User
	{
		public int Id { get; set; }
		public string? Name { get; set; }
		public ICollection<Comment> Comments { get; set; } = new List<Comment>();
	}
}
