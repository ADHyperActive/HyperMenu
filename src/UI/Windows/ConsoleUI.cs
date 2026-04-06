using Il2CppSystem;
using UnityEngine;
using System.Collections.Generic;

namespace MalumMenu;

public class ConsoleUI : MonoBehaviour
{
    private static Vector2 _scrollPosition = Vector2.zero;
    private static List<string> _logEntries = new();
    private const int MaxLogEntries = 300;
    private Rect _windowRect = new(320, 10, 550, 350);
    private GUIStyle _logStyle;

    public static void Log(string message)
    {
        if (_logEntries.Count >= MaxLogEntries)
        {
            _logEntries.RemoveAt(0);
        }

        _logEntries.Add(message);
        _scrollPosition.y = float.MaxValue;
    }

    private void OnGUI()
    {
        if (!CheatToggles.showConsole || !MenuUI.isGUIActive || MalumMenu.isPanicked) return;

        _logStyle ??= new GUIStyle(GUI.skin.label)
        {
            fontSize = 14,
            padding = new RectOffset { left = 4, right = 4, top = 2, bottom = 2 },
            normal = { textColor = new Color(0.95f, 0.95f, 0.95f) }
        };

        UIHelpers.ApplyUIColor();

        _windowRect = GUI.Window((int)WindowId.ConsoleUI, _windowRect, (GUI.WindowFunction)ConsoleWindow, "Console");
    }

    private void ConsoleWindow(int windowID)
    {
        GUILayout.BeginVertical(GUI.skin.box);

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, false, false);

        foreach (var log in _logEntries)
        {
            GUILayout.Label(log, _logStyle);
        }

        GUILayout.EndScrollView();

        GUILayout.EndVertical();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Clear Log", GUILayout.Height(30), GUILayout.Width(120f)))
        {
            _logEntries.Clear();
        }

        if (GUILayout.Button("Copy to Clipboard", GUILayout.Height(30)))
        {
            GUIUtility.systemCopyBuffer = String.Join("\n", _logEntries.ToArray());
        }

        GUILayout.EndHorizontal();

        GUI.DragWindow();
    }
}
