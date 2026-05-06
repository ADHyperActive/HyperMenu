using HarmonyLib;
using Hazel;
using InnerNet;

namespace MalumMenu.features
{
	internal class Protections
	{
		public static bool BlockInvalidVentOverload { get; set; } = true;
		public static bool BlockInvalidLadderOverload { get; set; } = true;

		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SetEndpoint))]
		public static class ForceDTLS
		{
			public static bool Enabled { get; set; } = true;

			static void Prefix(ref bool dtls)
			{
				if(Enabled) dtls = true;
			}
		}

		[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
		public static class BlockServerTeleports
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(CustomNetworkTransform __instance, byte callId)
			{
				if(!Enabled || callId != (byte)RpcCalls.SnapTo || __instance.myPlayer.PlayerId != PlayerControl.LocalPlayer.PlayerId) return true;

				MalumMenu.Log.LogMessage($"Recived SnapTo RPC for our player, since block server teleports is enabled we will disregard the RPC");
				return false;
			}
		}

		// Among Us had this bug where if you reported the body of a player who has left, the anticheat would incorrectly ban you from the lobby
		// To prevent incorrect lobby bans, we block reporting bodies of players who left
		/*
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
		public static class PreventReportBan
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(PlayerControl __instance, NetworkedPlayerInfo target)
			{
				if(
					// Only make it so only our body reports run
					__instance.NetId == PlayerControl.LocalPlayer.NetId &&
					// Make sure its not an emergency meeting
					target != null &&
					target.Disconnected &&
					// Hosts are exempt from the anticheat detection
					!AmongUsClient.Instance.AmHost
				)
				{
					MalumMenu.notifications.Send("Protections Alert", $"Saved you from getting banned by {target.PlayerName}'s glitched body.");
					return false;
				}
				return true;
			}
		}
		*/

		[HarmonyPatch(typeof(MessageReader), nameof(MessageReader.ReadPackedUInt32))]
		public static class HardenedReadPackedUInt
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(MessageReader __instance, ref uint __result)
			{
				if(!Enabled) return true;

				bool readMore = true;
				int shift = 0;
				uint output = 0;

				while(readMore)
				{
					if(__instance.BytesRemaining < 1) break;

					byte b = __instance.ReadByte();
					if(b >= 0x80)
					{
						readMore = true;
						b ^= 0x80;
					}
					else
					{
						readMore = false;
					}

					output |= (uint)(b << shift);
					shift += 7;
				}

				__result = output;
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
		public static class OnPlayerPhysicsRpc
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				int oldReadPosition = reader.Position;
				RpcCalls rpcId = (RpcCalls)callId;

				switch(rpcId)
				{
					case RpcCalls.EnterVent:
					case RpcCalls.ExitVent:
					case RpcCalls.BootFromVent:
						int ventId = reader.ReadPackedInt32();

						if(BlockInvalidVentOverload && !IsValidVentId(ventId))
						{
							return false;
						}
						break;

					case RpcCalls.ClimbLadder:
						byte ladderId = reader.ReadByte();

						if(BlockInvalidLadderOverload && (!ShipStatus.Instance || ladderId > ShipStatus.Instance.Ladders.Length - 1))
						{
							return false;
						}
						break;
				}

				reader.Position = oldReadPosition;
				return true;
			}
		}

		private static bool IsValidVentId(int ventId)
		{
			if(ShipStatus.Instance == null) return false;

			MapNames map = Utilities.GetCurrentMap();
			// On Mira, there is no vent with ID 0 for whatever reason
			if(map == MapNames.MiraHQ && (ventId == 0 || ShipStatus.Instance.AllVents.Length > ventId))
			{
				return false;
			}
			else if(map != MapNames.MiraHQ && ShipStatus.Instance.AllVents.Length - 1 > ventId)
			{
				return false;
			}

			return true;
		}

		[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
		public static class Votekicks
		{
			public static bool Enabled { get; set; } = true;

			static bool Prefix(int srcClient, int clientId)
			{
				MalumMenu.Log.LogInfo($"[VotekickLogger] {srcClient} voted to kick out {clientId}");
				if(clientId != PlayerControl.LocalPlayer.OwnerId) return true;

				ClientData player = AmongUsClient.Instance.GetClient(srcClient);

				MalumMenu.notifications.Send("Votekick Logger", $"{player.PlayerName} has voted to kick you out.");

				// Prevent players from being able to votekick you as host
				return !(Enabled && AmongUsClient.Instance.AmHost);
			}
		}
	}
}