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

		/// <summary>
		/// Check ip if exist in current subnet. Instead of subnet use allowed ip. 
		/// </summary>
		/// <param name="ip"></param>
		/// <param name="subnet"></param>
		/// <returns></returns>
		public static bool IpInSubNetOrEqual(string ip, string subnet)
		{
			//is subnet
			if (subnet.Contains('/'))
			{
				string[] strArr = subnet.Split('/');
				string[] ipPartSubnet = strArr[0].Split(".");
				string[] ipPart = ip.Split(".");

				if (ipPartSubnet[0] == "0") return true; //Allow any.

				for (int i = 0; i < 3; i++)
				{
					if (ipPart[i] != ipPartSubnet[i])
					{
						return false;
					}
				}

				return true;
			}
			else
			{
				return ip == subnet;
			}
		}
	}
}
