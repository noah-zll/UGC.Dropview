using System;
using UnityEngine;

namespace UGC.Dropview
{
    [Serializable]
    public class AnimationConfig
    {
        public float duration = 0.2f;
        public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public bool enableHeight = true;
        public bool enableFade = true;
        public bool enableScale = false;
        public float targetHeight = 200f;
    }
}
