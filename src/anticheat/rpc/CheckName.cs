using Hazel;
using InnerNet;

namespace MalumMenu.anticheat.rpc
{
	internal class CheckName : RpcCheck
	{
		public readonly int MAX_NAME_LENGTH = 10;

		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			string requestedName = reader.ReadString();

			/*
			ClientData clientData = AmongUsClient.Instance.GetClient(player.OwnerId);
			if(AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame && requestedName != clientData.PlayerName)
			{
				player.SetName(clientData.PlayerName);
				blockRpc = true;

				Anticheat.Flag(player, $"{clientData.PlayerName} requested a name that does not match their name in the login handshake.");
			}
			*/

			if(requestedName.Length > MAX_NAME_LENGTH)
			{
				blockRpc = true;
				Anticheat.Flag(player, $"{requestedName} tried setting their name to something too long ({requestedName.Length}).");
			}

			if(requestedName.Contains('<'))
			{
				blockRpc = true;
				Anticheat.Flag(player, $"{requestedName} requested a name with invalid characters.");
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.CheckName;
		}
	}
}
