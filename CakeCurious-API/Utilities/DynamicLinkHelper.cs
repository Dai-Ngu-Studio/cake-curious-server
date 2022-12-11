using BusinessObject;
using Google.Apis.FirebaseDynamicLinks.v1;
using Google.Apis.FirebaseDynamicLinks.v1.Data;
using System.Web;

namespace CakeCurious_API.Utilities
{
    public static class DynamicLinkHelper
    {
        public static async Task<CreateShortDynamicLinkResponse> CreateDynamicLink(
            FirebaseDynamicLinksService linkService, string path, string id, string name, 
            string description, string photoUrl, string? thumbnailUrl)
        {
            var webAppUri = Environment.GetEnvironmentVariable(EnvironmentHelper.WebAppUri);
            var sharePrefixUri = Environment.GetEnvironmentVariable(EnvironmentHelper.ShareUriPrefix);
            var androidPackageName = Environment.GetEnvironmentVariable(EnvironmentHelper.AndroidPackageName);
            var androidMinPackageVersion = Environment.GetEnvironmentVariable(EnvironmentHelper.AndroidMinPackageVersionCode);
            var suffixOption = Environment.GetEnvironmentVariable(EnvironmentHelper.SuffixOption);

            var nameEncoded = HttpUtility.UrlEncode(name);

            var linkRequest = linkService.ShortLinks.Create(new CreateShortDynamicLinkRequest
            {
                DynamicLinkInfo = new DynamicLinkInfo
                {
                    Link = $"{webAppUri}/{path}/{id}/?name={nameEncoded}&photoUrl={photoUrl}",
                    DomainUriPrefix = sharePrefixUri,
                    AndroidInfo = new AndroidInfo
                    {
                        AndroidPackageName = androidPackageName,
                        AndroidMinPackageVersionCode = androidMinPackageVersion,
                    },
                    SocialMetaTagInfo = new SocialMetaTagInfo
                    {
                        SocialTitle = name,
                        SocialImageLink = !string.IsNullOrWhiteSpace(thumbnailUrl) ? thumbnailUrl : photoUrl,
                        SocialDescription = description,
                    }
                },
                Suffix = new Suffix
                {
                    Option = suffixOption
                }
            });
            return await linkRequest.ExecuteAsync();
        }
    }
}
