using Hazel;

namespace MalumMenu.anticheat.rpc
{
	internal class SetStartCounter : RpcCheck
	{
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			reader.ReadPackedInt32();
			sbyte counter = reader.ReadSByte();

			// When a non-host player sends the SetStartCounter RPC, the counter value must always be -1
			// I'm not sure why non-host players even need to send this RPC, it's more something only the host should be sending
			if(player.OwnerId != AmongUsClient.Instance.HostId && counter != -1)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent a SetStartCounter RPC with an invalid value: {counter}.");
				blockRpc = true;

				// Revert the invalid start counter
				if(AmongUsClient.Instance.AmHost)
				{
					PlayerControl.LocalPlayer.RpcSetStartCounter(-1);
				}
			}

			/*
			if(PlayerControl.LocalPlayer != null && LobbyBehaviour.Instance == null)
			{
				// The vanilla game already ignores SetStartCounter RPCs when the game has started, so we do not need to revert it
				Anticheat.Flag(player, $"{player.Data.PlayerName} sent a SetStartCounter RPC when the lobby has despawned.");
				blockRpc = true;
			}
			*/
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.SetStartCounter;
		}
	}
}