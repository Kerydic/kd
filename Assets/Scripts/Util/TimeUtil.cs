using System;

namespace KDGame.Util
{
	public class TimeUtil
	{
		private static DateTime UtcZero => TimeZoneInfo.ConvertTimeToUtc(new DateTime(1970, 1, 1));

		public static long NowTimeStamp()
		{
			return DateToTimeStamp(DateTime.Now);
		}

		public static long DateToTimeStamp(DateTime date)
		{
			return (long) (date - UtcZero).TotalSeconds;
		}

		public static DateTime TimeStampToDate(long timeStamp)
		{
			var hour = (int) timeStamp / 3600;
			var minute = (int) timeStamp % 3600 / 60;
			var second = (int) timeStamp % 60;
			return UtcZero.Add(new TimeSpan(hour, minute, second));
		}
	}
}