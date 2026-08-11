using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using System.IO;
using UnityEngine;
using static Plon.Menu.Main;

namespace Plon.Patches
{
    [HarmonyPatch(typeof(MonoBehaviourPunCallbacks), "OnPlayerEnteredRoom")]
    internal class JoinPatch : MonoBehaviour
    {
        private static void Prefix(Player newPlayer)
        {
            if (newPlayer != oldnewplayer)
            {
                oldnewplayer = newPlayer;
            }
        }

        private static Player oldnewplayer;
    }
}