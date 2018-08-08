using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CanvasManager : MonoBehaviour {

    void Start()
    {
        float standard_width = 750f;        //初始宽度  
        float standard_height = 1334f;       //初始高度  
        float device_width = 0f;                //当前设备宽度  
        float device_height = 0f;               //当前设备高度  
        //获取设备宽高  
        device_width = Screen.width;
        device_height = Screen.height;
        //计算宽高比例  
        float standard_aspect = standard_width / standard_height;
        float device_aspect = device_width / device_height;

        CanvasScaler canvasScalerTemp = transform.GetComponent<CanvasScaler>();
        if (device_aspect > standard_aspect)
        {
            canvasScalerTemp.matchWidthOrHeight = 1;
        }
        else
        {
            canvasScalerTemp.matchWidthOrHeight = 0;
        }
    }
}
