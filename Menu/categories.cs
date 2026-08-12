using Plon.Classes;
using System.Collections.Generic;
using ShibaGTGenesisReborn.Libs;
using static Plon.Menu.Main;
using static Plon.Settings;

namespace Plon.Menu
{
    internal class SettingsMods
    {
        public static void EnterSettings()
        {
            buttonsType = 0;
            pageNumber = 0;
        }

        public static void MenuSettings()
        {
            buttonsType = 1;
            pageNumber = 0;
        }
        public static void master()
        {
            buttonsType = 9;
            pageNumber = 0;
        }
        public static void room()
        {
            buttonsType = 10;
            pageNumber = 0;
        }
        public static void moveset()
        {
            buttonsType = 11;
            pageNumber = 0;
        }


        public static void projset()
        {
            buttonsType = 13;
            pageNumber = 0;
        }

        public static void rig()
        {
            buttonsType = 12;
            pageNumber = 0;
        }

        public static void advantages()
        {
            buttonsType = 2;
            pageNumber = 0;
        }

        public static void movement()
        {
            buttonsType = 3;
            pageNumber = 0;
        }

        public static void visuals()
        {
            buttonsType = 4;
            pageNumber = 0;
        }

        public static void overpowered()
        {
            buttonsType = 5;
            pageNumber = 0;
        }

        public static void safety()
        {
            buttonsType = 6;
            pageNumber = 0;
        }

        public static void fun()
        {
            buttonsType = 7;
            pageNumber = 0;
        }

        public static void guardian()
        {
            buttonsType = 8;
            pageNumber = 0;
        }

        public static void enablemods()
        {
            UpdateEnabledMods();
            buttonsType = 14;
            pageNumber = 0;
        }

        public static void favouritemods()
        {
            Main.UpdateFavoritesCategory();
            buttonsType = 15;
            pageNumber = 0;
        }

        public static void ClearFavorites()
        {
            foreach (ButtonInfo[] buttonList in Buttons.buttons)
            {
                foreach (ButtonInfo button in buttonList)
                {
                    if (button != null)
                    {
                        button.isFavorite = false;
                    }
                }
            }
            Main.favoriteButtons.Clear();
            Main.UpdateFavoritesCategory();
            Main.RecreateMenu();
            NotificationLib.SendNotification(NotificationLib.NotificationType.Info, "Cleared all favorites");
        }

        public static void RightHand()
        {
            rightHanded = true;
        }

        public static void LeftHand()
        {
            rightHanded = false;
        }

        public static void EnableFPSCounter()
        {
            fpsCounter = true;
        }

        public static void DisableFPSCounter()
        {
            fpsCounter = false;
        }

        public static void EnableNotifications()
        {
            disableNotifications = false;
        }

        public static void DisableNotifications()
        {
            disableNotifications = true;
        }

        public static void EnableDisconnectButton()
        {
            disconnectButton = true;
        }

        public static void DisableDisconnectButton()
        {
            disconnectButton = false;
        }

        public static void UpdateEnabledMods()
        {
            List<ButtonInfo> enabledMods = new List<ButtonInfo>();

            for (int i = 0; i < Buttons.buttons.Length; i++)
            {
                if (i == 14)
                    continue;

                foreach (ButtonInfo mod in Buttons.buttons[i])
                {
                    if (mod != null && mod.enabled && mod.isTogglable && !enabledMods.Contains(mod))
                    {
                        enabledMods.Add(mod);
                    }
                }
            }

            Buttons.buttons[14] = enabledMods.ToArray();
        }
    }
}