using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ShibaGTGenesisReborn.Libs
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; protected set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                gameObject.Obliterate();

                return;
            }

            Instance = this as T;
        }
    }
}

public static class Extensions
{
    public static void Obliterate(this GameObject obj) => Object.Destroy(obj);
    public static void Obliterate(this Component comp) => Object.Destroy(comp);

    public static void Obliterate(this GameObject obj, float delay) => Object.Destroy(obj, delay);
    public static void Obliterate(this Component comp, float delay) => Object.Destroy(comp, delay);
}