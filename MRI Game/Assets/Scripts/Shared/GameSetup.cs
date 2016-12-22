using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


namespace Shared{

/*
 * GameSetup stores global information to be shared amongst all modules.
 */ 
public static class GameSetup {

	public static int AlgorithmID {get;set;}
	public static bool GameMode {get;set;}
	public static int AvatarType {get;set;}

	public static string Score {get; set;}
	public static int Level {get; set;}
	public static MainMenu.State CurrentState {get;set;}
	public static CalibrationController CalibrationController {get; set;}
	public static Input Input {get; set;}
	//These are inteded to be changed as the game advances, these are just temps
	//TODO: There are better approaches to this..
	public static List<string> AlgorithmSelectList {
		get { return new List<string>{ "Base Line" , "Wave Form" }; }
	}
	public static List<string> AvatarSelectList {
		get { return new List<string>{ "Sub", "Mermaid" }; }
	}

}

}