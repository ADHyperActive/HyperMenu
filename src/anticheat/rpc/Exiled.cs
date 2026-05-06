using Hazel;

namespace MalumMenu.anticheat.rpc
{
	internal class Exiled : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			// The Exiled RPC is unused and is never sent in-game
			Anticheat.Flag(player, $"{player.Data.PlayerName} sent an invalid Exiled RPC.");
			blockRpc = true;
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.Exiled;
		}
	}
}