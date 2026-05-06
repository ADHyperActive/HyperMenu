using Hazel;
using System;

namespace MalumMenu.anticheat.rpc
{
	internal class RpcCheck
	{
		public virtual bool Enabled { get; set; } = true;

		public virtual void Validate(PlayerControl player, MessageReader reader, ref bool blockRpc) { }

		public virtual RpcCalls GetRpcCall()
		{
			throw new InvalidOperationException("Unimplemented");
		}

		public virtual bool IsHostOnly()
		{
			return false;
		}

		public virtual Type GetExpectedNetObject()
		{
			// There are more RPCs for the PlayerControl net object than for any other net object
			// To make it easier for us, each instance of RpcCheck will be for the PlayerControl net object unless stated otherwise
			return typeof(PlayerControl);
		}
	}
}