using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandMouse : MonoBehaviour
{
    public RectTransform rectTransform;
    Canvas canvas;

    private void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        Vector2 pos;

        // Convert mouse position -> UI position
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            canvas.worldCamera,
            out pos
        );

        rectTransform.localPosition = pos;
    }
}
