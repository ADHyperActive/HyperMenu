namespace MalumMenu.anticheat.rpc
{
	internal class SetColor : RpcCheck
	{
		public override RpcCalls GetRpcCall()
		{
			return RpcCalls.SetColor;
		}

		public override bool IsHostOnly()
		{
			return true;
		}
	}
}
