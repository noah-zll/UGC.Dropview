using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace UGC.Dropview.Editor
{
    [CustomEditor(typeof(UGC.Dropview.Dropview))]
    public class DropviewEditor : UnityEditor.Editor
    {
        SerializedProperty Mode;
        SerializedProperty Animation;
        SerializedProperty Positioning;
        SerializedProperty autoCloseOnSelect;
        SerializedProperty closeOnOutsideClick;
        SerializedProperty enableVirtualization;
        SerializedProperty itemHeight;
        SerializedProperty maxVisibleCount;
        SerializedProperty maxScrollHeight;
        SerializedProperty ensureSorting;
        SerializedProperty sortingOrder;
        SerializedProperty scrollRect;
        SerializedProperty viewportRect;
        SerializedProperty contentRect;
        SerializedProperty canvasGroup;

        void OnEnable()
        {
            Mode = serializedObject.FindProperty("Mode");
            Animation = serializedObject.FindProperty("Animation");
            Positioning = serializedObject.FindProperty("Positioning");
            autoCloseOnSelect = serializedObject.FindProperty("autoCloseOnSelect");
            closeOnOutsideClick = serializedObject.FindProperty("closeOnOutsideClick");
            enableVirtualization = serializedObject.FindProperty("enableVirtualization");
            itemHeight = serializedObject.FindProperty("itemHeight");
            maxVisibleCount = serializedObject.FindProperty("maxVisibleCount");
            maxScrollHeight = serializedObject.FindProperty("maxScrollHeight");
            ensureSorting = serializedObject.FindProperty("ensureSorting");
            sortingOrder = serializedObject.FindProperty("sortingOrder");
            scrollRect = serializedObject.FindProperty("scrollRect");
            viewportRect = serializedObject.FindProperty("viewportRect");
            contentRect = serializedObject.FindProperty("contentRect");
            canvasGroup = serializedObject.FindProperty("canvasGroup");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            if (Mode != null) EditorGUILayout.PropertyField(Mode);
            if (Animation != null) EditorGUILayout.PropertyField(Animation);
            if (Positioning != null) EditorGUILayout.PropertyField(Positioning);
            if (autoCloseOnSelect != null) EditorGUILayout.PropertyField(autoCloseOnSelect);
            if (closeOnOutsideClick != null) EditorGUILayout.PropertyField(closeOnOutsideClick);
            if (enableVirtualization != null) EditorGUILayout.PropertyField(enableVirtualization);
            if (enableVirtualization.boolValue)
            {
                if (itemHeight != null) EditorGUILayout.PropertyField(itemHeight);
                if (maxVisibleCount != null) EditorGUILayout.PropertyField(maxVisibleCount);
            }
            if (maxScrollHeight != null) EditorGUILayout.PropertyField(maxScrollHeight);
            if (ensureSorting != null) EditorGUILayout.PropertyField(ensureSorting);
            if (ensureSorting.boolValue && sortingOrder != null) EditorGUILayout.PropertyField(sortingOrder);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Container Binding", EditorStyles.boldLabel);
            if (scrollRect != null) EditorGUILayout.PropertyField(scrollRect);
            if (viewportRect != null) EditorGUILayout.PropertyField(viewportRect);
            if (contentRect != null) EditorGUILayout.PropertyField(contentRect);
            if (canvasGroup != null) EditorGUILayout.PropertyField(canvasGroup);
            serializedObject.ApplyModifiedProperties();

            var view = target as UGC.Dropview.Dropview;
            EditorGUILayout.Space();
            if (GUILayout.Button("Set Default Content Builder"))
            {
                view.SetContentBuilder(new UGC.Dropview.DefaultContentBuilder());
                EditorUtility.SetDirty(view);
            }
            if (GUILayout.Button("Generate Sample Items"))
            {
                var list = new List<UGC.Dropview.DropItemData>();
                for (int i = 0; i < 12; i++)
                {
                    list.Add(new UGC.Dropview.DropItemData { id = "item_" + i, text = "选项 " + (i + 1) });
                }
                view.SetItems(list);
                EditorUtility.SetDirty(view);
            }
            if (GUILayout.Button("Bind From Children"))
            {
                var sr = view.GetComponentInChildren<ScrollRect>(true);
                if (sr != null)
                {
                    scrollRect.objectReferenceValue = sr;
                    viewportRect.objectReferenceValue = sr.viewport != null ? sr.viewport : null;
                    contentRect.objectReferenceValue = sr.content != null ? sr.content : null;
                    var cg = sr.GetComponent<CanvasGroup>();
                    if (cg == null) cg = sr.gameObject.AddComponent<CanvasGroup>();
                    canvasGroup.objectReferenceValue = cg;
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(view);
                }
                else
                {
                    var host = view.transform.Find("DropviewContainer");
                    if (host != null)
                    {
                        var hostGO = host.gameObject;
                        var sr2 = hostGO.GetComponent<ScrollRect>();
                        if (sr2 == null) sr2 = hostGO.AddComponent<ScrollRect>();
                        scrollRect.objectReferenceValue = sr2;
                        var vpT = host.Find("Viewport");
                        if (vpT != null) viewportRect.objectReferenceValue = vpT.GetComponent<RectTransform>();
                        var ctT = vpT != null ? vpT.Find("Content") : null;
                        if (ctT != null) contentRect.objectReferenceValue = ctT.GetComponent<RectTransform>();
                        var cg2 = hostGO.GetComponent<CanvasGroup>();
                        if (cg2 == null) cg2 = hostGO.AddComponent<CanvasGroup>();
                        canvasGroup.objectReferenceValue = cg2;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(view);
                    }
                }
            }
            if (GUILayout.Button("Create Container Under Dropview"))
            {
                var hostGO = new GameObject("DropviewContainer");
                hostGO.transform.SetParent(view.transform, false);
                var hostRect = hostGO.AddComponent<RectTransform>();
                hostRect.pivot = new Vector2(0, 1);
                hostRect.anchorMin = new Vector2(0, 1);
                hostRect.anchorMax = new Vector2(0, 1);
                var cg = hostGO.AddComponent<CanvasGroup>();
                var bg = hostGO.AddComponent<Image>();
                bg.color = new Color(0.95f, 0.95f, 0.95f, 1f);
                var vlg = hostGO.AddComponent<VerticalLayoutGroup>();
                vlg.childControlHeight = true;
                vlg.childForceExpandHeight = false;
                vlg.childControlWidth = true;
                vlg.childForceExpandWidth = true;
                var viewportGO = new GameObject("Viewport");
                viewportGO.transform.SetParent(hostGO.transform, false);
                var vpRect = viewportGO.AddComponent<RectTransform>();
                var viewportLE = viewportGO.AddComponent<LayoutElement>();
                viewportLE.flexibleHeight = 1;
                var mask = viewportGO.AddComponent<Mask>();
                mask.showMaskGraphic = false;
                var viewportImg = viewportGO.AddComponent<Image>();
                viewportImg.color = new Color(1, 1, 1, 0.01f);
                var contentGO = new GameObject("Content");
                contentGO.transform.SetParent(viewportGO.transform, false);
                var ctRect = contentGO.AddComponent<RectTransform>();
                ctRect.anchorMin = new Vector2(0, 1);
                ctRect.anchorMax = new Vector2(1, 1);
                ctRect.pivot = new Vector2(0, 1);
                var contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
                contentLayout.childControlHeight = true;
                contentLayout.childForceExpandHeight = false;
                contentLayout.childControlWidth = true;
                contentLayout.childForceExpandWidth = true;
                var sr = hostGO.AddComponent<ScrollRect>();
                sr.viewport = vpRect;
                sr.content = ctRect;
                sr.horizontal = false;
                scrollRect.objectReferenceValue = sr;
                viewportRect.objectReferenceValue = vpRect;
                contentRect.objectReferenceValue = ctRect;
                canvasGroup.objectReferenceValue = cg;
                hostGO.SetActive(false);
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(view);
                Selection.activeGameObject = hostGO;
            }
            if (GUILayout.Button("Auto Setup"))
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
                view.SetContentBuilder(new UGC.Dropview.DefaultContentBuilder());
                EditorUtility.SetDirty(view);
                Selection.activeGameObject = btnGO;
            }
            EditorGUILayout.Space();
            GUI.enabled = EditorApplication.isPlaying;
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview Open"))
            {
                view.Open();
            }
            if (GUILayout.Button("Preview Close"))
            {
                view.Close();
            }
            EditorGUILayout.EndHorizontal();
            GUI.enabled = true;
        }
    }
}
