using System;
using AmongUs.Data;
using UnityEngine;

namespace MalumMenu;

public static class MalumRandomizer
{
    // Randomly sets a new name, color, and cosmetics for the local player
    public static void Randomize()
    {
        try
        {
            var name = Utils.GetRandomName();
            var colorId = (byte)Random.Range(0, Palette.PlayerColors.Length);

            DataManager.Player.Customization.Name = name;
            DataManager.Player.Customization.Color = colorId;

            var hatManager = DestroyableSingleton<HatManager>.Instance;
            if (hatManager != null)
            {
                var allHats = hatManager.allHats;
                var allSkins = hatManager.allSkins;
                var allVisors = hatManager.allVisors;
                var allPets = hatManager.allPets;

                if (allHats != null && allHats.Count > 0)
                    DataManager.Player.Customization.Hat = allHats[Random.Range(0, allHats.Count)].ProdId;

                if (allSkins != null && allSkins.Count > 0)
                    DataManager.Player.Customization.Skin = allSkins[Random.Range(0, allSkins.Count)].ProdId;

                if (allVisors != null && allVisors.Count > 0)
                    DataManager.Player.Customization.Visor = allVisors[Random.Range(0, allVisors.Count)].ProdId;

                if (allPets != null && allPets.Count > 0)
                    DataManager.Player.Customization.Pet = allPets[Random.Range(0, allPets.Count)].ProdId;
            }

            DataManager.Player.Save();

            if (!Utils.isPlayer) return;

            PlayerControl.LocalPlayer.RpcSetColor(colorId);
            PlayerControl.LocalPlayer.RpcSetName(name);
            PlayerControl.LocalPlayer.RpcSetHat(DataManager.Player.Customization.Hat);
            PlayerControl.LocalPlayer.RpcSetSkin(DataManager.Player.Customization.Skin);
            PlayerControl.LocalPlayer.RpcSetVisor(DataManager.Player.Customization.Visor);
            PlayerControl.LocalPlayer.RpcSetPet(DataManager.Player.Customization.Pet);
        }
        catch (Exception e)
        {
            MalumMenu.Log.LogError($"Randomizer error: {e.Message}");
        }
    }
}
