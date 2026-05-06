using System.Collections;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using MalumMenu.features;
using InnerNet;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;


namespace MalumMenu;

public class HostOnlyTab2 : ITab
{
    public string name => "Host-Only 2";

    private byte selectedMap = 0;

    public void Draw()
    {
        if (PlayerControl.LocalPlayer == null)
        {
            GUILayout.Label("You are not currently in a game, these options will not work.");
        }
        else if (!AmongUsClient.Instance.AmHost)
        {
            GUILayout.Label("You are not the host of the current lobby. Using these options will either do nothing or get you banned by the anticheat");
        }
        Host.BanMidGame.Enabled = GUILayout.Toggle(Host.BanMidGame.Enabled, "Be able to ban players mid-game");

        Host.FlippedSkeld = GUILayout.Toggle(Host.FlippedSkeld, "Use Flipped Skeld Map");

        Host.DisableMeetings.Enabled = GUILayout.Toggle(Host.DisableMeetings.Enabled, "Disable Meetings");
        Host.DisableSabotages.Enabled = GUILayout.Toggle(Host.DisableSabotages.Enabled, "Disable Sabotages");
        Host.DisableCloseDoors.Enabled = GUILayout.Toggle(Host.DisableCloseDoors.Enabled, "Disable Close Doors");
        Host.DisableCameras.Enabled = GUILayout.Toggle(Host.DisableCameras.Enabled, "Disable Security Cameras");
        Host.DisableGameEnd.Enabled = GUILayout.Toggle(Host.DisableGameEnd.Enabled, "Disable Game End");

        GUILayout.BeginHorizontal();
        Host.BlockLowLevels.Enabled = GUILayout.Toggle(Host.BlockLowLevels.Enabled, $"Kick players with less than {Host.BlockLowLevels.MinLevel} levels");
        Host.BlockLowLevels.MinLevel = (uint)GUILayout.HorizontalSlider(Host.BlockLowLevels.MinLevel, 0, 100);
        GUILayout.EndHorizontal();

        MalumMenu.routines.reportBodySpam.Enabled = GUILayout.Toggle(MalumMenu.routines.reportBodySpam.Enabled, "Spam Report Bodies");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Force Crewmate Victory"))
        {
            // Just incase the user has this enabled
            Host.DisableGameEnd.Enabled = false;

            GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
            MalumMenu.notifications.Send("Game Finished", "You ended the game with a crewmate victory.", 5);
        }

        if (GUILayout.Button("Force Imposter Victory"))
        {
            // Just incase the user has this enabled
            Host.DisableGameEnd.Enabled = false;

            GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByKill, false);
            MalumMenu.notifications.Send("Game Finished", "You ended the game with an imposter victory.", 5);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.Label("Map Spawner/Despawner:");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Despawn Lobby"))
        {
            if (LobbyBehaviour.Instance != null)
            {
                LobbyBehaviour.Instance.Despawn();
                MalumMenu.notifications.Send("Lobby Map", "The lobby map has been despawned.", 5);
            }
            else
            {
                MalumMenu.notifications.Send("Lobby Map", "The lobby map has already been despawned.", 5);
            }
        }

        if (GUILayout.Button("Spawn Lobby"))
        {
            // From GameStartManager::Start
            LobbyBehaviour.Instance = Object.Instantiate<LobbyBehaviour>(GameStartManager.Instance.LobbyPrefab);
            AmongUsClient.Instance.Spawn(LobbyBehaviour.Instance, -2, SpawnFlags.None);

            MalumMenu.notifications.Send("Lobby Map", "A new instance of the lobby map has been spawned", 5);
        }
        GUILayout.EndHorizontal();

        GUILayout.Label($"Selected map: {(MapNames)selectedMap}");
        selectedMap = (byte)GUILayout.HorizontalSlider(selectedMap, 0, 5);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Despawn Map"))
        {
            if (ShipStatus.Instance != null)
            {
                ShipStatus.Instance.Despawn();
                MalumMenu.notifications.Send("Game Map", "The current map has been despawned.", 5);
            }
            else
            {
                MalumMenu.notifications.Send("Game Map", "The game map has already been despawned.", 5);
            }
        }

        if (GUILayout.Button("Spawn Map"))
        {
            AmongUsClient.Instance.StartCoroutine(SpawnMap(selectedMap).WrapToIl2Cpp());
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.Label("Disco Party:");
        MalumMenu.routines.discoHost.Enabled = GUILayout.Toggle(MalumMenu.routines.discoHost.Enabled, "Enabled");
        GUILayout.Label($"Color randomization delay: {MalumMenu.routines.discoHost.randomizationDelay:F2}s");
        MalumMenu.routines.discoHost.randomizationDelay = GUILayout.HorizontalSlider(MalumMenu.routines.discoHost.randomizationDelay, 0.1f, 2.0f);
    }
    private static IEnumerator SpawnMap(byte mapId)
    {
        MalumMenu.Log.LogInfo($"Attempting to spawn in map id {mapId}");

        AsyncOperationHandle<GameObject> asyncHandle = AmongUsClient.Instance.ShipPrefabs[mapId].InstantiateAsync(null, false);
        yield return asyncHandle;

        ShipStatus ship = asyncHandle.Result.GetComponent<ShipStatus>();
        AmongUsClient.Instance.Spawn(ship, -2, SpawnFlags.None);

        MalumMenu.notifications.Send("Map Spawner", $"{(MapNames)mapId} has been spawned.", 5);
    }
}