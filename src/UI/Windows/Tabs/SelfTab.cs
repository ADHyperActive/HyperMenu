using System.Collections;
using UnityEngine;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using MalumMenu.features;

namespace MalumMenu
{
    internal class SelfTab : ITab
    {
        public string name => "Roles";

        private uint level = 0;

        public void Draw()
        {
            GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));
            if (PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
            {
                GUILayout.Label("You are not currently in a game, these options will not work.");
            }
            else
            {
                GUILayout.Label($"Role: {PlayerControl.LocalPlayer.Data.RoleType}");
            }

            // Self.BypassIntentionalDisconnectionBlocks.Enabled = GUILayout.Toggle(Self.BypassIntentionalDisconnectionBlocks.Enabled, "Bypass intentional disconnection temp bans");
            Self.UpdateStatsFreeplay.Enabled = GUILayout.Toggle(Self.UpdateStatsFreeplay.Enabled, "Update Stats in Freeplay");
            Immortality.Enabled = GUILayout.Toggle(Immortality.Enabled, "Become Immortal");
            Self.AlwaysShowTaskAnimations = GUILayout.Toggle(Self.AlwaysShowTaskAnimations, "Always Show Task Animations");
            Self.NoLadderCooldown.Enabled = GUILayout.Toggle(Self.NoLadderCooldown.Enabled, "No Ladder Cooldown");
            Self.UnlimitedMeetings.enabled = GUILayout.Toggle(Self.UnlimitedMeetings.enabled, "Unlimited Meetings");

            if (GUILayout.Button("Call Meeting"))
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    MalumMenu.Log.LogInfo("We are the host, we can force a meeting");
                    Utilities.OpenMeeting(PlayerControl.LocalPlayer, null);
                }
                else
                {
                    PlayerControl.LocalPlayer.CmdReportDeadBody(null);
                }
            }

            if (GUILayout.Button("Complete All Tasks"))
            {
                PlayerControl.LocalPlayer.StartCoroutine(CompleteAllTasks().WrapToIl2Cpp());
            }

            if (GUILayout.Button("Randomize Avatar"))
            {
                if (AmongUsClient.Instance.AmConnected)
                {
                    Utilities.RandomizePlayer(true);

                    MalumMenu.notifications.Send("Player Randomizer", "Your avatar has been randomized for this game.", 5);
                }
                else
                {
                    AccountManager.Instance.RandomizeName();
                    Utilities.RandomizePlayer();

                    MalumMenu.notifications.Send("Player Randomizer", "Your name and avatar has been randomized.", 5);
                }
            }

            GUILayout.Label("Task Animations:");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Medbay Scan"))
            {
                Network.SendSetScanner(true);
            }

            if (GUILayout.Button("Finish Medbay Scan"))
            {
                Network.SendSetScanner(false);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear Asteroids"))
            {
                Network.SendPlayAnimation((byte)TaskTypes.ClearAsteroids);
            }

            if (GUILayout.Button("Empty Garbage"))
            {
                Network.SendPlayAnimation((byte)TaskTypes.EmptyGarbage);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Prime Shields"))
            {
                Network.SendPlayAnimation((byte)TaskTypes.PrimeShields);
            }

            GUILayout.Space(5);
            GUILayout.Label($"Update level to: {level + 1}");
            level = (uint)GUILayout.HorizontalSlider(level, 0, 199);

            if (GUILayout.Button("Send Level Update"))
            {
                PlayerControl.LocalPlayer.RpcSetLevel(level);
                MalumMenu.notifications.Send("Level Updater", $"Your level has been changed to {level + 1}", 5);
            }
            GUILayout.EndVertical();
        }
        private IEnumerator CompleteAllTasks()
        {
            Il2CppSystem.Collections.Generic.List<PlayerTask> allTasks = PlayerControl.LocalPlayer.myTasks;

            MalumMenu.Log.LogInfo("Completing all tasks...");
            foreach (PlayerTask task in allTasks)
            {
                if (task.IsComplete)
                {
                    MalumMenu.Log.LogInfo($"Task {task.Id} has already been completed, skipping");
                    continue;
                }

                MalumMenu.Log.LogInfo($"Sent CompleteTask RPC for task {task.Id}");
                PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);

                // If we want to complete more than six tasks then a delay needs to be implemented
                // otherwise the vanilla anticheat will kick us for violating ratelimits
                yield return Effects.Wait(0.05f);
            }

            MalumMenu.notifications.Send("Task Finisher", "All your tasks have been finished.", 5);
        }

    }
}
