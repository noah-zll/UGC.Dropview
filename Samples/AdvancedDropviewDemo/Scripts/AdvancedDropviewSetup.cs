using UnityEngine;
using UnityEngine.UI;

namespace UGC.Dropview.Samples
{
    public class AdvancedDropviewSetup : MonoBehaviour
    {
        public UGC.Dropview.Dropview singleView;
        public UGC.Dropview.Dropview multiView;

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

            var btnSingle = CreateButton(canvas.transform, "单选下拉", new Vector2(1500, -1000));
            var btnMulti = CreateButton(canvas.transform, "多选下拉", new Vector2(1700, -1000));

            singleView = btnSingle.gameObject.AddComponent<UGC.Dropview.Dropview>();
            singleView.Mode = UGC.Dropview.SelectionMode.Single;
            singleView.SetContentBuilder(new UGC.Dropview.DefaultContentBuilder());
            singleView.SetItemRenderer(new UGC.Dropview.DefaultItemRenderer());
            singleView.enableVirtualization = false;
            var itemsSingle = new System.Collections.Generic.List<UGC.Dropview.DropItemData>();
            for (int i = 0; i < 8; i++)
            {
                itemsSingle.Add(new UGC.Dropview.DropItemData { id = "s_" + i, text = "选项 " + (i + 1) });
            }
            singleView.SetItems(itemsSingle);

            multiView = btnMulti.gameObject.AddComponent<UGC.Dropview.Dropview>();
            multiView.Mode = UGC.Dropview.SelectionMode.Multiple;
            multiView.SetSelectionPolicy(new UGC.Dropview.DefaultSelectionPolicy(3));
            multiView.SetContentBuilder(new UGC.Dropview.DefaultContentBuilder());
            multiView.SetItemRenderer(new UGC.Dropview.DefaultItemRenderer());
            multiView.enableVirtualization = true;
            multiView.itemHeight = 28f;
            multiView.maxVisibleCount = 8;
            var itemsMulti = new System.Collections.Generic.List<UGC.Dropview.DropItemData>();
            for (int i = 0; i < 30; i++)
            {
                itemsMulti.Add(new UGC.Dropview.DropItemData { id = "m_" + i, text = "项目 " + (i + 1) });
            }
            multiView.SetItems(itemsMulti);

            btnSingle.onClick.AddListener(() => { singleView.Toggle(); });
            btnMulti.onClick.AddListener(() => { multiView.Toggle(); });
        }

        Button CreateButton(Transform parent, string text, Vector2 anchoredPos)
        {
            var btnGO = new GameObject(text);
            btnGO.transform.SetParent(parent, false);
            var rt = btnGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = anchoredPos;
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 160);
            rt.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 40);
            var img = btnGO.AddComponent<Image>();
            img.color = new Color(0.25f, 0.55f, 0.95f, 1f);
            var btn = btnGO.AddComponent<Button>();
            var labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btnGO.transform, false);
            var lrt = labelGO.AddComponent<RectTransform>();
            lrt.anchorMin = new Vector2(0, 0);
            lrt.anchorMax = new Vector2(1, 1);
            lrt.offsetMin = new Vector2(8, 6);
            lrt.offsetMax = new Vector2(-8, -6);
            var ltext = labelGO.AddComponent<Text>();
            ltext.text = text;
            ltext.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            ltext.color = Color.white;
            ltext.alignment = TextAnchor.MiddleCenter;
            return btn;
        }
    }
}
