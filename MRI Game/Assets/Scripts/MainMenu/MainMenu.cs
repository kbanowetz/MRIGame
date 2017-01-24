using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using Shared;

public class MainMenu : MonoBehaviour {

	public enum State : int {FirstLoad, Main, Calibration, PostCalibration};

	//assets - the components that are mapped to gameObjects in Scene
	public Button PreferencesButton;
	public Button CalibrationButton;
	public Button PlayTutorialButton;
	public Button PlayGameButton;
	public Button QuitButton;
	public Dropdown AvatarDropdown;
	public Dropdown GameDropdown;
	public ScrollRect ScrollView;

	private Log log;

	/*
	 * Every time MainMenu Scene loads, the dropDown lists are populated with available
	 * options from GameSetup class, and updateState() is called to enter either 
	 * A) the state is was left on or B) it enters 'FirstLoad' state which occurs 
	 * if it's the first time on this Scene from Start Up.
	*/
	void Start () {
		GameDropdown.AddOptions (GameSetup.AlgorithmSelectList);
		AvatarDropdown.AddOptions (GameSetup.AvatarSelectList);
		log = Log.GetInstance ();
		updateState ();
	}

	/*
	 * Update() is called once per frame automatically. It is used here to check whether
	 * the game is in the Calibration State, and updates the CalibrationController's Max & Min.
	 * (Performs Calibration).
	*/
	void Update() {
		if (GameSetup.CurrentState == State.Calibration && GameSetup.CalibrationController != null) {
			GameSetup.CalibrationController.Update ();
		}
	}

	/*
	 * The Four States as described by the enum 'State,' are FirstLoad, Main, Calibration, PostCalibration.
	 * Depending on which button is pressed and the state the menu is currently in will trigger 
	 * buttons to either become active or inactive and whether or not calibration is occuring.
	 */
	public void updateState(string buttonPressed = null){
		
		if (buttonPressed == "quit") {
			Application.Quit();
		}

		switch (GameSetup.CurrentState)
		{
		case State.FirstLoad:
			adjustAssetsInteractivity (false, false, false, false, true);
			GameSetup.CurrentState = State.Main;
			break;

		case State.Main:
			if (buttonPressed == "calibrate") {
				adjustAssetsInteractivity (false, false, false, false, false);
				CalibrationButton.GetComponentInChildren<Text> ().text = "stop calibration";
				log.Start ();  // Start log for calibration
				GameSetup.CalibrationController = new CalibrationController (GameSetup.Input);
				GameSetup.CurrentState = State.Calibration;
			} else if (buttonPressed == "preferences") {
				adjustAssetsInteractivity (false, false, false, false, true);
				SceneManager.LoadScene ("PreferencesScene");
			} else {
				adjustAssetsInteractivity (false, false, false, false, true);
			}
			break;

		case State.Calibration:
			if (buttonPressed == "calibrate") {
				CalibrationButton.GetComponentInChildren<Text> ().text = "calibrate";
				log.LogCalibration (GameSetup.CalibrationController.Calibration);
				log.Stop ();  // Stop log now that calibration is done
				ScrollView.content = LogCalibration;
				GameSetup.CurrentState = State.PostCalibration;
				adjustAssetsInteractivity (true, true, true, true, true);
			}
			break;

		case State.PostCalibration:
			if (buttonPressed == "playGame" || buttonPressed == "playTutorial") {
				GameSetup.GameMode = (buttonPressed == "playGame");
				GameSetup.AvatarType = AvatarDropdown.value;
				GameSetup.AlgorithmID = GameDropdown.value;
				SceneManager.LoadScene("ScanView");
			} else if (buttonPressed == "calibrate") {
				CalibrationButton.GetComponentInChildren<Text> ().text = "Stop Calibration";
				log.Start ();  // Start log for calibration
				GameSetup.CalibrationController = new CalibrationController (GameSetup.Input);
				GameSetup.CurrentState = State.Calibration;
				adjustAssetsInteractivity (false, false, false, false, false);
			} else if (buttonPressed == "preferences") {
				adjustAssetsInteractivity (true, true, true, true, true);
				SceneManager.LoadScene ("PreferencesScene");
			}
			break;

		}
	}

	private void adjustAssetsInteractivity(bool playTutorialBtn, bool playGameBtn, bool avatarDropDown, bool gameDropDown, bool preferencesBtn)
	{
		PlayTutorialButton.interactable = playTutorialBtn;
		PlayGameButton.interactable = playGameBtn;
		AvatarDropdown.interactable = avatarDropDown;
		GameDropdown.interactable = gameDropDown;
		PreferencesButton.interactable = preferencesBtn;
	}
}