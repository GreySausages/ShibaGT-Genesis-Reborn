using Photon.Pun;
using Plon.Menu;
using UnityEngine;
using static Plon.Menu.Main;
using static Plon.Settings;

namespace Plon.Classes
{
    internal class Button : MonoBehaviour
    {
        public string relatedText;

        public static float buttonCooldown = 0f;



        public void OnTriggerEnter(Collider collider)
        {
            if (Time.time > buttonCooldown && collider == buttonCollider && menu != null)
            {
                buttonCooldown = Time.time + 0.2f;
                GorillaTagger.Instance.StartVibration(rightHanded, GorillaTagger.Instance.tagHapticStrength / 2f, GorillaTagger.Instance.tagHapticDuration / 2f);
                VRRig.LocalRig.PlayHandTapLocal((int)mods.num, rightHanded, 0.4f);
                Toggle(this.relatedText);
            }
        }
    }
}
