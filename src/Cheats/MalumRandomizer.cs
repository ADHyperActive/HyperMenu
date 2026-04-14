using System;
using System.Collections;
using AmongUs.Data;
using InnerNet;
using UnityEngine;

namespace MalumMenu;

public static class MalumRandomizer
{
    // True while a leave-randomize-rejoin sequence is in progress.
    // Used to suppress the auto-randomize trigger in LobbyBehaviour_Start after we rejoin.
    public static bool isRejoinInProgress = false;

    // Seconds to wait after ExitGame before attempting to rejoin (allows the disconnect to complete).
    private const float DisconnectWaitSeconds = 1.5f;

    // Seconds to keep isRejoinInProgress true after calling JoinGame so that the incoming
    // LobbyBehaviour_Start patch fires while the flag is still set and does not trigger
    // a second randomize cycle.
    private const float LobbyStartWaitSeconds = 3.0f;

    // Randomly sets a new name, color, and cosmetics for the local player.
    // When connected to a server the anticheat may flag an in-session RpcSetName call,
    // so we instead leave the server, apply cosmetics locally, then rejoin so the new
    // name is sent as part of the join handshake rather than as a mid-session RPC.
    public static void Randomize()
    {
        try
        {
            if (isRejoinInProgress) return;

            // Apply random cosmetics to DataManager (safe at any time, no network calls).
            ApplyRandomCosmetics();

            if (Utils.isLobby)
            {
                // Leave the lobby, then rejoin so cosmetics are transmitted via the join
                // handshake instead of a post-join RpcSetName that can trigger anticheat.
                var gameIdString = AmongUsClient_OnGameJoined.lastGameIdString;
                AmongUsClient.Instance.StartCoroutine(RejoinAfterRandomize(gameIdString));
                return;
            }

            if (Utils.isInGame)
            {
                // During an active game, skip RPC calls entirely.
                // The updated DataManager values will take effect on the next lobby join.
                return;
            }

            if (!Utils.isPlayer) return;

            // Offline or in free-play: broadcast cosmetics directly via RPC.
            PlayerControl.LocalPlayer.RpcSetColor(DataManager.Player.Customization.Color);
            PlayerControl.LocalPlayer.RpcSetName(DataManager.Player.Customization.Name);
            PlayerControl.LocalPlayer.RpcSetHat(DataManager.Player.Customization.Hat);
            PlayerControl.LocalPlayer.RpcSetSkin(DataManager.Player.Customization.Skin);
            PlayerControl.LocalPlayer.RpcSetVisor(DataManager.Player.Customization.Visor);
            PlayerControl.LocalPlayer.RpcSetPet(DataManager.Player.Customization.Pet);
        }
        catch (Exception e)
        {
            MalumMenu.Log.LogError($"Randomizer error: {e.Message}");
        }
    }

    // Applies random cosmetics to DataManager only (no network calls).
    private static void ApplyRandomCosmetics()
    {
        DataManager.Player.Customization.Name = Utils.GetRandomName();
        DataManager.Player.Customization.Color = (byte)UnityEngine.Random.Range(0, Palette.PlayerColors.Length);

        var hatManager = DestroyableSingleton<HatManager>.Instance;
        if (hatManager != null)
        {
            var allHats = hatManager.allHats;
            var allSkins = hatManager.allSkins;
            var allVisors = hatManager.allVisors;
            var allPets = hatManager.allPets;

            if (allHats != null && allHats.Count > 0)
                DataManager.Player.Customization.Hat = allHats[UnityEngine.Random.Range(0, allHats.Count)].ProdId;

            if (allSkins != null && allSkins.Count > 0)
                DataManager.Player.Customization.Skin = allSkins[UnityEngine.Random.Range(0, allSkins.Count)].ProdId;

            if (allVisors != null && allVisors.Count > 0)
                DataManager.Player.Customization.Visor = allVisors[UnityEngine.Random.Range(0, allVisors.Count)].ProdId;

            if (allPets != null && allPets.Count > 0)
                DataManager.Player.Customization.Pet = allPets[UnityEngine.Random.Range(0, allPets.Count)].ProdId;
        }

        DataManager.Player.Save();
    }

    // Leaves the current lobby, waits for disconnect, then rejoins so that the new
    // cosmetics (already saved to DataManager) are sent via the join handshake rather
    // than an in-session RpcSetName that anticheat may flag.
    private static IEnumerator RejoinAfterRandomize(string gameIdString)
    {
        isRejoinInProgress = true;

        AmongUsClient.Instance.ExitGame(DisconnectReasons.ExitGame);

        yield return new WaitForSeconds(DisconnectWaitSeconds);

        if (!string.IsNullOrEmpty(gameIdString) && AmongUsClient.Instance != null)
        {
            AmongUsClient.Instance.JoinGame(GameCode.GameNameToInt(gameIdString), MatchMakerModes.Client);
        }

        // Keep the flag set long enough for LobbyBehaviour_Start to fire on the rejoined
        // lobby (which would otherwise trigger a second randomize and restart the cycle).
        yield return new WaitForSeconds(LobbyStartWaitSeconds);

        isRejoinInProgress = false;
    }
}
