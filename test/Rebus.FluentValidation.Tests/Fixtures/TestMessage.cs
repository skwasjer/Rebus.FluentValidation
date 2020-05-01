namespace Rebus.FluentValidation.Fixtures
{
	public class TestMessage
	{
		public bool ShouldPassValidation { get; set; }
		public bool IsValidated { get; set; }
	}

	public class TestMessage1 : TestMessage { }
	public class TestMessage2 : TestMessage { }
}
