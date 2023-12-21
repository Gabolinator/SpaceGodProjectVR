using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GUIColorPicker : MonoBehaviour
{
public UnityEvent<Color> ColorPickerEvent;
        [SerializeField] Texture2D colorChart;
        [SerializeField] GameObject chart;
        [SerializeField] RectTransform cursor;
        [SerializeField] Image button;
        [SerializeField] Image cursorColor;
        
        public void PickColor(BaseEventData data)
        {
            PointerEventData pointer = data as PointerEventData;
            cursor.position = pointer.position;
            Color pickedColor = colorChart.GetPixel((int)(cursor.localPosition.x * (colorChart.width / transform.GetChild(0).GetComponent<RectTransform>().rect.width)), (int)(cursor.localPosition.y * (colorChart.height / transform.GetChild(0).GetComponent<RectTransform>().rect.height)));
            button.color = pickedColor;
            cursorColor.color = pickedColor;
            ColorPickerEvent.Invoke(pickedColor);
        }
    }

