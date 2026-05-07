using UnityEngine;
using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using InnerNet;
using System;
using System.Collections;

namespace MalumMenu;

public class PlayersTab : ITab
{
    public string name => "Players";

    private Vector2 _subsectionScrollVector = Vector2.zero;
    private Vector2 _subsectionScrollVector2 = Vector2.zero;
    private static CrewmateColor _selectedColor = CrewmateColor.Red;

    public void Draw()
    {
        if (PlayerControl.AllPlayerControls.Count == 0)
        {
            GUILayout.Label("There are currently no online players.");
            return;
        }

        GUILayout.BeginHorizontal();

        // Left panel: Player list
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.35f));
        _subsectionScrollVector = GUILayout.BeginScrollView(_subsectionScrollVector);
        DrawPlayerList();
        GUILayout.EndScrollView();
        GUILayout.EndVertical();

        // Right panel: Player controls
        if (PlayersSection.selectedPlayer != null)
        {
            GUILayout.BeginVertical();
            _subsectionScrollVector2 = GUILayout.BeginScrollView(_subsectionScrollVector2);
            DrawPlayerControls(PlayersSection.selectedPlayer);
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();
    }

    private void DrawPlayerList()
    {
        for (byte i = 0; i < PlayerControl.AllPlayerControls.Count; i++)
        {
            PlayerControl player = PlayerControl.AllPlayerControls[i];
            if (player.Data == null) continue;

            RenderPlayerSelection(i, player);
        }
    }

    private void RenderPlayerSelection(byte position, PlayerControl player)
    {
        string playerName = player.Data.PlayerName;
        playerName += $"\n<color=\"{GetRoleColor(player.Data.RoleType)}\">{player.Data.RoleType}</color>";

        bool isSelected = player == PlayersSection.selectedPlayer;
        GUIStyle style = GUI.skin.button;

        if (player.OwnerId == AmongUsClient.Instance.HostId)
        {
            style.normal.textColor = new Color(1.0f, 0.84f, 0.0f);
        }

        if (GUILayout.Button(playerName, style))
        {
            PlayersSection.selectedPlayer = player;
        }
    }

    private string GetRoleColor(RoleTypes role)
    {
        return RoleManager.IsImpostorRole(role) ? "red" : "#8afcfc";
    }

    private static void DrawPlayerControls(PlayerControl target)
    {
        if (target == null)
        {
            GUILayout.Label("Specified target is not valid.");
            return;
        }

        ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(target);
        if (clientData != null)
        {
            PlatformSpecificData platform = clientData.PlatformData;
            bool streamerMode = DataManager.Settings.Gameplay.StreamerMode;

            GUILayout.Label(
                $"Name: {target.Data.PlayerName} {target.Data.ColorName}" +
                $"\nRole: {target.Data.RoleType}" +
                $"\nState: " + (target.Data.IsDead ? "Dead" : "Alive") +
                $"\nFriendcode: " + (streamerMode ? "REDACTED" : target.Data.FriendCode) +
                $"\nPUID: " + (streamerMode ? "REDACTED" : target.Data.Puid) +
                $"\nLevel: {target.Data.PlayerLevel + 1}" +
                $"\nDevice: {platform.Platform}" +
                (target.OwnerId == AmongUsClient.Instance.HostId ? "\nHost: true" : "")
            );
        }
        else
        {
            GUILayout.Label(
                $"Name: {target.Data.PlayerName} {target.Data.ColorName}" +
                $"\nRole: {target.Data.RoleType}" +
                $"\nState: " + (target.Data.IsDead ? "Dead" : "Alive") +
                $"\nIs Dummy: true"
            );
        }

        if (GUILayout.Button("Teleport"))
        {
            Teleporter.TeleportTo(target.transform.position);
        }

        if (GUILayout.Button("Murder"))
        {
            if (AmongUsClient.Instance.AmHost)
            {
                MalumMenu.Log.LogInfo($"Attempting to kill {target.Data.PlayerName}, we are host so we are using the MurderPlayer RPC");
                PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
            }
            else
            {
                MalumMenu.Log.LogInfo($"Attempting to kill {target.Data.PlayerName}, we are not the host so we have to use the CheckMurder RPC");
                PlayerControl.LocalPlayer.CmdCheckMurder(target);
            }
        }

        if (GUILayout.Button("Copy Avatar"))
        {
            Utilities.CopyPlayer(target);
        }

        if (GUILayout.Button("Report Body"))
        {
            AttemptReportBody(target);
        }

        GUILayout.Space(5);
        GUILayout.Label("Host Only Features:" + (AmongUsClient.Instance.AmHost ? "" : "\n(Using these will get you kicked!)"));

        if (GUILayout.Button("Force Meeting As"))
        {
            if (Utils.isHost)
            {
                Utilities.OpenMeeting(target, null);
            } else
            {
                MalumMenu.notifications.Send("Meeting Forcer", "This is a host-only cheat.");
            }
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Force All Votes To"))
        {
            if (MeetingHud.Instance == null)
               {
                   MalumMenu.notifications.Send("Vote Forcer", "This option can only be used when there is an active meeting.");
            }
            else if (!Utils.isHost)
            {
                MalumMenu.notifications.Send("Vote Forcer", "This is a host-only cheat.");
            }
            else
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    PlayerVoteArea votingArea = MeetingHud.Instance.playerStates[player.PlayerId];
                    votingArea.SetVote(target.PlayerId);
                }

                MeetingHud.Instance.SetDirtyBit(1);
                MeetingHud.Instance.CheckForEndVoting();
            }
        }

        if (GUILayout.Button("Eject"))
        {
            if (!Utils.isHost)
            {
                MalumMenu.notifications.Send("Eject", "This is a host-only cheat.");
            } else
            {
                if (MeetingHud.Instance == null)
                {
                    MeetingHud.Instance = UnityEngine.Object.Instantiate<MeetingHud>(HudManager.Instance.MeetingPrefab);
                    AmongUsClient.Instance.Spawn(MeetingHud.Instance, -2, SpawnFlags.None);
                }

                MeetingHud.VoterState[] votes = Array.Empty<MeetingHud.VoterState>();
                MeetingHud.Instance.RpcVotingComplete(votes, target.Data, false);
                MeetingHud.Instance.RpcClose();
            }
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Frame Shapeshift"))
        {
            if (!Utils.isHost)
            {
                MalumMenu.notifications.Send("Frame Shapesift", "This is a host-only cheat.");
            } else
            {
                target.StartCoroutine(AttemptShapeshiftFrame(target).WrapToIl2Cpp());
            }
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Flood Player with Tasks"))
        {
            if (!Utils.isHost)
            {
                MalumMenu.notifications.Send("Task Flooder", "This is a host-only cheat.");
            }
            else
            {
                byte[] taskIds = new byte[255];
                for (byte i = 0; i < 255; i++)
                {
                    taskIds[i] = i;
                }
                target.Data.RpcSetTasks(taskIds);
            }
        }

        if (GUILayout.Button("Clear Tasks"))
        {
            if (!Utils.isHost)
            {
                MalumMenu.notifications.Send("Clear Tasks", "This is a host-only cheat.");
            } else
            {
                target.Data.RpcSetTasks(Array.Empty<byte>());
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);
        GUILayout.Label("Game Options Modifier:");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Blind"))
        {
            IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
            gameOptions.SetFloat(FloatOptionNames.CrewLightMod, -1.0f);
            gameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, -1.0f);
            GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
        }

        if (GUILayout.Button("Fullbright"))
        {
            IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
            gameOptions.SetFloat(FloatOptionNames.CrewLightMod, 1000f);
            gameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, 1000f);
            GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Slow Speed"))
        {
            IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
            gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, 0.1f);
            GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
        }

        if (GUILayout.Button("Super Speed"))
        {
            IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
            gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, 3.0f);
            GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Reset to Defaults"))
        {
            IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
            GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
        }

        GUILayout.Space(5);
        GUILayout.Label($"Change color to: {_selectedColor}");
        _selectedColor = (CrewmateColor)GUILayout.HorizontalSlider((float)_selectedColor, 0, 17);

        if (GUILayout.Button("Set Color"))
        {
            target.RpcSetColor((byte)_selectedColor);
        }
    }

    private static void AttemptReportBody(PlayerControl target)
    {
        if (AmongUsClient.Instance.AmHost)
        {
            MalumMenu.Log.LogInfo($"Attempting to report {target.Data.PlayerName}'s body, we are the host so we directly use the StartMeeting RPC");
            Utilities.OpenMeeting(PlayerControl.LocalPlayer, target.Data);
            return;
        }

        MalumMenu.Log.LogInfo($"Attempting to report {target.Data.PlayerName}'s body, we are not the host so we have to use the ReportDeadBody RPC");

        bool hasAnticheat = AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame && !Constants.IsVersionModded();
        if (hasAnticheat)
        {
             if (LobbyBehaviour.Instance != null)
            {
                MalumMenu.notifications.Send("Report Body", "The game must have started for this option to work.");
                return;
            }

             if (!target.Data.IsDead)
            {
                MalumMenu.notifications.Send("Report Body", "You can only report bodies of players who have died in this round.");
                return;
            }

            bool bodyExists = false;
            foreach (Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(0, 0), 99999f, Constants.PlayersOnlyMask))
            {
                if (collider.tag != "DeadBody") continue;

                DeadBody bodyComponent = collider.GetComponent<DeadBody>();
                if (bodyComponent && bodyComponent.ParentId == target.PlayerId)
                {
                    bodyExists = true;
                    break;
                }
            }

             if (!bodyExists)
            {
                MalumMenu.notifications.Send("Report Body", "Unable to find a dead body for this player, you can only report a player's body if they have died this round and their body has not dissolved.");
                return;
            }
        }

        MalumMenu.Log.LogInfo($"All checks passed, we are able to report {target.Data.PlayerName}'s body.");

        PlayerControl.LocalPlayer.CmdReportDeadBody(target.Data);
    }

    private static IEnumerator AttemptShapeshiftFrame(PlayerControl target)
    {
        bool hasAnticheat = AmongUsClient.Instance.NetworkMode == NetworkModes.OnlineGame && !Constants.IsVersionModded();

         if (ShipStatus.Instance == null && hasAnticheat)
        {
            MalumMenu.notifications.Send("Framer", "The game must have started for this option to work.");
            yield break;
        }

        PlayerControl randomPl = Utilities.GetRandomPlayer(false, false, false, false);

        if (target.Data.RoleType != RoleTypes.Shapeshifter && hasAnticheat)
        {
            RoleTypes currentRole = target.Data.RoleType;

            target.RpcSetRole(RoleTypes.Shapeshifter, true);
            yield return Effects.Wait(0.5f);
            target.RpcShapeshift(randomPl, true);
            target.RpcSetRole(currentRole, true);
        }
        else
        {
            target.RpcShapeshift(randomPl, true);
        }
    }
}

public static class PlayersSection
{
    public static PlayerControl selectedPlayer;
}
