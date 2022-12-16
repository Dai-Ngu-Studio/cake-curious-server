namespace CakeCurious_API.Utilities
{
    public static class EnvironmentHelper
    {
        public static readonly string WebAppUri = "WEB_APP_URI";
        public static readonly string ShareUriPrefix = "SHARE_URI_PREFIX";
        public static readonly string AndroidPackageName = "ANDROID_PACKAGE_NAME";
        public static readonly string AndroidMinPackageVersionCode = "ANDROID_MIN_PACKAGE_VERSION_CODE";
        public static readonly string AndroidFallbackLink = "ANDROID_FALLBACK_LINK";
        public static readonly string SuffixOption = "SUFFIX_OPTION";
        public static readonly string ShareImageWidth = "SHARE_IMAGE_WIDTH";
        public static readonly string ShareImageHeight = "SHARE_IMAGE_HEIGHT";
        public static readonly string InputGMT = "INPUT_GMT";
        public static readonly string DailyScheduledTime = "DAILY_SCHEDULED_TIME";
        public static readonly string SmtpSenderMailAddress = "SMTP_SENDER_MAIL_ADDRESS";
        public static readonly string SmtpSenderName = "SMTP_SENDER_NAME";
        public static readonly string NewStaffMailSubject = "NEW_STAFF_MAIL_SUBJECT";
        public static readonly string NewStaffMailBody = "NEW_STAFF_MAIL_BODY";

        public static void AddAppInfoEnvironmentVariables(IConfigurationSection appInfo)
        {
            Environment.SetEnvironmentVariable(WebAppUri, appInfo.GetValue<string>("WebAppUri"));
            Environment.SetEnvironmentVariable(ShareUriPrefix, appInfo.GetValue<string>("ShareUriPrefix"));
            Environment.SetEnvironmentVariable(AndroidPackageName, appInfo.GetValue<string>("AndroidPackageName"));
            Environment.SetEnvironmentVariable(AndroidMinPackageVersionCode, appInfo.GetValue<string>("AndroidMinPackageVersionCode"));
            Environment.SetEnvironmentVariable(AndroidFallbackLink, appInfo.GetValue<string>("AndroidFallbackLink"));
            Environment.SetEnvironmentVariable(SuffixOption, appInfo.GetValue<string>("SuffixOption"));
            Environment.SetEnvironmentVariable(ShareImageWidth, appInfo.GetValue<string>("ShareImageWidth"));
            Environment.SetEnvironmentVariable(ShareImageHeight, appInfo.GetValue<string>("ShareImageHeight"));
            Environment.SetEnvironmentVariable(SmtpSenderMailAddress, appInfo.GetValue<string>("SmtpSenderMailAddress"));
            Environment.SetEnvironmentVariable(SmtpSenderName, appInfo.GetValue<string>("SmtpSenderName"));
            Environment.SetEnvironmentVariable(NewStaffMailSubject, appInfo.GetValue<string>("NewStaffMailSubject"));
            Environment.SetEnvironmentVariable(NewStaffMailBody, appInfo.GetValue<string>("NewStaffMailBody"));
        }

        public static void AddServiceConfigurationEnvironmentVariables(IConfigurationSection serviceConfiguration)
        {
            Environment.SetEnvironmentVariable(InputGMT, serviceConfiguration.GetValue<string>(nameof(InputGMT)));
            Environment.SetEnvironmentVariable(DailyScheduledTime, serviceConfiguration.GetValue<string>(nameof(DailyScheduledTime)));
        }
    }
}
