using UnityEngine;
using System.Collections;

public class OptionScript : MonoBehaviour {
	
	
	private GameObject theSelected;
	public GameObject[] menuItem;
	private Vector3[] Rotate;
	public float speedScale;
	private float loadColor;
	public float speedColor;
	public float speedColorFade;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);	
		RaycastHit hit;
				
		if(Physics.Raycast(ray, out hit))
		{
			
			var theGo = hit.transform.gameObject;
			if(theGo != null){
				theSelected = theGo;
				if(!theSelected.transform.GetChild(0).particleSystem.isPlaying) theSelected.transform.GetChild(0).particleSystem.Play();
				theSelected.renderer.material.color = Color.Lerp(theSelected.renderer.material.color, theSelected.GetComponent<ColorGO>().myColor, loadColor);
				theSelected.transform.localScale = Vector3.Lerp(theSelected.transform.localScale, new Vector3(1.4f, 1.4f, 1.4f), Time.deltaTime/speedScale);
				if(loadColor < 1f) loadColor += Time.deltaTime*speedColor;
			}else if(theSelected != null){
				loadColor = 0f;
				if(theSelected.transform.GetChild(0).particleSystem.isPlaying) theSelected.transform.GetChild(0).particleSystem.Stop();
				theSelected = null;
			}
			
		}else if(theSelected != null){
			loadColor = 0f;
			if(theSelected.transform.GetChild(0).particleSystem.isPlaying) theSelected.transform.GetChild(0).particleSystem.Stop();
			theSelected = null;
		}
		
		var white = new Color(1f, 1f, 1f, 1f);
		for(int i = 0; i < menuItem.Length;i++){
			if(menuItem[i].renderer.material.color != white && menuItem[i] != theSelected){
				var r = menuItem[i].renderer.material.color.r < 1f ? menuItem[i].renderer.material.color.r + Time.deltaTime*speedColorFade : 1f;
				var g = menuItem[i].renderer.material.color.g < 1f ? menuItem[i].renderer.material.color.g + Time.deltaTime*speedColorFade : 1f;
				var b = menuItem[i].renderer.material.color.b < 1f ? menuItem[i].renderer.material.color.b + Time.deltaTime*speedColorFade : 1f;
				menuItem[i].renderer.material.color = new Color( r, g, b, 1f);
			}
			if(menuItem[i].transform.localScale.x > 1.21f){
				menuItem[i].transform.localScale = Vector3.Lerp(menuItem[i].transform.localScale, new Vector3(1.2f, 1.2f, 1.2f), Time.deltaTime/speedScale);
			}else if(menuItem[i].transform.localScale.x != 1.2f){
				menuItem[i].transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);	
			}
			
		}
		
		
		
		
	}
	
}
