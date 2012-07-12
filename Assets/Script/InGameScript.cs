using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InGameScript : MonoBehaviour {
	
	public GameObject arrow;
	public Camera MainCamera;
	public GameObject arrowLeft;
	public GameObject arrowRight;
	public GameObject arrowDown;
	public GameObject arrowUp;
	
	
	
	private Song thesong;
	
	private double timebpm; //Temps jou� sur la totalit�, non live, non remise � 0
	private float timechart; //Temps jou� sur le bpm actuel en live avec remise � 0
	private float timestop; //Temps utilis� pour le freeze
	private float totaltimestop; //Temps total � �tre stopp�
	private double timetotalchart; //Temps jou� sur la totalit� en live (timebpm + timechart)
	private float changeBPM; //Position en y depuis un changement de bpm (nouvelle reference)
	
	private int nextSwitchBPM; //Prochain changement de bpm
	private int nextSwitchStop;
	
	private double actualBPM; //Bpm actuel
	
	private double actualstop;
	
	private bool dontstopmenow;
	
	public float speedmod = 4f; //speedmod (trop lent ?)
	
	
	//fps count
	private long _count;
	private long fps;
	private float _timer;
	
	//stepchart divis� par input
	private Dictionary<GameObject, double> arrowLeftList;
	private Dictionary<GameObject, double> arrowRightList;
	private Dictionary<GameObject, double> arrowDownList;
	private Dictionary<GameObject, double> arrowUpList;
	
	// Use this for initialization
	
	void Start () {
		thesong = OpenChart.Instance.readChart("BrokenTheMoon")[0];
		createTheChart(thesong);
		Application.targetFrameRate = -1;
		nextSwitchBPM = 1;
		nextSwitchStop = 0;
		actualBPM = thesong.bpms.First().Value;
		actualstop = (double)0;
		changeBPM = 0;
		
		_count = 0L;
		
		timebpm = (double)0;
		timechart = 0f;
		timetotalchart = (double)0;
		
		dontstopmenow = true;
	}
	
	
	void OnGUI(){
		GUI.Label(new Rect(0.9f*Screen.width, 0.05f*Screen.height, 200f, 200f), fps.ToString());	
	}
	
	// Update is called once per frame
	void Update () {
		
		_count++;
		_timer += Time.deltaTime;
		if(_timer >= 1f){
			fps = _count;
			_count = 0L;
			_timer = 0;
		}
		
		timetotalchart = timebpm + timechart + totaltimestop;
		
		VerifyKeys();
		
		
		if(nextSwitchBPM < thesong.bpms.Count && (thesong.bpms.ElementAt(nextSwitchBPM).Key <= timetotalchart)){
			
			
			var bps = thesong.getBPS(actualBPM);
			changeBPM += -((float)(bps*(timechart - (float)(timetotalchart - thesong.bpms.ElementAt(nextSwitchBPM).Key))))*speedmod;
			timebpm += (double)timechart - (double)(timetotalchart - thesong.bpms.ElementAt(nextSwitchBPM).Key);
			timechart = 0f;
			actualBPM = thesong.bpms.ElementAt(nextSwitchBPM).Value;
			
			nextSwitchBPM++;
		}
		
		
		if(nextSwitchStop < thesong.stops.Count && (thesong.stops.ElementAt(nextSwitchStop).Key <= timetotalchart)){
			timetotalchart = timebpm + timechart + totaltimestop;
			timechart -= (float)timetotalchart - (float)thesong.stops.ElementAt(nextSwitchStop).Key;
			timetotalchart = timebpm + timechart + totaltimestop;
			actualstop = thesong.stops.ElementAt(nextSwitchStop).Value;
			dontstopmenow =  true;
			nextSwitchStop++;
			//Debug.Log("entry stop : " + timetotalchart);
		}
		
		
		if(actualstop != 0){
			if(timestop >= actualstop){
				timechart += timestop - (float)actualstop;
				totaltimestop += (float)actualstop;
				timetotalchart = timebpm + timechart + totaltimestop;
				//Debug.Log("exit stop : " + timetotalchart);
				actualstop = (double)0;
				timestop = 0f;
				//may be ?
				//timechart += Time.deltaTime;
			}else if(!dontstopmenow){
				
				timestop += Time.deltaTime;
				//totaltimestop += Time.deltaTime;
			}else{
				dontstopmenow = false;
			}
			//Debug.Log("stopped : " + timestop);
		}else{
			timechart += Time.deltaTime;
		}
		
		
		
	}
	
	void FixedUpdate(){
		
		var bps = thesong.getBPS(actualBPM);
		MainCamera.transform.position = new Vector3(3f, -((float)(bps*timechart))*speedmod +  changeBPM - 6, -10f);
		arrowLeft.transform.position = new Vector3(0f, -((float)(bps*timechart))*speedmod  + changeBPM, 2f);
		arrowRight.transform.position = new Vector3(6f, -((float)(bps*timechart))*speedmod + changeBPM, 2f);
		arrowDown.transform.position = new Vector3(2f, -((float)(bps*timechart))*speedmod + changeBPM, 2f);
		arrowUp.transform.position = new Vector3(4f, -((float)(bps*timechart))*speedmod + changeBPM, 2f);
	}
	
	
	
	void VerifyKeys(){
		if(Input.GetKeyDown(KeyCode.LeftArrow)){
			var ar = findNextLeftArrow();
			double prec = Mathf.Abs((float)(ar.Value - (timetotalchart - Time.deltaTime)));
			Debug.Log("AL ! " + prec);
			if(prec < (double)0.102){ //great
				arrowLeftList.Remove(ar.Key);
				DestroyImmediate(ar.Key);
				Debug.Log("Great !");
			//Start coroutine score
			}else if(prec < (double)0.300){ //miss
				ar.Key.GetComponent<ArrowScript>().missed = true;
				arrowLeftList.Remove(ar.Key);
				Debug.Log("Miss... !");
			}else{
				Debug.Log("Not so close");
			}
		
		}
		
		
		if(Input.GetKeyDown(KeyCode.DownArrow)){
			var ar = findNextDownArrow();
			double prec = Mathf.Abs((float)(ar.Value - (timetotalchart - Time.deltaTime)));
			Debug.Log("AL ! " + prec);
			if(prec < (double)0.102){ //great
				arrowDownList.Remove(ar.Key);
				DestroyImmediate(ar.Key);
				Debug.Log("Great !");
			//Start coroutine score
			}else if(prec < (double)0.300){ //miss
				ar.Key.GetComponent<ArrowScript>().missed = true;
				arrowDownList.Remove(ar.Key);
				Debug.Log("Miss... !");
			}else{
				Debug.Log("Not so close");
			}
		
		}
		
		
		if(Input.GetKeyDown(KeyCode.UpArrow)){
			var ar = findNextUpArrow();
			double prec = Mathf.Abs((float)(ar.Value - (timetotalchart - Time.deltaTime)));
			Debug.Log("AL ! " + prec);
			if(prec < (double)0.102){ //great
				arrowUpList.Remove(ar.Key);
				DestroyImmediate(ar.Key);
				Debug.Log("Great !");
			//Start coroutine score
			}else if(prec < (double)0.300){ //miss
				ar.Key.GetComponent<ArrowScript>().missed = true;
				arrowUpList.Remove(ar.Key);
				Debug.Log("Miss... !");
			}else{
				Debug.Log("Not so close");
			}
		
		}
		
		
		if(Input.GetKeyDown(KeyCode.RightArrow)){
			var ar = findNextRightArrow();
			double prec = Mathf.Abs((float)(ar.Value - (timetotalchart - Time.deltaTime)));
			Debug.Log("AL ! " + prec);
			if(prec < (double)0.102){ //great
				arrowRightList.Remove(ar.Key);
				DestroyImmediate(ar.Key);
				Debug.Log("Great !");
			//Start coroutine score
			}else if(prec < (double)0.300){ //miss
				ar.Key.GetComponent<ArrowScript>().missed = true;
				arrowRightList.Remove(ar.Key);
				Debug.Log("Miss... !");
			}else{
				Debug.Log("Not so close");
			}
		
		}
		
	}
	
	void createTheChart(Song s){
		
		var ypos = 0f;
		arrowLeftList = new Dictionary<GameObject, double>();
		arrowUpList = new Dictionary<GameObject, double>();
		arrowDownList = new Dictionary<GameObject, double>();
		arrowRightList = new Dictionary<GameObject, double>();
		
		
		var theBPMCounter = 1;
		var theSTOPCounter = 0;
		double mesurecount = 0;
		double prevmesure = 0;
		double timecounter = 0;
		double timeBPM = 0;
		double timestop = 0;
		double timetotal = 0;
		float prec = 0.001f;
		foreach(var mesure in s.stepchart){
			
			for(int beat=0;beat<mesure.Count;beat++){
			
			
				var bps = s.getBPS(s.bpms.ElementAt(theBPMCounter-1).Value);
				if(theBPMCounter < s.bpms.Count && theSTOPCounter < s.stops.Count){
					while((theBPMCounter < s.bpms.Count && theSTOPCounter < s.stops.Count) 
						&& (s.mesureBPMS.ElementAt(theBPMCounter) < mesurecount - prec || s.mesureSTOPS.ElementAt(theSTOPCounter) < mesurecount - prec)){
					
						if(s.mesureBPMS.ElementAt(theBPMCounter) < s.mesureSTOPS.ElementAt(theSTOPCounter)){
							timecounter += (s.mesureBPMS.ElementAt(theBPMCounter) - prevmesure)/bps;
							
							timeBPM += timecounter;
							timecounter = 0;
							prevmesure = s.mesureBPMS.ElementAt(theBPMCounter);
							theBPMCounter++;
							bps = s.getBPS(s.bpms.ElementAt(theBPMCounter-1).Value);
							//Debug.Log("And bpm change before / bpm");
						}else if(s.mesureBPMS.ElementAt(theBPMCounter) > s.mesureSTOPS.ElementAt(theSTOPCounter)){
							timestop += s.stops.ElementAt(theSTOPCounter).Value;
							theSTOPCounter++;
							//Debug.Log("And stop change before");
						}else{
							timecounter += (s.mesureBPMS.ElementAt(theBPMCounter) - prevmesure)/bps;
							timeBPM += timecounter;
							timecounter = 0;
							prevmesure = s.mesureBPMS.ElementAt(theBPMCounter);
							theBPMCounter++;
							bps = s.getBPS(s.bpms.ElementAt(theBPMCounter-1).Value);
							
							timestop += s.stops.ElementAt(theSTOPCounter).Value;
							theSTOPCounter++;
							//Debug.Log("And bpm change before");
							//Debug.Log("And stop change before");
						}
						
					}
				}else if(theBPMCounter < s.bpms.Count){
					while((theBPMCounter < s.bpms.Count) && s.mesureBPMS.ElementAt(theBPMCounter) < mesurecount - prec){
						
						timecounter += (s.mesureBPMS.ElementAt(theBPMCounter) - prevmesure)/bps;
							
						timeBPM += timecounter;
						timecounter = 0;
						prevmesure = s.mesureBPMS.ElementAt(theBPMCounter);
						theBPMCounter++;
						bps = s.getBPS(s.bpms.ElementAt(theBPMCounter-1).Value);
					
					}
				}else if((theSTOPCounter < s.stops.Count) && theSTOPCounter < s.stops.Count){
					while(s.mesureSTOPS.ElementAt(theSTOPCounter) < mesurecount - prec){
						
						timestop += s.stops.ElementAt(theSTOPCounter).Value;
						theSTOPCounter++;
					
					}
				}
				
				
				timecounter += (mesurecount - prevmesure)/bps;
				
				
				timetotal = timecounter + timeBPM + timestop;
				
				char[] note = mesure.ElementAt(beat).Trim().ToCharArray();
				//var barrow = false;
				for(int i =0;i<4; i++){
					if(note[i] == '1' || note[i] == '2'){
						//var theArrow = (GameObject) Instantiate(arrow, new Vector3(i*2, -ypos  + (float)s.offset, 0f), arrow.transform.rotation);
						var theArrow = (GameObject) Instantiate(arrow, new Vector3(i*2, -ypos, 0f), arrow.transform.rotation);
						theArrow.renderer.material.color = chooseColor(beat + 1, mesure.Count);
						switch(i){
						case 0:
							arrowLeftList.Add(theArrow, timetotal);
							break;
						case 1:
							arrowDownList.Add(theArrow, timetotal);
							break;
						case 2:
							arrowUpList.Add(theArrow, timetotal);
							break;
						case 3:
							arrowRightList.Add(theArrow, timetotal);
							break;
						}
						
						//barrow = true;
					}
					
				
					
				}
				
				
				
				//if(barrow)Debug.Log("ARROW : " + timetotal);
				//if(!barrow)Debug.Log("no arrow : " + timetotal);	
				
				
				if(theBPMCounter < s.bpms.Count){
					if(Mathf.Abs((float)(s.mesureBPMS.ElementAt(theBPMCounter) - mesurecount)) < prec){
							timeBPM += timecounter;
							timecounter = 0;
							theBPMCounter++;
							//Debug.Log("And bpm change");
					}
				}
				
				if(theSTOPCounter < s.stops.Count){
					if(Mathf.Abs((float)(s.mesureSTOPS.ElementAt(theSTOPCounter) - mesurecount)) < prec){
							timestop += s.stops.ElementAt(theSTOPCounter).Value;
							theSTOPCounter++;
							//Debug.Log("And stop");
					}
				}
				prevmesure = mesurecount;
				mesurecount += (4f/(float)mesure.Count);
				
				
				ypos += (4f/(float)mesure.Count)*speedmod;
				
			}
		}
	}
	
	
	
	public void removeArrowFromList(GameObject ar, string state){
		
		switch(state){
			case "left":
				arrowLeftList.Remove(ar);
				break;
			case "down":
				arrowDownList.Remove(ar);
				break;
			case "up":
				arrowUpList.Remove(ar);
				break;
			case "right":
				arrowRightList.Remove(ar);
				break;
				
		}
	}
	
	public KeyValuePair<GameObject, double> findNextUpArrow(){

		return arrowUpList.FirstOrDefault(s => s.Value == arrowUpList.Min(c => c.Value));
			
	}
	
	public KeyValuePair<GameObject, double> findNextDownArrow(){

		return arrowDownList.FirstOrDefault(s => s.Value == arrowDownList.Min(c => c.Value));
			
	}
	
	public KeyValuePair<GameObject, double> findNextLeftArrow(){

		return arrowLeftList.FirstOrDefault(s => s.Value == arrowLeftList.Min(c => c.Value));
			
	}
	
	public KeyValuePair<GameObject, double> findNextRightArrow(){

		return arrowRightList.FirstOrDefault(s => s.Value == arrowRightList.Min(c => c.Value));
			
	}
	
	
	Color chooseColor(int posmesure, int mesuretot){
		
		
		switch(mesuretot){
		case 4:	
			return new Color(1f, 0f, 0f, 1f);
		
			
		case 8:
			if(posmesure%2 == 0){
				return new Color(0f, 0f, 1f, 1f);
			}
			return new Color(1f, 0f, 0f, 1f);
			
			
		case 12:
			if((posmesure-1)%3 == 0){
				return new Color(1f, 0f, 0f, 1f);
			}
			return new Color(1f, 0f, 1f, 1f);
			
		case 16:
			if((posmesure-1)%4 == 0){
				return new Color(1f, 0f, 0f, 1f);
			}else if((posmesure+1)%4 == 0){
				return new Color(0f, 0f, 1f, 1f);
			}
			return new Color(0f, 1f, 0f, 1f);
		
			
		case 24:
			if((posmesure+2)%6 == 0){
				return new Color(0f, 0f, 1f, 1f);
			}else if((posmesure-1)%6 == 0){
				return new Color(1f, 0f, 0f, 1f);
			}
			return new Color(1f, 0f, 1f, 1f);
			
			
		case 32:
			if((posmesure-1)%8 == 0){
				return new Color(1f, 0f, 0f, 1f);
			}else if((posmesure+3)%8 == 0){
				return new Color(0f, 0f, 1f, 1f);
			}else if((posmesure+1)%4 == 0){
				return new Color(0f, 1f, 0f, 1f);
			}
			return new Color(1f, 0.8f, 0f, 1f);
			
		case 48:
			if((posmesure+2)%6 == 0){
				return new Color(0f, 1f, 0f, 1f);
			}else if((posmesure-1)%12 == 0){
				return new Color(1f, 0f, 0f, 1f);
			}else if((posmesure+5)%12 == 0){
				return new Color(0f, 0f, 1f, 1f);
			}
			return new Color(1f, 0f, 1f, 1f);
		
			
		case 64:
			if((posmesure-1)%16 == 0){
				return new Color(1f, 0f, 0f, 1f);
			}else if((posmesure+7)%16 == 0){
				return new Color(0f, 0f, 1f, 1f);
			}else if((posmesure+3)%8 == 0){
				return new Color(0f, 1f, 0f, 1f);
			}else if((posmesure+1)%4 == 0){
				return new Color(1f, 0.8f, 0f, 1f);
			}
			return new Color(0f, 1f, 0.8f, 1f);
			
		case 192:
			if((posmesure-1)%48 == 0){
				return new Color(1f, 0f, 0f, 1f);
			}else if((posmesure+13)%48 == 0){
				return new Color(0f, 0f, 1f, 1f);
			}else if((posmesure+11)%24 == 0){
				return new Color(0f, 1f, 0f, 1f);
			}else if((posmesure+5)%12 == 0){
				return new Color(1f, 0.8f, 0f, 1f);
			}
			return new Color(0f, 1f, 0.8f, 1f);
			
		default:
			return new Color(1f, 1f, 1f, 1f);
			
		}
	
		
	}
}
