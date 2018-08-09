using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class HttpUtilDemo : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
        //// 1. get
        //HttpUtil.GetApi("http://192.168.1.27:8080/app/register_sms", (r) =>
        //{
        //    Debug.Log("data: " + r);
        //});

        //// 2. post 无参数
        //HttpUtil.PostApi("http://192.168.1.27:8080/app/logout", null, (r) =>
        //{
        //    Debug.Log("data: " + r);
        //});

        // 3. post 有参数
        Dictionary<string, string> parameters = new Dictionary<string, string>();
        parameters.Add("platformId", "PLF58c4c5026842cdbe3f859p2p");
        parameters.Add("mobile", "18000000000");
        parameters.Add("password", "123456");
        HttpUtil.PostApi("http://192.168.1.27:8080/app/login", parameters, (r) =>
        {
            JObject data = JObject.Parse(r);
            string aesKey = data["aesKey"].Value<string>();
           // double balance = data["balance"].Value<double>();
            Debug.Log("aesKey: " + JObject.Parse(r)["aesKey"].Value<string>());

            HttpUtil.PostApi("http://192.168.1.27:8080/app/info_balance", null, (r2) =>
            {
                InfoBalanceResponse infoBalance = JsonConvert.DeserializeObject<InfoBalanceResponse>(r2);
                Debug.Log("infoBalance: " + JsonConvert.SerializeObject(infoBalance.PayPasswordSet));

                HttpUtil.PostApi("http://192.168.1.27:8080/app/logout", null, (r3) =>
                {
                });
            });
        });

    }

	class InfoBalanceResponse
	{
		public string LoginName { get; set; }

		public string NickName { get; set; }

		public double Balance { get; set; }

		public double Bonus { get; set; }

		public bool PayPasswordSet{ get; set; }
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}
}
