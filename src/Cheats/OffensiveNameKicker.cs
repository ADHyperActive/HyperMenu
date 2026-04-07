using System.Collections.Generic;
using UnityEngine;

namespace MalumMenu;

public static class OffensiveNameKicker
{
    // List of offensive names to auto-kick
    private static readonly List<string> BannedNames = new()
    {
        "epstein",
        "epstien",
        "diddy",
        "hitler",
        "mussolini",
        "bin laden",
        "stalin",
        "pol pot",
        "khmer rouge",
        "auser"
    };

    public static void CheckAndKickOffensiveNames()
    {
        if (!CheatToggles.kickOffensiveNames || !Utils.isHost) return;

        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player == null || player.Data == null || player.Data.Disconnected) continue;

            string playerName = player.Data.PlayerName.ToLower().Trim();

            // Check if player name contains any banned names
            foreach (var bannedName in BannedNames)
            {
                if (playerName.Contains(bannedName.ToLower()))
                {
                    // Kick the player by forcing them to disconnect
                    AmongUsClient.Instance.KickPlayer((int)player.Data.NetId, false);
                    break;
                }
            }
        }
    }
}
