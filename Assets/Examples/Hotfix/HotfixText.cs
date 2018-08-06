using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotfixText : MonoBehaviour {

    private int A;
    private int B;

    void Init()
    {

    }
    void Start()
    {
        Init();
        UnityEngine.Debug.Log(Add(A, B));
    }

    int Add(int a, int b)
    {
        return a - b;
    }
}
