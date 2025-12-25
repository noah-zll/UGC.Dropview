using System.Collections.Generic;
using UnityEngine;

namespace UGC.Dropview
{
    public interface IDropItemRenderer
    {
        DropItemBase CreateItem(DropItemData data, Transform parent);
        void UpdateItem(DropItemBase item, DropItemData data);
    }
}
