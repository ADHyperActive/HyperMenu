using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace MalumMenu;

public class MenuUI : MonoBehaviour
{
    public List<GroupInfo> groups = new List<GroupInfo>();
    private Rect windowRect = new(10, 10, 720, 570);
    public static bool isGUIActive = false;
    public int selectedTab;

    // Styles
    private GUIStyle tabButtonStyle;
    private GUIStyle tabButtonActiveStyle;
    public GUIStyle tabTitleStyle;
    public GUIStyle tabSubtitleStyle;
    public static float hue; // For RGB mode
    private GUIStyle windowStyle;

    // Config input fields
    private Vector2 configScrollPosition = Vector2.zero;
    private Vector2 contentScrollPosition = Vector2.zero;

    // Keybind popup state
    private bool showKeybindPopup = false;
    private string keybindTargetToggle = "";
    private string keybindTargetLabel = "";
    private bool waitingForKey = false;
    private Rect keybindPopupRect = new(200, 200, 300, 150);

    // Platform spoofing options
    private readonly string[] platformOptions = new string[] 
    { 
        "Disabled", "Unknown", "StandaloneEpicPC", "StandaloneSteamPC", 
        "StandaloneMac", "StandaloneWin10", "StandaloneItch", 
        "IPhone", "Android", "Switch", "Xbox", "Playstation" 
    };
    private int selectedPlatformIndex = 0;
    private uint spoofLevelValue = 0;
    private bool spoofLevelEnabled = false;

