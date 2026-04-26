using UnityEngine;

namespace MalumMenu;

public static class GUIStylePreset
{
    private static GUIStyle _separator;
    private static GUIStyle _darkSeparator;
    private static GUIStyle _normalButton;
    private static GUIStyle _normalToggle;
    private static GUIStyle _tabButton;
    private static GUIStyle _tabTitle;
    private static GUIStyle _tabSubtitle;
    private static GUIStyle _modernBox;
    private static GUIStyle _sectionHeader;
    private static GUIStyle _modernLabel;

    public static GUIStyle Separator
    {
        get
        {
            if (_separator == null)
            {
                _separator = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = Texture2D.whiteTexture },
                    margin = new RectOffset { top = 6, bottom = 6, left = 2, right = 2 },
                    padding = new RectOffset(),
                    border = new RectOffset()
                };
            }

            return _separator;
        }
    }

    public static GUIStyle DarkSeparator
    {
        get
        {
            if (_darkSeparator == null)
            {
                _darkSeparator = new GUIStyle(GUI.skin.box)
                {
                    normal = { background = Texture2D.grayTexture },
                    margin = new RectOffset { top = 4, bottom = 4 },
                    padding = new RectOffset(),
                    border = new RectOffset()
                };
            }

            return _darkSeparator;
        }
    }

    public static GUIStyle NormalButton
    {
        get
        {
            if (_normalButton == null)
            {
                _normalButton = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter,
                    padding = new RectOffset { left = 12, right = 12, top = 7, bottom = 7 },
                    margin = new RectOffset { left = 3, right = 3, top = 4, bottom = 4 },
                    fontStyle = FontStyle.Normal,
                    richText = true,
                    wordWrap = false,
                    normal = { textColor = new Color(0.95f, 0.95f, 0.95f, 1f) },
                    hover = { textColor = Color.white },
                    active = { textColor = Color.white }
                };
            }

            return _normalButton;
        }
    }

    public static GUIStyle NormalToggle
    {
        get
        {
            if (_normalToggle == null)
            {
                _normalToggle = new GUIStyle(GUI.skin.toggle)
                {
                    fontSize = 14,
                    padding = new RectOffset { left = 20, right = 5, top = 5, bottom = 5 },
                    margin = new RectOffset { left = 3, right = 3, top = 4, bottom = 4 },
                    alignment = TextAnchor.MiddleLeft,
                    richText = true,
                    normal = { textColor = new Color(0.9f, 0.9f, 0.9f, 1f) },
                    onNormal = { textColor = Color.white }
                };
            }

            return _normalToggle;
        }
    }

    public static GUIStyle TabButton
    {
        get
        {
            if (_tabButton == null)
            {
                _tabButton = new GUIStyle(GUI.skin.button)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    padding = new RectOffset { left = 10, right = 10, top = 9, bottom = 9 },
                    margin = new RectOffset { left = 2, right = 2, top = 4, bottom = 4 },
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true,
                    richText = true,
                    normal = { textColor = new Color(0.92f, 0.92f, 0.92f, 1f) },
                    hover = { textColor = Color.white },
                    active = { textColor = Color.white }
                };
            }

            return _tabButton;
        }
    }

    public static GUIStyle TabTitle
    {
        get
        {
            if (_tabTitle == null)
            {
                _tabTitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset { left = 8, right = 8, top = 6, bottom = 6 },
                    margin = new RectOffset { left = 0, right = 0, top = 0, bottom = 4 },
                    richText = true,
                    normal = { textColor = Color.white }
                };
            }

            return _tabTitle;
        }
    }

    public static GUIStyle TabSubtitle
    {
        get
        {
            if (_tabSubtitle == null)
            {
                _tabSubtitle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset { left = 8, right = 8, top = 4, bottom = 4 },
                    margin = new RectOffset { left = 0, right = 0, top = 2, bottom = 2 },
                    richText = true,
                    normal = { textColor = new Color(0.85f, 0.85f, 0.85f, 1f) }
                };
            }

            return _tabSubtitle;
        }
    }

    public static GUIStyle ModernBox
    {
        get
        {
            if (_modernBox == null)
            {
                _modernBox = new GUIStyle(GUI.skin.box)
                {
                    padding = new RectOffset { left = 10, right = 10, top = 10, bottom = 10 },
                    margin = new RectOffset { left = 4, right = 4, top = 5, bottom = 5 },
                    border = new RectOffset { left = 1, right = 1, top = 1, bottom = 1 }
                };
            }

            return _modernBox;
        }
    }

    public static GUIStyle SectionHeader
    {
        get
        {
            if (_sectionHeader == null)
            {
                _sectionHeader = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 15,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset { left = 6, right = 6, top = 4, bottom = 4 },
                    margin = new RectOffset { left = 2, right = 2, top = 6, bottom = 4 },
                    richText = true,
                    normal = { textColor = Color.white }
                };
            }

            return _sectionHeader;
        }
    }

    public static GUIStyle ModernLabel
    {
        get
        {
            if (_modernLabel == null)
            {
                _modernLabel = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 13,
                    alignment = TextAnchor.MiddleLeft,
                    padding = new RectOffset { left = 4, right = 4, top = 3, bottom = 3 },
                    margin = new RectOffset { left = 2, right = 2, top = 1, bottom = 1 },
                    richText = true,
                    wordWrap = true,
                    normal = { textColor = new Color(0.88f, 0.88f, 0.88f, 1f) }
                };
            }

            return _modernLabel;
        }
    }
}
