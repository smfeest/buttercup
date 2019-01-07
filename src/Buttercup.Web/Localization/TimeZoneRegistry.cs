using System;
using System.Collections.Generic;

namespace Buttercup.Web.Localization
{
    public class TimeZoneRegistry : ITimeZoneRegistry
    {
        public IList<TimeZoneInfo> GetSupportedTimeZones() => TimeZoneInfo.GetSystemTimeZones();

        public TimeZoneInfo GetTimeZone(string id) => TimeZoneInfo.FindSystemTimeZoneById(id);
    }
}
