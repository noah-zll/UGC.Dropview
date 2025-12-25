using UnityEngine;
using UnityEngine.UI;

namespace UGC.Dropview.Samples
{
    public class BasicDropviewSetup : MonoBehaviour
    {
        public UGC.Dropview.Dropview view;

        void Start()
        {
            var canvas = Object.FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var cgo = new GameObject("Canvas");
                canvas = cgo.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                cgo.AddComponent<CanvasScaler>();
                cgo.AddComponent<GraphicRaycaster>();
            }

            var btnGO = new GameObject("TriggerButton");
            btnGO.transform.SetParent(canvas.transform, false);
            var brt = btnGO.AddComponent<RectTransform>();
            brt.anchorMin = new Vector2(0, 1);
            brt.anchorMax = new Vector2(0, 1);
            brt.pivot = new Vector2(0, 1);
            brt.anchoredPosition = new Vector2(20, -20);
            brt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 160);
            brt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40);
            var bimg = btnGO.AddComponent<Image>();
            bimg.color = new Color(0.2f, 0.6f, 1f, 1f);
            var btn = btnGO.AddComponent<Button>();
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            var lrt = labelGO.AddComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0, 0);
            lrt.anchorMax = new Vector2(1, 1);
            lrt.offsetMin = new Vector2(8, 6);
            lrt.offsetMax = new Vector2(-8, -6);
            var ltext = labelGO.AddComponent<Text>();
            ltext.text = "打开下拉";
            ltext.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ltext.color = Color.white;
            ltext.alignment = TextAnchor.MiddleCenter;

            view = btnGO.AddComponent<UGC.Dropview.Dropview>();
            var items = new System.Collections.Generic.List<UGC.Dropview.DropItemData>();
            for (int i = 0; i < 6; i++)
            {
                items.Add(new UGC.Dropview.DropItemData { id = "item_" + i, text = "选项 " + (i + 1) });
            }
            view.Mode = UGC.Dropview.SelectionMode.Single;
            view.SetItems(items);
            btn.onClick.AddListener(() =>
            {
                view.Toggle();
            });
        }
    }
}
