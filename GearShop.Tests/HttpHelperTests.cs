using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GearShop.Helpers;

namespace GearShop.Tests
{
	[TestFixture]
	public class HttpHelperTests
	{
		[TestCase("10.10.10.1", "10.10.10.0/24", ExpectedResult = true)]
		[TestCase("10.10.10.4", "10.10.10.0/24", ExpectedResult = true)]
		[TestCase("10.10.15.4", "10.10.10.0/24", ExpectedResult = false)]
		[TestCase("192.10.15.4", "10.10.10.0/24", ExpectedResult = false)]
		[TestCase("192.10.15.5", "192.10.15.6", ExpectedResult = false)]
		[TestCase("192.10.15.5", "192.10.15.5", ExpectedResult = true)]
		[TestCase("192.10.15.5", "0.0.0.0/24", ExpectedResult = true)]
		public bool IpInSubNet(string ip, string subnet)
		{
			return HttpHelper.IpInSubNetOrEqual(ip, subnet);
		}
	}
}
