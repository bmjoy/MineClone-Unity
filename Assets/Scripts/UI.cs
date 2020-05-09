using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
	public static UI instance { get; private set; }
	public Hotbar hotbar;
	public LoadingScreen loadingScreen;
	public void Initialize()
	{
		instance = this;
		hotbar.Initialize();
		loadingScreen.Initialize();
	}
	public void UpdateUI()
	{
		hotbar.UpdateHotbar();
	}
}
