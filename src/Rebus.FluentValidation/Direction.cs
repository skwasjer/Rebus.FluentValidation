using System;

namespace Rebus.FluentValidation
{
	/// <summary>
	/// The validation direction.
	/// </summary>
	[Flags]
	public enum Direction
	{
		/// <summary>
		/// Incoming messages.
		/// </summary>
		Incoming = 0x1,
		/// <summary>
		/// Outgoing messages.
		/// </summary>
		Outgoing = 0x2,
		/// <summary>
		/// Both incoming and outgoing messages.
		/// </summary>
		Both = Incoming | Outgoing
	}
}
