using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class InGameScript : MonoBehaviour {
	
	#region Declarations
	//Game object song scene
	public GameObject arrow;
	public GameObject freeze;
	public Camera MainCamera;
	public GameObject arrowLeft;
	public GameObject arrowRight;
	public GameObject arrowDown;
	public GameObject arrowUp;
	public Material matArrowModel;
	public GameObject particlePrec;
	public GameObject lifeBar;
	public GameObject progressBar;
	
	
	private LifeBar theLifeBar;
	
	private Song thesong;
	
	
	private AudioSource source;
	
	//USED FOR UPDATE FUNCTION
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
	public float speedmod = 4f; //speedmod (trop lent ?)
	
	//Temps pour le lachement de freeze
	public float unfrozed = 0.350f;
	
	//DISPLAY PREC
	
	//Temps pour l'affichage des scores
	public float limitDisplayScore = 2f;
	private float timeDisplayScore = 0f;
	public float timeClignotementFantastic = 0.05f;
	public float baseZoom = 20f;
	public float vitesseZoom = 0.1f;
	private bool sensFantastic;
	private float alpha;
	private float zoom;
	private Precision scoreToDisplay;
	public Rect posScore = new Rect(0.38f,0.3f,0.25f,0.25f);
	
	
	//LIFE
	private float life;
	private Dictionary<string, float> lifeBase;
	
	
	//SCORE
	private float score;
	private Dictionary<string, float> scoreBase;
	public Rect posPercent = new Rect(0.39f, 0f, 0.45f, 0.05f);
	//FPS
	private long _count;
	private long fps;
	private float _timer;
	
	//CHART
	private List<Arrow> arrowLeftList;
	private List<Arrow> arrowRightList;
	private List<Arrow> arrowDownList;
	private List<Arrow> arrowUpList;
	//Dico des arrow prises en freeze
	private Dictionary<Arrow, float> arrowFrozen;
	
	//PARTICLE SYSTEM
	private GameObject precLeft;
	private GameObject precRight;
	private GameObject precUp;
	private GameObject precDown;
	
	
	
	//GUI
	private Dictionary<string, Texture2D> TextureBase;
	
	
	//DISPLAY
	private Color bumpColor;
	public float bumpfadeSpeed = 0.5f;
	
	
	//START
	private bool firstUpdate;
	private float oneSecond;
	private float startTheSong; //Time pour d�marrer la chanson
	
	//BUMP
	private int nextBump;
	private List<double> Bumps;
	public float speedBumps;
	
	
	//PROGRESSBAR
	private double firstArrow;
	private double lastArrow;
	
	//COMBO
	private int combo;

	//DEBUG
	//private int iwashere;
	
	#endregion
	
	
	//Start
	void Start () {
		firstArrow = -10f;
		lastArrow = -10f;
		thesong = OpenChart.Instance.readChart("BulletProof")[0];
		createTheChart(thesong);
		Application.targetFrameRate = -1;
		nextSwitchBPM = 1;
		nextSwitchStop = 0;
		actualBPM = thesong.bpms.First().Value;
		actualstop = (double)0;
		changeBPM = 0;
		
		_count = 0L;
		
		timebpm = (double)0;
		timechart = 0f;//-(float)thesong.offset;
		timetotalchart = (double)0;
		
		
		arrowFrozen = new Dictionary<Arrow, float>();
		
		
		//Prepare the scene
		precLeft = (GameObject) arrowLeft.transform.GetChild(0).gameObject;
		precDown = (GameObject) arrowDown.transform.GetChild(0).gameObject;
		precRight = (GameObject) arrowRight.transform.GetChild(0).gameObject;
		precUp = (GameObject) arrowUp.transform.GetChild(0).gameObject;
		
		//Textures
		TextureBase = new Dictionary<string, Texture2D>();
		TextureBase.Add("FANTASTIC", (Texture2D) Resources.Load("Fantastic"));
		TextureBase.Add("EXCELLENT", (Texture2D) Resources.Load("Excellent"));
		TextureBase.Add("GREAT", (Texture2D) Resources.Load("Great"));
		TextureBase.Add("DECENT", (Texture2D) Resources.Load("Decent"));
		TextureBase.Add("WAYOFF", (Texture2D) Resources.Load("Wayoff"));
		TextureBase.Add("MISS", (Texture2D) Resources.Load("Miss"));
		TextureBase.Add("SCORENUMBER", (Texture2D) Resources.Load("NumberScore"));
		TextureBase.Add("SCORESYMBOL", (Texture2D) Resources.Load("SymbolScore"));
		
		//stuff
		scoreToDisplay = Precision.NONE;
		timeDisplayScore = Mathf.Infinity;
		sensFantastic = true;
		alpha = 1f;
		
		
		
		
		//init score and lifebase
		scoreBase = new Dictionary<string, float>();
		lifeBase = new Dictionary<string, float>();
		var fantasticValue = 100f/(thesong.numberOfStepsWithoutJumps + thesong.numberOfJumps + thesong.numberOfFreezes + thesong.numberOfRolls);
		foreach(Precision el in Enum.GetValues(typeof(Precision))){
			if(el != Precision.NONE){
				scoreBase.Add(el.ToString(), fantasticValue*DataManager.Instance.ScoreWeightValues[el.ToString()]);
				lifeBase.Add(el.ToString(), DataManager.Instance.LifeWeightValues[el.ToString()]);
			}
		}
		
		life = 50f;
		score = 0f;
		
		
		theLifeBar = lifeBar.GetComponent<LifeBar>();
		
		
		source = gameObject.GetComponent<AudioSource>();
		//var bps = thesong.getBPS(actualBPM);
		//changeBPM -= (float)(bps*thesong.offset)*speedmod;
		firstUpdate = true;
		oneSecond = 0f;
		startTheSong = (float)thesong.offset + DataManager.Instance.globalOffsetSeconds;
		
		
		//bump
		nextBump = 0;
		
	}
	
	
	
	
	//only for FPS
	void OnGUI(){
		
		//fake stuff
		GUI.Label(new Rect(0.9f*Screen.width, 0.05f*Screen.height, 200f, 200f), fps.ToString());	
		GUI.Label(new Rect(0.9f*Screen.width, 0.1f*Screen.height, 200f, 200f), ((float)Screen.width/(float)Screen.height).ToString());
		GUI.Label(new Rect(0.9f*Screen.width, 0.15f*Screen.height, 200f, 200f), life.ToString() + " hp");
		GUI.Label(new Rect(0.9f*Screen.width, 0.20f*Screen.height, 200f, 200f), score.ToString("0.00") + "%");
		//end fake stuff
		
		
		if(timeDisplayScore < limitDisplayScore){

			GUI.color = new Color(1f, 1f, 1f, alpha);
			GUI.DrawTexture(new Rect(posScore.x*Screen.width - zoom, posScore.y*Screen.height, posScore.width*Screen.width + zoom*2, posScore.height*Screen.height), TextureBase[scoreToDisplay.ToString()]); 
			
		}
		
		if(Event.current.type.Equals(EventType.Repaint)){
			var wd = posPercent.width*128;
			var hg = posPercent.height*1024;
			var hgt = posPercent.height*254;
			var ecart = 92f;
			var displaying = scoreDecoupe(score);
			if(score >= 10f) Graphics.DrawTexture(new Rect(posPercent.x*Screen.width, posPercent.y*Screen.height, wd, hg), TextureBase["SCORENUMBER"], new Rect(0f, - displaying.x, 1f, 0.1f), 0,0,0,0);
			Graphics.DrawTexture(new Rect(posPercent.x*Screen.width + posPercent.width*ecart, posPercent.y*Screen.height, wd, hg), TextureBase["SCORENUMBER"], new Rect(0f, - displaying.y, 1f, 0.1f), 0,0,0,0);
			Graphics.DrawTexture(new Rect(posPercent.x*Screen.width + posPercent.width*(ecart+(ecart/2f)), posPercent.y*Screen.height, wd, hgt*4), TextureBase["SCORESYMBOL"], new Rect(0f, 0f, 1f, 0.5f), 0,0,0,0);
			Graphics.DrawTexture(new Rect(posPercent.x*Screen.width + posPercent.width*ecart*2, posPercent.y*Screen.height, wd, hg), TextureBase["SCORENUMBER"], new Rect(0f, - displaying.width, 1f, 0.1f), 0,0,0,0);
			Graphics.DrawTexture(new Rect(posPercent.x*Screen.width + posPercent.width*ecart*3, posPercent.y*Screen.height, wd, hg), TextureBase["SCORENUMBER"], new Rect(0f, - displaying.height, 1f, 0.1f), 0,0,0,0);
			Graphics.DrawTexture(new Rect(posPercent.x*Screen.width + posPercent.width*ecart*4, posPercent.y*Screen.height, wd, hgt*4), TextureBase["SCORESYMBOL"], new Rect(0f, 0.5f, 1f, 0.5f), 0,0,0,0);
		
		}
		
	}

	
	// Update is called once per frame
	void Update () {
		
		
		//FPS
		_count++;
		_timer += Time.deltaTime;
		if(_timer >= 1f){
			fps = _count;
			_count = 0L;
			_timer = 0;
		}
		
		if(oneSecond >= 1f){
			//timetotal for this frame
			
			timetotalchart = timebpm + timechart + totaltimestop;
			//var totalchartbegin = timetotalchart;
			
			//iwashere = 0;
			
			//Move Arrows
			MoveArrows();
				
			//Verify inputs
			VerifyValidFrozenArrow();
			VerifyKeysInput();
			VerifyKeysOutput();
			
			//Changement bpm et stops
			VerifyBPMnSTOP();
			
			
			
			//Progress time chart
			if(actualstop != 0){
				
					
					
					if(timestop >= actualstop){
						timechart += timestop - (float)actualstop;
						totaltimestop += (float)actualstop - timestop;
						timetotalchart = timebpm + timechart + totaltimestop;
						actualstop = (double)0;
						timestop = 0f;
						timechart += Time.deltaTime;
						//iwashere = 1;
					}else{
						totaltimestop += Time.deltaTime;
						timestop += Time.deltaTime;
						//iwashere = 2;
					}

			}else{
				timechart += Time.deltaTime;
				//iwashere = 3;
				//Debug.Log(timechart);
			}
			
			
			
			//GUI part, because GUI is so slow :(
			//Utile ?
			RefreshGUIPart();
			
			//Start song
			timetotalchart = timebpm + timechart + totaltimestop;
			if(firstUpdate){
				if(startTheSong <= 0f){
					source.PlayOneShot(thesong.song);
					timechart += startTheSong;
					//Debug.Log(startTheSong);
					timetotalchart = timebpm + timechart + totaltimestop;
					firstUpdate = false;
				}else{
					startTheSong -= Time.deltaTime;	
				}
			}
			
			BumpsBPM();
			
			//timetotalchart = timebpm + timechart + totaltimestop;
			//if((timetotalchart - totalchartbegin) < 0.001f)Debug.Log("Progression : " + (timetotalchart - totalchartbegin) + " / dt : " + Time.deltaTime + " / washere : " + iwashere + " / time : " + timetotalchart);
			
		}else{
			oneSecond += Time.deltaTime;
		}
	}
	
	
	void BumpsBPM(){
		if(nextBump < Bumps.Count && Bumps[nextBump] <= timetotalchart){
			matArrowModel.color = new Color(1f, 1f, 1f, 1f);
			nextBump++;
		}	
		
	}
	
	
	void FixedUpdate(){
		if(matArrowModel.color.r > 0.5f){
			var m = 0.5f*Time.deltaTime/speedBumps;
			matArrowModel.color -= new Color(m,m,m, 0f);
		}
		if(timetotalchart >= firstArrow && timetotalchart < lastArrow){
			progressBar.transform.localPosition = new Vector3(-30f, - (10f - 10f*(float)((timetotalchart - firstArrow)/(lastArrow - firstArrow))), 8f);
			progressBar.transform.localScale = new Vector3(0.7f, 10f*(float)((timetotalchart - firstArrow)/(lastArrow - firstArrow)), 1f);
		}
	}
	
	
	
	//For moving arrows or do some displaying things
	
	#region Defilement chart
	void MoveArrows(){
		
		var bps = thesong.getBPS(actualBPM);
		MainCamera.transform.position = new Vector3(3f, -((float)(bps*timechart))*speedmod +  changeBPM - 5, -10f);
		arrowLeft.transform.position = new Vector3(0f, -((float)(bps*timechart))*speedmod  + changeBPM, 2f);
		arrowRight.transform.position = new Vector3(6f, -((float)(bps*timechart))*speedmod + changeBPM, 2f);
		arrowDown.transform.position = new Vector3(2f, -((float)(bps*timechart))*speedmod + changeBPM, 2f);
		arrowUp.transform.position = new Vector3(4f, -((float)(bps*timechart))*speedmod + changeBPM, 2f);
		
		foreach(var el in arrowFrozen.Keys){
			var pos = el.goArrow.transform.position;
			el.goArrow.transform.position = new Vector3(pos.x, -((float)(bps*timechart))*speedmod  + changeBPM, pos.z);
			pos = el.goArrow.transform.position;
			
			el.goFreeze.transform.position = new Vector3(el.goFreeze.transform.position.x, (pos.y + ((el.posEnding.y - pos.y)/2f)) , el.goFreeze.transform.position.z);
			el.goFreeze.transform.localScale = new Vector3(1f, -((el.posEnding.y - pos.y)/2f), 0.1f);
			el.goFreeze.transform.GetChild(0).transform.localScale = new Vector3((el.posEnding.y - pos.y)/(el.posEnding.y - el.posBegining.y), 1f, 0.1f);
			el.changeColorFreeze(arrowFrozen[el], unfrozed);
		}
	}
	
	//IL FAUT TOUT REVOIR
	void VerifyBPMnSTOP(){
		//BPM change verify
		if(nextSwitchBPM < thesong.bpms.Count && (thesong.bpms.ElementAt(nextSwitchBPM).Key <= timetotalchart)){
			
			//iwashere = 1;
			var bps = thesong.getBPS(actualBPM);
			changeBPM += -((float)(bps*(timechart - (float)(timetotalchart - thesong.bpms.ElementAt(nextSwitchBPM).Key))))*speedmod;
			timebpm += (double)timechart - (timetotalchart - thesong.bpms.ElementAt(nextSwitchBPM).Key);
			timechart = (float)(timetotalchart - thesong.bpms.ElementAt(nextSwitchBPM).Key);
			actualBPM = thesong.bpms.ElementAt(nextSwitchBPM).Value;
			nextSwitchBPM++;
		}
		
		
		//Stop verif
		if(nextSwitchStop < thesong.stops.Count && (thesong.stops.ElementAt(nextSwitchStop).Key <= timetotalchart)){
			//iwashere = 2;
			timetotalchart = timebpm + timechart + totaltimestop;
			timechart -= (float)timetotalchart - (float)thesong.stops.ElementAt(nextSwitchStop).Key;
			timestop += (float)timetotalchart - (float)thesong.stops.ElementAt(nextSwitchStop).Key;
			totaltimestop += (float)timetotalchart - (float)thesong.stops.ElementAt(nextSwitchStop).Key;
			timetotalchart = timebpm + timechart + totaltimestop;
			actualstop = thesong.stops.ElementAt(nextSwitchStop).Value;
			nextSwitchStop++;
		}
		
	}
	
	#endregion
	
	
	void RefreshGUIPart(){
		if(scoreToDisplay == Precision.FANTASTIC){
			if(sensFantastic){
				alpha -= 0.2f*Time.deltaTime/timeClignotementFantastic;
				sensFantastic = alpha > 0.8f;
			}else{
				alpha += 0.2f*Time.deltaTime/timeClignotementFantastic;
				sensFantastic = alpha >= 1f;
			}
		}else{
			alpha = 1f;
		}
		
		if(zoom > 0 && scoreToDisplay != Precision.DECENT && scoreToDisplay != Precision.WAYOFF){
			zoom -= baseZoom*Time.deltaTime/vitesseZoom;
		}else{
			zoom = 0;
		}
		
		
		timeDisplayScore += Time.deltaTime;
		
	}
	
	
	#region Inputs verify
	//Valid or deny the frozen arrow
	void VerifyValidFrozenArrow(){
		if(arrowFrozen.Count > 0){
			var KeyToRemove = new List<Arrow>();
			for(int i=0; i<arrowFrozen.Count;i++){
				var el = arrowFrozen.ElementAt(i);
				arrowFrozen[el.Key] += Time.deltaTime;
				
				if(el.Key.timeEndingFreeze <= timetotalchart){
					switch((int)el.Key.goArrow.transform.position.x){
					case 0:
						StartParticleFreezeLeft(false);
						break;
					case 2:
						StartParticleFreezeDown(false);
						break;
					case 4:
						StartParticleFreezeUp(false);
						break;
					case 6:
						StartParticleFreezeRight(false);
						break;
					}
					GainScoreAndLife("FREEZE");
					DestroyImmediate(el.Key.goArrow);
					DestroyImmediate(el.Key.goFreeze);
					
					KeyToRemove.Add(el.Key);
				}
				
				
				if(el.Value >= unfrozed && !KeyToRemove.Contains(el.Key)){
					el.Key.goArrow.GetComponent<ArrowScript>().missed = true;
					switch((int)el.Key.goArrow.transform.position.x){
					case 0:
						StartParticleFreezeLeft(false);
						break;
					case 2:
						StartParticleFreezeDown(false);
						break;
					case 4:
						StartParticleFreezeUp(false);
						break;
					case 6:
						StartParticleFreezeRight(false);
						break;
					}
					GainScoreAndLife("UNFREEZE");
					KeyToRemove.Add(el.Key);
				}
				
				
			}
			
			
			
			foreach(var k in KeyToRemove){
				arrowFrozen.Remove(k);
			}
			
		}
	}
	
	
	
	//Verify keys Input at this frame
	//Null ref exception quand il n'y a plus de fl�che !
	void VerifyKeysInput(){
		
		if(Input.GetKeyDown(KeyCode.LeftArrow) && arrowLeftList.Any()){
			var ar = findNextLeftArrow();
			double prec = Mathf.Abs((float)(ar.time - (timetotalchart - Time.deltaTime)));
			
			if(prec < precToTime(Precision.GREAT)){ //great
				
				
				
				if(ar.imJump){
					ar.alreadyValid = true;
					ar.goArrow.GetComponent<ArrowScript>().valid = true;
					
					if(!ar.neighboors.Any(c => c.alreadyValid == false)){
						var left = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 0);
						var down = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 2);
						var up = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 4);
						var right = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 6);
						if(left != null){
							if(left.arrowType == ArrowType.NORMAL || left.arrowType == ArrowType.MINE){
								DestroyImmediate(left.goArrow);
								
							}else{
								arrowFrozen.Add(left,0f);
								left.displayFrozenBar();
								StartParticleFreezeLeft(true);
								
							}
							
							arrowLeftList.Remove(left);
							StartParticleLeft(timeToPrec(prec));
						}
						if(down != null){
							if(down.arrowType == ArrowType.NORMAL || down.arrowType == ArrowType.MINE){
								DestroyImmediate(down.goArrow);
								
							}else{
								arrowFrozen.Add(down,0f);
								down.displayFrozenBar();
								StartParticleFreezeDown(true);
								
							}
							
							arrowDownList.Remove(down);
							StartParticleDown(timeToPrec(prec));
						}
						if(up != null){
							if(up.arrowType == ArrowType.NORMAL || up.arrowType == ArrowType.MINE){
								DestroyImmediate(up.goArrow);
								
							}else{
								arrowFrozen.Add(up,0f);
								up.displayFrozenBar();
								StartParticleFreezeUp(true);
								
							}
							arrowUpList.Remove(up);
							StartParticleUp(timeToPrec(prec));
						}
						if(right != null){
							if(right.arrowType == ArrowType.NORMAL || right.arrowType == ArrowType.MINE){
								DestroyImmediate(right.goArrow);
								
							}else{
								arrowFrozen.Add(right,0f);
								right.displayFrozenBar();
								StartParticleFreezeRight(true);
								
							}
							arrowRightList.Remove(right);
							StartParticleRight(timeToPrec(prec));
						}
						GainScoreAndLife(timeToPrec(prec).ToString());
						displayPrec(prec);
						combo += ar.neighboors.Count;
					}
				}else{
					if(ar.arrowType == ArrowType.NORMAL || ar.arrowType == ArrowType.MINE){
						DestroyImmediate(ar.goArrow);
						
					}else{
						ar.goArrow.GetComponent<ArrowScript>().valid = true;
						arrowFrozen.Add(ar,0f);
						ar.displayFrozenBar();
						StartParticleFreezeLeft(true);
						
					}
					combo++;
					arrowLeftList.Remove(ar);
					StartParticleLeft(timeToPrec(prec));
					GainScoreAndLife(timeToPrec(prec).ToString());
					displayPrec(prec);
				}
				
			}else if(prec < precToTime(Precision.WAYOFF)){ //miss
					if(ar.imJump){
					
						ar.alreadyValid = true;
						ar.goArrow.GetComponent<ArrowScript>().missed = true;
						if(!ar.neighboors.Any(c => c.alreadyValid == false)){
							var left = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 0);
							var down = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 2);
							var up = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 4);
							var right = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 6);
							if(left != null){
								
								arrowLeftList.Remove(left);
								StartParticleLeft(timeToPrec(prec));
							}
							if(down != null){
								
								
								arrowDownList.Remove(down);
								StartParticleDown(timeToPrec(prec));
							}
							if(up != null){
								
								arrowUpList.Remove(up);
								StartParticleUp(timeToPrec(prec));
							}
							if(right != null){
								
								arrowRightList.Remove(right);
								StartParticleRight(timeToPrec(prec));
							}
							GainScoreAndLife(timeToPrec(prec).ToString());
							displayPrec(prec);
						}
					}else{
						ar.goArrow.GetComponent<ArrowScript>().missed = true;
						arrowLeftList.Remove(ar);
						StartParticleLeft(timeToPrec(prec));
						displayPrec(prec);
						GainScoreAndLife(timeToPrec(prec).ToString());
					}
					ComboStop();
			}
			
			
			if(arrowFrozen.Any(c => c.Key.goArrow.transform.position.x == 0f && c.Key.arrowType == ArrowType.ROLL))
			{
				var froz = arrowFrozen.First(c => c.Key.goArrow.transform.position.x == 0f);
				arrowFrozen[froz.Key] = 0f;
			}
			
		}
		
		
		if(Input.GetKeyDown(KeyCode.DownArrow) && arrowDownList.Any()){
			var ar = findNextDownArrow();
			double prec = Mathf.Abs((float)(ar.time - (timetotalchart - Time.deltaTime)));
			
			if(prec < precToTime(Precision.GREAT)){ //great
				
				
				if(ar.imJump){
					ar.alreadyValid = true;
					ar.goArrow.GetComponent<ArrowScript>().valid = true;
					
					if(!ar.neighboors.Any(c => c.alreadyValid == false)){
						var left = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 0);
						var down = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 2);
						var up = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 4);
						var right = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 6);
						if(left != null){
							if(left.arrowType == ArrowType.NORMAL || left.arrowType == ArrowType.MINE){
								DestroyImmediate(left.goArrow);
								
							}else{
								arrowFrozen.Add(left,0f);
								left.displayFrozenBar();
								StartParticleFreezeLeft(true);
								
							}
							
							arrowLeftList.Remove(left);
							StartParticleLeft(timeToPrec(prec));
						}
						if(down != null){
							if(down.arrowType == ArrowType.NORMAL || down.arrowType == ArrowType.MINE){
								DestroyImmediate(down.goArrow);
								
							}else{
								arrowFrozen.Add(down,0f);
								down.displayFrozenBar();
								StartParticleFreezeDown(true);
								
							}
							
							arrowDownList.Remove(down);
							StartParticleDown(timeToPrec(prec));
						}
						if(up != null){
							if(up.arrowType == ArrowType.NORMAL || up.arrowType == ArrowType.MINE){
								DestroyImmediate(up.goArrow);
								
							}else{
								arrowFrozen.Add(up,0f);
								up.displayFrozenBar();
								StartParticleFreezeUp(true);
								
							}
							arrowUpList.Remove(up);
							StartParticleUp(timeToPrec(prec));
						}
						if(right != null){
							if(right.arrowType == ArrowType.NORMAL || right.arrowType == ArrowType.MINE){
								DestroyImmediate(right.goArrow);
								
							}else{
								arrowFrozen.Add(right,0f);
								right.displayFrozenBar();
								StartParticleFreezeRight(true);
								
							}
							arrowRightList.Remove(right);
							StartParticleRight(timeToPrec(prec));
						}
						combo += ar.neighboors.Count;
						GainScoreAndLife(timeToPrec(prec).ToString());
						displayPrec(prec);
					}
				}else{
					if(ar.arrowType == ArrowType.NORMAL || ar.arrowType == ArrowType.MINE){
						DestroyImmediate(ar.goArrow);
						
					}else{
						ar.goArrow.GetComponent<ArrowScript>().valid = true;
						arrowFrozen.Add(ar,0f);
						ar.displayFrozenBar();
						StartParticleFreezeDown(true);
						
					}
					combo++;
					arrowDownList.Remove(ar);
					StartParticleDown(timeToPrec(prec));
					GainScoreAndLife(timeToPrec(prec).ToString());
					displayPrec(prec);
				}
				
			}else if(prec < precToTime(Precision.WAYOFF)){ //miss
					if(ar.imJump){
					
						ar.alreadyValid = true;
						ar.goArrow.GetComponent<ArrowScript>().missed = true;
						if(!ar.neighboors.Any(c => c.alreadyValid == false)){
							var left = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 0);
							var down = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 2);
							var up = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 4);
							var right = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 6);
							if(left != null){
								
								arrowLeftList.Remove(left);
								StartParticleLeft(timeToPrec(prec));
							}
							if(down != null){
								
								
								arrowDownList.Remove(down);
								StartParticleDown(timeToPrec(prec));
							}
							if(up != null){
								
								arrowUpList.Remove(up);
								StartParticleUp(timeToPrec(prec));
							}
							if(right != null){
								
								arrowRightList.Remove(right);
								StartParticleRight(timeToPrec(prec));
							}
							GainScoreAndLife(timeToPrec(prec).ToString());
							displayPrec(prec);
						}
					}else{
						ar.goArrow.GetComponent<ArrowScript>().missed = true;
						arrowDownList.Remove(ar);
						StartParticleDown(timeToPrec(prec));
						displayPrec(prec);
						GainScoreAndLife(timeToPrec(prec).ToString());
					}
					ComboStop();
			}
		
			if(arrowFrozen.Any(c => c.Key.goArrow.transform.position.x == 2f && c.Key.arrowType == ArrowType.ROLL))
			{
				var froz = arrowFrozen.First(c => c.Key.goArrow.transform.position.x == 2f);
				arrowFrozen[froz.Key] = 0f;
			}
		}
		
		
		if(Input.GetKeyDown(KeyCode.UpArrow) && arrowUpList.Any()){
			var ar = findNextUpArrow();
			double prec = Mathf.Abs((float)(ar.time - (timetotalchart - Time.deltaTime)));
			
			if(prec < precToTime(Precision.GREAT)){ //great
				
				
				if(ar.imJump){
					ar.alreadyValid = true;
					ar.goArrow.GetComponent<ArrowScript>().valid = true;
					
					if(!ar.neighboors.Any(c => c.alreadyValid == false)){
						var left = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 0);
						var down = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 2);
						var up = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 4);
						var right = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 6);
						if(left != null){
							if(left.arrowType == ArrowType.NORMAL || left.arrowType == ArrowType.MINE){
								DestroyImmediate(left.goArrow);
								
							}else{
								arrowFrozen.Add(left,0f);
								left.displayFrozenBar();
								StartParticleFreezeLeft(true);
								
							}
							
							arrowLeftList.Remove(left);
							StartParticleLeft(timeToPrec(prec));
						}
						if(down != null){
							if(down.arrowType == ArrowType.NORMAL || down.arrowType == ArrowType.MINE){
								DestroyImmediate(down.goArrow);
								
							}else{
								arrowFrozen.Add(down,0f);
								down.displayFrozenBar();
								StartParticleFreezeDown(true);
								
							}
							
							arrowDownList.Remove(down);
							StartParticleDown(timeToPrec(prec));
						}
						if(up != null){
							if(up.arrowType == ArrowType.NORMAL || up.arrowType == ArrowType.MINE){
								DestroyImmediate(up.goArrow);
								
							}else{
								arrowFrozen.Add(up,0f);
								up.displayFrozenBar();
								StartParticleFreezeUp(true);
								
							}
							arrowUpList.Remove(up);
							StartParticleUp(timeToPrec(prec));
						}
						if(right != null){
							if(right.arrowType == ArrowType.NORMAL || right.arrowType == ArrowType.MINE){
								DestroyImmediate(right.goArrow);
								
							}else{
								arrowFrozen.Add(right,0f);
								right.displayFrozenBar();
								StartParticleFreezeRight(true);
								
							}
							arrowRightList.Remove(right);
							StartParticleRight(timeToPrec(prec));
						}
						combo += ar.neighboors.Count;
						GainScoreAndLife(timeToPrec(prec).ToString());
						displayPrec(prec);
					}
				}else{
					if(ar.arrowType == ArrowType.NORMAL || ar.arrowType == ArrowType.MINE){
						DestroyImmediate(ar.goArrow);
						
					}else{
						ar.goArrow.GetComponent<ArrowScript>().valid = true;
						arrowFrozen.Add(ar,0f);
						ar.displayFrozenBar();
						StartParticleFreezeUp(true);
						
					}
					combo++;
					arrowUpList.Remove(ar);
					StartParticleUp(timeToPrec(prec));
					GainScoreAndLife(timeToPrec(prec).ToString());
					displayPrec(prec);
				}
				
			}else if(prec < precToTime(Precision.WAYOFF)){ //miss
					if(ar.imJump){
					
						ar.alreadyValid = true;
						ar.goArrow.GetComponent<ArrowScript>().missed = true;
						if(!ar.neighboors.Any(c => c.alreadyValid == false)){
							var left = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 0);
							var down = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 2);
							var up = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 4);
							var right = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 6);
							if(left != null){
								
								arrowLeftList.Remove(left);
								StartParticleLeft(timeToPrec(prec));
							}
							if(down != null){
								
								
								arrowDownList.Remove(down);
								StartParticleDown(timeToPrec(prec));
							}
							if(up != null){
								
								arrowUpList.Remove(up);
								StartParticleUp(timeToPrec(prec));
							}
							if(right != null){
								
								arrowRightList.Remove(right);
								StartParticleRight(timeToPrec(prec));
							}
							GainScoreAndLife(timeToPrec(prec).ToString());
							displayPrec(prec);
						}
					}else{
						ar.goArrow.GetComponent<ArrowScript>().missed = true;
						arrowUpList.Remove(ar);
						StartParticleUp(timeToPrec(prec));
						displayPrec(prec);
						GainScoreAndLife(timeToPrec(prec).ToString());
					}
					ComboStop();
			}
			
			if(arrowFrozen.Any(c => c.Key.goArrow.transform.position.x == 4f && c.Key.arrowType == ArrowType.ROLL))
			{
				var froz = arrowFrozen.First(c => c.Key.goArrow.transform.position.x == 4f);
				arrowFrozen[froz.Key] = 0f;
			}
		
		}
		
		
		if(Input.GetKeyDown(KeyCode.RightArrow) && arrowRightList.Any()){
			var ar = findNextRightArrow();
			double prec = Mathf.Abs((float)(ar.time - (timetotalchart - Time.deltaTime)));
			
			if(prec < precToTime(Precision.GREAT)){ //great
				
				
				if(ar.imJump){
					ar.alreadyValid = true;
					ar.goArrow.GetComponent<ArrowScript>().valid = true;
					
					if(!ar.neighboors.Any(c => c.alreadyValid == false)){
						var left = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 0);
						var down = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 2);
						var up = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 4);
						var right = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 6);
						if(left != null){
							if(left.arrowType == ArrowType.NORMAL || left.arrowType == ArrowType.MINE){
								DestroyImmediate(left.goArrow);
								
							}else{
								arrowFrozen.Add(left,0f);
								left.displayFrozenBar();
								StartParticleFreezeLeft(true);
								
							}
							
							arrowLeftList.Remove(left);
							StartParticleLeft(timeToPrec(prec));
						}
						if(down != null){
							if(down.arrowType == ArrowType.NORMAL || down.arrowType == ArrowType.MINE){
								DestroyImmediate(down.goArrow);
								
							}else{
								arrowFrozen.Add(down,0f);
								down.displayFrozenBar();
								StartParticleFreezeDown(true);
								
							}
							
							arrowDownList.Remove(down);
							StartParticleDown(timeToPrec(prec));
						}
						if(up != null){
							if(up.arrowType == ArrowType.NORMAL || up.arrowType == ArrowType.MINE){
								DestroyImmediate(up.goArrow);
								
							}else{
								arrowFrozen.Add(up,0f);
								up.displayFrozenBar();
								StartParticleFreezeUp(true);
								
							}
							arrowUpList.Remove(up);
							StartParticleUp(timeToPrec(prec));
						}
						if(right != null){
							if(right.arrowType == ArrowType.NORMAL || right.arrowType == ArrowType.MINE){
								DestroyImmediate(right.goArrow);
								
							}else{
								arrowFrozen.Add(right,0f);
								right.displayFrozenBar();
								StartParticleFreezeRight(true);
								
							}
							arrowRightList.Remove(right);
							StartParticleRight(timeToPrec(prec));
						}
						combo += ar.neighboors.Count;
						GainScoreAndLife(timeToPrec(prec).ToString());
						displayPrec(prec);
					}
				}else{
					if(ar.arrowType == ArrowType.NORMAL || ar.arrowType == ArrowType.MINE){
						DestroyImmediate(ar.goArrow);
						
					}else{
						ar.goArrow.GetComponent<ArrowScript>().valid = true;
						arrowFrozen.Add(ar,0f);
						ar.displayFrozenBar();
						StartParticleFreezeRight(true);
						
					}
					combo++;
					arrowRightList.Remove(ar);
					StartParticleRight(timeToPrec(prec));
					GainScoreAndLife(timeToPrec(prec).ToString());
					displayPrec(prec);
				}
				
			}else if(prec < precToTime(Precision.WAYOFF)){ //miss
					if(ar.imJump){
					
						ar.alreadyValid = true;
						ar.goArrow.GetComponent<ArrowScript>().missed = true;
						if(!ar.neighboors.Any(c => c.alreadyValid == false)){
							var left = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 0);
							var down = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 2);
							var up = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 4);
							var right = ar.neighboors.FirstOrDefault(c => c.goArrow.transform.position.x == 6);
							if(left != null){
								
								arrowLeftList.Remove(left);
								StartParticleLeft(timeToPrec(prec));
							}
							if(down != null){
								
								
								arrowDownList.Remove(down);
								StartParticleDown(timeToPrec(prec));
							}
							if(up != null){
								
								arrowUpList.Remove(up);
								StartParticleUp(timeToPrec(prec));
							}
							if(right != null){
								
								arrowRightList.Remove(right);
								StartParticleRight(timeToPrec(prec));
							}
							GainScoreAndLife(timeToPrec(prec).ToString());
							displayPrec(prec);
						}
					}else{
						ar.goArrow.GetComponent<ArrowScript>().missed = true;
						arrowRightList.Remove(ar);
						StartParticleRight(timeToPrec(prec));
						displayPrec(prec);
						GainScoreAndLife(timeToPrec(prec).ToString());
					}
					ComboStop();
			}
			
			if(arrowFrozen.Any(c => c.Key.goArrow.transform.position.x == 6f && c.Key.arrowType == ArrowType.ROLL))
			{
				var froz = arrowFrozen.First(c => c.Key.goArrow.transform.position.x == 6f);
				arrowFrozen[froz.Key] = 0f;
			}
		
		}
		
	}
	
	
	//Verify Key for all frames
	void VerifyKeysOutput(){
		if(arrowFrozen.Count > 0){
			if(Input.GetKey(KeyCode.LeftArrow)){
				if(arrowFrozen.Any(c => c.Key.goArrow.transform.position.x == 0f && c.Key.arrowType == ArrowType.FREEZE)){
					var ar = arrowFrozen.First(c => c.Key.goArrow.transform.position.x == 0f);
					arrowFrozen[ar.Key] = 0f;
				}
			
			}
			
			
			if(Input.GetKey(KeyCode.DownArrow)){
				if(arrowFrozen.Any(c => c.Key.goArrow.transform.position.x == 2f && c.Key.arrowType == ArrowType.FREEZE)){
					var ar = arrowFrozen.First(c => c.Key.goArrow.transform.position.x == 2f);
					arrowFrozen[ar.Key] = 0f;
				}
			
			}
			
			
			if(Input.GetKey(KeyCode.UpArrow)){
				if(arrowFrozen.Any(c => c.Key.goArrow.transform.position.x == 4f && c.Key.arrowType == ArrowType.FREEZE)){
					var ar = arrowFrozen.First(c => c.Key.goArrow.transform.position.x == 4f);
					arrowFrozen[ar.Key] = 0f;
				}
			}
			
			
			if(Input.GetKey(KeyCode.RightArrow)){
				if(arrowFrozen.Any(c => c.Key.goArrow.transform.position.x == 6f && c.Key.arrowType == ArrowType.FREEZE)){
					var ar = arrowFrozen.First(c => c.Key.goArrow.transform.position.x == 6f);
					arrowFrozen[ar.Key] = 0f;
				}
			}
		}
	}
	
	#endregion
	
	#region Particules	
	void StartParticleLeft(Precision prec){
		var displayPrec = prec.ToString();
		if(prec < Precision.DECENT && combo >= 100) displayPrec += "C";
		((ParticleSystem) precLeft.transform.FindChild(displayPrec).particleSystem).Play();
	}
	
	void StartParticleDown(Precision prec){
		var displayPrec = prec.ToString();
		if(prec < Precision.DECENT && combo >= 100) displayPrec += "C";
		((ParticleSystem) precDown.transform.FindChild(displayPrec).particleSystem).Play();
	}
	
	void StartParticleUp(Precision prec){
		var displayPrec = prec.ToString();
		if(prec < Precision.DECENT && combo >= 100) displayPrec += "C";
		((ParticleSystem) precUp.transform.FindChild(displayPrec).particleSystem).Play();
	}
	
	void StartParticleRight(Precision prec){
		var displayPrec = prec.ToString();
		if(prec < Precision.DECENT && combo >= 100) displayPrec += "C";
		((ParticleSystem) precRight.transform.FindChild(displayPrec).particleSystem).Play();
	}
	
	void StartParticleFreezeLeft(bool state){
		var ps = ((ParticleSystem) precLeft.transform.FindChild("FREEZE").particleSystem);
		ps.Play();
		ps.time = 1f;
		ps.loop = state;
		
	}
	
	void StartParticleFreezeRight(bool state){
		var ps = ((ParticleSystem) precRight.transform.FindChild("FREEZE").particleSystem);
		ps.Play();
		ps.time = 1f;
		ps.loop = state;
		
	}
	
	void StartParticleFreezeDown(bool state){
		var ps = ((ParticleSystem) precDown.transform.FindChild("FREEZE").particleSystem);
		ps.Play();
		ps.time = 1f;
		ps.loop = state;
		
	}
	
	void StartParticleFreezeUp(bool state){
		var ps = ((ParticleSystem) precUp.transform.FindChild("FREEZE").particleSystem);
		ps.Play();
		ps.time = 1f;
		ps.loop = state;
		
	}
	
	#endregion
	
	#region util
	public void GainScoreAndLife(string s){
		life += lifeBase[s];
		if(life > 100f){
			life = 100f;	
		}else if(life < 0f){
			life = 0f;	
		}
		theLifeBar.ChangeBar(life);
		score += scoreBase[s];
		if(score > 100f){
			score = 100f;	
		}else if(score < 0f){
			score = 0f;	
		}
	}
	
	Rect scoreDecoupe(float dascore){
		var thescorestring = dascore.ToString("00.00");
		var thescorestringentier = thescorestring.Split('.')[0];
		var thescorestringdecim = thescorestring.Split('.')[1];
		return new Rect(roolInt("0." + thescorestringentier[0]), roolInt("0." + thescorestringentier[1]), roolInt("0." + thescorestringdecim[0]), roolInt("0." + thescorestringdecim[1]));
	}
	
	float roolInt(string i){
		var ic = (float)Convert.ToDouble(i);
		if(ic == 0.9f){
			return 0f;	
		}else{
			return ic+0.1f;		
		}
	}
	
	public void ComboStop(){
		combo = 0;	
	}
	#endregion
	
	#region Precision
	double precToTime(Precision prec){
		switch(prec){
		case Precision.FANTASTIC:
			return 0.0215;
		case Precision.EXCELLENT:
			return 0.043;
		case Precision.GREAT:
			return 0.102;
		case Precision.DECENT:
			return 0.135;
		case Precision.WAYOFF:
			return 0.180;
		default:
			return 0;
		}
	}
	
	
	public void displayPrec(double prec){
		timeDisplayScore = 0f;
		zoom = baseZoom;
		scoreToDisplay = timeToPrec(prec);
	}
	
	Precision timeToPrec(double prec){
		if(prec <= 0.0215){
			return Precision.FANTASTIC;
		}else if(prec <= 0.043){
			return Precision.EXCELLENT;
		}else if(prec <= 0.102){
			return Precision.GREAT;
		}else if(prec <= 0.135){
			return Precision.DECENT;
		}else if(prec <= 0.180){
			return Precision.WAYOFF;
		}
		return Precision.MISS;
		
	}
	
	
	#endregion
	
	#region Arrow and time
	public Arrow findNextUpArrow(){

		return arrowUpList.FirstOrDefault(s => s.time == arrowUpList.Min(c => c.time));
			
	}
	
	public Arrow findNextDownArrow(){

		return arrowDownList.FirstOrDefault(s => s.time == arrowDownList.Min(c => c.time));
			
	}
	
	public Arrow findNextLeftArrow(){

		return arrowLeftList.FirstOrDefault(s => s.time == arrowLeftList.Min(c => c.time));
			
	}
	
	public Arrow findNextRightArrow(){

		return arrowRightList.FirstOrDefault(s => s.time == arrowRightList.Min(c => c.time));
			
	}
	
	//Remove key from arrow list
	public void removeArrowFromList(Arrow ar, string state){
		
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
	
	
	public double getTotalTimeChart(){
		return timetotalchart;	
	}
	
	#endregion
	
	#region Chart creation
	//Create the chart
	void createTheChart(Song s){
		
		var ypos = 0f;
		arrowLeftList = new List<Arrow>();
		arrowUpList = new List<Arrow>();
		arrowDownList = new List<Arrow>();
		arrowRightList = new List<Arrow>();
		
		
		var theBPMCounter = 1;
		var theSTOPCounter = 0;
		double mesurecount = 0;
		double prevmesure = 0;
		double timecounter = 0;
		double timeBPM = 0;
		double timestop = 0;
		double timetotal = 0;
		float prec = 0.001f;
		
		var ArrowFreezed = new Arrow[4];
		Bumps = new List<double>();
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
				if((beat)%(mesure.Count/4) == 0) Bumps.Add(timetotal);
				
				char[] note = mesure.ElementAt(beat).Trim().ToCharArray();
				
				var listNeighboors = new List<Arrow>();
				var barr = false;
				for(int i =0;i<4; i++){
					if(note[i] == '1'){
						//var theArrow = (GameObject) Instantiate(arrow, new Vector3(i*2, -ypos  + (float)s.offset, 0f), arrow.transform.rotation);
						barr = true;
						var goArrow = (GameObject) Instantiate(arrow, new Vector3(i*2, -ypos, 0f), arrow.transform.rotation);
						goArrow.renderer.material.color = chooseColor(beat + 1, mesure.Count);
						var theArrow = new Arrow(goArrow, ArrowType.NORMAL, timetotal);
						
						switch(i){
						case 0:
							arrowLeftList.Add(theArrow);
							break;
						case 1:
							arrowDownList.Add(theArrow);
							break;
						case 2:
							arrowUpList.Add(theArrow);
							break;
						case 3:
							arrowRightList.Add(theArrow);
							break;
						}
						listNeighboors.Add(theArrow);
						//barrow = true;
					}else if(note[i] == '2'){
						barr = true;
						var goArrow = (GameObject) Instantiate(arrow, new Vector3(i*2, -ypos, 0f), arrow.transform.rotation);
						goArrow.renderer.material.color = chooseColor(beat + 1, mesure.Count);
						var theArrow = new Arrow(goArrow, ArrowType.FREEZE, timetotal);
						ArrowFreezed[i] = theArrow;
						switch(i){
						case 0:
							arrowLeftList.Add(theArrow);
							break;
						case 1:
							arrowDownList.Add(theArrow);
							break;
						case 2:
							arrowUpList.Add(theArrow);
							break;
						case 3:
							arrowRightList.Add(theArrow);
							break;
						}
						listNeighboors.Add(theArrow);
					}else if(note[i] == '3'){
						barr = true;
						var theArrow = ArrowFreezed[i];
						var goFreeze = (GameObject) Instantiate(freeze, new Vector3(i*2, (theArrow.goArrow.transform.position.y + ((-ypos - theArrow.goArrow.transform.position.y)/2f)) , 0.5f), freeze.transform.rotation);
						goFreeze.transform.localScale = new Vector3(1f, -((-ypos - theArrow.goArrow.transform.position.y)/2f), 0.1f);
						goFreeze.transform.GetChild(0).renderer.material.color = theArrow.goArrow.renderer.material.color;
						theArrow.setArrowFreeze(timetotal, new Vector3(i*2,-ypos, 0f), goFreeze, null);
					
					}else if(note[i] == '4'){
						barr = true;
						var goArrow = (GameObject) Instantiate(arrow, new Vector3(i*2, -ypos, 0f), arrow.transform.rotation);
						goArrow.renderer.material.color = chooseColor(beat + 1, mesure.Count);
						var theArrow = new Arrow(goArrow, ArrowType.ROLL, timetotal);
						ArrowFreezed[i] = theArrow;
						switch(i){
						case 0:
							arrowLeftList.Add(theArrow);
							break;
						case 1:
							arrowDownList.Add(theArrow);
							break;
						case 2:
							arrowUpList.Add(theArrow);
							break;
						case 3:
							arrowRightList.Add(theArrow);
							break;
						}
						listNeighboors.Add(theArrow);
					}else if(note[i] == 'M'){
						/*var goArrow = (GameObject) Instantiate(arrow, new Vector3(i*2, -ypos, 0f), arrow.transform.rotation);
						goArrow.renderer.material.color = chooseColor(beat + 1, mesure.Count);
						var theArrow = new Arrow(goArrow, ArrowType.MINE, timetotal);
						switch(i){
						case 0:
							arrowLeftList.Add(theArrow);
							break;
						case 1:
							arrowDownList.Add(theArrow);
							break;
						case 2:
							arrowUpList.Add(theArrow);
							break;
						case 3:
							arrowRightList.Add(theArrow);
							break;
						}*/
						
					
					}
					
				}
				
				if(barr){
					if(firstArrow <= 0f) firstArrow = timetotal;
					lastArrow = timetotal;	
				}
				
				if(listNeighboors.Count > 1){
					foreach(var el in listNeighboors){
						el.neighboors = listNeighboors;
						el.imJump = true;
					}
				}
				
				
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
	
	
	
	Color chooseColor(int posmesure, int mesuretot){
		
		
		switch(mesuretot){
		case 4:	
			return new Color(1f, 0f, 0f, 1f); //
		
			
		case 8:
			if(posmesure%2 == 0){
				return new Color(0f, 0f, 1f, 1f);
			}
			return new Color(1f, 0f, 0f, 1f); //
			
			
		case 12:
			if((posmesure-1)%3 == 0){
				return new Color(1f, 0f, 0f, 1f); //
			}
			return new Color(1f, 0f, 1f, 1f);
			
		case 16:
			if((posmesure-1)%4 == 0){
				return new Color(1f, 0f, 0f, 1f); //
			}else if((posmesure+1)%4 == 0){
				return new Color(0f, 0f, 1f, 1f);
			}
			return new Color(0f, 1f, 0f, 1f);
		
			
		case 24:
			if((posmesure+2)%6 == 0){
				return new Color(0f, 0f, 1f, 1f);
			}else if((posmesure-1)%6 == 0){
				return new Color(1f, 0f, 0f, 1f); //
			}
			return new Color(1f, 0f, 1f, 1f);
			
			
		case 32:
			if((posmesure-1)%8 == 0){
				return new Color(1f, 0f, 0f, 1f); //
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
				return new Color(1f, 0f, 0f, 1f); //
			}else if((posmesure+5)%12 == 0){
				return new Color(0f, 0f, 1f, 1f);
			}
			return new Color(1f, 0f, 1f, 1f);
		
			
		case 64:
			if((posmesure-1)%16 == 0){
				return new Color(1f, 0f, 0f, 1f); //
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
				return new Color(1f, 0f, 0f, 1f); //
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
	
	#endregion
}
