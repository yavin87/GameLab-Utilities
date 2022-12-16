using TMPro;
using System;

namespace GameLabGraz.UI
{
    public class InputField : TMP_InputField
    {
        public bool absoluteValue = false;
        public void SetText(float value)
        {
            if (absoluteValue)
                text = $"{Mathf.Abs(value):0.###}";
            else
                text = $"{value:0.###}";
        }
    }
}
