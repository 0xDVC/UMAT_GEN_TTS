namespace UMAT_GEN_TTS.Core.Helpers;

public static class TimeHelper
{
    public static long ToTicks(TimeSpan time) => time.Ticks;

    // Common time conversions
    public static class Times
    {
        // Start times
        public const long SEVEN_AM = 252000000000;   // 7:00 AM
        public const long EIGHT_AM = 288000000000;   // 8:00 AM
        public const long NINE_AM = 324000000000;    // 9:00 AM
        
        // Break times
        public const long ONE_PM = 468000000000;     // 1:00 PM
        public const long THIRTY_MINUTES = 18000000000;  // 30 min break
        
        // End times
        public const long FIVE_PM = 612000000000;    // 5:00 PM
        
        // Session durations
        public const long ONE_HOUR = 36000000000;    // 1 hour
        public const long TWO_HOURS = 72000000000;   // 2 hours
        public const long THREE_HOURS = 108000000000; // 3 hours
    }

    public static TimeSpan FromTicks(long ticks) => TimeSpan.FromTicks(ticks);
} 