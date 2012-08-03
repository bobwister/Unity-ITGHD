using UnityEngine;
using System.Collections;

public class FadeManager : MonoBehaviour {
	
	
	private Texture2D fade;
	
	public float posxfinal;
	public float speedFade;
	private float posFadex;
	
	private FadeState fs;
	
	public bool startFeded;
	// Use this for initialization
	void Start () {
		fade = (Texture2D) Resources.Load("Fade");
		fs = FadeState.NONE;
		if(startFeded) fs = FadeState.DISPLAY;
	}
	
	
	void OnGUI(){
		if(fs != FadeState.NONE){
			switch(fs){
			case FadeState.FADEIN:
				GUI.DrawTexture(new Rect(posFadex*Screen.width, 0f, Screen.width, Screen.height), fade);
				posFadex -= Time.deltaTime/speedFade;
				if(posFadex <= posxfinal){
					fs = FadeState.DISPLAY;	
				}
				break;
			case FadeState.FADEOUT:
				GUI.DrawTexture(new Rect(posFadex*Screen.width, 0f, Screen.width, Screen.height), fade);
				posFadex += Time.deltaTime/speedFade;
				if(posFadex >= 1f){
					fs = FadeState.NONE;	
				}
				break;
			case FadeState.DISPLAY:
				GUI.DrawTexture(new Rect(posFadex*Screen.width, 0f, Screen.width, Screen.height), fade);
				break;
			}
		}
	}
	
	public void FadeIn(){
		posFadex = 1f;
		fs = FadeState.FADEIN;
	}
	
	public void FadeOut(){
		posFadex = posxfinal;
		fs = FadeState.FADEOUT;
	}
}
