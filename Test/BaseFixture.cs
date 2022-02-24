using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;

namespace Test;

public abstract class BaseFixture
{

	protected MockRepository Mockery { get; private set; } = new MockRepository(MockBehavior.Loose);

}