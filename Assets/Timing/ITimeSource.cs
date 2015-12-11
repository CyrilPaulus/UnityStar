using System.Collections;
using UnityEngine;

namespace UnityStar.Assets.Timing
{
    public abstract class ITimeSource : MonoBehaviour
    {
        public abstract double GetTime();
    }
}