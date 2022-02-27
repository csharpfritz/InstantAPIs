using Microsoft.EntityFrameworkCore;

namespace WorkingApi;

public class MyContext : DbContext 
{
	public MyContext(DbContextOptions<MyContext> options) : base(options) {}

	public DbSet<Contact> Contacts => Set<Contact>();

}

public class Contact
{

	public int Id { get; set; }
	public string? Name { get; set; }
	public string? Email { get; set; }

}
