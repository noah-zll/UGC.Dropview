using System;
using UnityEngine;

namespace UGC.Dropview
{
    [Serializable]
    public class DropItemData
    {
        public string id;
        public string text;
        public Sprite icon;
        public bool isEnabled = true;
        public bool isSelected = false;
        public object payload;
    }
}
