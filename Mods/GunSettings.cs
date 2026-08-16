using ShibaGTGenesisReborn.Libs;
using ShibaGTGenesisReborn.Menu;

namespace ShibaGTGenesisReborn.Mods
{
    public partial class mods
    {
        public static bool G = false;
        public static bool hasTpd = false;
        public static float num = 8f;

        public static void EquipGun()
        {
            GunLib.StartGun(() =>
            {
                G = !G;
                if (!G)
                {
                    GunLib.CleanupPointer();
                }
            }, false);
        }

        public static void GunSmoothNess()
        {
            if (num == 8f)
            {
                num = 66f;
                Main.GetIndex("Click Sound: Normal").overlapText = "Click Sound: Keyboard";
            }
            else if (num == 66f)
            {
                num = 144f;
                Main.GetIndex("Click Sound: Normal").overlapText = "Click Sound: Thick";
            }
            else
            {
                num = 8f;
                Main.GetIndex("Click Sound: Normal").overlapText = "Click Sound: Normal";
            }
        }
    }
}
