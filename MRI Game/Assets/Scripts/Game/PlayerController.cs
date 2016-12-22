using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Shared;
using System.IO;
using System;
using System.Net.Mail;

/* PlayerController
 * This class is a script attached to the Player game object in the ScanView scene.
 * It controls the behavior of the Player game object and acts as a controller for the entire ScanView scene.
 */
public class PlayerController : MonoBehaviour {

	public Text scoreText;  // Scoreboard text
	public Text powerUpValue;  // Value of currently contacted power-up
	public Text levelText;  // Text box that indicates a new level
	public GameObject[] avatars;  // Array of available avatars
	public GameObject[] backgrounds;  // Array of different level backgrounds

	//private string ScoreFilePath = "Assets/Scripts/Game/high_score" + GameSetup.AlgorithmID.ToString() + ".txt";
	private string ScoreFilePath = "high_score" + GameSetup.AlgorithmID.ToString() + ".txt";
	private bool isTutorial;  // True if game is in tutorial mode
	private int score;  // Holds score
	private float startTime;  // Used to keep track of how long the tutorial has been running
	private float scoreTime;  // Used to keep track of how long the powerUpValue has been displaying
	private float levelTime;  // Used to keep track of how long the levelText has been displaying
	private Input input;  // input from Input Module
	private double m;  // Holds data used in normalization calculation
	private double b;  // Holds data used in normalization calculation
	private Log log;  // Reference to Log object the game should log events to 
	private Calibration calibration;  // Calibration object which holds the calibration data the game uses
	private int pointValue;  // The current point value for a power-up
	private int highScore;  // Current high score
	private int lastPowerUp;  // Number of last power up

	/* Start is used for initialization */
	void Start () 
	{
		SetAvatar ();
		score = 0;
		GameSetup.Level = 1;
		powerUpValue.text = "";  // Blank until contact a power-up
		levelText.text = "Level " + GameSetup.Level.ToString();
		startTime = Time.realtimeSinceStartup;  // Record start time
		levelTime = Time.realtimeSinceStartup;  // Used to remove level up text
		input = GameSetup.Input;
		calibration = GameSetup.CalibrationController.Calibration;
		SetNormalization (calibration.Max, calibration.Min);  // Map bellows positions to player positions in game
		isTutorial = !(GameSetup.GameMode);  // GameMode is false if in tutorial mode
		if (!isTutorial)
		{
			log = Log.GetInstance ();
			log.Start ();
		}
		pointValue = 10;
		SetHighScore ();
		SetScoreText ();  // Set initial score text to 0, needs to be done after SetHighScore()
		lastPowerUp = -1;  // First power-up spawned will have a number of 1, so will not count as consecutive

        //Clear buffer from menu
        GameSetup.Input.GetInput();

	}

	/* FixedUpdate is used for physics related updates; called once per frame
	 */
	void FixedUpdate () 
	{

	}

	/* OnTriggerEnter2D is called whenever player hits a trigger collider
	 * Used to detect collisions between the player and power-ups
	 * 
	 * other  The Collider2D component attached to the object the player collided with
	 */
	void OnTriggerEnter2D(Collider2D other)
	{
		// Only want to register collisions with power-ups
		if (other.gameObject.CompareTag ("PowerUp"))
		{
			int value = PowerUpValue (other.gameObject);
			Destroy (other.gameObject);
			score += value;
			SetScoreText ();  // Update scoreboard
			powerUpValue.text = "+" + value.ToString();
			scoreTime = Time.realtimeSinceStartup;  // Record when power-up value starts displaying
			if (!isTutorial)  // Log event if not in tutorial mode
			{
				string eventDescription = "Power-Up|Value:" +value.ToString() + "|Score:" + score.ToString();
				GameEvent gameEvent = new GameEvent (eventDescription);
				log.LogEvent (gameEvent); //to record power up
			}

			// New Level
			if ((score > 100) && (GameSetup.Level == 1)) {
				LevelUp ();
			}
		}
	}

	/* SetScoreText updates the scoreboard */
	void SetScoreText()
	{
		scoreText.text = "Score: " + score.ToString () + "\nHigh Score: " + highScore.ToString();
	}

	/* Update is used for non-physics related updates; called once per frame
	 * Used to check if in tutorial mode, and if so, timeout after 1 minute.
	 */
	void Update()
	{
		// GetInput to get most recent MRI data
		int position = input.GetInput();  // Remove true when using with server

		// Make sure the position is not out of the bounds set by the calibration
		if (position > calibration.Max)
			position = calibration.Max;
		if (position < calibration.Min)
			position = calibration.Min;

		// Use GetVertical to translate the bellows position to the player's vertical position on the screen
		float vertical = GetVertical(position);
		// Player does not move in X direction
		float horizontal = this.gameObject.transform.position.x;
		// Set player's new position
		this.gameObject.transform.position = new Vector3 (horizontal, vertical);

		// Remove power up value after half a second
		if (Time.realtimeSinceStartup >= (scoreTime + 0.5))
		{
			powerUpValue.text = "";
		}

		// Clear level up text after 1 second
		if (Time.realtimeSinceStartup >= (levelTime + 1))
		{
			levelText.text = "";
		}

		// If tutorial mode, time out after one minute
		if (isTutorial && (Time.realtimeSinceStartup >= (startTime + 60)))
		{
			CancelInvoke ();  // Calls same code used by Stop button
		}
	}

