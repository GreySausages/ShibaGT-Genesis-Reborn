using HarmonyLib;
using Photon.Pun;
using ShibaGTGenesisReborn.Menu;
using ShibaGTGenesisReborn.Mods;
using System;
using UnityEngine;

namespace ShibaGTGenesisReborn.Patches
{
    [HarmonyPatch(typeof(MonkeAgent), "SendReport")]
    internal class AntiCheat : MonoBehaviour
    {
        private static bool Prefix(string susReason, string susId, string susNick)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(MonkeAgent), "LogErrorCount")]
    public class NoLogErrorCount : MonoBehaviour
    {
        private static bool Prefix(string logString, string stackTrace, LogType type)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(MonkeAgent), "CloseInvalidRoom")]
    public class NoCloseInvalidRoom : MonoBehaviour
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(MonkeAgent), "CheckReports")]
    public class NoCheckReports : MonoBehaviour
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(MonkeAgent), "QuitDelay", MethodType.Enumerator)]
    public class NoQuitDelay : MonoBehaviour
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(MonkeAgent), "IncrementRPCCallLocal")]
    public class NoIncrementRPCCallLocal : MonoBehaviour
    {
        private static bool Prefix(PhotonMessageInfoWrapped infoWrapped, string rpcFunction)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(MonkeAgent), "GetRPCCallTracker")]
    internal class NoGetRPCCallTracker : MonoBehaviour
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(MonkeAgent), "DispatchReport")]
    public class NoDispatchReport : MonoBehaviour
    {
        private static bool Prefix()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(MonkeAgent), "IncrementRPCCall", new Type[] { typeof(PhotonMessageInfo), typeof(string) })]
    public class NoIncrementRPCCall : MonoBehaviour
    {
        private static bool Prefix(PhotonMessageInfo info, string callingMethod = "")
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(MonkeAgent), "IncrementRPCCall", new Type[] { typeof(PhotonMessageInfoWrapped), typeof(string) })]
    public class NoIncrementRPCCallWrapped : MonoBehaviour
    {
        private static bool Prefix(PhotonMessageInfoWrapped infoWrapped, string callingMethod = "")
        {
            return false;
        }
    }

    // Thanks DrPerky
    [HarmonyPatch(typeof(VRRig), "IncrementRPC", new Type[] { typeof(PhotonMessageInfoWrapped), typeof(string) })]
    public class NoIncrementRPC : MonoBehaviour
    {
        private static bool Prefix(PhotonMessageInfoWrapped info, string sourceCall)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(VRRig), "IncrementRPC", new Type[] { typeof(PhotonMessageInfo), typeof(string) })]
    public class NoIncrementRPCUnwrapped : MonoBehaviour
    {
        private static bool Prefix(PhotonMessageInfo info, string sourceCall)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(GorillaTag.Audio.GTMicWrapper), "Read")]
    public class GTMicWrapperEchoPatch
    {
        private static readonly float[] delayBuffer = new float[48000];
        private static int writeHead;

        private static void Postfix(bool __result, float[] buffer)
        {
            if (!__result || !mods.microphoneEchoForOthers || buffer == null || buffer.Length == 0)
            {
                return;
            }

            int delaySamples = (int)(16000 * mods.echoDelaySeconds);
            if (delaySamples <= 0 || delaySamples >= delayBuffer.Length)
            {
                delaySamples = 4000;
            }

            float decay = Mathf.Clamp(mods.echoDecayFactor, 0.1f, 0.9f);

            for (int i = 0; i < buffer.Length; i++)
            {
                int readIndex = (writeHead - delaySamples + delayBuffer.Length) % delayBuffer.Length;
                float delayedSample = delayBuffer[readIndex];

                float mixed = buffer[i] + delayedSample * decay;
                buffer[i] = Mathf.Clamp(mixed, -1f, 1f);

                delayBuffer[writeHead] = buffer[i];
                writeHead = (writeHead + 1) % delayBuffer.Length;
            }
        }
    }
}
