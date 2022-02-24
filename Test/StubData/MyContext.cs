using Microsoft.EntityFrameworkCore;

namespace Test.StubData;

internal class MyContext : DbContext
{

	public MyContext(DbContextOptions<MyContext> options) : base(options) { }

	public DbSet<Contact> Contacts => Set<Contact>();

	public DbSet<Address> Addresses => Set<Address>();

}
