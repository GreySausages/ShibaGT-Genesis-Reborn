using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using ShibaGTGenesisReborn.Libs;
using ShibaGTGenesisReborn.Menu;
using UnityEngine;

namespace ShibaGTGenesisReborn.Mods
{
    public partial class mods
    {
        public static int lagindex;
        public static int[] lagthings =
        {
            300,
            1250,
            3000
        };

        public static readonly string[] lagnames =
        {
            "Weak",
            "Strong",
            "Spike",
        };

        public static float[] lagcooldowns =
        {
            0.5f,
            3f,
            8f
        };

        public static void lagchange()
        {
            lagindex = (lagindex + 1) % lagthings.Length;
            Main.GetIndex("lagpwr").overlapText = "Lag Power: " + lagnames[lagindex];
        }

        public static float tagTimer;
        public static float CDown;

        public static void LagGun()
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null)
                {
                    if (Time.time > CDown)
                    {
                        for (int i = 0; i < lagthings[lagindex]; i++)
                        {
                            SendOPRaiseEvent202(GunLib.LockedPlayer);
                        }
                        CDown = Time.time + lagcooldowns[lagindex];
                    }
                }
            }, true);
        }

        public static void LagAll()
        {
            if (Time.time > CDown)
            {
                for (int i = 0; i < lagthings[lagindex]; i++)
                {
                    SendOPRaiseEvent202();
                }
                CDown = Time.time + lagcooldowns[lagindex];
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
