using System;
using UnityEngine;

namespace UGC.Dropview
{
    public interface IDropviewContentBuilder
    {
        void Build(Transform host, RectTransform viewport, RectTransform content, Action<string> onSearchInput);
    }
}
