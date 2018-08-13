using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UISubManger : MonoBehaviour {


    Dictionary<string, GameObject> subChild;
    string panelName = "";

    void Awake()
    {
        panelName = transform.GetComponentInParent<UIBase>().transform.name;
        UIManager.Instance.RegistGameObject(panelName, transform.name, gameObject);

        RegistAllChild();

    }


    public void RegistAllChild()
    {

        subChild = new Dictionary<string, GameObject>();


      Transform[]  allChild=  transform.GetComponentsInChildren<Transform>();


      for (int i = 0; i < allChild.Length; i++)
      {

          if (allChild[i].name.EndsWith("_C"))
              subChild.Add(allChild[i].name,allChild[i].gameObject);
         
      }


    }


    public void AddButtonListen(string objName ,UnityAction tmpAction)
    {

       // Debug.Log(" AddButtonListen  =="+objName);
        if (subChild.ContainsKey(objName))
        {
            
            Button tmpBtn = subChild[objName].GetComponent<Button>();

            if (tmpBtn != null)
                tmpBtn.onClick.AddListener(tmpAction);

        }



    }


    public void AddSliderListen(string  objName,UnityAction<float> tmpAction)
    {

        if (subChild.ContainsKey(objName))
        {

            Slider tmpBtn = subChild[objName].GetComponent<Slider>();


            if (tmpBtn != null)
                tmpBtn.onValueChanged.AddListener(tmpAction);

        }

    }

    public void AddInputEndEditro(string objName, UnityAction<string> tmpAction)
    {




        if (subChild.ContainsKey(objName))
        {

            InputField tmpBtn = subChild[objName].GetComponent<InputField>();


            if (tmpBtn != null)
                tmpBtn.onEndEdit.AddListener(tmpAction);

        }



    }

    public void AddInputValueChange(string objName,UnityAction<string> tmpAction)
    {



        if (subChild.ContainsKey(objName))
        {

            InputField tmpBtn = subChild[objName].GetComponent<InputField>();


            if (tmpBtn != null)
                tmpBtn.onValueChanged.AddListener(tmpAction);

        }



    }



    /// <summary>
    ///  动态 添加  接口时间 回调
    /// </summary>
    /// <param name="action"></param>
    public void AddPointDownLisenter(string  objName ,UnityAction<BaseEventData> action)
    {

        if (subChild.ContainsKey(objName))
        {

            EventTrigger trigger = subChild[objName].GetComponent<EventTrigger>();

            if (trigger == null)
                trigger = subChild[objName].AddComponent<EventTrigger>();



            EventTrigger.Entry entry = new EventTrigger.Entry();

            entry.eventID = EventTriggerType.PointerClick;

            entry.callback = new EventTrigger.TriggerEvent();

            entry.callback.AddListener(action);


            trigger.triggers.Add(entry);




        }

    
    }
}
