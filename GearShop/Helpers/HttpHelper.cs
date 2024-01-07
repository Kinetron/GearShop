namespace GearShop.Helpers
{
	public static class HttpHelper
	{
		/// <summary>
		/// Get remove ip if use Kestrel + Apache.
		/// </summary>
		/// <param name="context"></param>
		/// <param name="accessor"></param>
		/// <returns></returns>
		public static string GetRemoteIp(this HttpContext context)
		{
			string remoteIpAddress = string.Empty;
			if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
				remoteIpAddress = context.Request.Headers["X-Forwarded-For"];

			return remoteIpAddress;
		}
	}
}
