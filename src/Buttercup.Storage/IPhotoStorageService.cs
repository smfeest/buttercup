namespace Buttercup.Storage;

/// <summary>
/// Provides methods for storing and managing photos in blob storage.
/// </summary>
public interface IPhotoStorageService
{
    /// <summary>
    /// Uploads a photo to blob storage.
    /// </summary>
    /// <param name="fileName">The file name.</param>
    /// <param name="contentType">The content type of the file.</param>
    /// <param name="stream">The file stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The URL of the uploaded photo.</returns>
    Task<string> UploadPhotoAsync(
        string fileName,
        string contentType,
        Stream stream,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a photo from blob storage.
    /// </summary>
    /// <param name="photoUrl">The URL of the photo to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DeletePhotoAsync(string photoUrl, CancellationToken cancellationToken = default);
}
