using InstantAPIs.Repositories;
using InstantAPIs;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Linq.Expressions;

namespace Test.Configuration;

public abstract class InstantAPIsConfigBuilderFixture : BaseFixture
{
	internal InstantAPIsBuilder<MyContext> _Builder;

	public InstantAPIsConfigBuilderFixture()
	{
		var contextMock = new Mock<IContextHelper<MyContext>>();
		contextMock.Setup(x => x.NameTable(It.Is<Expression<Func<MyContext, DbSet<Contact>>>>(a => ((MemberExpression)a.Body).Member.Name == "Contacts"))).Returns("Contacts");
		contextMock.Setup(x => x.NameTable(It.Is<Expression<Func<MyContext, DbSet<Address>>>>(a => ((MemberExpression)a.Body).Member.Name == "Addresses"))).Returns("Addresses");
		contextMock.Setup(x => x.DiscoverFromContext(It.IsAny<Uri>()))
			.Returns(new InstantAPIsOptions.ITable[] {
				new InstantAPIsOptions.Table<MyContext, DbSet<Contact>, Contact, int>("Contacts", new Uri("Contacts", UriKind.Relative), c => c.Contacts , new InstantAPIsOptions.TableOptions<Contact, int>() { KeySelector = x => x.Id }),
				new InstantAPIsOptions.Table<MyContext, DbSet<Address>, Address, int>("Addresses", new Uri("Addresses", UriKind.Relative), c => c.Addresses, new InstantAPIsOptions.TableOptions<Address, int>() { KeySelector = x => x.Id })
			});
		_Builder = new(new InstantAPIsOptions(), contextMock.Object);
	}
}
