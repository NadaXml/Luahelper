using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NSlider : MonoBehaviour
{
    public RectTransform fill;

    public float a;
    public float b;

    public float progress;
  
        public void upprogress()
        {
            Vector2 d = new Vector2(a * Mathf.Cos(progress), b * Mathf.Sin(progress));
            fill.anchoredPosition = d;
        }
    
}
