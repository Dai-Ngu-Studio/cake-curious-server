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
        }

        public static void AddServiceConfigurationEnvironmentVariables(IConfigurationSection serviceConfiguration)
        {
            Environment.SetEnvironmentVariable(InputGMT, serviceConfiguration.GetValue<string>(nameof(InputGMT)));
            Environment.SetEnvironmentVariable(DailyScheduledTime, serviceConfiguration.GetValue<string>(nameof(DailyScheduledTime)));
        }
    }
}