    // Create all groups (buttons) and their toggles on start
    private void Start()
    {
        groups.Add(new GroupInfo("Player", false,
            new List<ToggleInfo>() {
                new ToggleInfo(" NoClip", () => CheatToggles.noClip, x => CheatToggles.noClip = x),
                new ToggleInfo(" Fake Revive", () => CheatToggles.fakeRevive, x => CheatToggles.fakeRevive = x),
                new ToggleInfo(" Invert Controls", () => CheatToggles.invertControls, x => CheatToggles.invertControls = x)
            },
            new List<SubmenuInfo> {
                new SubmenuInfo("Teleport", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" to Cursor", () => CheatToggles.teleportCursor, x => CheatToggles.teleportCursor = x),
                        new ToggleInfo(" to Player", () => CheatToggles.teleportPlayer, x => CheatToggles.teleportPlayer = x)
                    }
                )
            }
        ));

        groups.Add(new GroupInfo("ESP", false,
            new List<ToggleInfo>() {
                new ToggleInfo(" Show Player Info", () => CheatToggles.showPlayerInfo, x => CheatToggles.showPlayerInfo = x),
                new ToggleInfo(" See Roles", () => CheatToggles.seeRoles, x => CheatToggles.seeRoles = x),
                new ToggleInfo(" See Ghosts", () => CheatToggles.seeGhosts, x => CheatToggles.seeGhosts = x),
                new ToggleInfo(" No Shadows", () => CheatToggles.fullBright, x => CheatToggles.fullBright = x),
                new ToggleInfo(" Task Arrows", () => CheatToggles.taskArrows, x => CheatToggles.taskArrows = x),
                new ToggleInfo(" Reveal Votes", () => CheatToggles.revealVotes, x => CheatToggles.revealVotes = x),
                new ToggleInfo(" Show Lobby Info", () => CheatToggles.showLobbyInfo, x => CheatToggles.showLobbyInfo = x),
            },
            new List<SubmenuInfo> {
                new SubmenuInfo("Camera", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Zoom Out", () => CheatToggles.zoomOut, x => CheatToggles.zoomOut = x),
                        new ToggleInfo(" Spectate", () => CheatToggles.spectate, x => CheatToggles.spectate = x),
                        new ToggleInfo(" Freecam", () => CheatToggles.freecam, x => CheatToggles.freecam = x)
                    }
                ),
                new SubmenuInfo("Tracers", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Crewmates", () => CheatToggles.tracersCrew, x => CheatToggles.tracersCrew = x),
                        new ToggleInfo(" Impostors", () => CheatToggles.tracersImps, x => CheatToggles.tracersImps = x),
                        new ToggleInfo(" Ghosts", () => CheatToggles.tracersGhosts, x => CheatToggles.tracersGhosts = x),
                        new ToggleInfo(" Dead Bodies", () => CheatToggles.tracersBodies, x => CheatToggles.tracersBodies = x),
                        new ToggleInfo(" Color-based", () => CheatToggles.colorBasedTracers, x => CheatToggles.colorBasedTracers = x),
                        new ToggleInfo(" Distance-based", () => CheatToggles.distanceBasedTracers, x => CheatToggles.distanceBasedTracers = x)
                    }
                ),
                new SubmenuInfo("Minimap", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Crewmates", () => CheatToggles.mapCrew, x => CheatToggles.mapCrew = x),
                        new ToggleInfo(" Impostors", () => CheatToggles.mapImps, x => CheatToggles.mapImps = x),
                        new ToggleInfo(" Ghosts", () => CheatToggles.mapGhosts, x => CheatToggles.mapGhosts = x),
                        new ToggleInfo(" Color-based", () => CheatToggles.colorBasedMap, x => CheatToggles.colorBasedMap = x)
                    }
                )
            }
        ));

        groups.Add(new GroupInfo("Roles", false,
            new List<ToggleInfo>() {
                new ToggleInfo(" Set Fake Role", () => CheatToggles.changeRole, x => CheatToggles.changeRole = x)
            },
            new List<SubmenuInfo> {
                new SubmenuInfo("Impostor", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Kill Reach", () => CheatToggles.killReach, x => CheatToggles.killReach = x),
                        //new ToggleInfo(" Do Tasks", () => CheatToggles.impostorTasks, x => CheatToggles.impostorTasks = x)
                    }
                ),
                new SubmenuInfo("Shapeshifter", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" No Ss Animation", () => CheatToggles.noShapeshiftAnim, x => CheatToggles.noShapeshiftAnim = x),
                        new ToggleInfo(" Endless Ss Duration", () => CheatToggles.endlessSsDuration, x => CheatToggles.endlessSsDuration = x)
                    }
                ),
                new SubmenuInfo("Crewmate", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Show Tasks Menu", () => CheatToggles.showTasksMenu, x => CheatToggles.showTasksMenu = x)
                    }
                ),
                new SubmenuInfo("Tracker", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Endless Tracking", () => CheatToggles.endlessTracking, x => CheatToggles.endlessTracking = x),
                        new ToggleInfo(" No Track Delay", () => CheatToggles.noTrackingDelay, x => CheatToggles.noTrackingDelay = x),
                        new ToggleInfo(" No Track Cooldown", () => CheatToggles.noTrackingCooldown, x => CheatToggles.noTrackingCooldown = x),
                        new ToggleInfo(" Track Reach", () => CheatToggles.trackReach, x => CheatToggles.trackReach = x)
                    }
                ),

                new SubmenuInfo("Engineer", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Endless Vent Time", () => CheatToggles.endlessVentTime, x => CheatToggles.endlessVentTime = x),
                        new ToggleInfo(" No Vent Cooldown", () => CheatToggles.noVentCooldown, x => CheatToggles.noVentCooldown = x)
                    }
                ),

                new SubmenuInfo("Scientist", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Endless Battery", () => CheatToggles.endlessBattery, x => CheatToggles.endlessBattery = x),
                        new ToggleInfo(" No Vitals Cooldown", () => CheatToggles.noVitalsCooldown, x => CheatToggles.noVitalsCooldown = x)
                    }
                ),

                new SubmenuInfo("Detective", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Interrogate Reach", () => CheatToggles.interrogateReach, x => CheatToggles.interrogateReach = x)
                    }
                )
            }
        ));

        groups.Add(new GroupInfo("Ship", false,
            new List<ToggleInfo>() {
                new ToggleInfo(" Unfixable Lights", () => CheatToggles.unfixableLights, x => CheatToggles.unfixableLights = x),
                //new ToggleInfo(" Report Body", () => CheatToggles.reportBody, x => CheatToggles.reportBody = x),
                new ToggleInfo(" Call Meeting", () => CheatToggles.callMeeting, x => CheatToggles.callMeeting = x),
                new ToggleInfo(" Close Meeting", () => CheatToggles.closeMeeting, x => CheatToggles.closeMeeting = x),
                new ToggleInfo(" Auto-Open Doors On Use", () => CheatToggles.autoOpenDoorsOnUse, x => CheatToggles.autoOpenDoorsOnUse = x)
            },
            new List<SubmenuInfo> {
                new SubmenuInfo("Sabotage", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Reactor", () => CheatToggles.reactorSab, x => CheatToggles.reactorSab = x),
                        new ToggleInfo(" Oxygen", () => CheatToggles.oxygenSab, x => CheatToggles.oxygenSab = x),
                        new ToggleInfo(" Lights", () => CheatToggles.elecSab, x => CheatToggles.elecSab = x),
                        new ToggleInfo(" Comms", () => CheatToggles.commsSab, x => CheatToggles.commsSab = x),
                        new ToggleInfo(" Show Doors Menu", () => CheatToggles.showDoorsMenu, x => CheatToggles.showDoorsMenu = x),
                        new ToggleInfo(" Mushroom Mixup", () => CheatToggles.mushSab, x => CheatToggles.mushSab = x),
                        new ToggleInfo(" Trigger Spores", () => CheatToggles.mushSpore, x => CheatToggles.mushSpore = x),
                        new ToggleInfo(" Open Sabotage Map", () => CheatToggles.sabotageMap, x => CheatToggles.sabotageMap = x)
                    }
                ),
                new SubmenuInfo("Vents", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Unlock Vents", () => CheatToggles.useVents, x => CheatToggles.useVents = x),
                        new ToggleInfo(" Kick All From Vents", () => CheatToggles.kickVents, x => CheatToggles.kickVents = x),
                        new ToggleInfo(" Walk In Vents", () => CheatToggles.walkVent, x => CheatToggles.walkVent = x)
                    }
                )
            }
        ));

        groups.Add(new GroupInfo("Chat", false,
            new List<ToggleInfo>() {
                new ToggleInfo(" Enable Chat", () => CheatToggles.alwaysChat, x => CheatToggles.alwaysChat = x),
                new ToggleInfo(" Bypass URL Block", () => CheatToggles.bypassUrlBlock, x => CheatToggles.bypassUrlBlock = x),
                new ToggleInfo(" Lower Rate Limits", () => CheatToggles.lowerRateLimits, x => CheatToggles.lowerRateLimits = x),
            },
            new List<SubmenuInfo>() {
                new SubmenuInfo("Textbox", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Unlock Extra Characters", () => CheatToggles.unlockCharacters, x => CheatToggles.unlockCharacters = x),
                        new ToggleInfo(" Allow Longer Messages", () => CheatToggles.longerMessages, x => CheatToggles.longerMessages = x),
                        new ToggleInfo(" Unlock Clipboard", () => CheatToggles.unlockClipboard, x => CheatToggles.unlockClipboard = x)
                    }
                )
            }
        ));

        groups.Add(new GroupInfo("Console", false,
            new List<ToggleInfo>() {
                new ToggleInfo(" Show Console", () => CheatToggles.showConsole, x => CheatToggles.showConsole = x),
                new ToggleInfo(" Log Deaths", () => CheatToggles.logDeaths, x => CheatToggles.logDeaths = x),
                new ToggleInfo(" Log Shapeshifts", () => CheatToggles.logShapeshifts, x => CheatToggles.logShapeshifts = x),
                new ToggleInfo(" Log Vents", () => CheatToggles.logVents, x => CheatToggles.logVents = x),
            },
            new List<SubmenuInfo>()
        ));

        groups.Add(new GroupInfo("Host-Only", false,
            new List<ToggleInfo>() {
                new ToggleInfo(" Kill While Vanished", () => CheatToggles.killVanished, x => CheatToggles.killVanished = x),
                new ToggleInfo(" Kill Anyone", () => CheatToggles.killAnyone, x => CheatToggles.killAnyone = x),
                new ToggleInfo(" No Kill Cooldown", () => CheatToggles.zeroKillCd, x => CheatToggles.zeroKillCd = x),
                new ToggleInfo(" Show Protect Menu", () => CheatToggles.showProtectMenu, x => CheatToggles.showProtectMenu = x),
                //new ToggleInfo(" Force Role", () => CheatToggles.showRolesMenu, x => CheatToggles.showRolesMenu = x),
                //new ToggleInfo(" No Options Limits", () => CheatToggles.noOptionsLimits, x => CheatToggles.noOptionsLimits = x)
            },
            new List<SubmenuInfo>() {
                new SubmenuInfo("Murder", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Kill Player", () => CheatToggles.killPlayer, x => CheatToggles.killPlayer = x),
                        new ToggleInfo(" Telekill Player", () => CheatToggles.telekillPlayer, x => CheatToggles.telekillPlayer = x),
                        new ToggleInfo(" Kill All Crewmates", () => CheatToggles.killAllCrew, x => CheatToggles.killAllCrew = x),
                        new ToggleInfo(" Kill All Impostors", () => CheatToggles.killAllImps, x => CheatToggles.killAllImps = x),
                        new ToggleInfo(" Kill Everyone", () => CheatToggles.killAll, x => CheatToggles.killAll = x)
                    }),
                new SubmenuInfo("Game State", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Force Start Game", () => CheatToggles.forceStartGame, x => CheatToggles.forceStartGame = x),
                        new ToggleInfo(" No Game End", () => CheatToggles.noGameEnd, x => CheatToggles.noGameEnd = x)
                    }
                ),
                new SubmenuInfo("Meetings", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Skip Meeting", () => CheatToggles.skipMeeting, x => CheatToggles.skipMeeting = x),
                        new ToggleInfo(" Vote Immune", () => CheatToggles.voteImmune, x => CheatToggles.voteImmune = x),
                        new ToggleInfo(" Eject Player", () => CheatToggles.ejectPlayer, x => CheatToggles.ejectPlayer = x),
                    }
                )
            }
        ));

        groups.Add(new GroupInfo("Passive", false,
            new List<ToggleInfo>() {
                new ToggleInfo(" Free Cosmetics", () => CheatToggles.freeCosmetics, x => CheatToggles.freeCosmetics = x),
                new ToggleInfo(" Avoid Penalties", () => CheatToggles.avoidBans, x => CheatToggles.avoidBans = x),
                new ToggleInfo(" Unlock Extra Features", () => CheatToggles.unlockFeatures, x => CheatToggles.unlockFeatures = x),
                new ToggleInfo(" Copy Lobby Code on Disconnect", () => CheatToggles.copyLobbyCodeOnDisconnect, x => CheatToggles.copyLobbyCodeOnDisconnect = x),
                new ToggleInfo(" Spoof Date to April 1st", () => CheatToggles.spoofAprilFoolsDate, x => CheatToggles.spoofAprilFoolsDate = x),
                new ToggleInfo(" Stealth Mode", () => CheatToggles.stealthMode, x => CheatToggles.stealthMode = x),
                new ToggleInfo(" Panic (Disable MalumMenu)", () => CheatToggles.panic, x => CheatToggles.panic = x)
            },
            new List<SubmenuInfo>()
        ));

        groups.Add(new GroupInfo("Animations", false,
            new List<ToggleInfo>() {
                new ToggleInfo(" Shields", () => CheatToggles.animShields, x => CheatToggles.animShields = x),
                new ToggleInfo(" Asteroids", () => CheatToggles.animAsteroids, x => CheatToggles.animAsteroids = x),
                new ToggleInfo(" Empty Garbage", () => CheatToggles.animEmptyGarbage, x => CheatToggles.animEmptyGarbage = x),
                new ToggleInfo(" Medbay Scan", () => CheatToggles.animScan, x => CheatToggles.animScan = x),
                new ToggleInfo(" Cams In Use", () => CheatToggles.animCamsInUse, x => CheatToggles.animCamsInUse = x),
                //new ToggleInfo(" Pet", () => CheatToggles.animPet, x => CheatToggles.animPet = x)
            },
            new List<SubmenuInfo>() {
                new SubmenuInfo("Client-Sided", false,
                    new List<ToggleInfo>() {
                        new ToggleInfo(" Moonwalk", () => CheatToggles.moonWalk, x => CheatToggles.moonWalk = x)
                    }
                )
            }
        ));

        groups.Add(new GroupInfo("Config", false,
            new List<ToggleInfo>() {
                new ToggleInfo(" Reload Config", () => CheatToggles.reloadConfig, x => CheatToggles.reloadConfig = x),
                new ToggleInfo(" Save to Profile", () => false, x => CheatToggles.SaveTogglesToProfile()),
                new ToggleInfo(" Load from Profile", () => false, x => CheatToggles.LoadTogglesFromProfile())
            },
            new List<SubmenuInfo>()
        ));
    }

    public void InitStyles()
    {
        GUI.skin.toggle.fontSize = GUI.skin.button.fontSize = GUI.skin.label.fontSize = 15;

        var buttonBg = MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f, 0.8f));
        var buttonActiveBg = MakeTex(2, 2, new Color(0.35f, 0.4f, 0.5f, 0.95f));
        var buttonHoverBg = MakeTex(2, 2, new Color(0.3f, 0.3f, 0.35f, 0.9f));

        tabButtonStyle = new GUIStyle(GUI.skin.button)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleCenter,
            normal = { background = buttonBg, textColor = new Color(0.85f, 0.85f, 0.85f) },
            hover = { background = buttonHoverBg, textColor = Color.white },
            padding = new RectOffset { left = 8, right = 8, top = 8, bottom = 8 },
            margin = new RectOffset { left = 2, right = 2, top = 3, bottom = 3 }
        };

        tabButtonActiveStyle = new GUIStyle(tabButtonStyle)
        {
            normal = { background = buttonActiveBg, textColor = Color.white },
            hover = { background = buttonActiveBg, textColor = Color.white },
            fontStyle = FontStyle.Bold
        };

        tabTitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 22,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = Color.white },
            padding = new RectOffset { left = 0, right = 0, top = 5, bottom = 10 }
        };

        tabSubtitleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 16,
            fontStyle = FontStyle.Bold,
            alignment = TextAnchor.MiddleLeft,
            normal = { textColor = new Color(0.75f, 0.85f, 1f) },
            padding = new RectOffset { left = 0, right = 0, top = 8, bottom = 5 }
        };

        windowStyle = new GUIStyle(GUI.skin.window)
        {
            padding = new RectOffset { left = 10, right = 10, top = 25, bottom = 10 }
        };
    }

    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

    private void Update()
    {

        if (Input.GetKeyDown(Utils.StringToKeycode(MalumMenu.menuKeybind.Value)))
        {
            // Enable or disable GUI with DELETE key
            isGUIActive = !isGUIActive;

            if (MalumMenu.menuOpenOnMouse.Value)
            {
                // Teleport the window to the mouse for immediate use
                Vector2 mousePosition = Input.mousePosition;
                windowRect.position = new Vector2(mousePosition.x, Screen.height - mousePosition.y);
            }
        }

        if (CheatToggles.rgbMode)
        {
            hue += Time.deltaTime * 0.3f; // Adjust speed of color change, higher multiplier = faster
            if (hue > 1f) hue -= 1f; // Loop hue back to 0 when it exceeds 1
        }

        if (CheatToggles.stealthMode != MalumMenu.inStealthMode)
        {
            MalumMenu.inStealthMode = CheatToggles.stealthMode;

            Scene scene = SceneManager.GetActiveScene();

            if (scene.name == "MainMenu")
            {
                SceneManager.LoadScene(scene.name);
            }
        }

        if (CheatToggles.panic) Utils.Panic();

        var stamp = ModManager.Instance.ModStamp;
        if (stamp) stamp.enabled = false;

        // Passive cheats are always on to avoid problems
        // CheatToggles.unlockFeatures = CheatToggles.freeCosmetics = CheatToggles.avoidBans = true;

        // Some cheats only work if the LocalPlayer exists, so they are turned off if it does not
        if(!Utils.isPlayer)
        {
            CheatToggles.changeRole = CheatToggles.killAll = CheatToggles.telekillPlayer = CheatToggles.killAllCrew = CheatToggles.killAllImps = CheatToggles.teleportCursor = CheatToggles.teleportPlayer = CheatToggles.spectate = CheatToggles.freecam = CheatToggles.killPlayer = false;
        }

        if(!Utils.isHost && !Utils.isFreePlay)
        {
            CheatToggles.killAll = CheatToggles.telekillPlayer = CheatToggles.killAllCrew = CheatToggles.killAllImps = CheatToggles.killPlayer = CheatToggles.ejectPlayer = CheatToggles.zeroKillCd = CheatToggles.killAnyone = CheatToggles.killVanished = CheatToggles.forceStartGame = CheatToggles.noGameEnd = CheatToggles.skipMeeting = false;
        }

        // Host-only cheats are turned off if LocalPlayer is not the game's host
        // if(!CheatChecks.isHost){
        //     CheatToggles.voteImmune = CheatToggles.godMode = CheatToggles.impostorHack = CheatToggles.evilVote = false;
        // }

        // Some cheats only work if the ship exists, so they are turned off if it does not
        if(!Utils.isShip)
        {
            CheatToggles.fakeRevive = CheatToggles.sabotageMap = CheatToggles.unfixableLights = CheatToggles.completeMyTasks = CheatToggles.kickVents = CheatToggles.reportBody = CheatToggles.ejectPlayer = CheatToggles.closeMeeting = CheatToggles.skipMeeting = CheatToggles.reactorSab = CheatToggles.oxygenSab = CheatToggles.commsSab = CheatToggles.elecSab = CheatToggles.mushSab = CheatToggles.closeAllDoors = CheatToggles.openAllDoors = CheatToggles.spamCloseAllDoors = CheatToggles.spamOpenAllDoors = CheatToggles.autoOpenDoorsOnUse = CheatToggles.mushSpore = CheatToggles.animShields = CheatToggles.animAsteroids = CheatToggles.animEmptyGarbage = CheatToggles.animScan = CheatToggles.animCamsInUse = false;
        }

        if(!Utils.isHost && !Utils.isFreePlay)
        {
            CheatToggles.skipMeeting = CheatToggles.voteImmune = CheatToggles.ejectPlayer = CheatToggles.forceStartGame = CheatToggles.noGameEnd = CheatToggles.killAll = CheatToggles.killAllCrew = CheatToggles.killAllImps = CheatToggles.killAnyone = CheatToggles.killPlayer = CheatToggles.telekillPlayer = CheatToggles.killVanished = CheatToggles.zeroKillCd = CheatToggles.showProtectMenu = CheatToggles.showRolesMenu = CheatToggles.noOptionsLimits = false;
        }
    }

    public void OnGUI()
    {
        if (!isGUIActive || MalumMenu.isPanicked) return;

        InitStyles();

        UIHelpers.ApplyUIColor();

        windowRect = GUI.Window(0, windowRect, (GUI.WindowFunction)WindowFunction, "HyperMenuV" + MalumMenu.hyperVersion + ", forked from MalumMenu V" + MalumMenu.malumVersion, windowStyle);
    }

    public void WindowFunction(int windowID)
    {
        GUILayout.BeginHorizontal();

        // Left tab selector (15% width) with modern styling
        GUILayout.BeginVertical(GUILayout.Width(windowRect.width * 0.15f));
        GUILayout.Space(5);
        for (var i = 0; i < groups.Count; i++)
        {
            var style = (i == selectedTab) ? tabButtonActiveStyle : tabButtonStyle;
            if (GUILayout.Button(groups[i].name, style, GUILayout.Height(38)))
                selectedTab = i;
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        // Vertical separator line + invisible space to create gap between the tab selector and the content
        GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Width(2f), GUILayout.ExpandHeight(true));
        GUILayout.Space(12f);

        // Right tab content and controls (85% width)
        GUILayout.BeginVertical(GUILayout.Width(windowRect.width * 0.82f));
        GUILayout.Space(5);

        // Tab-specific content
        if (selectedTab >= 0 && selectedTab < groups.Count)
        {
            GUILayout.Label(groups[selectedTab].name, tabTitleStyle);

            GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Height(2f), GUILayout.ExpandWidth(true));
            GUILayout.Space(8);

            DrawTabContents(selectedTab);
        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        // Make the window draggable
        GUI.DragWindow();
    }

    // Gets the hardcoded number of left column submenus for each tab
    private int GetLeftSubmenuCount(int groupId)
    {
        return groups[groupId].name switch
        {
            "Player" => 1,
            "ESP" => 1,
            "Roles" => 4,
            "Ship" => 1,
            "Chat" => 1,
            "Console" => 1,
            "Host-Only" => 2,
            "Passive" => 1,
            "Animations" => 1,
            "Config" => 1,
            _ => 2
        };
    }

    public void DrawTabContents(int groupId)
    {
        var group = groups[groupId];

        // Special handling for Config tab
        if (group.name == "Config")
        {
            DrawConfigTab(group);
            return;
        }

        var submenuCount = group.submenus.Count;

        bool needSpace = false;

        contentScrollPosition = GUILayout.BeginScrollView(contentScrollPosition, false, true, GUILayout.Height(windowRect.height - 90));

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical(GUILayout.Width(windowRect.width * 0.425f));

        if (group.toggles.Count > 0)
        {
            DrawToggles(group.toggles);
            needSpace = true;
        }

        if (group.name == "Player")
        {
            try
            {
                if (PlayerControl.LocalPlayer.Data.IsDead)
                {
                    PlayerControl.LocalPlayer.MyPhysics.GhostSpeed = GUILayout.HorizontalSlider(PlayerControl.LocalPlayer.MyPhysics.GhostSpeed, 0f, 20f, GUILayout.Width(250f));
                    Utils.SnapSpeedToDefault(0.05f, true);
                    GUILayout.Label($"Current Speed: {PlayerControl.LocalPlayer?.MyPhysics.GhostSpeed} {(Utils.IsSpeedDefault(true) ? "(Default)" : "")}");
                }
                else
                {
                    PlayerControl.LocalPlayer.MyPhysics.Speed = GUILayout.HorizontalSlider(PlayerControl.LocalPlayer.MyPhysics.Speed, 0f, 20f, GUILayout.Width(250f));
                    Utils.SnapSpeedToDefault(0.05f);
                    GUILayout.Label($"Current Speed: {PlayerControl.LocalPlayer?.MyPhysics.Speed} {(Utils.IsSpeedDefault() ? "(Default)" : "")}");
                }
            } catch (NullReferenceException) {}
        }

        var desiredLeft = GetLeftSubmenuCount(groupId);
        var leftCount = Mathf.Clamp(desiredLeft, 0, submenuCount);

        // Draw left column submenus
        var leftSubmenus = group.submenus.GetRange(0, leftCount);
        foreach (var submenu in leftSubmenus)
        {
            if (needSpace) GUILayout.Space(15);
            needSpace = true;

            GUILayout.Label(submenu.name, tabSubtitleStyle);
            DrawToggles(submenu.toggles);
        }

        GUILayout.EndVertical();

        GUILayout.BeginVertical();

        // Draw right column submenus if there are any
        if (submenuCount > leftCount)
        {
            needSpace = false;

            var rightSubmenus = group.submenus.GetRange(leftCount, submenuCount - leftCount);
            foreach (var submenu in rightSubmenus)
            {
                if (needSpace) GUILayout.Space(15);
                needSpace = true;

                GUILayout.Label(submenu.name, tabSubtitleStyle);
                DrawToggles(submenu.toggles);
            }
        }

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
    }

    private void DrawConfigTab(GroupInfo group)
    {
        // Initialize platform selection from config
        if (selectedPlatformIndex == 0 && !string.IsNullOrEmpty(MalumMenu.spoofPlatform.Value))
        {
            for (int i = 0; i < platformOptions.Length; i++)
            {
                if (platformOptions[i] == MalumMenu.spoofPlatform.Value)
                {
                    selectedPlatformIndex = i;
                    break;
                }
            }
        }

        // Initialize level from config
        if (!spoofLevelEnabled && !string.IsNullOrEmpty(MalumMenu.spoofLevel.Value))
        {
            if (uint.TryParse(MalumMenu.spoofLevel.Value, out uint parsedLevel))
            {
                spoofLevelValue = parsedLevel;
                spoofLevelEnabled = true;
            }
        }

        configScrollPosition = GUILayout.BeginScrollView(configScrollPosition, false, true, GUILayout.Height(windowRect.height - 80));

        GUILayout.BeginHorizontal();

        // Left column
        GUILayout.BeginVertical(GUILayout.Width(windowRect.width * 0.38f));

        GUILayout.Label("Profile Management", tabSubtitleStyle);
        GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Height(1f), GUILayout.Width(windowRect.width * 0.35f));
        GUILayout.Space(5);
        DrawToggles(group.toggles);

        GUILayout.Space(15);
        GUILayout.Label("Visual Settings", tabSubtitleStyle);
        GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Height(1f), GUILayout.Width(windowRect.width * 0.35f));
        GUILayout.Space(5);

        var currentRgbState = CheatToggles.rgbMode;
        var newRgbState = GUILayout.Toggle(currentRgbState, " RGB Rainbow Mode");
        if (newRgbState != currentRgbState)
        {
            CheatToggles.rgbMode = newRgbState;
        }

        GUILayout.Space(10);
        GUILayout.Label("Current Menu Color: " + (string.IsNullOrEmpty(MalumMenu.menuHtmlColor.Value) ? "(default)" : MalumMenu.menuHtmlColor.Value));

        GUILayout.Space(15);
        GUILayout.Label("Menu Settings", tabSubtitleStyle);
        GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Height(1f), GUILayout.Width(windowRect.width * 0.35f));
        GUILayout.Space(5);

        GUILayout.Label("Current Keybind: " + MalumMenu.menuKeybind.Value);

        GUILayout.Space(10);
        var currentOpenOnMouse = MalumMenu.menuOpenOnMouse.Value;
        var newOpenOnMouse = GUILayout.Toggle(currentOpenOnMouse, " Open Menu at Mouse");
        if (newOpenOnMouse != currentOpenOnMouse)
        {
            MalumMenu.menuOpenOnMouse.Value = newOpenOnMouse;
        }

        var currentAutoLoad = MalumMenu.autoLoadProfile.Value;
        var newAutoLoad = GUILayout.Toggle(currentAutoLoad, " Auto-Load Profile");
        if (newAutoLoad != currentAutoLoad)
        {
            MalumMenu.autoLoadProfile.Value = newAutoLoad;
        }

        GUILayout.EndVertical();

        GUILayout.Space(10);

        // Right column
        GUILayout.BeginVertical(GUILayout.Width(windowRect.width * 0.38f));

        GUILayout.Label("Privacy Settings", tabSubtitleStyle);
        GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Height(1f), GUILayout.Width(windowRect.width * 0.35f));
        GUILayout.Space(5);

        var currentHideDevice = MalumMenu.spoofDeviceId.Value;
        var newHideDevice = GUILayout.Toggle(currentHideDevice, " Hide Device ID");
        if (newHideDevice != currentHideDevice)
        {
            MalumMenu.spoofDeviceId.Value = newHideDevice;
        }

        var currentNoTelemetry = MalumMenu.noTelemetry.Value;
        var newNoTelemetry = GUILayout.Toggle(currentNoTelemetry, " Disable Telemetry");
        if (newNoTelemetry != currentNoTelemetry)
        {
            MalumMenu.noTelemetry.Value = newNoTelemetry;
            if (newNoTelemetry)
            {
                UnityEngine.Analytics.Analytics.enabled = false;
                UnityEngine.Analytics.Analytics.deviceStatsEnabled = false;
                UnityEngine.Analytics.PerformanceReporting.enabled = false;
            }
        }

        GUILayout.Space(15);
        GUILayout.Label("Spoofing Settings", tabSubtitleStyle);
        GUILayout.Box("", GUIStylePreset.Separator, GUILayout.Height(1f), GUILayout.Width(windowRect.width * 0.35f));
        GUILayout.Space(5);

        // Level Spoofing
        GUILayout.Label("Spoof Level:");

        var newLevelEnabled = GUILayout.Toggle(spoofLevelEnabled, spoofLevelEnabled ? " Enabled: " + spoofLevelValue : " Disabled");
        if (newLevelEnabled != spoofLevelEnabled)
        {
            spoofLevelEnabled = newLevelEnabled;
            UpdateSpoofLevel();
        }

        if (spoofLevelEnabled)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("-100", GUILayout.Width(45))) { AdjustLevel(-100); }
            if (GUILayout.Button("-10", GUILayout.Width(40))) { AdjustLevel(-10); }
            if (GUILayout.Button("-1", GUILayout.Width(35))) { AdjustLevel(-1); }
            if (GUILayout.Button("+1", GUILayout.Width(35))) { AdjustLevel(1); }
            if (GUILayout.Button("+10", GUILayout.Width(40))) { AdjustLevel(10); }
            if (GUILayout.Button("+100", GUILayout.Width(45))) { AdjustLevel(100); }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Set 0", GUILayout.Width(60))) { spoofLevelValue = 0; UpdateSpoofLevel(); }
            if (GUILayout.Button("Set 100", GUILayout.Width(60))) { spoofLevelValue = 100; UpdateSpoofLevel(); }
            if (GUILayout.Button("Set Max", GUILayout.Width(60))) { spoofLevelValue = uint.MaxValue; UpdateSpoofLevel(); }
            GUILayout.EndHorizontal();
        }

        GUILayout.Space(10);

        // Platform Spoofing
        GUILayout.Label("Spoof Platform:");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("<", GUILayout.Width(30)))
        {
            selectedPlatformIndex = (selectedPlatformIndex - 1 + platformOptions.Length) % platformOptions.Length;
            UpdateSpoofPlatform();
        }

        GUILayout.Label(platformOptions[selectedPlatformIndex], GUILayout.Width(140));

        if (GUILayout.Button(">", GUILayout.Width(30)))
        {
            selectedPlatformIndex = (selectedPlatformIndex + 1) % platformOptions.Length;
            UpdateSpoofPlatform();
        }
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.EndScrollView();
    }

    private void AdjustLevel(int amount)
    {
        if (amount < 0 && spoofLevelValue < (uint)(-amount))
        {
            spoofLevelValue = 0;
        }
        else if (amount > 0 && spoofLevelValue > uint.MaxValue - (uint)amount)
        {
            spoofLevelValue = uint.MaxValue;
        }
        else
        {
            spoofLevelValue = (uint)(spoofLevelValue + amount);
        }
        UpdateSpoofLevel();
    }

    private void UpdateSpoofLevel()
    {
        if (spoofLevelEnabled)
        {
            MalumMenu.spoofLevel.Value = spoofLevelValue.ToString();
        }
        else
        {
            MalumMenu.spoofLevel.Value = "";
        }
    }

    private void UpdateSpoofPlatform()
    {
        if (selectedPlatformIndex == 0) // "Disabled"
        {
            MalumMenu.spoofPlatform.Value = "";
        }
        else
        {
            MalumMenu.spoofPlatform.Value = platformOptions[selectedPlatformIndex];
        }
    }

    public void DrawToggles(List<ToggleInfo> toggles)
    {
        foreach (var toggle in toggles)
        {
            var currentState = toggle.getState();
            var newState = GUILayout.Toggle(currentState, toggle.label, GUIStylePreset.ModernToggle);

            if (newState != currentState)
            {
                toggle.setState(newState);
            }
        }
    }
}
