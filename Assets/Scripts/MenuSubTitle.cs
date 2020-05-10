using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class MenuSubTitle : MonoBehaviour
{
	public TextMeshProUGUI text1, text2;
	private readonly string[] titles = new string[]{
		"This is just a sub-title",
		"Woah, made in Unity",
		"Also try Minecraft!"
	};

	private void Start()
	{
		string textToDisplay = titles[Random.Range(0, titles.Length)];
		text1.text = text2.text = textToDisplay;
	}
	void Update()
    {
		float t = Time.time;
		float s = (Mathf.Max( Mathf.Sin(t*12f)+0.75f, 0))*0.05f+1;
		transform.localScale = Vector3.one * s;
    }
}
