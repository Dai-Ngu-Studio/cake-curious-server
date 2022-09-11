using Microsoft.AspNetCore.Mvc.Formatters;

namespace CakeCurious_API.Utilities
{
    public class TextPlainInputFormatter : Microsoft.AspNetCore.Mvc.Formatters.InputFormatter
    {
        private const string ContentType = "text/plain";

        public TextPlainInputFormatter()
        {
            SupportedMediaTypes.Add(ContentType);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            using (var reader = new StreamReader(request.Body))
            {
                var content = await reader.ReadToEndAsync();
                return await InputFormatterResult.SuccessAsync(content);
            }
        }

        public override bool CanRead(InputFormatterContext context)
        {
            var contentType = context.HttpContext.Request.ContentType;
            if (contentType != null) return contentType.StartsWith(ContentType);
            return false;
        }
    }
}
