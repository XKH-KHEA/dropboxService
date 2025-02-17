using Dropbox.Api;
using Dropbox.Api.Files;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Threading.Tasks;

namespace DropboxService.Services
{
    public class DropboxServices
    {
        private readonly string _accessToken;

        public DropboxServices(IConfiguration configuration)
        {
            _accessToken = configuration["Dropbox:AccessToken"] ?? throw new ArgumentNullException(nameof(configuration), "Dropbox access token is not configured.");
        }

        /// <summary>
        /// Uploads a file to Dropbox and returns the shared URL.
        /// </summary>
        public async Task<string> UploadFileAsync(Stream fileStream, string fileName)
        {
            using (var dbx = new DropboxClient(_accessToken))
            {
                string dropboxPath = $"/uploads/{fileName}";

                // Upload the file to Dropbox
                await dbx.Files.UploadAsync(
                    dropboxPath,
                    WriteMode.Overwrite.Instance,
                    body: fileStream
                );

                // Get the shared link for the uploaded file
                var sharedLink = await GetSharedLinkAsync(dbx, dropboxPath);
                return sharedLink;
            }
        }

        /// <summary>
        /// Generates a shared link for the uploaded file in Dropbox.
        /// </summary>
        private async Task<string> GetSharedLinkAsync(DropboxClient dbx, string dropboxPath)
        {
            try
            {
                // Check if a shared link already exists for the file
                var sharedLinks = await dbx.Sharing.ListSharedLinksAsync(dropboxPath);
                if (sharedLinks.Links.Count > 0)
                {
                    // If a shared link exists, return the direct URL
                    return ConvertToDirectUrl(sharedLinks.Links[0].Url);
                }

                // Create a new shared link if one doesn't exist
                var newLink = await dbx.Sharing.CreateSharedLinkWithSettingsAsync(dropboxPath);
                return ConvertToDirectUrl(newLink.Url);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating shared link: {ex.Message}");
            }
        }

        /// <summary>
        /// Converts Dropbox's default shared link to a direct image URL.
        /// </summary>
        private string ConvertToDirectUrl(string dropboxUrl)
        {
            return dropboxUrl.Replace("?dl=0", "?raw=1"); // Converts to direct image URL
        }

        /// <summary>
        /// Retrieves the shared link for an existing file in Dropbox by its filename.
        /// </summary>
        public async Task<string> GetFileUrlAsync(string fileName)
        {
            using (var dbx = new DropboxClient(_accessToken))
            {
                string dropboxPath = $"/uploads/{fileName}";

                try
                {
                    // Check if the file exists in Dropbox
                    var fileMetadata = await dbx.Files.GetMetadataAsync(dropboxPath);
                    if (fileMetadata is FileMetadata)
                    {
                        // Generate and return the shared link for the file
                        return await GetSharedLinkAsync(dbx, dropboxPath);
                    }

                    return null; // File not found
                }
                catch (ApiException<GetMetadataError> ex)
                {
                    // Handle file not found or other errors
                    if (ex.ErrorResponse.IsPath && ex.ErrorResponse.AsPath.Value.IsNotFound)
                    {
                        throw new Exception("File not found.");
                    }

                    throw new Exception($"Error fetching file metadata: {ex.Message}");
                }
            }
        }
    }
}
