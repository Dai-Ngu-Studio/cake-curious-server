using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CakeCurious_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CloudController : ControllerBase
    {
        private const string GoogleStorage = "https://storage.googleapis.com/";
        private const string BucketName = "cake-curious.appspot.com";
        private const string BaseUrl = $"{GoogleStorage}{BucketName}";

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
            return Unauthorized();
        }

        // Upload-Url is optional
        // Upload-Url must be acquired from /api/cloud/url if it were to be included in headers
        [HttpPost("upload")]
        [Authorize]
        [RequestSizeLimit(10L * 1024L * 1024L * 1024L)]
        [RequestFormLimits(MultipartBodyLengthLimit = 10L * 1024L * 1024L * 1024L)]
        public ActionResult Upload([FromForm] IFormFile file)
        {
            try
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
                    StorageClient storageClient = StorageClient.Create();
                    Google.Apis.Storage.v1.Data.Object gObject = storageClient.UploadObject(BucketName, $"{uid}/{destination}", file.ContentType, file.OpenReadStream());

                    return Ok($"{GoogleStorage}{gObject.Bucket}/{gObject.Name}");
                }
                return Unauthorized();
            }
            catch (Exception)
            {
                return BadRequest();
            }
        }
    }
}
