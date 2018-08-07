using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class LoadTexture : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Sprite _sprite = TextureManage.getInstance().LoadAtlasSprite("Texture/Number", "1s7");
        GetComponent<Image>().sprite = _sprite;
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
