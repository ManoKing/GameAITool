using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XLua;
public class HotFixs : MonoBehaviour
{
    public GameObject StartUp;
    [Serializable]
    struct VersionData
    {
        public string Version;
        public string FixUrl;
    }
    LuaEnv luaevn = new LuaEnv();
    void Awake()
    {
        StartCoroutine(LoadVersion("http://192.168.1.112:8080/version.txt"));
    }
    IEnumerator LoadVersion(string versionUrl)
    {
        WWW versionData = new WWW(versionUrl);

        yield return versionData;

        if (null != versionData.error)
        {
            Debug.LogError(versionData.error);
        }
        else
        {
            VersionData data = JsonUtility.FromJson<VersionData>(versionData.text);
            StartCoroutine(LoadFix(data.FixUrl, data.Version));
        }
    }

    IEnumerator LoadFix(string fixUrl, string version)
    {
        //todo: check storage hotfix version
        WWW fixData = new WWW(fixUrl);

        yield return fixData;

        if (null != fixData.error)
        {
            Debug.LogError(fixData.error);
        }
        else
        {
            ApplyHotFix(fixData.text);         
            SaveToStorage(fixData.text, version);
        }
    }
    void ApplyHotFix(string luastr)
    {
        luaevn.DoString(luastr);
        if (null != StartUp)
        {
            StartUp.SetActive(true);
        }
    }
    void SaveToStorage(string luastr, string version)
    {
        //todo
    }
    private void OnDestroy()
    {

    }
}