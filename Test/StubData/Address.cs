namespace Test.StubData
{
	public class Address
	{

		public int Id { get; set; }	

		public string? AddressLine1 { get; set; }

		public string AddressLine2 { get; set; } = string.Empty;

		public string? City { get; set; }

		public string? Region { get; set; }

		public string? PostalCode { get; set; }

		public string? Country { get; set; }

	}
}