using System;
namespace SpaceXLaunch.Models
{
    public class LaunchesResponse
    {
        public List<Launches> launches { get; set; } = default!;
    }

    public class Launches
    {
        public string id { get; set; } = string.Empty;
        public string mission_name { get; set; } = string.Empty;
    }

    public class LaunchResponse {
        public Launch launch { get; set; } = default!;
    }

    public class Launch
    {
        public DateTime cached_time { get; set; } = default!;
        public string mission_name { get; set; } = default!;
        public DateTime launch_date_local { get; set; }
        public LaunchRocket rocket { get; set; } = default!;
    }

    public class LaunchRocket
    {
        public Rocket rocket { get; set; } = default!;
    }

    public class Rocket
    {
        public string name { get; set; } = default!;
        public DateTime first_flight { get; set; }
        public int success_rate_pct { get; set; }
    }
}

