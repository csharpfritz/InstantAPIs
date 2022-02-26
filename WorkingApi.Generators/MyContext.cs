using Microsoft.EntityFrameworkCore;

namespace WorkingApi;

public sealed class MyContext : DbContext 
{
	public MyContext() { }

	public MyContext(DbContextOptions<MyContext> options) : base(options) {}

	public DbSet<Contact> Contacts => Set<Contact>();

	public DbSet<Order> Orders => Set<Order>();
}

public sealed class Contact
{
	public int Id { get; set; }
	public string? Name { get; set; }
	public string? Email { get; set; }
}

public sealed class Order
{
	public int Id { get; set; }
	public string? Name { get; set; }
}
