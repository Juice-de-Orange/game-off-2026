using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameOff.UI
{
    /// <summary>Raises Released when the player lets go of a slider (P18: volume preview tone after dragging or clicking).</summary>
    public class SliderRelease : MonoBehaviour, IPointerUpHandler
    {
        public event Action Released;

        public void OnPointerUp(PointerEventData eventData) => Released?.Invoke();
    }
}
