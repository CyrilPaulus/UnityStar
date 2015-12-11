using System.Collections;
using UnityEngine;

namespace UnityStar.Assets
{
    public class SpriteGlow : MonoBehaviour
    {
        public float GlowSpeed = 2;
        private bool fadeIn = false;

        // Use this for initialization
        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            var renderer = this.GetComponent<SpriteRenderer>();

            var a = renderer.color.a;

            if (fadeIn)
            {
                if (a >= 0.95)
                    fadeIn = false;
                else
                    a = Mathf.Lerp(a, 1, Time.deltaTime * GlowSpeed);
            }
            else
            {
                if (a <= 0.05)
                    fadeIn = true;
                else
                    a = Mathf.Lerp(a, 0, Time.deltaTime * GlowSpeed);
            }

            renderer.color = new Color(1f, 1f, 1f, a);
        }
    }
}