	/* CancelInvoke is here used to stop the game */
	void CancelInvoke()
	{
		if (isTutorial)
		{
			SceneManager.LoadScene ("MainMenuScene");
		} else {
			// Log final score and then update log
			string description = "Final Score:" + score.ToString ();
			log.LogEvent (new GameEvent (description));

			// Check if user recored high score, if so, update
			GameSetup.Score = description;
			if (score > highScore)
			{
				GameSetup.Score += "\nNew High Score!";
				try
				{
					using (var writer = new BinaryWriter (File.Open(ScoreFilePath, FileMode.OpenOrCreate)))
					{
						writer.Write (score.ToString());
						writer.Close();
					}
				}
				catch (Exception e)
				{
					Debug.Log("Problem saving high score" + e.Message);
				}

				// Log that player got high score
				string eventDescription = "High Score!";
				GameEvent gameEvent = new GameEvent (eventDescription);
				log.LogEvent (gameEvent);
				log.Stop ();  // Log started in Start(), so need to stop
			}

			// Send email with logs
			// SendEmail();  // Uncomment before releasing

			// Go to Game Over screen
			GameSetup.CurrentState = MainMenu.State.FirstLoad;  // Resets Main Menu
			SceneManager.LoadScene ("GameOver");
		}
	}
		
	/* SetNormalization normalizes Player position based on calibration data
	 * 
	 * max  The maximum calibration value
	 * min  The minimum calibration value
	 */
	private void SetNormalization(int max, int min)
	{
		// This conversion is based on the equation for a line
		// y = (m * x) + b
		// y is the player's position
		// m is the slope of the line
		// x is the bellows postion
		// b is the intercept
		// max gets mapped to the highest player position (2.65767)
		// min gets mapped to the lowest player position (-3)
		// All other positions fall somewhere on the line between the two
		m = 5.65767d / ( (double) max - (double) min);
		b = 2.65767d - (m * (double)max);
	}

	/* GetVertical calculates the player's vertical position in the game based on the
	 * given bellows position
	 * 
	 * bellowsPosition  The bellows position
	 * 
	 * return The player's position in the game based on the given bellow position
	 */
	private float GetVertical(int bellowsPosition)
	{
		return (float)(m * (double) bellowsPosition + b);  // Uses line equation set up in SetNormalization()
	}

	/* PowerUpValue calculates the value of the contacted power-up
	 * This needs to be done in the PlayerController, as it is not a property of the power-up,
	 * but rather whether the player contacts consecutive power-ups
	 * 
	 * return The power-up's value
	 */
	private int PowerUpValue(GameObject powerUp)
	{
		if (powerUp.GetComponent<PowerUpController>().PowerUpNumber == (lastPowerUp + 1))
		{
			pointValue += 5;
		} else {
			pointValue = 10;
		}

		lastPowerUp = powerUp.GetComponent<PowerUpController>().PowerUpNumber;

		return pointValue;
	}

	/* GetHighScore retreives the current high score from file and sets highScore
	 */
	private void SetHighScore()
	{
		if (!File.Exists (ScoreFilePath))
		{
			// File will get created if it does not exist if a high score is obtained
			highScore = 100;
		} else {
			try
			{
				// Read in high score
				using (BinaryReader reader = new BinaryReader(new FileStream(ScoreFilePath,FileMode.Open)))
				{
					highScore = Int32.Parse(reader.ReadString());
				}
			}
			catch (Exception e) 
			{
				Debug.Log("Problem reading high_score.txt " + e.Message);
			}
		}
	}

	/* SendEmail sends an email with the log files to the researcher upon the completion
	 * of the scan
	 */
	private void SendEmail()
	{
		Output output;

		output = new Output (new MailMessage (), SmtpMessageSender.FromSystem ());
		output.SendTo (PlayerPrefs.GetString("techEmail"));

		output.Dispose ();
	}

	/* SetAvatar sets the player avatar to the one selected */
	private void SetAvatar()
	{
		for (int i = 0; i < avatars.Length; i++)
		{
			if (i == GameSetup.AvatarType)
				avatars [i].SetActive (true);
			else
				avatars [i].SetActive (false);
		}
	}

	/* LevelUp changes the game to the next level */
	private void LevelUp()
	{
		// backgrounds index starts at 0, while Levels start at 1
		// so have to subtract 1 for background of current level
		backgrounds [(GameSetup.Level - 1)].SetActive (false);
		backgrounds [GameSetup.Level].SetActive (true);

		// Increment the level
		GameSetup.Level += 1;

		// Tell user about new level
		levelText.text = "Level " + GameSetup.Level.ToString();
		levelTime = Time.realtimeSinceStartup;  // Need to set so Update can remove level up text later

		// Clean up background objects (e.g. fish and seaweed) from previous level
		BackgroundCleanUp ();

		// Log level up event
		if (!isTutorial)
		{
			string eventDescription = "Level up: " + GameSetup.Level.ToString();
			GameEvent gameEvent = new GameEvent (eventDescription);
			log.LogEvent (gameEvent); //to record level up
		}
	}

	/* BackgroundCleanUp removes the old background objects */
	private void BackgroundCleanUp()
	{
		foreach (GameObject spawn in DynamicBackgroundSpawner.spawnList)
		{
			Destroy(spawn);
		}
		DynamicBackgroundSpawner.spawnList.Clear ();
	}
}
