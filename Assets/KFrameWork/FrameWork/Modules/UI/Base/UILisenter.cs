using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace KFrameWork
{
    public class UILisenter : EventTrigger
    {
        public delegate void TriggerDelegate(GameObject go, BaseEventData data);
        public TriggerDelegate onPointerEnter;
        public TriggerDelegate onPointerExit;
        public TriggerDelegate onDrag;
        public TriggerDelegate onDrop;
        public TriggerDelegate onPointerDown;
        public TriggerDelegate onPointerUp;
        public TriggerDelegate onPointerClick;
        public TriggerDelegate onSelect;
        public TriggerDelegate onDeselect;
        public TriggerDelegate onScroll;
        public TriggerDelegate onMove;
        public TriggerDelegate onUpdateSelected;
        public TriggerDelegate onBeginDrag;
        public TriggerDelegate onEndDrag;
        public TriggerDelegate onSubmit;
        public TriggerDelegate onCancel;

        public override void OnPointerEnter(PointerEventData eventData)
        {
            if (onPointerEnter != null) onPointerEnter(eventData.selectedObject, eventData);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (onPointerExit != null) onPointerExit(eventData.selectedObject, eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (onDrag != null)
            {
                onDrag(eventData.selectedObject, eventData);
            }
        }

        public override void OnDrop(PointerEventData eventData)
        {
            if (onDrop != null) onDrop(eventData.selectedObject, eventData);
        }

        public override void OnPointerDown(PointerEventData eventData)
        {
            if (onPointerDown != null) onPointerDown(eventData.selectedObject, eventData);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            if (onPointerUp != null) onPointerUp(eventData.selectedObject, eventData);
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            if (onPointerClick != null) onPointerClick(eventData.selectedObject, eventData);
        }

        public override void OnSelect(BaseEventData eventData)
        {
            if (onSelect != null) onSelect(eventData.selectedObject, eventData);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            if (onDeselect != null) onDeselect(eventData.selectedObject, eventData);
        }

        public override void OnScroll(PointerEventData eventData)
        {
            if (onScroll != null) onScroll(eventData.selectedObject, eventData);
        }

        public override void OnMove(AxisEventData eventData)
        {
            if (onPointerEnter != null) onPointerEnter(eventData.selectedObject, eventData);
        }

        public override void OnUpdateSelected(BaseEventData eventData)
        {
            if (onUpdateSelected != null) onUpdateSelected(eventData.selectedObject, eventData);
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (onBeginDrag != null) onBeginDrag(eventData.selectedObject, eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (onEndDrag != null)
            {
                onEndDrag(eventData.selectedObject, eventData);
            } 
        }

        public override void OnSubmit(BaseEventData eventData)
        {
            if (onSubmit != null) onSubmit(eventData.selectedObject, eventData);
        }

        public override void OnCancel(BaseEventData eventData)
        {
            if (onCancel != null) onCancel(eventData.selectedObject, eventData);
        }

        void OnDestroy()
        {
            this.onPointerEnter = null;
            this.onPointerExit = null;
            this.onDrag = null;
            this.onDrop = null;
            this.onPointerDown = null;
            this.onPointerUp = null;
            this.onPointerClick = null;
            this.onSelect = null;
            this.onDeselect = null;
            this.onScroll = null;
            this.onMove = null;
            this.onUpdateSelected = null;
            this.onBeginDrag = null;
            this.onEndDrag = null;
            this.onSubmit = null;
            this.onCancel = null;
        }
    }
}



