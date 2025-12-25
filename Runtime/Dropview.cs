using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UGC.Dropview
{
    public enum SelectionMode
    {
        None,
        Single,
        Multiple
    }

    public class Dropview : MonoBehaviour
    {
        public SelectionMode Mode = SelectionMode.Single;
        public RectTransform Anchor;
        public AnimationConfig Animation = new AnimationConfig();
        public PositioningConfig Positioning = new PositioningConfig();
        public bool IsOpen { get; private set; }
        public bool autoCloseOnSelect = true;
        public bool closeOnOutsideClick = true;
        public bool enableVirtualization = false;
        public float itemHeight = 28f;
        public int maxVisibleCount = 8;
        public float maxScrollHeight = 300f;
        public IDropviewContentBuilder contentBuilder;

        public System.Action OnOpening;
        public System.Action OnOpened;
        public System.Action OnClosing;
        public System.Action OnClosed;
        public System.Action<DropItemData> OnItemClicked;
        public System.Action<IReadOnlyList<string>> OnSelectionChanged;

        [SerializeField] RectTransform contentRect;
        [SerializeField] CanvasGroup canvasGroup;
        RectTransform blockerRect;
        Button blockerButton;
        List<DropItemData> items = new List<DropItemData>();
        HashSet<string> selectedIds = new HashSet<string>();
        Coroutine animCoroutine;
        IDropItemRenderer itemRenderer;
        ISelectionPolicy selectionPolicy;
        List<DropItemBase> itemGOs = new List<DropItemBase>();
        [SerializeField] ScrollRect scrollRect;
        [SerializeField] RectTransform viewportRect;
        GameObjectPool pool;
        int firstVisibleIndex;
        int visibleCount;
        List<DropItemData> allItems = new List<DropItemData>();
        string currentFilter = null;
        bool lastIsAbove;
        float lastTopY;
        float lastHeightForPosition;

        void Awake()
        {
            EnsureContentInitialized();
        }

        void EnsureContentInitialized()
        {
            if (contentRect != null && scrollRect != null && viewportRect != null) return;
            if (scrollRect == null)
            {
                scrollRect = GetComponentInChildren<ScrollRect>(true);
            }
            if (scrollRect != null)
            {
                if (viewportRect == null && scrollRect.viewport != null) viewportRect = scrollRect.viewport as RectTransform;
                if (contentRect == null && scrollRect.content != null) contentRect = scrollRect.content as RectTransform;
                if (canvasGroup == null) canvasGroup = scrollRect.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = scrollRect.gameObject.AddComponent<CanvasGroup>();
                if (viewportRect == null)
                {
                    var viewportGO = new GameObject("Viewport");
                    viewportGO.transform.SetParent(scrollRect.transform, false);
                    viewportRect = viewportGO.AddComponent<RectTransform>();
                    var viewportLE = viewportGO.AddComponent<LayoutElement>();
                    viewportLE.flexibleHeight = 1;
                    var mask = viewportGO.AddComponent<Mask>();
                    mask.showMaskGraphic = false;
                    var viewportImg = viewportGO.AddComponent<Image>();
                    viewportImg.color = new Color(1, 1, 1, 0.01f);
                }
                if (contentRect == null)
                {
                    var contentGO = new GameObject("Content");
                    contentGO.transform.SetParent(viewportRect.transform, false);
                    contentRect = contentGO.AddComponent<RectTransform>();
                    contentRect.anchorMin = new Vector2(0, 1);
                    contentRect.anchorMax = new Vector2(1, 1);
                    contentRect.pivot = new Vector2(0, 1);
                    var contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
                    contentLayout.childControlHeight = true;
                    contentLayout.childForceExpandHeight = false;
                    contentLayout.childControlWidth = true;
                    contentLayout.childForceExpandWidth = true;
                }
                scrollRect.viewport = viewportRect;
                scrollRect.content = contentRect;
                scrollRect.horizontal = false;
                scrollRect.onValueChanged.AddListener(OnScrollChanged);
                scrollRect.gameObject.SetActive(false);
                pool = new GameObjectPool(contentRect);
                if (contentBuilder != null)
                {
                    contentBuilder.Build(scrollRect.transform, viewportRect, contentRect, OnSearchInput);
                }
                return;
            }
            var hostT = transform.Find("DropviewContainer");
            if (hostT != null)
            {
                var hostRect = hostT.GetComponent<RectTransform>();
                canvasGroup = hostT.GetComponent<CanvasGroup>();
                if (canvasGroup == null) canvasGroup = hostT.gameObject.AddComponent<CanvasGroup>();
                scrollRect = hostT.GetComponent<ScrollRect>();
                if (scrollRect == null) scrollRect = hostT.gameObject.AddComponent<ScrollRect>();
                var viewportChild = hostT.Find("Viewport");
                if (viewportChild != null) viewportRect = viewportChild.GetComponent<RectTransform>();
                if (viewportRect == null)
                {
                    var viewportGO = new GameObject("Viewport");
                    viewportGO.transform.SetParent(hostT, false);
                    viewportRect = viewportGO.AddComponent<RectTransform>();
                    var viewportLE = viewportGO.AddComponent<LayoutElement>();
                    viewportLE.flexibleHeight = 1;
                    var mask = viewportGO.AddComponent<Mask>();
                    mask.showMaskGraphic = false;
                    var viewportImg = viewportGO.AddComponent<Image>();
                    viewportImg.color = new Color(1, 1, 1, 0.01f);
                }
                Transform contentChild = viewportRect != null ? viewportRect.Find("Content") : null;
                if (contentChild != null) contentRect = contentChild.GetComponent<RectTransform>();
                if (contentRect == null)
                {
                    var contentGO = new GameObject("Content");
                    contentGO.transform.SetParent(viewportRect.transform, false);
                    contentRect = contentGO.AddComponent<RectTransform>();
                    contentRect.anchorMin = new Vector2(0, 1);
                    contentRect.anchorMax = new Vector2(1, 1);
                    contentRect.pivot = new Vector2(0, 1);
                    var contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
                    contentLayout.childControlHeight = true;
                    contentLayout.childForceExpandHeight = false;
                    contentLayout.childControlWidth = true;
                    contentLayout.childForceExpandWidth = true;
                }
                scrollRect.viewport = viewportRect;
                scrollRect.content = contentRect;
                scrollRect.horizontal = false;
                scrollRect.onValueChanged.AddListener(OnScrollChanged);
                hostT.gameObject.SetActive(false);
                pool = new GameObjectPool(contentRect);
                if (contentBuilder != null)
                {
                    contentBuilder.Build(hostT, viewportRect, contentRect, OnSearchInput);
                }
                return;
            }
            {
                var host = new GameObject("DropviewContainer");
                host.transform.SetParent(transform, false);
                var hostRect = host.AddComponent<RectTransform>();
                hostRect.pivot = new Vector2(0, 1);
                hostRect.anchorMin = new Vector2(0, 1);
                hostRect.anchorMax = new Vector2(0, 1);
                canvasGroup = host.AddComponent<CanvasGroup>();
                var bg = host.AddComponent<Image>();
                bg.color = new Color(0.95f, 0.95f, 0.95f, 1f);
                var vlg = host.AddComponent<VerticalLayoutGroup>();
                vlg.childControlHeight = true;
                vlg.childForceExpandHeight = false;
                vlg.childControlWidth = true;
                vlg.childForceExpandWidth = true;
                var viewportGO = new GameObject("Viewport");
                viewportGO.transform.SetParent(host.transform, false);
                viewportRect = viewportGO.AddComponent<RectTransform>();
                var viewportLE = viewportGO.AddComponent<LayoutElement>();
                viewportLE.flexibleHeight = 1;
                var mask = viewportGO.AddComponent<Mask>();
                mask.showMaskGraphic = false;
                var viewportImg = viewportGO.AddComponent<Image>();
                viewportImg.color = new Color(1, 1, 1, 0.01f);
                var contentGO = new GameObject("Content");
                contentGO.transform.SetParent(viewportGO.transform, false);
                contentRect = contentGO.AddComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 1);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.pivot = new Vector2(0, 1);
                var contentLayout = contentGO.AddComponent<VerticalLayoutGroup>();
                contentLayout.childControlHeight = true;
                contentLayout.childForceExpandHeight = false;
                contentLayout.childControlWidth = true;
                contentLayout.childForceExpandWidth = true;
                scrollRect = host.AddComponent<ScrollRect>();
                scrollRect.viewport = viewportRect;
                scrollRect.content = contentRect;
                scrollRect.horizontal = false;
                scrollRect.onValueChanged.AddListener(OnScrollChanged);
                host.SetActive(false);
                pool = new GameObjectPool(contentRect);
                if (contentBuilder != null)
                {
                    contentBuilder.Build(host.transform, viewportRect, contentRect, OnSearchInput);
                }
            }
        }

        Canvas GetRootCanvas()
        {
            var c = GetComponentInParent<Canvas>();
            if (c != null) return c.rootCanvas;
            var canvas = Object.FindObjectOfType<Canvas>();
            return canvas != null ? canvas.rootCanvas : null;
        }

        void EnsureBlocker(Canvas canvas)
        {
            if (!closeOnOutsideClick) return;
            if (blockerRect != null) return;
            var go = new GameObject("DropviewBlocker");
            go.transform.SetParent(canvas.transform, false);
            blockerRect = go.AddComponent<RectTransform>();
            var img = go.AddComponent<Image>();
            img.color = new Color(0, 0, 0, 0);
            blockerButton = go.AddComponent<Button>();
            blockerButton.onClick.AddListener(() => { if (IsOpen) Close(); });
            blockerRect.anchorMin = Vector2.zero;
            blockerRect.anchorMax = Vector2.one;
            blockerRect.offsetMin = Vector2.zero;
            blockerRect.offsetMax = Vector2.zero;
            EnsureSorting(blockerRect, 29999);
            go.SetActive(false);
        }

        public void SetItems(IList<DropItemData> list)
        {
            allItems.Clear();
            if (list != null) allItems.AddRange(list);
            ApplyFilter(currentFilter);
        }

        void ApplyFilter(string filter)
        {
            currentFilter = filter;
            items.Clear();
            if (string.IsNullOrEmpty(filter))
            {
                items.AddRange(allItems);
            }
            else
            {
                string f = filter.ToLowerInvariant();
                for (int i = 0; i < allItems.Count; i++)
                {
                    var d = allItems[i];
                    var t = d.text ?? "";
                    if (t.ToLowerInvariant().Contains(f)) items.Add(d);
                }
            }
            itemGOs.Clear();
            if (pool != null) pool.Clear();
            for (int i = contentRect.childCount - 1; i >= 0; i--)
            {
                var child = contentRect.GetChild(i);
                if (child != null) Object.DestroyImmediate(child.gameObject);
            }
            if (!enableVirtualization)
            {
                for (int i = 0; i < items.Count; i++)
                {
                    var data = items[i];
                    DropItemBase dropItem = null;
                    if (itemRenderer != null)
                    {
                        dropItem = itemRenderer.CreateItem(data, contentRect);
                    }
                    else
                    {
                        var go = new GameObject("Item_" + data.id);
                        dropItem = go.AddComponent<DropItemBase>();
                        go.transform.SetParent(contentRect, false);
                        var rt = go.AddComponent<RectTransform>();
                        rt.anchorMin = new Vector2(0, 1);
                        rt.anchorMax = new Vector2(1, 1);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        var bg = go.AddComponent<Image>();
                        bg.color = new Color(1f, 1f, 1f, 1f);
                        var btn0 = go.AddComponent<Button>();
                        btn0.onClick.AddListener(() => { if (dropItem != null) dropItem.Trigger(); });
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
                    }
                    dropItem.Bind(data, this);
                    dropItem.OnTriggered += (v) =>
                    {
                        var dd = v.data;
                        OnItemClicked?.Invoke(dd);
                        if (Mode == SelectionMode.None) return;
                        if (selectionPolicy != null)
                        {
                            var current = new List<string>(selectedIds);
                            if (!selectionPolicy.CanSelect(current.AsReadOnly(), dd)) return;
                        }
                        if (Mode == SelectionMode.Single)
                        {
                            selectedIds.Clear();
                            selectedIds.Add(dd.id);
                            for (int j = 0; j < items.Count; j++) items[j].isSelected = items[j].id == dd.id;
                            RefreshSelectionVisuals();
                            if (autoCloseOnSelect) Close();
                        }
                        else if (Mode == SelectionMode.Multiple)
                        {
                            bool willAdd = !selectedIds.Contains(dd.id);
                            if (willAdd && selectionPolicy != null && selectionPolicy.MaxSelectionCount > 0 && selectedIds.Count >= selectionPolicy.MaxSelectionCount) willAdd = false;
                            if (selectedIds.Contains(dd.id))
                            {
                                selectedIds.Remove(dd.id);
                                dd.isSelected = false;
                            }
                            else if (willAdd)
                            {
                                selectedIds.Add(dd.id);
                                dd.isSelected = true;
                            }
                            RefreshSelectionVisuals();
                        }
                        OnSelectionChanged?.Invoke(new List<string>(selectedIds));
                    };
                    itemGOs.Add(dropItem);
                }
            }
            else
            {
                var sd = contentRect.sizeDelta;
                sd.y = Mathf.Max(0, items.Count * itemHeight);
                contentRect.sizeDelta = sd;
                firstVisibleIndex = 0;
                visibleCount = Mathf.Max(1, Mathf.Min(maxVisibleCount, Mathf.CeilToInt(viewportRect.rect.height / itemHeight)));
                RebuildVirtualItems();
            }
            {
                RefreshSelectionVisuals();
            }
        }

        void RefreshSelectionVisuals()
        {
            for (int i = 0; i < contentRect.childCount; i++)
            {
                var child = contentRect.GetChild(i).gameObject.GetComponent<DropItemBase>();
                var id = items[i].id;
                items[i].isSelected = selectedIds.Contains(id);
                if (itemRenderer != null)
                {
                    itemRenderer.UpdateItem(child, items[i]);
                }
                else
                {
                    var bg = child.gameObject.GetComponent<Image>();
                    if (bg != null)
                    {
                        bg.color = items[i].isSelected ? new Color(0.85f, 0.92f, 1f, 1f) : new Color(1f, 1f, 1f, 1f);
                    }
                    var btn = child.gameObject.GetComponent<Button>();
                    if (btn != null) btn.interactable = items[i].isEnabled;
                }
            }
        }

        public IReadOnlyList<string> GetSelectedIds()
        {
            return new List<string>(selectedIds);
        }

        public void Select(string id, bool selected = true)
        {
            if (Mode == SelectionMode.None) return;
            if (Mode == SelectionMode.Single)
            {
                selectedIds.Clear();
                if (selected) selectedIds.Add(id);
            }
            else
            {
                if (selected) selectedIds.Add(id); else selectedIds.Remove(id);
            }
            for (int i = 0; i < items.Count; i++)
            {
                items[i].isSelected = selectedIds.Contains(items[i].id);
            }
            RefreshSelectionVisuals();
            OnSelectionChanged?.Invoke(new List<string>(selectedIds));
        }

        public void ClearSelection()
        {
            selectedIds.Clear();
            RefreshSelectionVisuals();
            OnSelectionChanged?.Invoke(new List<string>(selectedIds));
        }

        public void Open()
        {
            if (IsOpen) return;
            OnOpening?.Invoke();
            EnsureContentInitialized();
            var canvas = GetRootCanvas();
            if (canvas == null) return;
            EnsureBlocker(canvas);
            
            // Ensure sorting
            var containerCanvas = EnsureSorting(scrollRect.transform);

            scrollRect.gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
            
            PositionContent(canvas);
            
            if (Positioning.matchTriggerWidth)
            {
                 var triggerRect = transform as RectTransform;
                 (scrollRect.transform as RectTransform).SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, triggerRect.rect.width);
            }
            
            
            containerCanvas.overrideSorting = true;
            if (blockerRect != null) blockerRect.gameObject.SetActive(true);
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(Animate(true));
            IsOpen = true;
            OnOpened?.Invoke();
        }

        Canvas EnsureSorting(Transform obj, int sortingOrder = 30000)
        {
            var containerCanvas = obj.GetComponent<Canvas>();
            if (containerCanvas == null) containerCanvas = obj.gameObject.AddComponent<Canvas>();
            containerCanvas.enabled = true;
            containerCanvas.overrideSorting = true;
            containerCanvas.sortingOrder = sortingOrder;
            
            var raycaster = obj.GetComponent<GraphicRaycaster>();
            if (raycaster == null) raycaster = obj.gameObject.AddComponent<GraphicRaycaster>();
            raycaster.enabled = true;

            Debug.Log($"EnsureSorting: {obj.gameObject.activeInHierarchy}, {containerCanvas.sortingOrder}");

            return containerCanvas;
        }

        void PositionContent(Canvas canvas)
        {
            var hostRect = scrollRect.transform as RectTransform;
            var triggerRect = transform as RectTransform;
            
            float width = Positioning.matchTriggerWidth ? triggerRect.rect.width : Mathf.Max(1f, (hostRect.sizeDelta.x > 0 ? hostRect.sizeDelta.x : 200f));
            hostRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);

            float height = ComputeContainerHeight();
            lastHeightForPosition = height;

            // Decide direction
            Vector3[] triggerCorners = new Vector3[4];
            triggerRect.GetWorldCorners(triggerCorners);
            Vector3[] canvasCorners = new Vector3[4];
            (canvas.transform as RectTransform).GetWorldCorners(canvasCorners);
            
            float bottomSpace = triggerCorners[0].y - canvasCorners[0].y;
            float topSpace = canvasCorners[1].y - triggerCorners[1].y;
            
            bool preferDown = Positioning.preferredDirections.Length > 0 && Positioning.preferredDirections[0] == Direction.Down;
            bool isDown = preferDown;
            
            if (preferDown && bottomSpace < height && topSpace > height) isDown = false;
            else if (!preferDown && topSpace < height && bottomSpace > height) isDown = true;
            
            lastIsAbove = !isDown;

            if (isDown)
            {
                hostRect.pivot = new Vector2(0, 1);
                hostRect.anchorMin = new Vector2(0, 0);
                hostRect.anchorMax = new Vector2(0, 0);
                hostRect.anchoredPosition = new Vector2(Positioning.edgePadding.x, -Positioning.edgePadding.y);
                if (contentRect != null) contentRect.pivot = new Vector2(0, 1);
            }
            else
            {
                hostRect.pivot = new Vector2(0, 0);
                hostRect.anchorMin = new Vector2(0, 1);
                hostRect.anchorMax = new Vector2(0, 1);
                hostRect.anchoredPosition = new Vector2(Positioning.edgePadding.x, Positioning.edgePadding.y);
                if (contentRect != null) contentRect.pivot = new Vector2(0, 0);
            }

            // Clamp Horizontal
            Canvas.ForceUpdateCanvases();
            Vector3[] hostCorners = new Vector3[4];
            hostRect.GetWorldCorners(hostCorners);
            
            float hostMaxX = Mathf.Max(hostCorners[0].x, hostCorners[1].x, hostCorners[2].x, hostCorners[3].x);
            float hostMinX = Mathf.Min(hostCorners[0].x, hostCorners[1].x, hostCorners[2].x, hostCorners[3].x);
            float screenRight = canvasCorners[2].x - Positioning.edgePadding.x;
            float screenLeft = canvasCorners[0].x + Positioning.edgePadding.x;
            
            float shift = 0;
            if (hostMaxX > screenRight) shift = screenRight - hostMaxX;
            else if (hostMinX < screenLeft) shift = screenLeft - hostMinX;
            
            if (Mathf.Abs(shift) > 0.1f && hostRect.lossyScale.x != 0)
            {
                Vector2 pos = hostRect.anchoredPosition;
                pos.x += shift / hostRect.lossyScale.x;
                hostRect.anchoredPosition = pos;
            }
        }
        
        float ComputeContainerHeight()
        {
            float h = 1f;
            if (contentRect != null)
            {
                if (enableVirtualization)
                {
                    h = Mathf.Max(1f, items.Count * itemHeight);
                }
                else
                {
                    float preferred = LayoutUtility.GetPreferredHeight(contentRect);
                    if (preferred <= 1f) preferred = contentRect.rect.height;
                    h = Mathf.Max(1f, preferred);
                }
            }
            if (maxScrollHeight > 0f) return Mathf.Min(maxScrollHeight, h);
            return h;
        }

        IEnumerator Animate(bool opening)
        {
            float elapsed = 0;
            float duration = Animation.duration;
            var hostRect = scrollRect.transform as RectTransform;
            float targetH = opening ? lastHeightForPosition : 0;
            float startH = opening ? 0 : hostRect.rect.height;
            
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                float curveValue = Animation.curve.Evaluate(t);
                float h = Mathf.Lerp(startH, targetH, curveValue);
                hostRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
                yield return null;
            }
            hostRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, targetH);
            if (!opening)
            {
                scrollRect.gameObject.SetActive(false);
                if (blockerRect != null) 
                {
                    Destroy(blockerRect.gameObject);
                    blockerRect = null;
                }
                IsOpen = false;
                OnClosed?.Invoke();
            }
        }
        public void Close()
        {
            if (!IsOpen) return;
            OnClosing?.Invoke();
            if (animCoroutine != null) StopCoroutine(animCoroutine);
            animCoroutine = StartCoroutine(Animate(false));
        }

        public void Toggle()
        {
            if (IsOpen) Close(); else Open();
        }

        public void SetItemRenderer(IDropItemRenderer renderer)
        {
            itemRenderer = renderer;
        }

        public void SetSelectionPolicy(ISelectionPolicy policy)
        {
            selectionPolicy = policy;
        }

        public void SetContentBuilder(IDropviewContentBuilder builder)
        {
            contentBuilder = builder;
        }

        void OnScrollChanged(Vector2 v)
        {
            if (!enableVirtualization) return;
            var contentTop = contentRect.anchoredPosition.y;
            int newFirst = Mathf.Clamp(Mathf.FloorToInt(contentTop / itemHeight), 0, Mathf.Max(0, items.Count - 1));
            if (newFirst != firstVisibleIndex)
            {
                firstVisibleIndex = newFirst;
                RebuildVirtualItems();
            }
        }

        void OnSearchInput(string s)
        {
            ApplyFilter(s);
        }

        void RebuildVirtualItems()
        {
            for (int i = contentRect.childCount - 1; i >= 0; i--)
            {
                var child = contentRect.GetChild(i).gameObject;
                pool.Return(child);
            }
            int count = Mathf.Min(visibleCount, items.Count - firstVisibleIndex);
            for (int i = 0; i < count; i++)
            {
                int index = firstVisibleIndex + i;
                var data = items[index];
                GameObject go = pool.Borrow(() =>
                {
                    if (itemRenderer != null) return itemRenderer.CreateItem(data, contentRect).gameObject;
                    var obj = new GameObject("Item_" + data.id);
                    obj.transform.SetParent(contentRect, false);
                    obj.AddComponent<DropItemBase>();
                    var rt = obj.AddComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0, 1);
                    rt.anchorMax = new Vector2(1, 1);
                    rt.pivot = new Vector2(0.5f, 0.5f);
                    var bg = obj.AddComponent<Image>();
                    bg.color = new Color(1f, 1f, 1f, 1f);
                    var btn0 = obj.AddComponent<Button>();
                    var textGO = new GameObject("Label");
                    textGO.transform.SetParent(obj.transform, false);
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
                    return obj;
                });
                go.transform.SetParent(contentRect, false);
                var rt2 = go.GetComponent<RectTransform>();
                rt2.anchoredPosition = new Vector2(0, -index * itemHeight);
                var baseComp = go.GetComponent<DropItemBase>();
                if (baseComp == null)
                {
                    baseComp = go.AddComponent<DropItemBase>();
                    var btn = go.GetComponent<Button>();
                    if (btn == null) btn = go.AddComponent<Button>();
                    btn.onClick.AddListener(() => baseComp.Trigger());
                }
                baseComp.Bind(data, this);
                baseComp.OnTriggered += (v) =>
                {
                    var dd = v.data;
                    OnItemClicked?.Invoke(dd);
                    if (Mode == SelectionMode.None) return;
                    if (selectionPolicy != null)
                    {
                        var current = new List<string>(selectedIds);
                        if (!selectionPolicy.CanSelect(current.AsReadOnly(), dd)) return;
                    }
                    if (Mode == SelectionMode.Single)
                    {
                        selectedIds.Clear();
                        selectedIds.Add(dd.id);
                        for (int j = 0; j < items.Count; j++) items[j].isSelected = items[j].id == dd.id;
                        RefreshSelectionVirtual(go, dd);
                        if (autoCloseOnSelect) Close();
                    }
                    else if (Mode == SelectionMode.Multiple)
                    {
                        bool willAdd = !selectedIds.Contains(dd.id);
                        if (willAdd && selectionPolicy != null && selectionPolicy.MaxSelectionCount > 0 && selectedIds.Count >= selectionPolicy.MaxSelectionCount) willAdd = false;
                        if (selectedIds.Contains(dd.id))
                        {
                            selectedIds.Remove(dd.id);
                            dd.isSelected = false;
                        }
                        else if (willAdd)
                        {
                            selectedIds.Add(dd.id);
                            dd.isSelected = true;
                        }
                        RefreshSelectionVirtual(go, dd);
                    }
                    OnSelectionChanged?.Invoke(new List<string>(selectedIds));
                };
                if (itemRenderer != null) itemRenderer.UpdateItem(baseComp, data); else RefreshSelectionVirtual(go, data);
            }
        }

        void RefreshSelectionVirtual(GameObject go, DropItemData data)
        {
            var bg = go.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = data.isSelected ? new Color(0.85f, 0.92f, 1f, 1f) : new Color(1f, 1f, 1f, 1f);
            }
            var btn = go.GetComponent<Button>();
            if (btn != null) btn.interactable = data.isEnabled;
        }

        public DropItemBase GetItem(string id)
        {
            foreach (var item in itemGOs)
            {
                if (item.data.id == id) return item;
            }
            return null;
        }

        public DropItemBase GetItem(DropItemData data)
        {
            return GetItem(data.id);
        }

        public DropItemBase GetSelectedItem()
        {
            foreach (var item in itemGOs)
            {
                if (item.data.isSelected) return item;
            }
            return null;
        }

        public List<DropItemBase> GetSelectedItems()
        {
            var list = new List<DropItemBase>();
            foreach (var item in itemGOs)
            {
                if (item.data.isSelected) list.Add(item);
            }
            return list;
        }
    }
}
