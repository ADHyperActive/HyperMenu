using UnityEngine;

namespace MalumMenu;

public class ModesTab : ITab
{
    public string name => "Modes";

    public void Draw()
    {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
    }

    private void DrawGeneral()
    {
        CheatToggles.rgbMode = GUILayout.Toggle(CheatToggles.rgbMode, " RGB Mode");

        CheatToggles.stealthMode = GUILayout.Toggle(CheatToggles.stealthMode, " Stealth Mode");

        CheatToggles.panicMode = GUILayout.Toggle(CheatToggles.panicMode, " Panic Mode");
    }
}
