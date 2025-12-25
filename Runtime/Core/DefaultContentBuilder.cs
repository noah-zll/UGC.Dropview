using System;
using UnityEngine;
using UnityEngine.UI;

namespace UGC.Dropview
{
    public class DefaultContentBuilder : IDropviewContentBuilder
    {
        public void Build(Transform host, RectTransform viewport, RectTransform content, Action<string> onSearchInput)
        {
            var header = new GameObject("Header");
            header.transform.SetParent(host, false);
            header.transform.SetSiblingIndex(0);
            var hrect = header.AddComponent<RectTransform>();
            hrect.anchorMin = new Vector2(0, 1);
            hrect.anchorMax = new Vector2(1, 1);
            hrect.pivot = new Vector2(0, 1);
            var le = header.AddComponent<LayoutElement>();
            le.preferredHeight = 32;
            var bg = header.AddComponent<Image>();
            bg.color = new Color(0.93f, 0.93f, 0.93f, 1f);
            var inputGO = new GameObject("SearchInput");
            inputGO.transform.SetParent(header.transform, false);
            var ir = inputGO.AddComponent<RectTransform>();
            ir.anchorMin = new Vector2(0, 0.5f);
            ir.anchorMax = new Vector2(1, 0.5f);
            ir.pivot = new Vector2(0.5f, 0.5f);
            ir.anchoredPosition = Vector2.zero;
            ir.sizeDelta = new Vector2(0, 24);
            var img = inputGO.AddComponent<Image>();
            img.color = new Color(1f, 1f, 1f, 1f);
            var input = inputGO.AddComponent<InputField>();
            input.textComponent = CreateText(inputGO.transform, "Text", TextAnchor.MiddleLeft);
            var placeholder = CreateText(inputGO.transform, "Placeholder", TextAnchor.MiddleLeft);
            placeholder.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            placeholder.text = "搜索...";
            input.placeholder = placeholder;
            input.onValueChanged.AddListener(s => onSearchInput?.Invoke(s));
            var viewportLE = viewport.gameObject.GetComponent<LayoutElement>();
            if (viewportLE == null) viewportLE = viewport.gameObject.AddComponent<LayoutElement>();
            viewportLE.flexibleHeight = 1;
        }

        private Text CreateText(Transform parent, string name, TextAnchor anchor)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 1);
            rt.offsetMin = new Vector2(6, 2);
            rt.offsetMax = new Vector2(-6, -2);
            var text = go.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 14;
            text.color = Color.black;
            text.alignment = anchor;
            return text;
        }
    }
}
