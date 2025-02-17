using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DropboxImageAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DropboxController : ControllerBase
    {
        private const string AccessToken = "sl.u.AFhIryUMc-OIRt5QQ4R8UBEcCVdOtFi4aJFFU0fJtMl8z-bFfsl-eA43p8bBL88VEY-HWz_lSoDrvUVVdPLgjSKUe7xiq4EG2MGWLQQSQRRgggOvIWaSeXgZpwTBeh3PPxUc-R_SQuReTg1QHFJBptcrbzCtXDKFeQEbUeajF09H4iQnmPMbtu2bXNYuFfP1puGB_BZxDk-k9f5cOr4T9ivllQdun79ZN0ZzkcFjmIbVQwsjdryNXUcHgJ5feCSK7V4DvOVRysnCSNBmKkUWabDxqdeTO7wWQQHbVD6D1YiTZHoRp24w0IVm7W_FsBTcHqFFCNEFyhveDVXCYXy_JSgjI8XveRi-mm895Fu4-EYFXXYCO3KAF0lwnYux3QRXnvwvI6DwxvgCpQutQfhsJe1pYpFJmHbvfqanvfdYkOOKUCdb6rA2secgunwtjNJsbh3ZwH4ox8a0c9LlijC31aVgC078pD7ieoqnklrtcwNlK0K4o_EdW8p7cZ2-nuFUw4v4Eg50nD38azaTZIGZyCf_Rfz4AvWjqGpPmfXWMQ0TFRA_ENZHwXW40ZrA15r5L4GWQqKBvlucozBqJDu3fBAmRJfBYkmmou__RXIFlmIskshkkEzXY3TL7y9RQxkLIANNZJii76NbOk4ANkQc4E6enGtb0TuG2YLDDxJzBmClXYpEVo512bZa1ryMBXBSAZ72hJyv-PIaK8wG0CYLXThL_yIAp8RHqRjuUo-y3KqxqK289loukDXEpoCNxjr5vih1QY7heg2bDG7yI9x4VW8lF9Uyr-peWmIl6iQQcLNOf3B1TbpX7QQR8QUYRLB9-Ab0K2BEHNG0rapps1pJC1hvsp4R6k32_XM_ClJECn0ma5RMb1d1zJQCaC2QK61oaOUFGbrE1aPXRMmGCmg5lZndNbdCw5rlxJlA_xvQXPlJn2KDjboExCA0pEMaQYWMKK844N8SrSeVabG8ddN2tsKDBfLtfHOlx6JLUX1CFmMtpFOdxVrB9Wkam_VC8Oo_4VpPSiTO4K2u5_ao3IEkikgaoqrJL-1DIWGX5Y7C4if6YZdvxMSLTFUs_Myl7XyZ3NpF0nHz4p1vk_EUE9t241WXk63WLHPBxCMn7GaokR5PWImTCqubXp-c1NkalwRwfMVevVFIFfwxmMoOtbU-vogl_z5WzTm2M8w6bM8cDRZUFZm_1tsSPaNS_7CfiKPTr9VXZ7rjunISiyqHB84PibasmTmMTfYQdgoHpI5a5ihcnjguANncfdd9PVChSuSQdOFcIMlhnNIo43J14XPG6dDZ3jf8qhxT2x3WiqRUq9pgq47IfetmsKqdi6mysWI_6mlSf8DBYIwpKqjSJhZrC6L1Qy1lUoZr80oct6lQjPBd5uzkrIDpWq2ncCTSU_h00a493uS1fZ7F248JxCJGQzoG"; // Replace with your access token
        private const string DropboxFolderPath = "/images";

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty or missing.");

            using var dropboxClient = new DropboxClient(AccessToken);
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            string dropboxFilePath = $"{DropboxFolderPath}/{file.FileName}";

            try
            {
                await dropboxClient.Files.UploadAsync(
                    dropboxFilePath,
                    WriteMode.Overwrite.Instance,
                    body: memoryStream);
                return Ok(new { Message = "File uploaded successfully", FileName = file.FileName });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error uploading file", Error = ex.Message });
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> GetImageList()
        {
            using var dropboxClient = new DropboxClient(AccessToken);
            try
            {
                var list = await dropboxClient.Files.ListFolderAsync(DropboxFolderPath);
                var files = list.Entries.Where(i => i.IsFile).Select(f => f.Name).ToList();
                return Ok(files);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error fetching file list", Error = ex.Message });
            }
        }

        [HttpGet("download/{fileName}")]
        public async Task<IActionResult> DownloadImage(string fileName)
        {
            using var dropboxClient = new DropboxClient(AccessToken);
            string dropboxFilePath = $"{DropboxFolderPath}/{fileName}";

            try
            {
                var response = await dropboxClient.Files.DownloadAsync(dropboxFilePath);
                var fileStream = await response.GetContentAsStreamAsync();
                return File(fileStream, "image/jpeg", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error downloading file", Error = ex.Message });
            }
        }
    }
}
