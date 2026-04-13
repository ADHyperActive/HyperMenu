using HarmonyLib;
using UnityEngine;

namespace MalumMenu;

[HarmonyPatch(typeof(LobbyBehaviour), nameof(LobbyBehaviour.Start))]
public static class LobbyBehaviour_Start
{
    // Postfix patch of LobbyBehaviour.Start to apply randomized cosmetics when entering a lobby
    // Fires both when joining a new lobby and when returning to the same lobby after a game ends
    public static void Postfix(LobbyBehaviour __instance)
    {
        if (!CheatToggles.randomizeCosmetics) return;

        __instance.StartCoroutine(DelayedRandomize());
    }

    private static System.Collections.IEnumerator DelayedRandomize()
    {
        yield return new WaitForSeconds(1.0f);
        MalumRandomizer.Randomize();
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Update))]
public static class AmongUsClient_Update
{
    public static void Postfix()
    {
        MalumSpoof.SpoofLevel();

        // GuestMode cheats are commented out as they are broken in latest updates

        // Code to treat temp accounts the same as full accounts, including access to friend codes
        // if (!EOSManager.Instance.loginFlowFinished || !MalumMenu.guestMode.Value) return;
        // DataManager.Player.Account.LoginStatus = EOSManager.AccountLoginStatus.LoggedIn;

        // if (!string.IsNullOrWhiteSpace(EOSManager.Instance.FriendCode)) return;
        // var friendCode = MalumSpoof.spoofFriendCode();
        // var editUsername = EOSManager.Instance.editAccountUsername;
        // editUsername.UsernameText.SetText(friendCode);
        // editUsername.SaveUsername();
        // EOSManager.Instance.FriendCode = friendCode;
    }
}

[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
public static class AmongUsClient_OnGameJoined
{
    // Postfix patch of AmongUsClient.OnGameJoined to store the last joined game ID string
    public static string lastGameIdString = "";

    public static void Postfix(string gameIdString)
    {
        lastGameIdString = gameIdString;
    }
}
