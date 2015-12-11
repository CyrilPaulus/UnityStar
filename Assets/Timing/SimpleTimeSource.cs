using System.Collections;
using UnityEngine;

namespace UnityStar.Assets.Timing
{
    public class SimpleTimeSource : ITimeSource
    {
        // Use this for initialization

        private double _time = 0;

        private void Start()
        {
        }

        // Update is called once per frame
        private void Update()
        {
            _time += Time.deltaTime;
        }

        public override double GetTime()
        {
            return _time;
        }

        public void Reset()
        {
            _time = 0;
        }
    }
}