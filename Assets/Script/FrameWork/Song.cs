using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Song {

	//global information
	public string title;
	public string subtitle;
	public string artist;
	public string banner;
	public string bpmToDisplay;
	public Texture2D background;
	public double offset;
	public double samplestart;
	public double samplelenght;
	public List<double> mesureBPMS;
	public List<double> mesureSTOPS;
	public Dictionary<double, double> bpms;
	public Dictionary<double, double> stops;
	public string stepartist;
	public Difficulty difficulty;
	public int level;
	public int numberOfSteps;
	public int numberOfStepsWithoutJumps;
	public int numberOfFreezes;
	public int numberOfRolls;
	public int numberOfMines;
	public int numberOfJumps;
	public int numberOfHands;
	public double duration;
	public float[] intensityGraph;
	
	public double stepPerSecondAverage;
	public double stepPerSecondMaximum;
	public double stepPerSecondStream;
	public double longestStream;
	public int numberOfCross;
	public int numberOfFootswitch;
	public string song;
	
	public SongInfoProfil sip;
	
	//stepchart
	public List<List<string>> stepchart;
	
	
	public double distanceRed;
	
	public Song(){
		bpms = new Dictionary<double, double>();
		stops = new Dictionary<double, double>();
		stepchart = new List<List<string>>();
		
	}
	
	public AudioClip GetAudioClip(){
		var thewww = new WWW(song);
		while(!thewww.isDone){ }
		var ac = thewww.GetAudioClip(false, true,AudioType.OGGVORBIS);
		thewww.Dispose();
		return ac;
	}
	
	public string GetAudioClipUnstreamed(){
		return song;
	}
	
	/*
	public AudioClip SetAudioClip(AudioClip ac){
		var thewww = new WWW(song);
		while(!thewww.isDone){ }
		return thewww.GetAudioClip(false, true);
	}
	*/
	
	public Texture2D GetBanner(Texture2D tex){
		if(banner != "noBanner"){
			WWW www = new WWW(banner);
			while(!www.isDone){}
			
    		www.LoadImageIntoTexture(tex);
			
			www.Dispose();
			return tex;
			
		}
		return null;
	}
	
	public Texture2D GetBanner(){
		WWW www = new WWW(banner);
		while(!www.isDone){}
		var tex = new Texture2D(256,256);
		www.LoadImageIntoTexture(tex);
		
		www.Dispose();
		return tex;
	}
	
	public double getBPS(double bpmValue){
		double bps = bpmValue/(double)60.0;
		return bps;
	}
	
	public void setDifficulty(string dif){
		switch(dif){
		case "Challenge":
			difficulty = Difficulty.EXPERT;
			break;
		case "Hard":
			difficulty = Difficulty.HARD;
			break;
		case "Medium":
			difficulty = Difficulty.MEDIUM;
			break;
		case "Easy":
			difficulty = Difficulty.EASY;
			break;
		case "Beginner":
			difficulty = Difficulty.BEGINNER;
			break;
		case "Edit":
			difficulty = Difficulty.EDIT;
			break;
		case "DChallenge":
			difficulty = Difficulty.DEXPERT;
			break;
		case "DHard":
			difficulty = Difficulty.DHARD;
			break;
		case "DMedium":
			difficulty = Difficulty.DMEDIUM;
			break;
		case "DEasy":
			difficulty = Difficulty.DEASY;
			break;
		case "DBeginner":
			difficulty = Difficulty.DBEGINNER;
			break;
		case "DEdit":
			difficulty = Difficulty.DEDIT;
			break;
		default:
			difficulty = Difficulty.EDIT;
			break;
		}
	}
	
	
	
	
}