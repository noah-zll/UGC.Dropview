using UnityEngine;

namespace UGC.Dropview
{
    public class DropItemBase : MonoBehaviour
    {
        public DropItemData data;
        public Dropview owner;
        public System.Action<DropItemBase> OnTriggered;

        public virtual void Bind(DropItemData d, Dropview o)
        {
            owner = o;
            data = d;
        }

        public virtual void SetData(DropItemData d)
        {
            data = d;
        }

        public void Trigger()
        {
            var selected = owner.GetSelectedItem();
            if (selected != null)
            {
                selected.OnDeselect();
            }
            OnTriggered?.Invoke(this);
            OnSelect();
        }

        public virtual void OnSelect()
        {
        }

        public virtual void OnDeselect()
        {
        }
    }
}
