using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LoadTexture : MonoBehaviour {

	// Use this for initialization
	void Start () {
        StartCoroutine(Number());
    }
	// Update is called once per frame
	void Update () {
       
	}
    IEnumerator Number()
    {
        int i = 1;
        while (true)
        {
            Sprite _sprite = TextureManage.getInstance().LoadAtlasSprite("Texture/Number", i.ToString());
            GetComponent<Image>().sprite = _sprite;
            GetComponent<Image>().SetNativeSize();
            yield return new WaitForSeconds(0.1f);
            i++;
            if (i>49)
            {
                i = 1;
            }
        }   
    }
}
