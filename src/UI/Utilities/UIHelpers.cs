using UnityEngine;

namespace MalumMenu;

public static class UIHelpers
{
    private static readonly Color DefaultDarkColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    private static readonly Color ModernAccentColor = new Color(0.2f, 0.4f, 0.8f, 1f);

    public static void ApplyUIColor()
    {
        if (CheatToggles.rgbMode)
        {
            // Use HSV color for RGB mode with better saturation and value
            Color hsvColor = Color.HSVToRGB(MenuUI.hue, 0.8f, 0.95f);
            GUI.backgroundColor = hsvColor;
        }
        else
        {
            var configHtmlColor = MalumMenu.menuHtmlColor.Value;

            // Try to parse the custom color
            if (!string.IsNullOrEmpty(configHtmlColor))
            {
                if (ColorUtility.TryParseHtmlString(configHtmlColor, out var uiColor))
                {
                    GUI.backgroundColor = uiColor;
                    return;
                }

                // Try with # prefix if not present
                if (!configHtmlColor.StartsWith("#"))
                {
                    if (ColorUtility.TryParseHtmlString("#" + configHtmlColor, out uiColor))
                    {
                        GUI.backgroundColor = uiColor;
                        return;
                    }
                }
            }

            // Fall back to modern default color
            GUI.backgroundColor = DefaultDarkColor;
        }
    }

    /// <summary>
    /// Gets a contrast color based on the current UI color for better readability
    /// </summary>
    public static Color GetContrastColor(Color baseColor)
    {
        float luminance = 0.299f * baseColor.r + 0.587f * baseColor.g + 0.114f * baseColor.b;
        return luminance > 0.5f ? Color.black : Color.white;
    }

    /// <summary>
    /// Creates a modern highlighted color for interactive elements
    /// </summary>
    public static Color GetHighlightColor(Color baseColor)
    {
        return new Color(baseColor.r + 0.1f, baseColor.g + 0.1f, baseColor.b + 0.1f, baseColor.a);
    }
}
