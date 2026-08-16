using Photon.Pun;
using ShibaGTGenesisReborn.Menu;
using ShibaGTGenesisReborn.Mods;
using UnityEngine;
using static ShibaGTGenesisReborn.Menu.Main;
using static ShibaGTGenesisReborn.Settings;

namespace ShibaGTGenesisReborn.Classes
{
    internal class Button : MonoBehaviour
    {
        public string relatedText;
        public ButtonInfo buttonInfo;

        public static float buttonCooldown = 0f;
        public static bool customAudio = false;

        private static void plaything()
        {
            if (customAudio)
                MenuAudio.PlayClickSound();
            else
                VRRig.LocalRig.PlayHandTapLocal((int)mods.num, rightHanded, 0.4f);
        }

        public void OnTriggerEnter(Collider collider)
        {
            if (Time.time > buttonCooldown && collider == buttonCollider && menu != null)
            {
                buttonCooldown = Time.time + 0.15f;
                GorillaTagger.Instance.StartVibration(rightHanded, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);
                plaything();
                Toggle(this.relatedText, this.buttonInfo);
            }
        }
    }
}
