namespace Buttercup.Storage;

/// <summary>
/// Options for photo storage.
/// </summary>
public sealed class StorageOptions
{
    /// <summary>
    /// Gets or sets the container name for storing photos.
    /// </summary>
    public string ContainerName { get; set; } = "recipe-photos";
}
