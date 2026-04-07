using UnityEngine;
using System.Globalization;
using System.Collections.Generic;

namespace MalumMenu;

public class SettingsTab : ITab
{
    public string name => "Settings";

    private string _menuKeybindInput = "";
    private string _menuColorInput = "";
    private string _spoofLevelInput = "";
    private string _spoofPlatformInput = "";

    private bool _initialized = false;

    // Text field focus and cursor tracking
    private Dictionary<string, bool> _focusedFields = new();
    private Dictionary<string, float> _lastBlinkTime = new();
    private Dictionary<string, bool> _cursorVisible = new();
    private Dictionary<string, Rect> _fieldRects = new();
    private float _cursorBlinkTime = 0.5f;

    public void Draw()
    {
        if (!_initialized)
        {
            InitializeInputFields();
            _initialized = true;
        }

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGUISettings();

        GUILayout.Space(15);

        DrawSpoofingSettings();

        GUILayout.Space(15);

        DrawPrivacySettings();

        GUILayout.EndVertical();
    }

    private void InitializeInputFields()
    {
        _menuKeybindInput = MalumMenu.menuKeybind.Value;
        _menuColorInput = MalumMenu.menuHtmlColor.Value;
        _spoofLevelInput = MalumMenu.spoofLevel.Value;
        _spoofPlatformInput = MalumMenu.spoofPlatform.Value;

        // Initialize focus tracking
        _focusedFields["menuKeybind"] = false;
        _focusedFields["menuColor"] = false;
        _focusedFields["spoofLevel"] = false;
        _focusedFields["spoofPlatform"] = false;

        _cursorVisible["menuKeybind"] = true;
        _cursorVisible["menuColor"] = true;
        _cursorVisible["spoofLevel"] = true;
        _cursorVisible["spoofPlatform"] = true;

        _lastBlinkTime["menuKeybind"] = 0f;
        _lastBlinkTime["menuColor"] = 0f;
        _lastBlinkTime["spoofLevel"] = 0f;
        _lastBlinkTime["spoofPlatform"] = 0f;
    }

    private void HandleCustomTextField(ref string content, string fieldKey, int width = 200, int height = 20)
    {
        GUILayout.Box("", GUILayout.Width(width), GUILayout.Height(height));

        if (Event.current.type == EventType.Repaint)
        {
            _fieldRects[fieldKey] = GUILayoutUtility.GetLastRect();
        }

        if (!_focusedFields.ContainsKey(fieldKey))
        {
            _focusedFields[fieldKey] = false;
        }

        // Handle mouse click to focus
        if (Event.current.type == EventType.MouseDown && _fieldRects.ContainsKey(fieldKey))
        {
            if (_fieldRects[fieldKey].Contains(Event.current.mousePosition))
            {
                _focusedFields[fieldKey] = true;
                _lastBlinkTime[fieldKey] = Time.time;
                _cursorVisible[fieldKey] = true;
                Event.current.Use();
            }
            else
            {
                _focusedFields[fieldKey] = false;
            }
        }

        // Handle keyboard input
        if (_focusedFields[fieldKey] && Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == KeyCode.Backspace)
            {
                if (content.Length > 0)
                {
                    content = content.Substring(0, content.Length - 1);
                    Event.current.Use();
                }
            }
            else if (Event.current.character != '\0' && !char.IsControl(Event.current.character))
            {
                content += Event.current.character;
                Event.current.Use();
            }
        }

