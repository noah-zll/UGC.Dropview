using UnityEngine;
using UnityEngine.UI;

namespace UGC.Dropview
{
    public class DefaultItemRenderer : IDropItemRenderer
    {
        public DropItemBase CreateItem(DropItemData data, Transform parent)
        {
            var go = new GameObject("Item_" + data.id);
            var dropItem = go.AddComponent<DropItemBase>();
            go.transform.SetParent(parent, false);
            var rt = go.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 0.5f);
            var bg = go.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 1f);
            var btn = go.AddComponent<Button>();
            btn.onClick.AddListener(() => dropItem.Trigger());
            var textGO = new GameObject("Label");
            textGO.transform.SetParent(go.transform, false);
            var tr = textGO.AddComponent<RectTransform>();
            tr.anchorMin = new Vector2(0, 0);
            tr.anchorMax = new Vector2(1, 1);
            tr.offsetMin = new Vector2(8, 4);
            tr.offsetMax = new Vector2(-8, -4);
            var label = textGO.AddComponent<Text>();
            label.text = data.text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 14;
            label.color = Color.black;
            return dropItem;
        }

        public void UpdateItem(DropItemBase item, DropItemData data)
        {
            var bg = item.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = data.isSelected ? new Color(0.85f, 0.92f, 1f, 1f) : new Color(1f, 1f, 1f, 1f);
            }
            var btn = item.GetComponent<Button>();
            if (btn != null)
            {
                btn.interactable = data.isEnabled;
            }
            var label = item.transform.Find("Label");
            if (label != null)
            {
                var text = label.GetComponent<Text>();
                if (text != null) text.text = data.text;
            }
        }
    }
}
