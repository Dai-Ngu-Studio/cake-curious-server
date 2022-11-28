using Google.Cloud.Translation.V2;

namespace CakeCurious_API.Utilities
{
    public static class TranslationHelper
    {
        public static async Task<List<string>> TranslateSingle(TranslationClient translationClient, string text, List<string> output)
        {
            var detection = await translationClient.DetectLanguageAsync(text);
            switch (detection.Language)
            {
                case LanguageCodes.Vietnamese:
                    output.Add((await translationClient.TranslateTextAsync(text, LanguageCodes.English, LanguageCodes.Vietnamese)).TranslatedText);
                    break;
                case LanguageCodes.English:
                    output.Add((await translationClient.TranslateTextAsync(text, LanguageCodes.Vietnamese, LanguageCodes.English)).TranslatedText);
                    break;
                default:
                    break;
            }
            return output;
        }

        public static async Task<List<string>> TranslateList(TranslationClient translationClient, List<string> originSource, List<string> output)
        {
            var splitSource = new DualLanguageList();
            var detections = await translationClient.DetectLanguagesAsync(originSource);
            foreach (var detection in detections)
            {
                switch (detection.Language)
                {
                    case LanguageCodes.Vietnamese:
                        splitSource.Vietnamese.Add(detection.Text);
                        break;
                    case LanguageCodes.English:
                        splitSource.English.Add(detection.Text);
                        break;
                    default:
                        break;
                }
            }

            if (splitSource.Vietnamese.Count > 0)
            {
                var english = await translationClient.TranslateTextAsync(splitSource.Vietnamese, LanguageCodes.English, LanguageCodes.Vietnamese);
                output.AddRange(english.Select(x => x.TranslatedText));
            }

            if (splitSource.English.Count > 0)
            {
                var vietnamese = await translationClient.TranslateTextAsync(splitSource.English, LanguageCodes.Vietnamese, LanguageCodes.English);
                output.AddRange(vietnamese.Select(x => x.TranslatedText));
            }

            return output;
        }
    }

    public class DualLanguageList
    {
        public List<string> Vietnamese { get; set; } = new List<string>();
        public List<string> English { get; set; } = new List<string>();
    }
}
