using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {



    //  第一个 string  表示  panel     第二个  panle 下面自控件的名字   第三个  Gameobject  子控件的物体。
    public  Dictionary<string, Dictionary<string, GameObject>> allChild;


    GameObject mainCavas;

    public GameObject MainCavas
    {

        get
        {
            return mainCavas;
        }
    }


    public static UIManager Instance;

    void Awake()
    {
        allChild = new Dictionary<string, Dictionary<string, GameObject>>();
        Instance = this;

        mainCavas = GameObject.FindGameObjectWithTag("MainCanvas");

    }


    public GameObject GetGameObject(string panelName, string objName)
    {

        if(allChild.ContainsKey(panelName))
        {

             return  allChild[panelName][objName];
        }


        return null;
    }

   

    public void RegistGameObject(string panelName , string  objName, GameObject  obj)
    {

        if (!allChild.ContainsKey(panelName))
        {

            Dictionary<string, GameObject> tmpDict = new Dictionary<string, GameObject>();

            allChild.Add(panelName, tmpDict);

        }


        allChild[panelName].Add(objName, obj);
 
    }

    public void UnRegistPanelGameObject(string panelName)
    {

        if (allChild.ContainsKey(panelName))
        {
            allChild[panelName].Clear();
        }

    }

    public void UnRegistGameObject(string panelName, string objName)
    {

        if (allChild.ContainsKey(panelName))
        {
            if (allChild[panelName].ContainsKey(objName))
            {

                allChild[panelName].Remove(objName);
 
            }
        }

    }

	
}
