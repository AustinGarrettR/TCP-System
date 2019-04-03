using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
public class PacketAttribute : Attribute
{
    // Private fields.
    private int packetId;
    private string description;

    // This constructor defines two required parameters: packetId and description.

    public PacketAttribute(int packetId, string description)
    {
        this.packetId = packetId;
        this.description = description;
    }

    // This is a read-only attribute.

    public virtual int PacketId
    {
        get { return packetId; }
    }

    // This is a read-only attribute.

    public virtual string Description
    {
        get { return description; }
    }

}
