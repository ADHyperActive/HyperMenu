using Hazel;
using System;

namespace MalumMenu.anticheat.rpc
{
	internal class CloseDoorsOfType : RpcCheck
	{
		// Player is null here as we cannot determine who sent this RPC
		public override void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc)
		{
			// It would be nice if we could also add additional checks such as someone closing doors without being imposter
			// however we are not able to determine who send the CloseDoorsOfType RPC
			if(GameManager.Instance.IsHideAndSeek())
			{
				MalumMenu.notifications.Send("Anticheat", "Someone attempted to close doors while in Hide and Seek.", Anticheat.NotificationDuration);
				blockRpc = true;
			}
		}

		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.CloseDoorsOfType;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(ShipStatus);
		}
	}
}