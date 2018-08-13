using System.Collections;
using System.Collections.Generic;
using UnityEngine;


using  UnityEngine.Events;


public class UIBase : MonoBehaviour {


    public void AddComponetToChild()
    {

       Transform[]  allChild=  transform.GetComponentsInChildren<Transform>();

       for (int i = 0; i < allChild.Length; i++)
       {

           if (allChild[i].name.EndsWith("_N"))
           {

               allChild[i].gameObject.AddComponent<UIBehaviour>();
           }
           else if (allChild[i].name.EndsWith("_L"))
           {

               allChild[i].gameObject.AddComponent<UISubManger>();
           }
          
       }

    }


    public GameObject GetChildGameObject(string  objName)
    {

        return   UIManager.Instance.GetGameObject(transform.name, objName);
    }
    

   
    public void AddSubButtonListen(string cellName, string cellChild, UnityAction action)
    {


        GameObject cellObj = GetChildGameObject(cellName);

        cellObj.GetComponent<UISubManger>().AddButtonListen(cellChild, action);
    }

    public void AddButtonListen(string  objName,UnityAction  action)
    {

        GameObject sonChild = GetChildGameObject(objName);
        sonChild.GetComponent<UIBehaviour>().AddButtonListen(action);

    }

    public void AddInputFiledEndEditor(string objName, UnityAction<string> action)
    {
        GameObject sonChild = GetChildGameObject(objName);


        sonChild.GetComponent<UIBehaviour>().AddInputEndEditro(action);
    }


    void OnDestroy()
    {

        UIManager.Instance.UnRegistPanelGameObject(transform.name);
    }


    void Awake()
    {
        AddComponetToChild();
    }

	
}
