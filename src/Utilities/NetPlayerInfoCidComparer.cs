using System.Collections.Generic;

public sealed class NetPlayerInfoCidComparer : IEqualityComparer<NetworkedPlayerInfo>
{
    public bool Equals(NetworkedPlayerInfo data1, NetworkedPlayerInfo data2)
    {
        return data1.ClientId == data2.ClientId;
    }

    public int GetHashCode(NetworkedPlayerInfo data)
    {
        return data.ClientId;
    }
}
