using ExitGames.Client.Photon;
using GorillaGameModes;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Pun;
using ShibaGTGenesisReborn.Libs;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace ShibaGTGenesisReborn.Mods
{
    public partial class mods
    {
        public static string _leavesName;
        public static readonly List<GameObject> leaves = new List<GameObject>();
        private static readonly Dictionary<string, GameObject> objectPool = new Dictionary<string, GameObject>();

        public static void TagGun()
        {
            GunLib.StartGun(() =>
            {
                if (GunLib.LockedPlayer != null &&
                    !GunLib.LockedPlayer.mainSkin.material.name.Contains("fected") &&
                    !GunLib.LockedPlayer.isOfflineVRRig)
                {
                    if (VRRig.LocalRig.mainSkin.material.name.Contains("fected"))
                    {
                        VRRig.LocalRig.enabled = false;
                        VRRig.LocalRig.rightHandTransform.position = GunLib.LockedPlayer.headConstraint.position;
                        VRRig.LocalRig.leftHandTransform.position = GunLib.LockedPlayer.headConstraint.position;
                        VRRig.LocalRig.transform.position = GunLib.LockedPlayer.headConstraint.position;
                        GameMode.ReportTag(GunLib.LockedPlayer.Creator);
                        VRRig.LocalRig.enabled = true;
                    }
                }
            }, true);
        }

        public static void TagPlayer(VRRig p)
        {
            if (!p.mainSkin.material.name.Contains("fected") && VRRig.LocalRig.mainSkin.material.name.Contains("fected"))
            {
                GorillaGameModes.GameMode.ReportTag(PhotonNetwork.CurrentRoom.GetPlayer(p.Creator.ActorNumber));

                VRRig.LocalRig.enabled = false;
                GorillaTagger.Instance.offlineVRRig.enabled = false;

                GorillaTagger.Instance.offlineVRRig.rightHandTransform.position = p.bodyTransform.position;
                GorillaTagger.Instance.offlineVRRig.leftHandTransform.position = p.bodyTransform.position;
                GorillaTagger.Instance.offlineVRRig.transform.position = p.bodyTransform.position;

                VRRig.LocalRig.transform.position = p.bodyTransform.position;
                VRRig.LocalRig.leftHandTransform.position = p.bodyTransform.position;
                VRRig.LocalRig.rightHandTransform.position = p.bodyTransform.position;

                GorillaTagger.Instance.offlineVRRig.enabled = true;
                VRRig.LocalRig.enabled = true;
            }
        }

        public static void TagAll()
        {
            foreach (VRRig p in VRRigCache.ActiveRigs)
            {
                if (!p.isOfflineVRRig)
                {
                    TagPlayer(p);
                }
            }
        }

        public static void NoTagOnJoin()
        {
            PlayerPrefs.SetString("didTutorial", "nope");
            PlayerPrefs.SetString("tutorial", "nope");
            Hashtable hasht = new Hashtable();
            hasht.Add("didTutorial", false);
            PhotonNetwork.LocalPlayer.SetCustomProperties(hasht, null, null);
            PlayerPrefs.Save();
        }

        public static void removeleaves()
        {
            if (_leavesName == null)
            {
                var path = "Environment Objects/LocalObjects_Prefab/Forest";
                if (!objectPool.TryGetValue(path, out var f))
                {
                    f = GameObject.Find(path);
                    if (f != null)
                        objectPool.Add(path, f);
                }

                if (f != null)
                {
                    var counts = new Dictionary<string, (int count, int siblingIndex)>();
                    for (int i = 0; i < f.transform.childCount; i++)
                    {
                        var t = f.transform.GetChild(i);
                        if (!t.name.StartsWith("UnityTempFile"))
                            continue;
                        if (!counts.TryGetValue(t.name, out var entry))
                            counts[t.name] = (1, t.GetSiblingIndex());
                        else
                            counts[t.name] = (entry.count + 1, entry.siblingIndex);
                    }
                    _leavesName = counts.Where(kv => kv.Value.count == 3).OrderByDescending(kv => kv.Value.siblingIndex).FirstOrDefault().Key ?? "UnityTempFile";
                }
            }

            foreach (var path in new[] { "Environment Objects/LocalObjects_Prefab/Forest", "RankedMain/Ranked_Layout/Ranked_Forest_prefab" })
            {
                if (!objectPool.TryGetValue(path, out var forest))
                {
                    forest = GameObject.Find(path);
                    if (!forest && path.Contains("/"))
                    {
                        var split = path.Split('/');
                        var tr = GameObject.Find(split[0])?.transform.Find(path[(split[0].Length + 1)..]);
                        if (tr != null)
                            forest = tr.gameObject;
                    }
                    if (forest != null)
                        objectPool.Add(path, forest);
                }

                if (forest == null)
                    continue;
                for (int i = 0; i < forest.transform.childCount; i++)
                {
                    var child = forest.transform.GetChild(i).gameObject;
                    if (!child.name.Contains(_leavesName))
                        continue;

                    child.SetActive(false);
                    leaves.Add(child);
                }
            }
        }

        public static void addleaves()
        {
            foreach (var l in leaves)
                l.SetActive(true);
            leaves.Clear();
        }

        public static void FPS(int aa) => Application.targetFrameRate = aa;

        public static void NoTagFreeze()
        {
            GorillaTagger.Instance.statusEndTime = 0f;
            GorillaTagger.Instance.currentStatus = GorillaTagger.StatusEffect.None;
            GTPlayer.Instance.disableMovement = false;
        }

        private static float tagAuraCooldown;
        public static void TagAura(float radius = 3.5f)
        {
            if (!NetworkSystem.Instance.InRoom || VRRig.LocalRig == null) return;
            if (!VRRig.LocalRig.mainSkin.material.name.Contains("fected")) return;
            if (Time.time < tagAuraCooldown) return;

            Vector3 localHead = GorillaTagger.Instance.headCollider.transform.position;
            foreach (VRRig targetRig in VRRigCache.ActiveRigs)
            {
                if (targetRig == null || targetRig.isOfflineVRRig || targetRig == VRRig.LocalRig) continue;

                if (!targetRig.mainSkin.material.name.Contains("fected") && targetRig.Creator != null)
                {
                    Vector3 targetHead = targetRig.headConstraint != null ? targetRig.headConstraint.position : targetRig.transform.position;
                    if (Vector3.Distance(localHead, targetHead) <= radius)
                    {
                        tagAuraCooldown = Time.time + 0.35f;
                        TagPlayer(targetRig);
                        break;
                    }
                }
            }
        }

        private static float tagAssistCooldown;
        private static VRRig tagAssistTarget;

        public static void TagAssist(float assistRange = 8.5f, float pullSpeed = 22f)
        {
            if (!NetworkSystem.Instance.InRoom || VRRig.LocalRig == null) return;
            if (!VRRig.LocalRig.mainSkin.material.name.Contains("fected")) return;

            Vector3 localHead = GorillaTagger.Instance.headCollider.transform.position;
            VRRig closestTarget = null;
            float closestDistance = assistRange;

            foreach (VRRig targetRig in VRRigCache.ActiveRigs)
            {
                if (targetRig == null || targetRig.isOfflineVRRig || targetRig == VRRig.LocalRig) continue;

                if (!targetRig.mainSkin.material.name.Contains("fected") && targetRig.Creator != null)
                {
                    Vector3 targetPos = targetRig.headConstraint != null ? targetRig.headConstraint.position : targetRig.transform.position;
                    float distance = Vector3.Distance(localHead, targetPos);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestTarget = targetRig;
                    }
                }
            }

            tagAssistTarget = closestTarget;

            if (tagAssistTarget != null)
            {
                Vector3 targetHead = tagAssistTarget.headConstraint != null ? tagAssistTarget.headConstraint.position : tagAssistTarget.transform.position;
                Vector3 toTarget = (targetHead - localHead).normalized;
                GorillaTagger.Instance.rigidbody.linearVelocity = toTarget * pullSpeed;
                GorillaTagger.Instance.rightHandTransform.position = targetHead;
                if (VRRig.LocalRig != null)
                {
                    VRRig.LocalRig.rightHandTransform.position = targetHead;
                }

                if (closestDistance <= 4.5f && Time.time > tagAssistCooldown)
                {
                    tagAssistCooldown = Time.time + 0.3f;
                    TagPlayer(tagAssistTarget);
                }
            }
        }

        public static void TagAssistDisable()
        {
            tagAssistTarget = null;
        }
    }
}
