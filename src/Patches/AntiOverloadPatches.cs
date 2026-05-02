using HarmonyLib;
using Hazel;

namespace MalumMenu;

// Minimum expected payload sizes (bytes) for each RPC call.
// Based on PlayerPhysics.HandleRpc and PlayerControl.HandleRpc implementations.
// Any RPC with a shorter payload is considered malformed and dropped.
public static class RpcValidator
{
    // Returns true when the RPC should be processed, false when it is malformed.
    public static bool IsValidPhysicsRpc(byte callId, MessageReader reader)
    {
        // EnterVent / ExitVent write a packed int32 (minimum 1 byte).
        // ClimbLadder writes ladderId (byte) + climbLadderSid (byte) = 2 bytes.
        int minBytes = (RpcCalls)callId switch
        {
            RpcCalls.EnterVent     => 1,
            RpcCalls.ExitVent      => 1,
            RpcCalls.ClimbLadder   => 2,
            _                      => 0, // unknown RPCs are passed through
        };

        return reader.Length >= minBytes;
    }

    public static bool IsValidControlRpc(byte callId, MessageReader reader)
    {
        // RPCs that carry no payload are always valid (minBytes = 0).
        // Every other known RPC requires at least one byte.
        // SetScanner and SendChatNote each need exactly 2 bytes.
        // SnapTo encodes at minimum a 4-byte float component of the Vector2.
        // ClimbLadder on PlayerControl mirrors PlayerPhysics: 2 bytes.
        // Unknown / newer RPCs (values not listed) pass through unmodified.
        int minBytes = (RpcCalls)callId switch
        {
            RpcCalls.PlayAnimation      => 1,
            RpcCalls.CompleteTask       => 1,
            RpcCalls.SyncSettings       => 1,
            RpcCalls.SetInfected        => 1,
            RpcCalls.Exiled             => 0, // no payload
            RpcCalls.CheckName          => 1,
            RpcCalls.SetName            => 1,
            RpcCalls.CheckColor         => 1,
            RpcCalls.SetColor           => 1,
            RpcCalls.ReportDeadBody     => 1,
            RpcCalls.MurderPlayer       => 1,
            RpcCalls.SendChat           => 1,
            RpcCalls.StartMeeting       => 1,
            RpcCalls.SetScanner         => 2, // bool on + byte count
            RpcCalls.SendChatNote       => 2, // byte srcPlayerId + byte noteType
            RpcCalls.SetStartCounter    => 1,
            RpcCalls.EnterVent          => 1,
            RpcCalls.ExitVent           => 1,
            RpcCalls.SnapTo             => 8, // Vector2 x (float) + Vector2 y (float) = 8 bytes minimum
            RpcCalls.CloseMeeting       => 0, // no payload
            RpcCalls.VotingComplete     => 1,
            RpcCalls.CastVote           => 2, // byte srcPlayerId + byte suspectPlayerId
            RpcCalls.ClearVote          => 0, // no payload
            RpcCalls.AddVote            => 1,
            RpcCalls.CloseDoorsOfType   => 1,
            RpcCalls.SetTasks           => 1,
            RpcCalls.ClimbLadder        => 2, // byte ladderId + byte climbLadderSid
            RpcCalls.UsePlatform        => 0, // no payload
            _                           => 0, // unknown / newer RPCs pass through
        };

        return reader.Length >= minBytes;
    }
}

[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
public static class PlayerPhysics_HandleRpc
{
    // Prefix patch of PlayerPhysics.HandleRpc to drop RPCs with malformed payloads
    public static bool Prefix(byte callId, MessageReader reader)
    {
        if (!CheatToggles.antiOverload) return true;

        return RpcValidator.IsValidPhysicsRpc(callId, reader);
    }
}

[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
public static class PlayerControl_HandleRpc
{
    // Prefix patch of PlayerControl.HandleRpc to drop RPCs with malformed payloads
    public static bool Prefix(byte callId, MessageReader reader)
    {
        if (!CheatToggles.antiOverload) return true;

        return RpcValidator.IsValidControlRpc(callId, reader);
    }
}
