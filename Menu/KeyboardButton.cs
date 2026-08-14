using UnityEngine;
using ShibaGTGenesisReborn.Menu;

namespace ShibaGTGenesisReborn.Classes
{
    public class KeyboardButton : MonoBehaviour
    {
        public string key;
        private bool isHovering = false;
        private Renderer renderer;
        private Color originalColor;

        void Start()
        {
            renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                originalColor = renderer.material.color;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            isHovering = true;
            if (renderer != null)
            {
                renderer.material.color = Color.white;
            }
        }

        void OnTriggerExit(Collider other)
        {
            isHovering = false;
            if (renderer != null)
            {
                renderer.material.color = originalColor;
            }
        }

        void Update()
        {
            if (isHovering && Main.trigger_Button)
            {
                if (key == "enter")
                {
                    Main.ExecuteSearch();
                }
                else if (key == "backspace")
                {
                    if (Main.searchQuery.Length > 0)
                    {
                        Main.searchQuery = Main.searchQuery.Substring(0, Main.searchQuery.Length - 1);
                        Main.UpdateSearchDisplay();
                    }
                }
                else if (key == "space")
                {
                    Main.searchQuery += " ";
                    Main.UpdateSearchDisplay();
                }
                else if (key == "shift")
                {
                }
                else if (key.Length == 1)
                {
                    Main.searchQuery += key;
                    Main.UpdateSearchDisplay();
                }
            }
        }
    }
}