        // Display text content
        if (_fieldRects.ContainsKey(fieldKey))
        {
            GUI.Label(new Rect(_fieldRects[fieldKey].x + 5, _fieldRects[fieldKey].y + 2, _fieldRects[fieldKey].width - 10, _fieldRects[fieldKey].height), content);

            // Handle cursor blinking
            if (_focusedFields[fieldKey])
            {
                if (!_lastBlinkTime.ContainsKey(fieldKey))
                    _lastBlinkTime[fieldKey] = Time.time;

                if (Time.time - _lastBlinkTime[fieldKey] > _cursorBlinkTime)
                {
                    _cursorVisible[fieldKey] = !_cursorVisible[fieldKey];
                    _lastBlinkTime[fieldKey] = Time.time;
                }

                // Draw blinking cursor
                if (_cursorVisible[fieldKey])
                {
                    Vector2 textSize = GUI.skin.label.CalcSize(new GUIContent(content));
                    GUI.Label(new Rect(_fieldRects[fieldKey].x + textSize.x + 7, _fieldRects[fieldKey].y + 2, 10, _fieldRects[fieldKey].height - 4), "|");
                }
            }
        }
    }

    private void DrawGUISettings()
    {
        GUILayout.Label("GUI Settings", GUIStylePreset.TabSubtitle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Menu Keybind:", GUILayout.Width(150));
        HandleCustomTextField(ref _menuKeybindInput, "menuKeybind", 150);
        if (GUILayout.Button("Save", GUILayout.Width(100)))
        {
            MalumMenu.menuKeybind.Value = _menuKeybindInput;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Menu Color (HTML):", GUILayout.Width(150));
        HandleCustomTextField(ref _menuColorInput, "menuColor", 150);
        if (GUILayout.Button("Save", GUILayout.Width(100)))
        {
            MalumMenu.menuHtmlColor.Value = _menuColorInput;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        MalumMenu.menuOpenOnMouse.Value = GUILayout.Toggle(MalumMenu.menuOpenOnMouse.Value, " Open Menu on Mouse Position");

        GUILayout.Space(5);

        MalumMenu.autoLoadProfile.Value = GUILayout.Toggle(MalumMenu.autoLoadProfile.Value, " Auto-Load Profile on Startup");
    }

    private void DrawSpoofingSettings()
    {
        GUILayout.Label("Spoofing Settings", GUIStylePreset.TabSubtitle);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Spoof Level (1-100001):", GUILayout.Width(150));
        HandleCustomTextField(ref _spoofLevelInput, "spoofLevel", 150);
        if (GUILayout.Button("Save", GUILayout.Width(100)))
        {
            // Validate that it's a number between 1 and 100001
            if (int.TryParse(_spoofLevelInput, NumberStyles.Integer, CultureInfo.InvariantCulture, out int level) &&
                level >= 1 && level <= 100001)
            {
                MalumMenu.spoofLevel.Value = _spoofLevelInput;
            }
            else
            {
                _spoofLevelInput = MalumMenu.spoofLevel.Value;
            }
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.BeginHorizontal();
        GUILayout.Label("Spoof Platform:", GUILayout.Width(150));
        HandleCustomTextField(ref _spoofPlatformInput, "spoofPlatform", 150);
        if (GUILayout.Button("Save", GUILayout.Width(100)))
        {
            MalumMenu.spoofPlatform.Value = _spoofPlatformInput;
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(5);

        GUILayout.Label("Supported Platforms: StandaloneEpicPC, StandaloneSteamPC, StandaloneMac, StandaloneWin10, etc.");
    }

    private void DrawPrivacySettings()
    {
        GUILayout.Label("Privacy Settings", GUIStylePreset.TabSubtitle);

        MalumMenu.spoofDeviceId.Value = GUILayout.Toggle(MalumMenu.spoofDeviceId.Value, " Hide Device ID");

        GUILayout.Space(5);

        MalumMenu.noTelemetry.Value = GUILayout.Toggle(MalumMenu.noTelemetry.Value, " Disable Telemetry");

        GUILayout.Space(10);

        if (GUILayout.Button("Open Config File", GUILayout.Width(200)))
        {
            Utils.OpenConfigFile();
        }

        GUILayout.Space(5);

        GUILayout.Label("For more advanced configuration options, click 'Open Config File'", GUIStylePreset.TabSubtitle);
    }
}
