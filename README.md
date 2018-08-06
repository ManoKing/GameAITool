# ManoXLua
基于XLua的代码热更新和UIFramework（ToLua）的Assetbundle更新和依赖加载。
###(一)
首先是讲范例XLua:
一个有bug的类
```C#
public class HotfixText: MonoBehaviour {
    private int A;
    private int B;
    void Init() {
    }
    void Start () {
        Init();
        UnityEngine.Debug.Log(Add(A, B));
    }
    int Add(int a, int b) {
        return a - b;
    }
}
```
错误有 A,B 没有初始化
Add函数被写成了减法
我们在白名单中添加HotfixText标记，让我们有机会修复他。

然后是HotFix类
```C#
public class HotFix : MonoBehaviour {
    public GameObject StartUp;
    [Serializable]
    struct VersionData {
        public string Version;
        public string FixUrl;
    }
    LuaEnv luaevn = new LuaEnv();
    void Awake () {

        StartCoroutine(LoadVersion("http://192.168.1.112:8080/version.txt"));
    }

    IEnumerator LoadVersion(string versionUrl) {
        WWW versionData = new WWW(versionUrl);

        yield return versionData;

        if(null != versionData.error) {
            Debug.LogError(versionData.error);
        } else {
            VersionData data = JsonUtility.FromJson<VersionData>(versionData.text);
            StartCoroutine(LoadFix(data.FixUrl, data.Version));
        }
    }
    
    IEnumerator LoadFix(string fixUrl, string version) {

        //todo: check storage hotfix version

        WWW fixData = new WWW(fixUrl);

        yield return fixData;

        if (null != fixData.error) {
            Debug.LogError(fixData.error);
        } else {
            ApplyHotFix(fixData.text);
            SaveToStorage(fixData.text, version);
        }
    }

    void ApplyHotFix(string luastr) {
        luaevn.DoString(luastr);

        if(null != StartUp) {
            StartUp.SetActive(true);
        }
    }

    void SaveToStorage(string luastr, string version) {
        //todo
    }

    private void OnDestroy() {
        
    }
}
```
他会在游戏启动的时候去下载 http://192.168.1.112:8080/version.txt 版本号数据
```Json
{
    "Version": "1",
    "FixUrl": "http://192.168.1.112:8080/hotfix.txt"
}
```
之后去补丁地址下载补丁文件 http://192.168.1.112:8080/hotfix.lua

下载完毕后,缓存到本地, 然后调用游戏启动 StartUp

启动游戏后, 原本控制台输出 0 的结构, 热更新后 控制台输出 600

最后是lua部分的代码
```Lua
xlua.private_accessible(CS.HotfixText)
xlua.hotfix(CS.HotfixText, 'Init',
function(self)
    self.A = 300
    self.B = 300
end
)

xlua.hotfix(CS.HotfixText, 'Add', 
function(self, a, b)
    return a + b
end
)
```
lua部分的代码,就3段 第一段 xlua.private_accessible(CS.HotfixText)
让lua可以访问私有字段, 目的是为了改成员变量 A,B 他们是私有的

第二段是修改 类中的 Init函数, 来初始化A,B。

第三段是修改 类中的 Add函数,让他正确执行加法。

###(二)
然后简单叙述一下UIFramework中Assetbundle的加载方式。
UIFramework中对自动打包没有做过多的处理，我在框架中添加自动打包工具AddBuildMapUtility，使用合理的文件管理方式Art存放热更的资源，
自动打包后UIFramework会在StreamingAssets中建立files文件，通过Assetbundle名和MD5值，对比下载更改的Assetbundle，在ResourceManager中
有Assetbundle的[依赖加载方式](https://zhuanlan.zhihu.com/p/21442566)

在XLua和UIFramework框架的基础上实现用热更新，后续我会不断完善这个框架，增加各个模块，添加一些工具。
