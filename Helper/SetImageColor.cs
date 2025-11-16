using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;


public class SetImageColor : MonoBehaviour
{
    [InlineButton("ApplyDefaultColor", "Apply")]
    public ColorSO defaultColor;
    

    public ColorSO[] alternateColorArray; 
    public Image image;

    private void Start()
    {
        ApplyDefaultColor();
    }

    public void ApplyDefaultColor()
    {
        if (defaultColor == null)
        {
            Debug.LogError($"Tried to find color {defaultColor} that doesn't exist.");
            return;
        }
        image.color = defaultColor.color;
    }

    public void ApplyAlternateColor(int index)
    {
        if (alternateColorArray.Length < index)
        {
            Debug.LogError($"Tried to find alternate color #{index} that doesn't exist.");
            return;
        }
        
        image.color = alternateColorArray[index].color;
    }
}
