using GorillaLocomotion;
using Oculus.Interaction;
using Photon.Pun;
using ShibaGTGenesisReborn.Classes;
using ShibaGTGenesisReborn.Libs;
using ShibaGTGenesisReborn.Menu;
using ShibaGTGenesisReborn.Mods;
using UnityEngine;
using static ShibaGTGenesisReborn.Settings;

namespace ShibaGTGenesisReborn.Menu
{
    internal class Buttons
    {
        public static ButtonInfo[][] buttons = new ButtonInfo[][]
        {
            new ButtonInfo[]
            { // Main Mods
                new ButtonInfo { buttonText = "Save", method =() => mods.Save(), isTogglable = false, toolTip = "Save settings", enabled = false},
                new ButtonInfo { buttonText = "Enabled Mods", method =() => SettingsMods.enablemods(), isTogglable = false, toolTip = "View active mods"},
                new ButtonInfo { buttonText = "Favourite", method =() => SettingsMods.favouritemods(), isTogglable = false, toolTip = "View favorites"},
                new ButtonInfo { buttonText = "Room", method =() => SettingsMods.room(), isTogglable = false, toolTip = "Room mods"},
                new ButtonInfo { buttonText = "Advantages", method =() => SettingsMods.advantages(), isTogglable = false, toolTip = "Advantage mods"},
                new ButtonInfo { buttonText = "Movement", method =() => SettingsMods.movement(), isTogglable = false, toolTip = "Movement mods"},
                new ButtonInfo { buttonText = "Rig", method =() => SettingsMods.rig(), isTogglable = false, toolTip = "Rig mods"},
                new ButtonInfo { buttonText = "Fun", method =() => SettingsMods.fun(), isTogglable = false, toolTip = "Fun mods"},
                new ButtonInfo { buttonText = "Visual", method =() => SettingsMods.visuals(), isTogglable = false, toolTip = "Visual mods"},
                new ButtonInfo { buttonText = "Projectiles", method =() => SettingsMods.master(), isTogglable = false, toolTip = "Projectile mods"},
                new ButtonInfo { buttonText = "Overpowered", method =() => SettingsMods.overpowered(), isTogglable = false, toolTip = "OP mods"},
            },

            new ButtonInfo[]
            { // Menu Settings
                new ButtonInfo { buttonText = "Gunlib", method =() => SettingsMods.guardian(), toolTip = "Gun settings", isTogglable = false},
                new ButtonInfo { buttonText = "Menu", method =() => SettingsMods.safety(), toolTip = "Menu settings", isTogglable = false},
                new ButtonInfo { buttonText = "Movement", method =() => SettingsMods.moveset(), toolTip = "Move settings", isTogglable = false},
                new ButtonInfo { buttonText = "Projectiles", method =() => SettingsMods.projset(), toolTip = "Proj settings", isTogglable = false},
                new ButtonInfo { buttonText = "Anti Report", method =() => mods.AntiReport(), toolTip = "Block reports", isTogglable = true, enabled = true},
            },

            new ButtonInfo[]
            { // Advantages
                new ButtonInfo { buttonText = "Tag Gun", method =() => mods.TagGun(), isTogglable = true, toolTip = "Shoot tags"},
                new ButtonInfo { buttonText = "Tag All", method =() => mods.TagAll(), isTogglable = true, toolTip = "Tag everyone"},
                new ButtonInfo { buttonText = "No Tag On Join", method =() => mods.NoTagOnJoin(), isTogglable = true, toolTip = "No tag when joining"},
                new ButtonInfo { buttonText = "No Leaves", method =() => mods.removeleaves(), disableMethod =() => mods.addleaves(), isTogglable = true, toolTip = "Remove leaves"},
                new ButtonInfo { buttonText = "45 FPS", method =() => mods.FPS(45), isTogglable = true, toolTip = "Set 45 FPS"},
                new ButtonInfo { buttonText = "60 FPS", method =() => mods.FPS(60), isTogglable = true, toolTip = "Set 60 FPS"},
                new ButtonInfo { buttonText = "90 FPS", method =() => mods.FPS(90), isTogglable = true, toolTip = "Set 90 FPS"},
                new ButtonInfo { buttonText = "120 FPS", method =() => mods.FPS(120), isTogglable = true, toolTip = "Set 120 FPS"},
                new ButtonInfo { buttonText = "Unlock fps", method =() => { Application.targetFrameRate = int.MaxValue; QualitySettings.vSyncCount = 0; }, disableMethod =() => Application.targetFrameRate = 144, isTogglable = true, enabled = false, toolTip = "Unlocks FPS (doesnt work if nvidia control panel is limiting)" },
            },

            new ButtonInfo[]
            { // Movement
                new ButtonInfo { buttonText = "Platforms", method =() => mods.Platforms(), isTogglable = true, toolTip = "Spawn platforms on trigger/grip"},
                new ButtonInfo { buttonText = "Invis Platforms", method =() => mods.Platforms(true), isTogglable = true, toolTip = "Spawn invisible platforms"},
                new ButtonInfo { buttonText = "Noclip (RT)", method =() => mods.Noclip(), isTogglable = true, toolTip = "Hold right trigger to phase through walls"},
                new ButtonInfo { buttonText = "Fly (A)", method =() => mods.CarMonkeyandfly(15f, true), isTogglable = true, toolTip = "Hold A to fly where you look"},
                new ButtonInfo { buttonText = "WASD Fly", method =() => mods.WASDFly(), isTogglable = true, toolTip = "Fly and look around with WASD/mouse"},
                new ButtonInfo { buttonText = "Car Monkey (A)", method =() => mods.CarMonkeyandfly(15f, false), isTogglable = true, toolTip = "Hold A to drive forward"},
                new ButtonInfo { buttonText = "TP Gun", method =() => mods.TeleportGun(), isTogglable = true, toolTip = "Point and shoot to teleport"},
                new ButtonInfo { buttonText = "Pull Mods", method =() => mods.PullMod(), isTogglable = true, toolTip = "just pull mod"},
                new ButtonInfo { buttonText = "Low Gravity", method =() => mods.GravityManager(mods.Gravitytypes.Low), isTogglable = true, toolTip = "Lowers gravity."},
                new ButtonInfo { buttonText = "High Gravity", method =() => mods.GravityManager(mods.Gravitytypes.High), isTogglable = true, toolTip = "Increases gravity."},
                new ButtonInfo { buttonText = "Zero Gravity", method =() => mods.GravityManager(mods.Gravitytypes.Zero), isTogglable = true, toolTip = "Removes gravity."},
                new ButtonInfo { buttonText = "Reverse Gravity", method =() => mods.GravityManager(mods.Gravitytypes.Reverse), disableMethod = () => mods.Reset_upsidedown(), isTogglable = true, toolTip = "Reverses gravity."},
                new ButtonInfo { buttonText = "Up And Down", method =() => mods.UpAndDown(), isTogglable = true, toolTip = "RT to fly up, LT to fly down"},
                new ButtonInfo { buttonText = "CheckPoint", method =() => mods.CheckPoint(), disableMethod =() => mods.CheckPointDisable(), isTogglable = true, toolTip = "RG to set checkpoint, A to teleport"},
            },

            new ButtonInfo[]
            { // visuals
                new ButtonInfo { buttonText = "Tracers", method =() => mods.Tracers(), isTogglable = true, toolTip = "Draw lines to players"},
                new ButtonInfo { buttonText = "Infection Chams", method =() => mods.FullBodyESP(), disableMethod =() => mods.DisableFullBodyESP(), isTogglable = true, toolTip = "Highlight infected players"},
                new ButtonInfo { buttonText = "RGB Monke (stump)", method =() => mods.RGB(), isTogglable = true, toolTip = "Cycle player colors in stump"},
                new ButtonInfo { buttonText = "Strobe Monke (stump)", method =() => mods.RGB(true), isTogglable = true, toolTip = "Rapidly strobe player colors in stump"},
                new ButtonInfo { buttonText = "Casual Chams", method =() => mods.CasualFullBodyESP(), disableMethod =() => mods.DisableFullBodyESP(), isTogglable = true, toolTip = "Highlight all players"},
            },

            new ButtonInfo[]
            { // overpowered
                new ButtonInfo { buttonText = "Lag Gun", method =() => mods.LagGun(0.5f, 240), isTogglable = true, toolTip = "Lag target player with events"},
                new ButtonInfo { buttonText = "Lag Gun v2", method =() => mods.LagGun(3f, 1000), isTogglable = true, toolTip = "Heavy lag target player"},
                new ButtonInfo { buttonText = "Lag All", method =() => mods.LagAll(0.5f, 240), isTogglable = true, toolTip = "Lag all players in room"},
                new ButtonInfo { buttonText = "Lag All v2", method =() => mods.LagAll(3f, 1000), isTogglable = true, toolTip = "Heavy lag all players in room"},
                new ButtonInfo { buttonText = "Lag Spike Gun", method =() => mods.LagGun(8f, 3500), isTogglable = true, toolTip = "Massive lag spike at target"},
            },

            new ButtonInfo[]
            { // Menu Settings
                new ButtonInfo { buttonText = "Left Hand", enableMethod =() => SettingsMods.LeftHand(), disableMethod =() => SettingsMods.RightHand(), toolTip = "Toggle menu hand", enabled = !rightHanded},
                new ButtonInfo { buttonText = "FPS Counter", enableMethod =() => SettingsMods.EnableFPSCounter(), disableMethod =() => SettingsMods.DisableFPSCounter(), enabled = fpsCounter, toolTip = "Show FPS counter"},
                new ButtonInfo { buttonText = "Setting Button", enableMethod =() => SettingsButton = true, disableMethod =() => SettingsButton = false, enabled = SettingsButton, toolTip = "Show settings button"},
                new ButtonInfo { buttonText = "Leave Button", enableMethod =() => SettingsMods.EnableDisconnectButton(), disableMethod =() => SettingsMods.DisableDisconnectButton(), enabled = disconnectButton, toolTip = "Show disconnect button"},
                new ButtonInfo { buttonText = "Remove All Prefs", method =() => mods.Removeprefs(), isTogglable = false, enabled = false, toolTip = "Reset saved preferences"},
                new ButtonInfo { buttonText = "PPos", overlapText = "Menu Layout: ShibaGT", isTogglable = false, method =() => mods.SwitchPagePos(), enabled = false, toolTip = "Switch menu layout"},
                new ButtonInfo { buttonText = "OutlineMenu", isTogglable = true, enableMethod =() => Main.what3 = true, disableMethod =() => Main.what3 = false, enabled = Main.what3, toolTip = "Toggle menu outline"},
                new ButtonInfo { buttonText = "COC", overlapText = "Outline: Blue", isTogglable = false, method =() => mods.ChangeOutlineColor(), enabled = false, toolTip = "Cycle outline color"},
            },

            new ButtonInfo[]
            { // fun
                new ButtonInfo { buttonText = "Board Spam",  method =() => mods.HoverboardSpam(), isTogglable = true, toolTip = "Hold RG to spam hoverboards"},
                new ButtonInfo { buttonText = "Waterbend",  method =() => mods.WaterSplash(), isTogglable = true, toolTip = "Splash water around hands"},
                new ButtonInfo { buttonText = "Bracelet Spam",  method =() => mods.BraceletSpam(), disableMethod =() => mods.NoBracelet(), isTogglable = true, toolTip = "Spam friend bracelets"},
                new ButtonInfo { buttonText = "Networking Library", enableMethod =() => NetworkingLibrary.Instance.NetworkEnabled = true, disableMethod =() => NetworkingLibrary.Instance.NetworkEnabled = false, toolTip = "Toggle custom networking", enabled = !NetworkingLibrary.Instance.NetworkEnabled },
                new ButtonInfo { buttonText = "Boombox", method = () => BoomboxManager.BoomboxLoop("https://github.com/odinong/Groshable/releases/download/boomboxdiddy/212302951.obj", "https://github.com/odinong/Groshable/releases/download/boomboxdiddy/boomboxmesh.png"), disableMethod = () => BoomboxManager.Kill(), isTogglable = true, toolTip = "Spawn boombox"},
                new ButtonInfo { buttonText = "Change Boombox Audio", method = () => BoomboxManager.OpenNativePicker(), isTogglable = false, toolTip = "Change song"},
                new ButtonInfo { buttonText = "Boombox Volume +", method = () => BoomboxManager.AdjustVolume(0.1f), isTogglable = false, toolTip = "Volume up"},
                new ButtonInfo { buttonText = "Boombox Volume -", method = () => BoomboxManager.AdjustVolume(-0.1f), isTogglable = false, toolTip = "Volume down"},
                new ButtonInfo { buttonText = "Boombox Speed +", method = () => BoomboxManager.AdjustPitchSpeed(0.1f), isTogglable = false, toolTip = "Faster song"},
                new ButtonInfo { buttonText = "Boombox Speed -", method = () => BoomboxManager.AdjustPitchSpeed(-0.1f), isTogglable = false, toolTip = "Slower song"},
                new ButtonInfo { buttonText = "Boombox Visualizer", enableMethod = () => BoomboxManager.UseVisualizer = true, disableMethod = () => BoomboxManager.UseVisualizer = false, isTogglable = true, enabled = BoomboxManager.UseVisualizer, toolTip = "Show visualizer"},
                new ButtonInfo { buttonText = "Visualizer Intensity +", method = () => BoomboxManager.VisualizerIntensity = Mathf.Clamp(BoomboxManager.VisualizerIntensity + 1f, 0f, 10f), isTogglable = false, toolTip = "Bigger bars"},
                new ButtonInfo { buttonText = "Visualizer Intensity -", method = () => BoomboxManager.VisualizerIntensity = Mathf.Clamp(BoomboxManager.VisualizerIntensity - 1f, 0f, 10f), isTogglable = false, toolTip = "Smaller bars"},
                new ButtonInfo { buttonText = "Visualizer Base Scale +", method = () => BoomboxManager.BaseScale = Mathf.Clamp(BoomboxManager.BaseScale + 1f, 0.1f, 10f), isTogglable = false, toolTip = "Wider bars"},
                new ButtonInfo { buttonText = "Visualizer Base Scale -", method = () => BoomboxManager.BaseScale = Mathf.Clamp(BoomboxManager.BaseScale - 1f, 0.1f, 10f), isTogglable = false, toolTip = "Narrower bars"},
                new ButtonInfo { buttonText = "Grosh Holdable", method = () => GroshHolder.GroshLoop("https://github.com/odinong/Groshable/releases/download/gfddfggsdf/Grosh.Holdable.obj", "https://github.com/odinong/Groshable/blob/main/iidktexture.png?raw=true"), disableMethod = () => GroshHolder.Kill(), isTogglable = true, toolTip = "Hold Grosh"},
                new ButtonInfo { buttonText = "Tung Tung Tung Sahur", method = () => SusTung.TungShooter("https://github.com/odinong/Groshable/releases/download/tungtungsahur/TungTungTungSahur.obj", "https://github.com/odinong/Groshable/releases/download/tungtungsahur/shaded.png", "https://github.com/odinong/Groshable/releases/download/tungaudio/tungtung.wav"), disableMethod = () => SusTung.Kill(), isTogglable = true, toolTip = "Hold Tung"},
                new ButtonInfo { buttonText = "Fat Seal Spammer", method = () => FatSealSpammer.SealLoop("https://github.com/odinong/Groshable/raw/refs/heads/main/fatseal.obj", "https://github.com/odinong/Groshable/raw/refs/heads/main/fatseal.jpeg"), disableMethod = () => FatSealSpammer.Kill(), isTogglable = true, toolTip = "Spawn seals"},
                new ButtonInfo { buttonText = "Vape", method = () => Vape.InitVape("https://github.com/odinong/Groshable/raw/refs/heads/main/juul.obj", "https://github.com/odinong/Groshable/blob/main/JUUL_BOI_Color.png?raw=true"), disableMethod = () => Vape.Kill(), isTogglable = true, toolTip = "Hold vape"},
            },

            new ButtonInfo[]
            { // Gun Settings
                //new ButtonInfo { buttonText = "Equip Gun", method =() => mods.EquipGun(), isTogglable = true, toolTip = "Get gun"},
            },

            new ButtonInfo[]
            { // Master
                new ButtonInfo { buttonText = "Projectile Spam (B)", method =() => mods.SnowballSpam(GorillaLocomotion.GTPlayer.Instance.RightHand.velocityTracker.GetAverageVelocity(true, 0f), GTPlayer.Instance.RightHand.controllerTransform.position), enabled = false, isTogglable = true, toolTip = "Hold B to spam"},
                new ButtonInfo { buttonText = "Projectile Gun (B)", method =() => mods.SnowballSpam(GorillaLocomotion.GTPlayer.Instance.RightHand.controllerTransform.forward * 20f, GTPlayer.Instance.RightHand.controllerTransform.position), enabled = false, isTogglable = true, toolTip = "Press B to shoot"},
                new ButtonInfo { buttonText = "Snowball Fling Gun", method =() => mods.FlingGun(), enabled = false, isTogglable = true, toolTip = "Fling snowballs"},
            },
            new ButtonInfo[]
            { // Room
                new ButtonInfo { buttonText = "Disconnect", method =() => { PhotonNetwork.Disconnect(); NetworkSystem.Instance.ReturnToSinglePlayer();}, enabled = false, isTogglable = false, toolTip = "Leave room"},
                new ButtonInfo { buttonText = "B Disconnect", method =() => mods.BDisconnect(), enabled = false, isTogglable = true, toolTip = "Press B to leave"},
                new ButtonInfo { buttonText = "Join Genesis", method =() => mods.Joincodegenesis(), enabled = false, isTogglable = false, toolTip = "Join Genesis"},
                new ButtonInfo { buttonText = "Join Random Room", method =() => mods.JoinRandom(), enabled = false, isTogglable = false, toolTip = "Join random"},
                new ButtonInfo { buttonText = "Connect to Fastest Region", method =() => PhotonNetwork.ConnectToBestCloudServer(), enabled = false, isTogglable = false, toolTip = "Join US Central server"},
                new ButtonInfo { buttonText = "Connect to US Central", method =() => mods.ConnectToRegion("us"), enabled = false, isTogglable = false, toolTip = "Join US Central server"},
                new ButtonInfo { buttonText = "Connect to US West", method =() => mods.ConnectToRegion("usw"), enabled = false, isTogglable = false, toolTip = "Join US West server"},
                new ButtonInfo { buttonText = "Connect to EU", method =() => mods.ConnectToRegion("eu"), enabled = false, isTogglable = false, toolTip = "Join EU server"},
            },
            new ButtonInfo[]
            { // Move Set
                new ButtonInfo{ buttonText = "Change Plat Color", method =() => mods.PlatColorChange(), isTogglable = false, overlapText = "Plat Color: Blue", toolTip = "Change platform color"},
                new ButtonInfo{ buttonText = "Pull Mode", method =() => mods.ChangePullMode(), isTogglable = false, overlapText = "Pull Mode: Legit", toolTip = "Change pull mode"},
            },
            new ButtonInfo[]
            { // Rig
                new ButtonInfo{ buttonText = "Ghost Monkey", method =() => mods.GhostMonke(), isTogglable = true, toolTip = "See-through monkey"},
                new ButtonInfo{ buttonText = "Invis Monkey", method =() => mods.InvisMonke(), isTogglable = true, toolTip = "Invisible monkey"},
                new ButtonInfo{ buttonText = "Long Arms", method =() => mods.LongArms(), disableMethod =() => mods.NormalArms(), isTogglable = true, toolTip = "Long arms"},
                new ButtonInfo{ buttonText = "No Fingers", method =() => mods.NoFinger(), isTogglable = true, toolTip = "No fingers"},
                new ButtonInfo{ buttonText = "Spaz Rig", method =() => mods.SpazRig(), isTogglable = true, toolTip = "Spazzy monkey"},
                new ButtonInfo{ buttonText = "Upside Down Head", method =() => VRRig.LocalRig.head.trackingRotationOffset.z = 180f, disableMethod =() => mods.FixHead(), isTogglable = true, toolTip = "neck upsidedown"},
                new ButtonInfo{ buttonText = "Broken Neck", method =() => VRRig.LocalRig.head.trackingRotationOffset.z = 90f, disableMethod =() => mods.FixHead(), isTogglable = true, toolTip = "broken neck"},
                new ButtonInfo{ buttonText = "Backwards Head", method =() => VRRig.LocalRig.head.trackingRotationOffset.y = 180f, disableMethod =() => mods.FixHead(), isTogglable = true, toolTip = "backwards head"},
            },
            new ButtonInfo[]
            { // Proj Set
                new ButtonInfo{ buttonText = "Big Snowballs", enableMethod =() => mods.biig = true, disableMethod =() => mods.biig = false, isTogglable = true, toolTip = "Giant snowballs"},
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
                new ButtonInfo { buttonText = "home", method =() => SettingsMods.ReturnHome(), isTogglable = false, toolTip = "Go back"},
             },

        };
    }
}