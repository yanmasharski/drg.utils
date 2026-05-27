using System;

namespace DRG.Utils
{
	public static class DateTimeOffsetExtensions
	{
		private static readonly DateTimeOffset UnixEpoch = new(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);

		public static long ToUnixTimeMilliseconds(this DateTimeOffset value) =>
			(long)(value.ToUniversalTime() - UnixEpoch).TotalMilliseconds;

		public static long ToUnixTimeSeconds(this DateTimeOffset value) =>
			(long)(value.ToUniversalTime() - UnixEpoch).TotalSeconds;
	}
}
