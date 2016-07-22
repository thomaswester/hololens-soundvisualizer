using UnityEngine;
using MiniJSON;
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

public class RequestWWW
{
	public bool IsDone
	{
		get;
		private set;
	}

	public string Text
	{
		get;
		private set;
	}

	public string Error
	{
		get;
		private set;
	}

    Dictionary<string, string> headers;

	public IEnumerator doHttpPost(string url, byte[] payload, int meshKey)
	{
        Debug.LogFormat("Posting at {0}", Time.frameCount);
        // Create or clear a dictionary for the headers
		if(headers == null)
        {
            headers = new Dictionary<string, string>();
        } else
        {
            headers.Clear();
        }

		headers.Add("Content-Type", "application/octet-stream");
		headers.Add("Content-Length", payload.Length.ToString());

		if (meshKey != -1)
		{
			headers.Add("slot-key", meshKey.ToString());
		}

		WWW www = new WWW(url, payload, headers);
		yield return www;
		IsDone = true;
		Error = www.error;
		Text = www.text;
        
	}
}

public class MeshSenderHTTP : MonoBehaviour {
	private MeshSerializer serializer = new MeshSerializer();
	private RequestWWW wwwcall;

	private static string rootServerUrl = "";
	private static string authorName = "";
	private static string title = "";

	private static bool isRegistered = false;

	private static bool needsToSend = true;

	private static int meshSlot = -1;
	private static int meshKey = 0;

	private static Mesh meshToSend;

    public void SetMesh(Mesh _mesh)
    {
        meshToSend = _mesh;
    }

	public void Construct(string _rootServerUrl, string _authorName, string _title, Mesh _mesh = null)
	{
		rootServerUrl = _rootServerUrl;
		authorName = _authorName;
		title = _title;
		meshToSend = _mesh;
	}

	public void Register() {
		var regMsg = new Dictionary<string, object>();

		regMsg.Add("author", authorName);
		regMsg.Add("title", title);
		regMsg.Add("platform", "Unity3D");

		byte[] registration = Encoding.UTF8.GetBytes(Json.Serialize(regMsg));

		wwwcall = new RequestWWW();
		StartCoroutine(wwwcall.doHttpPost(rootServerUrl + "/mesh/register", registration, -1));
	}

	void Update () {
		if (wwwcall != null && wwwcall.IsDone && meshKey == 0 && isRegistered == false)
		{
			if (wwwcall.Error != null)
			{
				Debug.Log("error registering: " + wwwcall.Error);
			}
			else
			{
                
				var result = Json.Deserialize(wwwcall.Text) as Dictionary<string, object>;

				if ((bool)result["result"])
				{
					isRegistered = true;
					meshKey = (int)((long)result["key"]);
					meshSlot = (int)((long)result["index"]);
				}
				else
				{
					isRegistered = false;
					Debug.Log("Unable to register: " + (string)result["error"]);
				}
                
			}
			wwwcall = null;
		}
		else if (meshKey != 0 && isRegistered == true && needsToSend == true && meshToSend != null)
		{

            if (wwwcall == null)
			{
				wwwcall = new RequestWWW();
                serializer.Serialize(meshToSend);
                StartCoroutine(wwwcall.doHttpPost(rootServerUrl + "/mesh/" + meshSlot + "/frame", serializer.packet, meshKey));
			}
			else if (wwwcall != null && wwwcall.IsDone)
			{
				wwwcall = null;
			}

		}
	}
}
