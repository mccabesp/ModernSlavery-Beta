namespace ModernSlavery.WebUI.Admin.Classes
{
    public static class ReportingPeriodHelper
    {
        public static string FormatReportingPeriod(int reportingPeriodStartYear)
        {
            var fourDigitStartYear = reportingPeriodStartYear;

            var fourDigitEndYear = reportingPeriodStartYear + 1;
            var twoDigitEndYear = fourDigitEndYear % 100;

            var formattedYear = $"{fourDigitStartYear}-{twoDigitEndYear}";
            return formattedYear;
        }
    }
}