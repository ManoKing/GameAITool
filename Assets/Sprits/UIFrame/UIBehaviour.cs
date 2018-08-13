using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
public class UIBehaviour : MonoBehaviour {
    string panelName = "";

    void Awake()
    {


          panelName=  transform.GetComponentInParent<UIBase>().transform.name;
          UIManager.Instance.RegistGameObject(panelName,transform.name,gameObject);
    }

    public void AddButtonListen(UnityAction  tmpAction)
    {
 
         Button   tmpBtn =  gameObject.GetComponent<Button>();

         if (tmpBtn != null)
         tmpBtn.onClick.AddListener(tmpAction);
         
    }


    public void AddSliderListen(UnityAction<float> tmpAction)
    {

        Slider tmpBtn = gameObject.GetComponent<Slider>();

        if (tmpBtn != null)
            tmpBtn.onValueChanged.AddListener(tmpAction);

    }

    public void AddInputEndEditro(UnityAction<string> tmpAction)
    {



        InputField tmpBtn = gameObject.GetComponent<InputField>();

        if (tmpBtn != null)
            tmpBtn.onEndEdit.AddListener(tmpAction);

    }

    public void AddInputValueChange(UnityAction<string> tmpAction)
    {



        InputField tmpBtn = gameObject.GetComponent<InputField>();

        if (tmpBtn != null)
            tmpBtn.onValueChanged.AddListener(tmpAction);

    }



    /// <summary>
    ///  动态 添加  接口时间 回调
    /// </summary>
    /// <param name="action"></param>
    public void AddPointDownLisenter(UnityAction<BaseEventData> action)
    {

        EventTrigger trigger = gameObject.GetComponent<EventTrigger>();

        if (trigger == null)
            trigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();

        entry.eventID = EventTriggerType.PointerClick;

        entry.callback = new EventTrigger.TriggerEvent();

        entry.callback.AddListener(action);


        trigger.triggers.Add(entry);

    }

}
