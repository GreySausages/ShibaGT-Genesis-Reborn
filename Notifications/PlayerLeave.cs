using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine;
using static Plon.Menu.Main;

namespace Plon.Patches
{
    [HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnPlayerLeftRoom")]
    internal class LeavePatch : MonoBehaviour
    {
        private static void Prefix(Player otherPlayer)
        {
            if (otherPlayer != PhotonNetwork.LocalPlayer && otherPlayer != a)
            {
                a = otherPlayer;
            }
        }

        private static Player a;
    }
}