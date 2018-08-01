using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LuaFramework;
using System;
using System.IO;

public class ResourceDownloadManager : MonoBehaviour {

    private void Start()
    {
        StartCoroutine(OnUpdateResource());
    }
    /// <summary>
    /// 启动更新下载，此处可启动线程下载更新
    /// </summary>
    IEnumerator OnUpdateResource()
    {
        string dataPath = Util.DataPath;  //数据目录
        string url = AppConst.WebUrl;
        string message = string.Empty;
        string random = DateTime.Now.ToString("yyyymmddhhmmss");
        string listUrl = url + "files.txt?v=" + random;
        WWW www = new WWW(listUrl); yield return www;
        if (www.error != null)
        {
            Debug.Log("更新失败");
            yield break;
        }
        if (!Directory.Exists(dataPath))
        {
            Directory.CreateDirectory(dataPath);
        }
        File.WriteAllBytes(dataPath + "files.txt", www.bytes);
        string filesText = www.text;     //filesText-->  下载files文件
        string[] files = filesText.Split('\n');
        for (int i = 0; i < files.Length; i++)
        {
            if (string.IsNullOrEmpty(files[i])) continue;
            string[] keyValue = files[i].Split('|');
            string f = keyValue[0];
            string localfile = (dataPath + f).Trim();
            string path = Path.GetDirectoryName(localfile);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string fileUrl = url + f + "?v=" + random;
            bool canUpdate = !File.Exists(localfile);
            if (!canUpdate)
            {
                string remoteMd5 = keyValue[1].Trim();
                string localMd5 = Util.md5file(localfile);
                canUpdate = !remoteMd5.Equals(localMd5);
                if (canUpdate) File.Delete(localfile);
            }
            if (canUpdate)
            {   //本地缺少文件
                Debug.Log(fileUrl);            
                www = new WWW(fileUrl); yield return www;
                if (www.error != null) {
                    Debug.Log("更新失败");
                    yield break;
                }
                File.WriteAllBytes(localfile, www.bytes);
               //这里都是资源文件，用线程下载
            }
        }
        yield return new WaitForEndOfFrame();
        OnResourceInited();
    }
    /// <summary>
    /// 资源初始化结束
    /// </summary>
    public void OnResourceInited()
    {
        Debug.Log("下载资源初始化结束");
        ResourceManager.instence.Initialize(AppConst.AssetDir, delegate ()
        {
            this.OnInitialize();
        });
    }
    void OnInitialize()
    {
        Debug.Log("初始化回调");
        ResourceManager.instence.LoadPrefab("login", new string[] { "Button1", "Button2" }, OnLoadFinish);

        //加载游戏
        //加载网络
        //初始化网络
        //初始化完成
    }
    public void OnLoadFinish<T>(T[] objs)
    {
        Debug.Log("实例化");
        Debug.Log(objs[0]);
        Instantiate(objs[0] as GameObject,GameObject.Find("Canvas").transform);
        Instantiate(objs[1] as GameObject, GameObject.Find("Canvas").transform);
    }
}
