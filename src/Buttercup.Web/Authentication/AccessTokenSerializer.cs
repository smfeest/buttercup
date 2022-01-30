namespace Buttercup.Web.Authentication;

public sealed class AccessTokenSerializer : IAccessTokenSerializer
{
    public byte[] Serialize(AccessTokenPayload payload)
    {
        using var stream = new MemoryStream();

        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(payload.UserId);
            writer.Write(payload.SecurityStamp);
            writer.Write(payload.Issued.ToBinary());
        }

        return stream.ToArray();
    }

    public AccessTokenPayload Deserialize(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);

        return new(
            reader.ReadInt64(), reader.ReadString(), DateTime.FromBinary(reader.ReadInt64()));
    }
}
