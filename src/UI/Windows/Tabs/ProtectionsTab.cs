using UnityEngine;
using MalumMenu.features;

namespace MalumMenu;

public class ProtectionsTab : ITab
{
    public string name => "Protections";

    public void Draw()
    {
        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        // Network
        Protections.ForceDTLS.Enabled = GUILayout.Toggle(Protections.ForceDTLS.Enabled, "Force enable DTLS to encrypt network data");

        Protections.BlockServerTeleports.Enabled = GUILayout.Toggle(Protections.BlockServerTeleports.Enabled, "Block position updates from server");

        // Overloads
        Protections.HardenedReadPackedUInt.Enabled = GUILayout.Toggle(Protections.HardenedReadPackedUInt.Enabled, "Use hardened packed int deserializer");
        Protections.BlockInvalidVentOverload = GUILayout.Toggle(Protections.BlockInvalidVentOverload, "Protect against invalid vent overload");
        Protections.BlockInvalidLadderOverload = GUILayout.Toggle(Protections.BlockInvalidLadderOverload, "Protect against invalid ladder overload");

        Protections.Votekicks.Enabled = GUILayout.Toggle(Protections.Votekicks.Enabled, "Prevent being votekicked as host");

        GUILayout.EndVertical();
    }
}