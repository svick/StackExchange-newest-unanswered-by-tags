using System.Collections.Generic;
using System.Linq;
using Stacky;

namespace Newest_unaswered_by_tags
{
	static class StackySite
	{
		static Site[] sites = null;

		public static Site[] GetSites()
		{
			if (sites == null)
				sites = typeof(Sites).GetProperties().Select(prop => (Site)prop.GetGetMethod().Invoke(null, null)).ToArray();
			return sites;
		}

		public static Site GetSite(string siteName)
		{
			return GetSites().SingleOrDefault(site => site.Name == siteName);
		}
	}
}