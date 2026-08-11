using BepInEx;
using System.ComponentModel;

namespace Plon.Patches
{
    [Description(Plon.PluginInfo.Description)]
    [BepInPlugin(Plon.PluginInfo.GUID, Plon.PluginInfo.Name, Plon.PluginInfo.Version)]
    public class HarmonyPatches : BaseUnityPlugin
    {
        private void OnEnable()
        {
            Menu.ApplyHarmonyPatches();
        }

        private void OnDisable()
        {
            Menu.RemoveHarmonyPatches();
        }
    }
}
