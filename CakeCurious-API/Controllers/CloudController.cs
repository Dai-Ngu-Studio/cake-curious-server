using CakeCurious_API.Utilities;
using Google.Analytics.Data.V1Beta;
using Google.Cloud.Storage.V1;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CloudController : ControllerBase
    {
        private const string GoogleStorage = "https://storage.googleapis.com/";
        private const string BucketName = "cake-curious.appspot.com";
        private const string BaseUrl = $"{GoogleStorage}{BucketName}";
        private const string AnalyticsPropertyId = "331411032";
        private const string NoImageAvailable = $"{BaseUrl}/app/no-image-available.png";
        private readonly StorageClient storageClient;

        public CloudController(StorageClient _storageClient)
        {
            storageClient = _storageClient;
        }

        /// <summary>
        /// <para>Returns an URL which could then be put in a request to the upload endpoint.</para>
        /// <para>Headers must include File-Extension: &lt;file-extension&gt;</para>
        /// <para>Replace &lt;file-extension&gt; with file extension.</para>
        /// <example>
        /// This shows examples for File-Extension header.
        /// <code>
        /// File-Extension: mp4
        /// File-Extensoin: png
        /// </code>
        /// </example>
        /// </summary>
        /// <returns>An URL</returns>
        [HttpGet("url")]
        [Authorize]
        public ActionResult<string> GetUploadUrl()
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                // Get file extension
                var fileExtension = Request.Headers["File-Extension"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(fileExtension))
                {
                    // Create URL
                    var url = $"{BaseUrl}/{uid}/{Guid.NewGuid()}.{fileExtension}";
                    return Ok(url);
                }
                return BadRequest();
            }
            return Forbid();
        }

        // Upload-Url is optional
        // Upload-Url must be acquired from /api/cloud/url if it were to be included in headers
        [HttpPost("upload")]
        [Authorize]
        [RequestSizeLimit(10L * 1024L * 1024L * 1024L)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024L * 1024L * 1024L)]
        public ActionResult Upload([FromForm] IFormFile file)
        {
            string? uid = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                // Get file extension
                var contentType = file.ContentType.Split('/');
                var fileExtension = contentType[1];

                // Set destination
                string destination = $"{Guid.NewGuid()}.{fileExtension}";
                // If Upload-Url was included, use Upload-Url for destination instead
                var uploadUrl = Request.Headers["Upload-Url"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(uploadUrl) && uploadUrl.Contains(BaseUrl))
                {
                    destination = uploadUrl.Substring(uploadUrl.LastIndexOf('/') + 1);
                }

                // Upload to Firebase Cloud Storage
                Google.Apis.Storage.v1.Data.Object gObject = storageClient.UploadObject(BucketName, $"{uid}/{destination}", file.ContentType, file.OpenReadStream());

                // If Thumbnail-Url was included, use Thumbnail-Url for compressed destination
                var thumbnailUrl = Request.Headers["Thumbnail-Url"].FirstOrDefault();
                if (!string.IsNullOrWhiteSpace(thumbnailUrl) && thumbnailUrl.Contains(BaseUrl))
                {
                    var compressedDestination = thumbnailUrl.Substring(thumbnailUrl.LastIndexOf('/') + 1);
                    try
                    {
                        // Upload compressed to Firebase Cloud Storage
                        using (var readStream = file.OpenReadStream())
                        {
                            using (var image = new MagickImage(readStream))
                            {
                                var imageWidth = int.Parse(Environment.GetEnvironmentVariable(EnvironmentHelper.ShareImageWidth) ?? "300");
                                var imageHeight = int.Parse(Environment.GetEnvironmentVariable(EnvironmentHelper.ShareImageHeight) ?? "200");
                                var size = new MagickGeometry(imageWidth, imageHeight);
                                var memoryStream = new MemoryStream();
                                image.Resize(size);
                                image.WriteAsync(memoryStream);
                                Google.Apis.Storage.v1.Data.Object compressedGObject = storageClient.UploadObject(BucketName, $"{uid}/{compressedDestination}", file.ContentType, memoryStream);
                            }
                        }
                    }
                    catch (MagickException)
                    {
                        thumbnailUrl = NoImageAvailable;
                    }

                    return Ok(new
                    {
                        PhotoUrl = $"{GoogleStorage}{gObject.Bucket}/{gObject.Name}",
                        ThumbnailUrl = thumbnailUrl,
                    });
                }
                else
                {
                    return Ok($"{GoogleStorage}{gObject.Bucket}/{gObject.Name}");
                }

            }
            return Forbid();
        }
    }
}
