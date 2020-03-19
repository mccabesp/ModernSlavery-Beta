using System;

namespace ModernSlavery.Extensions
{
    public static class VirtualDateTime
    {
        public static TimeSpan Offset { get; private set; } = TimeSpan.Zero;

        public static DateTime Now => DateTime.Now.Add(Offset);

        public static DateTime UtcNow => DateTime.UtcNow.Add(Offset);

        public static void Initialise(TimeSpan initialisationTimeSpan = default)
        {
            Offset = initialisationTimeSpan;
        }

        public static void Initialise(string dateTimeOffset)
        {
            Offset = string.IsNullOrWhiteSpace(dateTimeOffset) ? TimeSpan.Zero : TimeSpan.Parse(dateTimeOffset);
        }
    }
}