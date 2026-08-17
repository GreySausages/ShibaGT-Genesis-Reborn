using GorillaNetworking;
using GorillaTag.Audio;
using Photon.Pun;
using Photon.Realtime;
using Photon.Voice.Unity;
using ShibaGTGenesisReborn.Classes;
using ShibaGTGenesisReborn.Libs;
using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ShibaGTGenesisReborn.Mods
{
    public partial class mods
    {
        public static string lastmap;
        private static float actionDelay;

        public static void BDisconnect()
        {
            if (InputHandler.Instance.RightSecondary.IsPressed)
            {
                NetworkSystem.Instance.ReturnToSinglePlayer();
                PhotonNetwork.Disconnect();
            }
        }

        public static void Joincodegenesis()
        {
            PhotonNetworkController.Instance.AttemptToJoinSpecificRoom("GENESIS", GorillaNetworking.JoinType.Solo);
        }

        public static void JoinRandom()
        {
            if (PhotonNetworkController.Instance.currentJoinTrigger.networkZone != null)
            {
                lastmap = PhotonNetworkController.Instance.currentJoinTrigger.networkZone;
            }
            if (!NetworkSystem.Instance.InRoom)
            {
                PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.GetJoinTriggerForZone(lastmap), GorillaNetworking.JoinType.Solo);
            }
        }

        public static void ConnectToRegion(string region)
        {
            if (PhotonNetwork.CloudRegion != region) PhotonNetwork.ConnectToRegion(region);
            NetworkSystem.Instance.currentRegionIndex = Array.IndexOf(NetworkSystem.Instance.regionNames, region);
        }

        public static void RPCProt()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            try
            {
                if (MonkeAgent.instance != null)
                {
                    MonkeAgent.instance.rpcErrorMax = int.MaxValue;
                    MonkeAgent.instance.rpcCallLimit = int.MaxValue;
                    MonkeAgent.instance.logErrorMax = int.MaxValue;
                    // MonkeAgent.instance.logErrorCount = 0;
                    MonkeAgent.instance.userDecayTime = 0f;
                    // MonkeAgent.instance.lastCheck = float.MaxValue;
                    // MonkeAgent.instance.reportCheckCooldown = float.MaxValue;
                    // MonkeAgent.instance.testAssault = false;
                    // MonkeAgent.instance._sendReport = false;
                    // MonkeAgent.instance._suspiciousPlayerId = "";
                    // MonkeAgent.instance._suspiciousPlayerName = "";
                    // MonkeAgent.instance._suspiciousReason = "";

                    MonkeAgent.instance.reportedPlayers?.Clear();
                    MonkeAgent.instance.userRPCCalls?.Clear();

                    Application.logMessageReceived -= MonkeAgent.instance.LogErrorCount;
                    GorillaSlicerSimpleManager.UnregisterSliceable(MonkeAgent.instance, GorillaSlicerSimpleManager.UpdateStep.Update);
                }

                PhotonNetwork.MaxResendsBeforeDisconnect = int.MaxValue;
                PhotonNetwork.QuickResends = int.MaxValue;

                /*
                if (PhotonNetwork.NetworkingClient != null && PhotonNetwork.NetworkingClient.LoadBalancingPeer != null)
                {
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 60000;
                    PhotonNetwork.NetworkingClient.LoadBalancingPeer.SentCountAllowance = int.MaxValue;
                }

                if (GorillaScoreboardTotalUpdater.instance != null && GorillaScoreboardTotalUpdater.instance.reportDict != null)
                {
                    GorillaScoreboardTotalUpdater.instance.reportDict.Clear();
                }

                while (GorillaTelemetry.telemetryEventsQueueMothership != null && GorillaTelemetry.telemetryEventsQueueMothership.TryDequeue(out _)) { }
                */
            }
            catch { /* if it goes here its a skill issue */ }
        }

        /*
        public static void lbaction(GorillaPlayerLineButton.ButtonType type, NetPlayer player = null, bool? state = null)
        {
            if (type == GorillaPlayerLineButton.ButtonType.Mute)
            {
                foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
                {
                    if (player == null ? (state == true ? !line.muteButton.isAutoOn : line.muteButton.isAutoOn) : line.linePlayer == player)
                    {
                        bool on = state ?? !line.muteButton.isOn;
                        line.muteButton.isOn = on;
                        line.PressButton(on, GorillaPlayerLineButton.ButtonType.Mute);
                        if (player != null) break;
                    }
                }
            }
            else
            {
                if (player != null)
                    GorillaPlayerScoreboardLine.ReportPlayer(player.UserId, type, player.NickName);
                else
                    foreach (var p in NetworkSystem.Instance.PlayerListOthers)
                        GorillaPlayerScoreboardLine.ReportPlayer(p.UserId, type, p.NickName);
            }
        }
        */

        public static void MutePlayer(NetPlayer player, bool shouldMute)
        {
            if (player == null || string.IsNullOrEmpty(player.UserId)) return;

            int muteValue = shouldMute ? 1 : 0;
            PlayerPrefs.SetInt(player.UserId, muteValue);
            PlayerPrefs.Save();

            if (VRRigCache.Instance != null && VRRigCache.Instance.TryGetVrrig(player, out RigContainer rigContainer) && rigContainer != null)
            {
                rigContainer.hasManualMute = true;
                rigContainer.SetMuted(RigContainer.MuteReason.Manual, shouldMute);
            }

            GorillaScoreboardTotalUpdater.ReportMute(player, muteValue);

            foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer == player && line.muteButton != null)
                {
                    line.muteButton.isOn = shouldMute;
                    line.muteButton.UpdateColor();
                }
            }
        }

        public static void ReportPlayer(NetPlayer player)
        {
            if (player == null || string.IsNullOrEmpty(player.UserId)) return;

            string targetNickName = player.NickName ?? player.DefaultName ?? "GORILLA";

            GorillaPlayerScoreboardLine.ReportPlayer(player.UserId, GorillaPlayerLineButton.ButtonType.Cheating, targetNickName);

            if (GorillaScoreboardTotalUpdater.hasInstance && player.ActorNumber != -1)
            {
                var updater = GorillaScoreboardTotalUpdater.instance;

                if (updater.reportDict.TryGetValue(player.ActorNumber, out var existingReports))
                {
                    existingReports.cheating = true;
                    existingReports.pressedReport = true;
                    updater.reportDict[player.ActorNumber] = existingReports;
                }
                else
                {
                    updater.reportDict[player.ActorNumber] = new GorillaScoreboardTotalUpdater.PlayerReports
                    {
                        cheating = true,
                        pressedReport = true
                    };
                }
            }

            foreach (var line in GorillaScoreboardTotalUpdater.allScoreboardLines)
            {
                if (line.linePlayer == player && line.reportButton != null)
                {
                    line.reportButton.isOn = true;
                    line.reportButton.UpdateColor();
                }
            }
        }

        public static void lbaction(GorillaPlayerLineButton.ButtonType type, NetPlayer player = null, bool? state = null)
        {
            if (type == GorillaPlayerLineButton.ButtonType.Mute)
            {
                if (player != null)
                {
                    bool shouldMute = state ?? (PlayerPrefs.GetInt(player.UserId, 0) == 0);
                    MutePlayer(player, shouldMute);
                }
                else
                {
                    foreach (NetPlayer otherPlayer in NetworkSystem.Instance.PlayerListOthers)
                    {
                        bool shouldMute = state ?? (PlayerPrefs.GetInt(otherPlayer.UserId, 0) == 0);
                        MutePlayer(otherPlayer, shouldMute);
                    }
                }
            }
            else
            {
                if (player != null)
                    ReportPlayer(player);
                else
                    foreach (NetPlayer otherPlayer in NetworkSystem.Instance.PlayerListOthers)
                        ReportPlayer(otherPlayer);
            }
        }

        public static void MuteGun()
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null && !GunLib.LockedPlayer.isOfflineVRRig && Time.time > actionDelay)
                {
                    lbaction(GorillaPlayerLineButton.ButtonType.Mute, GunLib.LockedPlayer.Creator);
                    actionDelay = Time.time + 0.5f;
                }
            }, true);
        }

        public static void MuteAll() => lbaction(GorillaPlayerLineButton.ButtonType.Mute, state: true);
        public static void UnmuteAll() => lbaction(GorillaPlayerLineButton.ButtonType.Mute, state: false);

        private static Recorder GetActiveRecorder()
        {
            if (NetworkSystem.Instance?.LocalRecorder != null) return NetworkSystem.Instance.LocalRecorder;
            if (NetworkSystem.Instance?.VoiceConnection?.PrimaryRecorder != null) return NetworkSystem.Instance.VoiceConnection.PrimaryRecorder;
            if (GorillaTagger.Instance?.myRecorder != null) return GorillaTagger.Instance.myRecorder;
            return Object.FindFirstObjectByType<GTRecorder>() ?? (Recorder)Object.FindFirstObjectByType<Recorder>();
        }

        public static void LoudMicrophone(float volumeMultiplier = 15f)
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder == null) return;

            if (recorder is GTRecorder gtRecorder)
            {
                gtRecorder.AllowVolumeAdjustment = true;
                gtRecorder.VolumeAdjustment = volumeMultiplier;
            }

            recorder.VoiceDetection = false;
            recorder.TransmitEnabled = true;
        }

        public static void ResetMicrophoneVolume()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder == null) return;

            if (recorder is GTRecorder gtRecorder)
            {
                gtRecorder.AllowVolumeAdjustment = false;
                gtRecorder.VolumeAdjustment = 1f;
            }

            recorder.VoiceDetection = true;
            recorder.VoiceDetectionThreshold = 0.07f;
        }

        public static void MuteMicrophone()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder == null) return;

            if (recorder is GTRecorder gtRecorder)
            {
                gtRecorder.AllowVolumeAdjustment = true;
                gtRecorder.VolumeAdjustment = 0f;
            }

            recorder.TransmitEnabled = false;
            recorder.VoiceDetectionThreshold = 1f;

            if (GorillaTagger.Instance?.offlineVRRig != null)
            {
                GorillaTagger.Instance.offlineVRRig.shouldSendSpeakingLoudness = false;
            }
        }

        public static void UnmuteMicrophone()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder == null) return;

            if (recorder is GTRecorder gtRecorder)
            {
                gtRecorder.AllowVolumeAdjustment = false;
                gtRecorder.VolumeAdjustment = 1f;
            }

            recorder.TransmitEnabled = true;
            recorder.VoiceDetectionThreshold = 0.07f;

            if (GorillaTagger.Instance?.offlineVRRig != null)
            {
                GorillaTagger.Instance.offlineVRRig.shouldSendSpeakingLoudness = true;
            }
        }

        public static bool microphoneEchoForOthers;
        public static float echoDelaySeconds = 0.25f;
        public static float echoDecayFactor = 0.55f;

        public static void MicrophoneEcho(bool enableEcho = true)
        {
            if (!NetworkSystem.Instance.InRoom) return;
            microphoneEchoForOthers = enableEcho;
        }

        public static void HearSelf(bool enable = true)
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder == null) return;

            recorder.DebugEchoMode = enable;
            if (enable && !recorder.TransmitEnabled) recorder.TransmitEnabled = true;
        }

        public static void SetMicrophonePitch(float pitch)
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder is GTRecorder gtRecorder)
            {
                gtRecorder.AllowPitchAdjustment = true;
                gtRecorder.PitchAdjustment = pitch;
            }
        }

        public static void ResetMicrophonePitch()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            Recorder recorder = GetActiveRecorder();
            if (recorder is GTRecorder gtRecorder)
            {
                gtRecorder.AllowPitchAdjustment = false;
                gtRecorder.PitchAdjustment = 1f;
            }
        }

        public static void FixMicrophone()
        {
            if (!NetworkSystem.Instance.InRoom) return;
            microphoneEchoForOthers = false;

            Recorder recorder = GetActiveRecorder();
            if (recorder != null)
            {
                recorder.SourceType = Recorder.InputSourceType.Microphone;
                recorder.AudioClip = null;
                recorder.DebugEchoMode = false;
                recorder.TransmitEnabled = true;
                recorder.VoiceDetection = true;
                recorder.VoiceDetectionThreshold = 0.07f;
                recorder.VoiceDetectionDelayMs = 500;
                recorder.RecordOnlyWhenJoined = true;
                recorder.StopRecordingWhenPaused = false;

                if (recorder is GTRecorder gtRecorder)
                {
                    gtRecorder.AllowVolumeAdjustment = false;
                    gtRecorder.VolumeAdjustment = 1f;
                    gtRecorder.AllowPitchAdjustment = false;
                    gtRecorder.PitchAdjustment = 1f;
                }

                recorder.RestartRecording(true);
            }

            if (GorillaTagger.Instance?.offlineVRRig != null)
            {
                GorillaTagger.Instance.offlineVRRig.remoteUseReplacementVoice = false;
                GorillaTagger.Instance.offlineVRRig.localUseReplacementVoice = false;
                GorillaTagger.Instance.offlineVRRig.shouldSendSpeakingLoudness = true;
            }

            if (GorillaComputer.instance != null)
            {
                GorillaComputer.instance.voiceChatOn = "TRUE";
            }
        }

        public static void ReportGun()
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null && !GunLib.LockedPlayer.isOfflineVRRig && Time.time > actionDelay)
                {
                    lbaction(GorillaPlayerLineButton.ButtonType.Cheating, GunLib.LockedPlayer.Creator);
                    actionDelay = Time.time + 0.3f;
                }
            }, true);
        }

        public static void ReportAll() => lbaction(GorillaPlayerLineButton.ButtonType.Cheating);

        public static void CopyPlayerIdentity()
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null && !GunLib.LockedPlayer.isOfflineVRRig)
                {
                    string targetName = GunLib.LockedPlayer.Creator != null ? GunLib.LockedPlayer.Creator.NickName : GunLib.LockedPlayer.playerNameVisible;
                    if (!string.IsNullOrEmpty(targetName))
                    {
                        NetworkSystem.Instance.SetMyNickName(targetName);
                        GorillaComputer.instance.currentName = targetName;
                        GorillaComputer.instance.savedName = targetName;
                        GorillaTagger.Instance.offlineVRRig.SetNameTagText(targetName);
                        PhotonNetwork.LocalPlayer.NickName = targetName;
                        PlayerPrefs.SetString("playerName", targetName);
                    }

                    Color targetColor = GunLib.LockedPlayer.playerColor;
                    GorillaTagger.Instance.myVRRig.SendRPC("RPC_InitializeNoobMaterial", RpcTarget.All, targetColor.r, targetColor.g, targetColor.b);
                    PlayerPrefs.SetFloat("redValue", targetColor.r);
                    PlayerPrefs.SetFloat("greenValue", targetColor.g);
                    PlayerPrefs.SetFloat("blueValue", targetColor.b);
                    PlayerPrefs.Save();
                    VRRig.LocalRig.SetColor(targetColor);
                    GorillaTagger.Instance.offlineVRRig.SetColor(targetColor);
                }
            }, true);
        }

        public static void LobbyHop()
        {
            if (PhotonNetworkController.Instance.currentJoinTrigger?.networkZone != null)
                lastmap = PhotonNetworkController.Instance.currentJoinTrigger.networkZone;

            PhotonNetwork.Disconnect();
            NetworkSystem.Instance.ReturnToSinglePlayer();
            GorillaTagger.Instance.StartCoroutine(LobbyHopRoutine());
        }

        private static IEnumerator LobbyHopRoutine()
        {
            while (NetworkSystem.Instance.InRoom || NetworkSystem.Instance.InRoom)
                yield return null;

            yield return new WaitForSeconds(0.2f);
            PhotonNetworkController.Instance.AttemptToJoinPublicRoom(GorillaComputer.instance.GetJoinTriggerForZone(lastmap ?? "forest"), GorillaNetworking.JoinType.Solo);
        }
    }
}
