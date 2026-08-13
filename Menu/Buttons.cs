using Plon.Menu;
using Plon.Classes;
using Plon.Libs;
using Plon.Menu;
using Plon.Mods;
using static Plon.Settings;
using Photon.Pun;
using Plon.Mods;
using UnityEngine;
using GorillaLocomotion;

namespace Plon.Menu
{
    internal class Buttons
    {
        public static ButtonInfo[][] buttons = new ButtonInfo[][]
        {
            new ButtonInfo[]
            { // Main Mods
                new ButtonInfo { buttonText = "Save", method =() => mods.Save(), isTogglable = false, toolTip = "Puts the menu on your right hand.", enabled = false},
                new ButtonInfo { buttonText = "Enabled Mods", method =() => SettingsMods.enablemods(), isTogglable = false, toolTip = "Puts the menu on your right hand.", enabled = false},
                new ButtonInfo { buttonText = "Favourite", method =() => SettingsMods.favouritemods(), isTogglable = false, toolTip = "Puts the menu on your right hand.", enabled = false},
                //new ButtonInfo { buttonText = "Settings", method =() => SettingsMods.MenuSettings(), isTogglable = false, toolTip = "Opens the main settings page for the menu."},
                new ButtonInfo { buttonText = "Advantages", method =() => SettingsMods.advantages(), isTogglable = false, toolTip = "Opens the movement settings for the menu."},
                new ButtonInfo { buttonText = "Movement", method =() => SettingsMods.movement(), isTogglable = false, toolTip = "Opens the projectile settings for the menu."},
                new ButtonInfo { buttonText = "Fun", method =() => SettingsMods.fun(), isTogglable = false, toolTip = "Opens the movement settings for the menu."},
                new ButtonInfo { buttonText = "Projectiles", method =() => SettingsMods.master(), isTogglable = false, toolTip = "Opens the movement settings for the menu."},
                new ButtonInfo { buttonText = "Overpowered", method =() => SettingsMods.overpowered(), isTogglable = false, toolTip = "Opens the projectile settings for the menu."},
                new ButtonInfo { buttonText = "Room", method =() => SettingsMods.room(), isTogglable = false, toolTip = "Opens the projectile settings for the menu."},
                new ButtonInfo { buttonText = "Visual", method =() => SettingsMods.visuals(), isTogglable = false, toolTip = "Opens the projectile settings for the menu."},
                new ButtonInfo { buttonText = "Rig", method =() => SettingsMods.rig(), isTogglable = false, toolTip = "Opens the projectile settings for the menu."},
            },

            new ButtonInfo[]
            { // Menu Settings
                new ButtonInfo { buttonText = "Gunlib", method =() => SettingsMods.guardian(), toolTip = "", isTogglable = false},
                new ButtonInfo { buttonText = "Menu", method =() => SettingsMods.safety(), toolTip = "", isTogglable = false},
                new ButtonInfo { buttonText = "  Movement  ", method =() => SettingsMods.moveset(), toolTip = "", isTogglable = false},
                new ButtonInfo { buttonText = " Projectiles ", method =() => SettingsMods.projset(), toolTip = "", isTogglable = false},
                new ButtonInfo { buttonText = "Anti Report", method =() => mods.AntiReport(), toolTip = "", isTogglable = true, enabled = true},
            },

            new ButtonInfo[]
            { // Advantages
                new ButtonInfo { buttonText = "Tag Gun", method =() => mods.TagGun(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Tag All", method =() => mods.TagAll(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "No Tag On Join", method =() => mods.NoTagOnJoin(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "No Leaves", method =() => mods.removeleaves(), disableMethod =() => mods.addleaves(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "60 Hz", method =() => Application.targetFrameRate = 60, disableMethod =() => mods.addleaves(), isTogglable = true, toolTip = "placeholder."},
            },

            new ButtonInfo[]
            { // Movement
                new ButtonInfo { buttonText = "Platforms", method =() => mods.Platforms(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Invis Platforms", method =() => mods.Platforms(true), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Noclip (RT)", method =() => mods.Noclip(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Fly (A)", method =() => mods.CarMonkeyandfly(15f, true), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Wasd Fly", method =() => mods.CarMonkeyandfly(15f, true), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Car Monkey (A)", method =() => mods.CarMonkeyandfly(15f, false), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "TP Gun", method =() => mods.TPGun(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Low Gravity", method =() => mods.LowGravity(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "High Gravity", method =() => mods.HighGravity(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Up And Down", method =() => mods.UpAndDown(), isTogglable = true, toolTip = "placeholder."},
            },

            new ButtonInfo[]
            { // visuals
                new ButtonInfo { buttonText = "Tracers", method =() => mods.Tracers(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Infection Chams", method =() => mods.FullBodyESP(), disableMethod =() => mods.DisableFullBodyESP(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "RGB Monke (stump)", method =() => mods.RGB(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Casual Chams", method =() => mods.CasualFullBodyESP(), disableMethod =() => mods.DisableFullBodyESP(), isTogglable = true, toolTip = "placeholder."},
            },

            new ButtonInfo[]
            { // overpowered
                new ButtonInfo { buttonText = "Lag Gun", method =() => mods.LagGun(0.5f, 240), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Lag Gun v2", method =() => mods.LagGun(3f, 1000), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Lag All", method =() => mods.LagAll(0.5f, 240), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Lag All v2", method =() => mods.LagAll(3f, 1000), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Lag Spike Gun", method =() => mods.LagGun(8f, 3500), isTogglable = true, toolTip = "placeholder."},
                /*new ButtonInfo { buttonText = "Fist Bump Spam Self", method =() => Overpowered.SpamSelf((TagEffects.TagEffectsLibrary.EffectType)3), isTogglable = true,  enabled = false },
                new ButtonInfo { buttonText = "Fist Bump Spam Others", method =() => Overpowered.SpamOthers((TagEffects.TagEffectsLibrary.EffectType)3),  isTogglable = true, enabled = false },
                new ButtonInfo { buttonText = "High Five Spam Others", method =() => Overpowered.SpamOthers((TagEffects.TagEffectsLibrary.EffectType)2), isTogglable = true, enabled = false },
                new ButtonInfo { buttonText = "Super Saiyan All", method =() => Overpowered.SpamOthers(), isTogglable = true, enabled = false },*/
                //new ButtonInfo { buttonText = "Grey Zone (master)", enableMethod =() => mods.GreyScreen(), disableMethod =() => mods.NoGreyScreen(), isTogglable = true, toolTip = "placeholder."},
            },

            new ButtonInfo[]
            { // Menu Settings
                new ButtonInfo { buttonText = "Left Hand", enableMethod =() => SettingsMods.LeftHand(), disableMethod =() => SettingsMods.RightHand(), toolTip = "Puts the menu on your right hand.", enabled = !rightHanded},
                new ButtonInfo { buttonText = "FPS Counter", enableMethod =() => SettingsMods.EnableFPSCounter(), disableMethod =() => SettingsMods.DisableFPSCounter(), enabled = fpsCounter, toolTip = "Toggles the FPS counter."},
                new ButtonInfo { buttonText = "Setting Button", enableMethod =() => SettingsButton = true, disableMethod =() => SettingsButton = false, enabled = SettingsButton, toolTip = "Toggles the FPS counter."},
                new ButtonInfo { buttonText = "Leave Button", enableMethod =() => SettingsMods.EnableDisconnectButton(), disableMethod =() => SettingsMods.DisableDisconnectButton(), enabled = disconnectButton, toolTip = "Toggles the disconnect button."},
                new ButtonInfo { buttonText = "Remove All Prefs", method =() => mods.Removeprefs(), isTogglable = false, enabled = false, toolTip = "Toggles the disconnect button."},
                new ButtonInfo { buttonText = "PPos", overlapText = "Menu Layout: ShibaGT", isTogglable = false, method =() => mods.SwitchPagePos(), enabled = false, toolTip = "Toggles the disconnect button."},
                new ButtonInfo { buttonText = "OutlineMenu", isTogglable = true, enableMethod =() => Main.what3 = true, disableMethod =() => Main.what3 = false, enabled = Main.what3, toolTip = "Toggles the disconnect button."},
                new ButtonInfo { buttonText = "COC", overlapText = "Outline: Blue", isTogglable = false, method =() => mods.ChangeOutlineColor(), enabled = false, toolTip = "Toggles the disconnect button."},
            },

            new ButtonInfo[]
            { // fun
                new ButtonInfo { buttonText = "Board Spam",  method =() => mods.HoverboardSpam(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Waterbend",  method =() => mods.WaterSplash(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Bracelet Spam",  method =() => mods.BraceletSpam(), disableMethod =() => mods.NoBracelet(), isTogglable = true, toolTip = "placeholder."},
                //new ButtonInfo { buttonText = "Shiba gun",  method =() => mods.ShibaGun(), isTogglable = true, toolTip = "placeholder."},
                //new ButtonInfo { buttonText = "Silly Cat Holdable",  enableMethod = () => mods.sillycatholdable(), disableMethod = () => mods.RemoveCat(), isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Boombox", method = () => BoomboxManager.BoomboxLoop("https://github.com/odinong/Groshable/releases/download/boomboxdiddy/212302951.obj", "https://github.com/odinong/Groshable/releases/download/boomboxdiddy/boomboxmesh.png"), disableMethod = () => BoomboxManager.Kill(), isTogglable = true, toolTip = "Working boombox."},
                new ButtonInfo { buttonText = "Change Boombox Audio", method = () => BoomboxManager.OpenNativePicker(), isTogglable = false, toolTip = "Changes the audio of the boombox."},
                new ButtonInfo { buttonText = "Boombox Volume +", method = () => BoomboxManager.AdjustVolume(0.1f), isTogglable = false, toolTip = "Increases boombox volume."},
                new ButtonInfo { buttonText = "Boombox Volume -", method = () => BoomboxManager.AdjustVolume(-0.1f), isTogglable = false, toolTip = "Decreases boombox volume."},
                new ButtonInfo { buttonText = "Boombox Speed +", method = () => BoomboxManager.AdjustPitchSpeed(0.1f), isTogglable = false, toolTip = "Increases boombox speed."},
                new ButtonInfo { buttonText = "Boombox Speed -", method = () => BoomboxManager.AdjustPitchSpeed(-0.1f), isTogglable = false, toolTip = "Decreases boombox speed."},
                new ButtonInfo { buttonText = "Boombox Visualizer", enableMethod = () => BoomboxManager.UseVisualizer = true, disableMethod = () => BoomboxManager.UseVisualizer = false, isTogglable = true, enabled = BoomboxManager.UseVisualizer, toolTip = "Toggles the boombox visualizer."},
                new ButtonInfo { buttonText = "Visualizer Intensity +", method = () => BoomboxManager.VisualizerIntensity = Mathf.Clamp(BoomboxManager.VisualizerIntensity + 1f, 0f, 10f), isTogglable = false, toolTip = "Increases visualizer intensity by 1."},
                new ButtonInfo { buttonText = "Visualizer Intensity -", method = () => BoomboxManager.VisualizerIntensity = Mathf.Clamp(BoomboxManager.VisualizerIntensity - 1f, 0f, 10f), isTogglable = false, toolTip = "Decreases visualizer intensity by 1."},
                new ButtonInfo { buttonText = "Visualizer Base Scale +", method = () => BoomboxManager.BaseScale = Mathf.Clamp(BoomboxManager.BaseScale + 1f, 0.1f, 10f), isTogglable = false, toolTip = "Increases base scale by 1."},
                new ButtonInfo { buttonText = "Visualizer Base Scale -", method = () => BoomboxManager.BaseScale = Mathf.Clamp(BoomboxManager.BaseScale - 1f, 0.1f, 10f), isTogglable = false, toolTip = "Decreases base scale by 1."},
                new ButtonInfo { buttonText = "Grosh Holdable", enableMethod = () => GroshHolder.GroshLoop("https://github.com/odinong/Groshable/releases/download/gfddfggsdf/Grosh.Holdable.obj", "https://github.com/odinong/Groshable/blob/main/iidktexture.png?raw=true"), disableMethod = () => GroshHolder.Kill(), isTogglable = true, toolTip = "iiDk holdable."},
                new ButtonInfo { buttonText = "Tung Tung Tung Sahur", method = () => SusTung.TungShooter("https://github.com/odinong/Groshable/releases/download/tungtungsahur/TungTungTungSahur.obj", "https://github.com/odinong/Groshable/releases/download/tungtungsahur/shaded.png", "https://github.com/odinong/Groshable/releases/download/tungaudio/tungtung.wav"), disableMethod = () => SusTung.Kill(), isTogglable = true, toolTip = "Tung Tung Tung Sahur holdable."},
                new ButtonInfo { buttonText = "Fat Seal Spammer", method = () => FatSealSpammer.SealLoop("https://github.com/odinong/Groshable/raw/refs/heads/main/fatseal.obj", "https://github.com/odinong/Groshable/raw/refs/heads/main/fatseal.jpeg"), disableMethod = () => FatSealSpammer.Kill(), isTogglable = true, toolTip = "Hold keybind to spam bouncy fat seals."},
                new ButtonInfo { buttonText = "Vape", method = () => Vape.InitVape("https://github.com/odinong/Groshable/raw/refs/heads/main/juul.obj", "https://github.com/odinong/Groshable/blob/main/JUUL_BOI_Color.png?raw=true"), disableMethod = () => Vape.Kill(), isTogglable = true, toolTip = "Lets you vape."},
            },

            new ButtonInfo[]
            { // Gun Settings
                new ButtonInfo { buttonText = "Gunlock", enableMethod =() => GunLib.Lock = true, disableMethod =() => GunLib.Lock = false, enabled = GunLib.Lock, isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Equip Gun", method =() => mods.EquipGun(), isTogglable = true, toolTip = "placeholder."},
            },

            new ButtonInfo[]
            { // Master
                new ButtonInfo { buttonText = "Projectile Spam (B)", method =() => Projectiles.SnowballSpam(GorillaLocomotion.GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0f), GTPlayer.Instance.RightHand.controllerTransform.position), enabled = false, isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Projectile Gun (B)", method =() => Projectiles.SnowballSpam(GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.forward * 20f, GTPlayer.Instance.RightHand.controllerTransform.position), enabled = false, isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Snowball Fling Gun", method =() => Projectiles.FlingGun(), enabled = false, isTogglable = true, toolTip = "placeholder."},
            },
            new ButtonInfo[]
            { // Room
                new ButtonInfo { buttonText = "Disconnect", method =() => { PhotonNetwork.Disconnect(); NetworkSystem.Instance.ReturnToSinglePlayer();}, enabled = false, isTogglable = false, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "B Disconnect", method =() => mods.BDisconnect(), enabled = false, isTogglable = true, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Join Genesis", method =() => mods.Joincodegenesis(), enabled = false, isTogglable = false, toolTip = "placeholder."},
                new ButtonInfo { buttonText = "Join Random Room", method =() => mods.JoinRandom(), enabled = false, isTogglable = false, toolTip = "placeholder."},
            },
            new ButtonInfo[]
            { // Move Set
                new ButtonInfo{ buttonText = "Change Plat Color", method =() => mods.PlatColorChange(), isTogglable = false, overlapText = "Plat Color: Blue"}
            },
            new ButtonInfo[]
            { // Rig
                new ButtonInfo{ buttonText = "Ghost Monkey", method =() => mods.GhostMonkey(), isTogglable = true},
                new ButtonInfo{ buttonText = "Invis Monkey", method =() => mods.InvisMonkey(), isTogglable = true},
                new ButtonInfo{ buttonText = "Long Arms", method =() => mods.LongArms(), disableMethod =() => mods.NormalArms(), isTogglable = true},
                new ButtonInfo{ buttonText = "No Fingers", method =() => mods.NoFinger(), isTogglable = true},
                new ButtonInfo{ buttonText = "SpazRig", method =() => mods.SpazRig(), isTogglable = true},
            },
            new ButtonInfo[]
            { // Proj Set
                new ButtonInfo{ buttonText = "Big Snowballs", enableMethod =() => Mods.Projectiles.biig = true, disableMethod =() => Mods.Projectiles.biig = false, isTogglable = true},
            },

            new ButtonInfo[]
            { // favourite mods
                
            },
            new ButtonInfo[]
            { // enbled mods
                
            },

            //always keep this at the bottom if you add another tab (by going to categories) make sure you put that section above this one:

             new ButtonInfo[]
             {

             },

             new ButtonInfo[]
             {
                new ButtonInfo { buttonText = "home", method =() => Global.ReturnHome(), isTogglable = false, toolTip = "Opens the settings for the menu."},
             },

        };
    }
}
