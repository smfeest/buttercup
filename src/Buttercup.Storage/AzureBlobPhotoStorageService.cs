using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Options;

namespace Buttercup.Storage;

/// <summary>
/// Azure Blob Storage implementation of <see cref="IPhotoStorageService"/>.
/// </summary>
public sealed class AzureBlobPhotoStorageService(
    BlobServiceClient blobServiceClient,
    IOptions<StorageOptions> options) : IPhotoStorageService
{
    private readonly BlobServiceClient blobServiceClient = blobServiceClient;
    private readonly StorageOptions options = options.Value;

    /// <inheritdoc/>
    public async Task<string> UploadPhotoAsync(
        string fileName,
        string contentType,
        Stream stream,
        CancellationToken cancellationToken = default)
    {
        var containerClient = this.blobServiceClient.GetBlobContainerClient(this.options.ContainerName);
        await containerClient.CreateIfNotExistsAsync(
            PublicAccessType.Blob,
            cancellationToken: cancellationToken);

        var blobName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var blobClient = containerClient.GetBlobClient(blobName);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        await blobClient.UploadAsync(
            stream,
            new BlobUploadOptions { HttpHeaders = blobHttpHeaders },
            cancellationToken);

        return blobClient.Uri.ToString();
    }

    /// <inheritdoc/>
    public async Task DeletePhotoAsync(string photoUrl, CancellationToken cancellationToken = default)
    {
        var uri = new Uri(photoUrl);
        var blobName = Path.GetFileName(uri.LocalPath);

        var containerClient = this.blobServiceClient.GetBlobContainerClient(this.options.ContainerName);
        var blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }
}
