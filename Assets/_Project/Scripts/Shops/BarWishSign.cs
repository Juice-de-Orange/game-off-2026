using GameOff.Day;
using TMPro;
using UnityEngine;

namespace GameOff.Shops
{
    /// <summary>The sign at the bar (P65): every morning it shows what the barkeeper wants today (DayEntry.barWish).</summary>
    public class BarWishSign : MonoBehaviour
    {
        [SerializeField] private DayManager day;
        [SerializeField] private TMP_Text text;

        private void OnEnable()
        {
            if (day != null) day.DayStarted += Show;
        }

        private void OnDisable()
        {
            if (day != null) day.DayStarted -= Show;
        }

        private void Show(int n)
        {
            var wish = day.Entry(n).barWish;
            text.text = wish != null && wish.Active ? "THE LAST ORBIT\n<size=70%>TODAY I'D LIKE</size>\n" + wish.Text : "THE LAST ORBIT\n<size=70%>NOTHING TODAY</size>";
        }
    }
}
