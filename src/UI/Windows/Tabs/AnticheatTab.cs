using MalumMenu.anticheat;
using UnityEngine;

namespace MalumMenu
{
    internal class AnticheatTab : ITab
    {
        public string name => "Anticheat";

        public void Draw()
        {
            Anticheat.Enabled = GUILayout.Toggle(Anticheat.Enabled, "Enable HyperMenu Anticheat");

            Anticheat.CheckSpoofedPlatforms = GUILayout.Toggle(Anticheat.CheckSpoofedPlatforms, "Flag Spoofed Platform Data");

            GUILayout.Space(5);
            GUILayout.Label("RPCs that should be checked by the anticheat:");
            foreach (var (rpcCall, handler) in Anticheat.RpcHandlers)
            {
                handler.Enabled = GUILayout.Toggle(handler.Enabled, $"{rpcCall}");
            }

            GUILayout.Space(5);
            GUILayout.Label("When a cheater is detected:");
            Anticheat.sendNotification = GUILayout.Toggle(Anticheat.sendNotification, "Send notification");
            Anticheat.discardRpc = GUILayout.Toggle(Anticheat.discardRpc, "Discard RPC");

            GUILayout.BeginHorizontal();
            GUILayout.Label($"Punish the player with: {Anticheat.punishment}");
            Anticheat.punishment = (Anticheat.Punishments)GUILayout.HorizontalSlider((float)Anticheat.punishment, 0, 3);
            GUILayout.EndHorizontal();
        }
    }
}