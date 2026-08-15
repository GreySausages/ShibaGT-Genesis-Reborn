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
                new ButtonInfo { buttonText = "Gun Settings", method =() => SettingsMods.guardian(), toolTip = "Gun settings", isTogglable = false},
                new ButtonInfo { buttonText = "Menu", method =() => SettingsMods.safety(), toolTip = "Menu settings", isTogglable = false},
                new ButtonInfo { buttonText = "Movement", method =() => SettingsMods.moveset(), toolTip = "Move settings", isTogglable = false},
                new ButtonInfo { buttonText = "Projectiles", method =() => SettingsMods.projset(), toolTip = "Proj settings", isTogglable = false},
                new ButtonInfo { buttonText = "Anti Report", method =() => mods.AntiReport(), toolTip = "Block reports", isTogglable = true, enabled = true},
            },

            new ButtonInfo[]
            { // Advantages
                new ButtonInfo { buttonText = "Tag Gun", method =() => mods.TagGun(), isTogglable = true, toolTip = "Shoot tags"},
                new ButtonInfo { buttonText = "Tag All", method =() => mods.TagAll(), isTogglable = true, toolTip = "Tags everyone"},
                new ButtonInfo { buttonText = "Tag Aura", method =() => mods.TagAura(), isTogglable = true, toolTip = "Auto tag nearby uninfected monkeys"},
                new ButtonInfo { buttonText = "Tag Assist", method =() => mods.TagAssist(), disableMethod =() => mods.TagAssistDisable(), isTogglable = true, toolTip = "Blatantly snaps hands and pulls you towards nearest uninfected monkey"},
                new ButtonInfo { buttonText = "No Tag On Join", method =() => mods.NoTagOnJoin(), isTogglable = true, toolTip = "No tag when joining"},
                new ButtonInfo { buttonText = "No Leaves", method =() => mods.removeleaves(), disableMethod =() => mods.addleaves(), isTogglable = true, toolTip = "Remove leaves"},
                new ButtonInfo { buttonText = "45 FPS", method =() => mods.FPS(45), isTogglable = true, toolTip = "Set 45 FPS"},
                new ButtonInfo { buttonText = "60 FPS", method =() => mods.FPS(60), isTogglable = true, toolTip = "Set 60 FPS"},
                new ButtonInfo { buttonText = "90 FPS", method =() => mods.FPS(90), isTogglable = true, toolTip = "Set 90 FPS"},
                new ButtonInfo { buttonText = "120 FPS", method =() => mods.FPS(120), isTogglable = true, toolTip = "Set 120 FPS"},
                new ButtonInfo { buttonText = "Unlock fps", method =() => { Application.targetFrameRate = int.MaxValue; QualitySettings.vSyncCount = 0; }, disableMethod =() => Application.targetFrameRate = 144, isTogglable = true, enabled = false, toolTip = "Unlocks FPS (doesnt work if nvidia control panel is limiting)" },
                new ButtonInfo { buttonText = "No Tag Freeze", method =() => mods.NoTagFreeze(), isTogglable = true, toolTip = "Remove tag freeze and slowdown"},
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
                new ButtonInfo { buttonText = "Legit Slide Control", method =() => mods.SlideControl(0.05f), disableMethod =() => mods.SlideControl(0.00425f), isTogglable = true, toolTip = "Slightly more slide control"},
                new ButtonInfo { buttonText = "Blatant Slide Control", method =() => mods.SlideControl(0.08f), disableMethod =() => mods.SlideControl(0.00425f), isTogglable = true, toolTip = "High slide control"},
                new ButtonInfo { buttonText = "Grappling Hook", method =() => mods.GrapplingHook(), disableMethod =() => mods.GrapplingHookDisable(), isTogglable = true, toolTip = "Aim and pull with grappling hook"},
                new ButtonInfo { buttonText = "Air Swim", method =() => mods.AirSwim(), disableMethod =() => mods.AirSwimDisable(), isTogglable = true, toolTip = "Swim through the air"},
                new ButtonInfo { buttonText = "Jesus Monke", method =() => mods.JesusMonke(), disableMethod =() => mods.JesusMonkeDisable(), isTogglable = true, toolTip = "Walk and slide on water surfaces"},
                new ButtonInfo { buttonText = "Zipline Speed", method =() => mods.ZiplineSpeed(35f), disableMethod =() => mods.ZiplineSpeed(10f), isTogglable = true, toolTip = "Increase zipline speed"},
                new ButtonInfo { buttonText = "Catapult", method =() => mods.Catapult(), isTogglable = true, toolTip = "Shoot pointer to launch yourself"},
                new ButtonInfo { buttonText = "Sticky Hands", method =() => mods.StickyHands(), disableMethod =() => mods.ResetStickyHands(), isTogglable = true, toolTip = "Hold grip on surfaces to stick"},
                new ButtonInfo { buttonText = "PiggyBack", method =() => mods.PiggyBack(), disableMethod =() => mods.PiggyBackDisable(), isTogglable = true, toolTip = "Ride on another player's back"},
                new ButtonInfo { buttonText = "Follow Player", method =() => mods.FollowPlayer(), disableMethod =() => mods.FollowPlayerDisable(), isTogglable = true, toolTip = "Always follow slightly behind a player"},
                new ButtonInfo { buttonText = "Ender Pearl", method =() => mods.EnderPearl(), disableMethod =() => mods.EnderPearlDisable(), isTogglable = true, toolTip = "Grip to hold pearl, release to throw and teleport"},
                new ButtonInfo { buttonText = "Zipline Gun", method =() => mods.ZiplineGun(), disableMethod =() => mods.ZiplineGunDisable(), isTogglable = true, toolTip = "Shoot to create rideable zipline"},
            },

            new ButtonInfo[]
            { // visuals
                new ButtonInfo { buttonText = "Tracers", method =() => mods.Tracers(), isTogglable = true, toolTip = "Draw lines to players"},
                new ButtonInfo { buttonText = "Infection Chams", method =() => mods.FullBodyESP(), disableMethod =() => mods.DisableFullBodyESP(), isTogglable = true, toolTip = "Highlight infected players"},
                new ButtonInfo { buttonText = "RGB Monke (stump)", method =() => mods.RGB(), isTogglable = true, toolTip = "Cycle player colors in stump"},
                new ButtonInfo { buttonText = "Strobe Monke (stump)", method =() => mods.RGB(true), isTogglable = true, toolTip = "Rapidly strobe player colors in stump"},
                new ButtonInfo { buttonText = "Casual Chams", method =() => mods.CasualFullBodyESP(), disableMethod =() => mods.DisableFullBodyESP(), isTogglable = true, toolTip = "Highlight all players"},
                new ButtonInfo { buttonText = "Skeleton ESP", method =() => mods.SkeletonESP(), isTogglable = true, toolTip = "Draw bone skeletons on players"},
                new ButtonInfo { buttonText = "2D Box ESP", method =() => mods.TwoDBoxESP(), isTogglable = true, toolTip = "Draw filled 2D player boxes"},
                new ButtonInfo { buttonText = "3D Box ESP", method =() => mods.BoxESP(), isTogglable = true, toolTip = "Draw 3D wireframe boxes"},
                new ButtonInfo { buttonText = "Name Tags", method =() => mods.NameAndDistanceTags(), isTogglable = true, toolTip = "Show player name and distance"},
                new ButtonInfo { buttonText = "cursedgtag", overlapText = "Cursed Mode: Off", method =() => mods.CursedGTAG(), isTogglable = false, toolTip = "Change cursed time override"},
                new ButtonInfo { buttonText = "Time Switcher", overlapText = "Time: Default", method =() => mods.TimeSwitcher(), isTogglable = false, toolTip = "Change time of day"},
                new ButtonInfo { buttonText = "Weather Switcher", overlapText = "Weather: Default", method =() => mods.CycleWeather(), isTogglable = false, toolTip = "Change weather mode (Rain/Clear)"},
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
                new ButtonInfo { buttonText = "Panic Button", enableMethod =() => mods.EnablePanic(), disableMethod =() => mods.DisablePanic(), isTogglable = true, toolTip = "Disable all mods and disconnect safely"},
            },

            new ButtonInfo[]
            { // fun
                new ButtonInfo { buttonText = "Board Spam",  method =() => mods.HoverboardSpam(), isTogglable = true, toolTip = "Hold RG to spam hoverboards"},
                new ButtonInfo { buttonText = "Loud Microphone", method =() => mods.LoudMicrophone(), disableMethod =() => mods.ResetMicrophoneVolume(), isTogglable = true, toolTip = "Boost microphone volume (25x)"},
                new ButtonInfo { buttonText = "Earrape Mic", method =() => mods.LoudMicrophone(25f), disableMethod =() => mods.ResetMicrophoneVolume(), isTogglable = true, toolTip = "Extreme microphone volume boost (100x)"},
                new ButtonInfo { buttonText = "Mute Microphone", method =() => mods.MuteMicrophone(), disableMethod =() => mods.UnmuteMicrophone(), isTogglable = true, toolTip = "Mute local microphone transmission"},
                new ButtonInfo { buttonText = "Microphone Echo", method =() => mods.MicrophoneEcho(true), disableMethod =() => mods.MicrophoneEcho(false), isTogglable = true, toolTip = "Echo your voice for other players"},
                new ButtonInfo { buttonText = "Chipmunk Mic", method =() => mods.SetMicrophonePitch(1.6f), disableMethod =() => mods.ResetMicrophonePitch(), isTogglable = true, toolTip = "High pitch voice modulation"},
                new ButtonInfo { buttonText = "Deep Voice Mic", method =() => mods.SetMicrophonePitch(0.6f), disableMethod =() => mods.ResetMicrophonePitch(), isTogglable = true, toolTip = "Deep voice modulation"},
                new ButtonInfo { buttonText = "Fix Microphone", method =() => mods.FixMicrophone(), isTogglable = false, toolTip = "Reset and repair microphone settings"},
                new ButtonInfo { buttonText = "Hear Self", method =() => mods.HearSelf(true), disableMethod =() => mods.HearSelf(false), isTogglable = true, toolTip = "Hear your own microphone live to test audio"},
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
                new ButtonInfo { buttonText = "Triple T Holdable", method = () => SusTung.TungShooter("https://github.com/odinong/Groshable/releases/download/tungtungsahur/TungTungTungSahur.obj", "https://github.com/odinong/Groshable/releases/download/tungtungsahur/shaded.png", "https://github.com/odinong/Groshable/releases/download/tungaudio/tungtung.wav"), disableMethod = () => SusTung.Kill(), isTogglable = true, toolTip = "Hold Tung"},
                new ButtonInfo { buttonText = "Fat Seal Spammer", method = () => FatSealSpammer.SealLoop("https://github.com/odinong/Groshable/raw/refs/heads/main/fatseal.obj", "https://github.com/odinong/Groshable/raw/refs/heads/main/fatseal.jpeg"), disableMethod = () => FatSealSpammer.Kill(), isTogglable = true, toolTip = "Spawn seals"},
                new ButtonInfo { buttonText = "Vape", method = () => Vape.InitVape("https://github.com/odinong/Groshable/raw/refs/heads/main/juul.obj", "https://github.com/odinong/Groshable/blob/main/JUUL_BOI_Color.png?raw=true"), disableMethod = () => Vape.Kill(), isTogglable = true, toolTip = "Hold vape"},
                new ButtonInfo { buttonText = "Splash Gun", method =() => mods.SplashGun(), isTogglable = true, toolTip = "Shoot water splashes at pointer"},
                new ButtonInfo { buttonText = "Splash Hands", method =() => mods.SplashHands(), isTogglable = true, toolTip = "Hold grips to splash water from hands"},
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
                new ButtonInfo { buttonText = "Disconnect", method =() => { NetworkSystem.Instance.ReturnToSinglePlayer(); PhotonNetwork.Disconnect(); }, enabled = false, isTogglable = false, toolTip = "Leave room"},
                new ButtonInfo { buttonText = "B Disconnect", method =() => mods.BDisconnect(), enabled = false, isTogglable = true, toolTip = "Press B to leave"},
                new ButtonInfo { buttonText = "Join Genesis", method =() => mods.Joincodegenesis(), enabled = false, isTogglable = false, toolTip = "Join Genesis"},
                new ButtonInfo { buttonText = "Join Random Room", method =() => mods.JoinRandom(), enabled = false, isTogglable = false, toolTip = "Join random"},
                new ButtonInfo { buttonText = "Connect to Fastest Region", method =() => PhotonNetwork.ConnectToBestCloudServer(), enabled = false, isTogglable = false, toolTip = "Join US Central server"},
                new ButtonInfo { buttonText = "Connect to US Central", method =() => mods.ConnectToRegion("us"), enabled = false, isTogglable = false, toolTip = "Join US Central server"},
                new ButtonInfo { buttonText = "Connect to US West", method =() => mods.ConnectToRegion("usw"), enabled = false, isTogglable = false, toolTip = "Join US West server"},
                new ButtonInfo { buttonText = "Connect to EU", method =() => mods.ConnectToRegion("eu"), enabled = false, isTogglable = false, toolTip = "Join EU server"},
                new ButtonInfo { buttonText = "Mute Gun", method =() => mods.MuteGun(), isTogglable = true, toolTip = "Shoot to mute player"},
                new ButtonInfo { buttonText = "Mute All", method =() => mods.MuteAll(), enabled = false, isTogglable = false, toolTip = "Mute all players"},
                new ButtonInfo { buttonText = "Unmute All", method =() => mods.UnmuteAll(), enabled = false, isTogglable = false, toolTip = "Unmute all players"},
                new ButtonInfo { buttonText = "Report Gun", method =() => mods.ReportGun(), isTogglable = true, toolTip = "Shoot to report player"},
                new ButtonInfo { buttonText = "Report All", method =() => mods.ReportAll(), enabled = false, isTogglable = false, toolTip = "Report all players"},
                new ButtonInfo { buttonText = "Copy Identity", method =() => mods.CopyPlayerIdentity(), isTogglable = true, toolTip = "Shoot player to copy name and color"},
                new ButtonInfo { buttonText = "Lobby Hop", method =() => mods.LobbyHop(), isTogglable = false, toolTip = "Disconnect and join new random room"},
            },
            new ButtonInfo[]
            { // Move Set
                new ButtonInfo{ buttonText = "Change Plat Color", method =() => mods.PlatColorChange(), isTogglable = false, overlapText = "Plat Color: Blue", toolTip = "Change platform color"},
                new ButtonInfo{ buttonText = "Pull Mode", method =() => mods.ChangePullMode(), isTogglable = false, overlapText = "Pull Mode: Legit", toolTip = "Change pull mode"},
            },
            new ButtonInfo[]
            { // Rig
                new ButtonInfo{ buttonText = "Ghost Monkey", method =() => mods.GhostMonke(), isTogglable = true, toolTip = "Ghost Monkey"},
                new ButtonInfo{ buttonText = "Invis Monkey", method =() => mods.InvisMonke(), isTogglable = true, toolTip = "Invisible monkey"},
                new ButtonInfo{ buttonText = "Long Arms", method =() => mods.LongArms(), disableMethod =() => mods.NormalArms(), isTogglable = true, toolTip = "Long arms"},
                new ButtonInfo{ buttonText = "No Fingers", method =() => mods.NoFinger(), isTogglable = true, toolTip = "No fingers"},
                new ButtonInfo{ buttonText = "Spaz Rig", method =() => mods.SpazRig(), isTogglable = true, toolTip = "Spazzy monkey"},
                new ButtonInfo{ buttonText = "Upside Down Head", method =() => VRRig.LocalRig.head.trackingRotationOffset.z = 180f, disableMethod =() => mods.FixHead(), isTogglable = true, toolTip = "neck upsidedown"},
                new ButtonInfo{ buttonText = "Broken Neck", method =() => VRRig.LocalRig.head.trackingRotationOffset.z = 90f, disableMethod =() => mods.FixHead(), isTogglable = true, toolTip = "broken neck"},
                new ButtonInfo{ buttonText = "Backwards Head", method =() => VRRig.LocalRig.head.trackingRotationOffset.y = 180f, disableMethod =() => mods.FixHead(), isTogglable = true, toolTip = "backwards head"},
                new ButtonInfo{ buttonText = "Head Spinner", method =() => mods.HeadSpinner(), disableMethod =() => mods.FixHead(), isTogglable = true, toolTip = "Spin head continuously"},
                new ButtonInfo{ buttonText = "Helicopter Monkey", method =() => mods.HelicopterMonkey(), disableMethod =() => mods.FixHead(), isTogglable = true, toolTip = "Spin rig rapidly"},
                new ButtonInfo{ buttonText = "Face Expression", overlapText = "Face: Default", method =() => mods.CycleFaceExpression(), isTogglable = false, toolTip = "Cycle facial expressions"},
                new ButtonInfo{ buttonText = "T-Pose", method =() => mods.TPose(), isTogglable = true, toolTip = "T-pose your monkey rig"},
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