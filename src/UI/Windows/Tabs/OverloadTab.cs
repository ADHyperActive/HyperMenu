using System;
using UnityEngine;

namespace MalumMenu;

public class OverloadTab : ITab
{
    public string name => "Overload";

    private GUIStyle _sliderSubtitle;
    private int _maxStrength = 1000;
    private float _maxCooldown = 1f;
    private float _fpsEstimate = 0f;
    private float _rawCooldown;
    private float _rawStrength;

    public void Draw()
    {
        InitStyles();

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.425f));

        DrawGeneral();

        GUILayout.Space(15);

        DrawSettingsToggle();

        GUILayout.EndVertical();

        if (CheatToggles.showOverloadSettings)
        {
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(MenuUI.windowWidth * 0.75f));

            DrawSettingsSection();

            GUILayout.EndVertical();
        }
    }

    private void InitStyles()
    {
        if (_sliderSubtitle == null)
        {
            _sliderSubtitle = new(GUIStylePreset.TabSubtitle)
            {
                fontStyle = FontStyle.Normal
            };
        }
    }

    private void DrawGeneral()
    {
        CheatToggles.showOverload = GUILayout.Toggle(CheatToggles.showOverload, " Show Overload Menu");
    }

    private void DrawSettingsToggle()
    {
        GUILayout.Label("Settings", GUIStylePreset.TabSubtitle);

        CheatToggles.showOverloadSettings = GUILayout.Toggle(CheatToggles.showOverloadSettings, " Show Overload Settings");
    }

    private void DrawSettingsSection()
    {
        GUILayout.Space(15);

        GUILayout.BeginHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();

        CheatToggles.olAutoAdapt = GUILayout.Toggle(CheatToggles.olAutoAdapt, " Auto Adapt");

        int ping = Utils.GetPing();
        string pingStr = $"PING : {ping} ms";
        GUILayout.Label(Utils.GetColoredPingText(pingStr, ping));

        int strength = OverloadHandler.strength;
        float cooldown = OverloadHandler.cooldown;

        float numExecutionsPerSec;
        string extraStr = "";
        if (cooldown > Time.unscaledDeltaTime)
        {
            numExecutionsPerSec = 1f / cooldown;
        }
        else
        {
            float fps = Utils.GetFps();
            if (Math.Abs(fps - _fpsEstimate) > 5f)
            {
                _fpsEstimate = fps;
            }
            numExecutionsPerSec = (int)_fpsEstimate;
            extraStr = " (FPS Cap)";
        }

        int numTargetsPerSec = OverloadUI.currentTargets.Count <= numExecutionsPerSec ? OverloadUI.currentTargets.Count : (int)numExecutionsPerSec;

        int rpcPerTarget = numTargetsPerSec > 0 ? (int)(strength * numExecutionsPerSec / numTargetsPerSec) :
                                            (int)(strength * numExecutionsPerSec);

        if (CheatToggles.olShowRpcTotal)
        {
            CheatToggles.olShowRpcTotal = GUILayout.Toggle(CheatToggles.olShowRpcTotal, $" RPC/s : {rpcPerTarget*Math.Max(1, numTargetsPerSec)}{extraStr}");
        }
        else
        {
            CheatToggles.olShowRpcTotal = GUILayout.Toggle(CheatToggles.olShowRpcTotal, $" RPC/s : {rpcPerTarget}x{numTargetsPerSec}{extraStr}");
        }

        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        DrawSettingsSliders();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginVertical(GUILayout.Width(MenuUI.windowWidth * 0.35f));

        GUILayout.Label("General", GUIStylePreset.TabSubtitle);

        CheatToggles.olAutoStart = GUILayout.Toggle(CheatToggles.olAutoStart, " Auto Start when Ready");

        CheatToggles.olAutoStop = GUILayout.Toggle(CheatToggles.olAutoStop, " Auto Stop when Done");

        CheatToggles.olLockTargets = GUILayout.Toggle(CheatToggles.olLockTargets, " Lock Targets on Start");

        CheatToggles.olKillSwitch = GUILayout.Toggle(CheatToggles.olKillSwitch, " Kill Switch on Lag");

        if (CheatToggles.olKillSwitch)
        {
            Color standardBackgroundColor = GUI.backgroundColor;
            GUI.backgroundColor = Color.red;

            bool isPressed = GUILayout.Button($"{OverloadUI.killSwitchThreshold} ms", GUILayout.Width(70f));
            if (isPressed)
            {
                if (OverloadUI.killSwitchThreshold >= 3000)
                {
                    OverloadUI.killSwitchThreshold = 500;
                }
                else
                {
                    OverloadUI.killSwitchThreshold = OverloadUI.killSwitchThreshold + 500;
                }
            }

            GUI.backgroundColor = standardBackgroundColor;
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        GUILayout.Label("Logs", GUIStylePreset.TabSubtitle);

        CheatToggles.olLogStartStop = GUILayout.Toggle(CheatToggles.olLogStartStop, " Log START and STOP");

        CheatToggles.olLogAddRemove = GUILayout.Toggle(CheatToggles.olLogAddRemove, " Log ADD and REMOVE");

        CheatToggles.olLogAttack = GUILayout.Toggle(CheatToggles.olLogAttack, " Log Attack");

        CheatToggles.olLogDisconnect = GUILayout.Toggle(CheatToggles.olLogDisconnect, " Log Disconnect");

        CheatToggles.olVerboseLogs = GUILayout.Toggle(CheatToggles.olVerboseLogs, " Verbose Attack Logs");

        CheatToggles.olAutoClear = GUILayout.Toggle(CheatToggles.olAutoClear, " Auto Clear on Start");

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.Space(15);
    }

    private void DrawSettingsSliders()
    {
        GUILayout.Label($"Strength : {_rawStrength}", _sliderSubtitle);
        GUILayout.Space(1);

        GUILayout.BeginHorizontal();

        float inputStrength = GUILayout.HorizontalSlider(_rawStrength, 1, _maxStrength, GUILayout.Width(350f));

        if (inputStrength != _rawStrength)
        {
            CheatToggles.olAutoAdapt = false;
            _rawStrength = inputStrength;
        }

        GUILayout.Space(5);
        bool isPressedStrength = GUILayout.Button($"{_maxStrength}", GUILayout.Width(50f));

        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.Label($"Cooldown : {_rawCooldown:F2}", _sliderSubtitle);
        GUILayout.Space(1);

        GUILayout.BeginHorizontal();

        float inputCooldown = GUILayout.HorizontalSlider(_rawCooldown, 0f, _maxCooldown, GUILayout.Width(350f));

        if (inputCooldown != _rawCooldown)
        {
            CheatToggles.olAutoAdapt = false;
            _rawCooldown = inputCooldown;
        }

        GUILayout.Space(5);
        bool isPressedCooldown = GUILayout.Button($"{_maxCooldown:F0}", GUILayout.Width(50f));

        GUILayout.EndHorizontal();

        if (!CheatToggles.olAutoAdapt)
        {
            float strengthStep = _maxStrength / 100f;
            int clampStrength = Mathf.RoundToInt(Mathf.Clamp(Mathf.Round(_rawStrength / strengthStep) * strengthStep, 1, _maxStrength));
            OverloadHandler.strength = clampStrength;

            float cooldownStep = _maxCooldown / 100f;
            float clampCooldown = Mathf.Round(_rawCooldown / cooldownStep) * cooldownStep;
            OverloadHandler.cooldown = clampCooldown;
        }

        if (isPressedStrength)
        {
            if (_maxStrength >= 1000)
            {
                CheatToggles.olAutoAdapt = false;
                OverloadHandler.strength = Mathf.RoundToInt(OverloadHandler.strength/10f);
                _maxStrength = 100;
            }
            else
            {
                CheatToggles.olAutoAdapt = false;
                OverloadHandler.strength = OverloadHandler.strength*10;
                _maxStrength = _maxStrength*10;
            }
        }

        if (isPressedCooldown)
        {
            if (_maxCooldown >= 10f)
            {
                CheatToggles.olAutoAdapt = false;
                OverloadHandler.cooldown = OverloadHandler.cooldown/10f;
                _maxCooldown = 1f;
            }
            else
            {
                CheatToggles.olAutoAdapt = false;
                OverloadHandler.cooldown = OverloadHandler.cooldown*10;
                _maxCooldown = _maxCooldown*10;
            }
        }

        _rawStrength = OverloadHandler.strength;
        _rawCooldown = OverloadHandler.cooldown;
    }
}
