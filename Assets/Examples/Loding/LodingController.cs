using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LodingController : MonoBehaviour {

	void Start () {
        transform.GetChild(0).GetChild(0).GetComponent<Button>().onClick.AddListener(ActionGame);
	}
    public void ActionGame()
    {
        ResourceManager.instence.LoadPrefab("Loading", new string[] { "Game" }, OnLoadFinish);
    }
    public void OnLoadFinish<T>(T[] objs)
    {
        Instantiate(objs[0] as GameObject, GameObject.Find("Canvas").transform);
    }
}
