using Hazel;

namespace MalumMenu.anticheat.rpc
{
	internal class SetName : RpcCheck
	{
		// We increase the max name length to 12 instead of 10 as we need to account for the numbers at the end of player names when there are multiple players with the same name
		public readonly int MAX_NAME_LENGTH = 12;

		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			// On modded lobbies, it is common for players to have custom names
			if(Constants.IsVersionModded()) return;

			reader.ReadUInt32();
			string requestedName = reader.ReadString();

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
			return RpcCalls.SetName;
		}

		// On vanilla servers, the host is able to receive the SetName RPC due to how server-authority works
		// On modded servers, the host should never recieve the SetName RPC
		public override bool IsHostOnly()
		{
			return Constants.IsVersionModded();
		}
	}
}
