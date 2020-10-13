namespace ModernSlavery.WebUI.Admin.Classes
{
    public static class ReportingPeriodHelper
    {
        public static string FormatReportingPeriod(int reportingDeadlineYear)
        {
            var fourDigitStartYear = reportingDeadlineYear - 1;

            var fourDigitEndYear = reportingDeadlineYear;
            var twoDigitEndYear = fourDigitEndYear % 100;

            var formattedYear = $"{fourDigitStartYear}-{twoDigitEndYear}";
            return formattedYear;
        }
    }
}