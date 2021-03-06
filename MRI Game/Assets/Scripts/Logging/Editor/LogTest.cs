﻿using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[TestFixture]
public class LogUnitTests
{
	Log log;
	Random rng = new Random ();

	// Increase SLEEP_TIME for robustness, decrease for speed
	const int SLEEP_TIME = 300;
	const int DATA_AMOUNT = 1000;

	[SetUp]
	public void Initialize ()
	{
		log = Log.GetInstance ();
	}

	[Test]
	public void LogStoresBellowsData ()
	{
		log.Start ();
		int[] data = RandomInts (DATA_AMOUNT);

		log.LogBellowsData (data);
		log.Stop ();

		Assert.AreEqual (data, IntsParsedFrom (log.bellowsLogName));
	}

	[Test]
	public void LogStoresEvents ()
	{
		log.Start ();
		List<GameEvent> gameEvents = RandomEvents (DATA_AMOUNT);

		foreach (var gameEvent in gameEvents) {
			log.LogEvent (gameEvent);
		}
		log.Stop ();

		foreach (var gameEvent in gameEvents) {
			Assert.Contains (gameEvent.description, DescriptionsFrom (log.eventLogName));
		}
	}

	[Test]
	public void LogTimestampsEvents ()
	{
		log.Start ();
		GameEvent gameEvent = new GameEvent ("Test event");

		var timestamp = Timestamp ();
		log.LogEvent (gameEvent);
		log.Stop ();

		Assert.AreEqual (timestamp, TimestampsFrom (log.eventLogName).ToArray () [0]);
	}

	[Test]
	public void LogStoresCalibrations ()
	{
		log.Start ();
		List<Calibration> calibrations = RandomCalibrations (DATA_AMOUNT);

		foreach (var calibration in calibrations) {
			log.LogCalibration (calibration);
		}
		log.Stop ();

		foreach (var calibration in calibrations) {
			Assert.Contains (calibration.ToString (), DescriptionsFrom (log.calibrationLogName));
		}
	}

	[Test]
	public void LogTimestampsCalibrations ()
	{
		log.Start ();
		Calibration calibration = new Calibration ();

		var timestamp = Timestamp ();
		log.LogCalibration (calibration);
		log.Stop ();

		Assert.AreEqual (timestamp, TimestampsFrom (log.calibrationLogName).ToArray () [0]);
	}

	[Test]
	public void LogTimestampsFiles ()
	{
		string bellowsFiletag = "bellowslog_";
		string eventFileTag = "eventlog_";
		string calibrationFileTag = "calibrationlog_";

		SleepFor (SLEEP_TIME);
		
		var startTime = Timestamp ();
		// Resets the file name
		log.Start ();
		// Closes the file
		log.Stop ();

		// Verify file exists and can be opened.
		File.Open (log.bellowsLogName, FileMode.Open).Close();
		File.Open (log.calibrationLogName, FileMode.Open).Close();
		File.Open (log.eventLogName, FileMode.Open).Close();

		// Check that format is "<tag>_<timestamp>"
		Assert.AreEqual (bellowsFiletag + startTime, log.bellowsLogName);
		Assert.AreEqual (calibrationFileTag + startTime, log.calibrationLogName);
		Assert.AreEqual (eventFileTag + startTime, log.eventLogName);
	}

	[Test]
	public void StoppingIgnoresAllLogging ()
	{
		log.Start ();
		log.Stop ();

		LogManyThings ();

		Assert.IsEmpty (IntsParsedFrom (log.bellowsLogName));
		Assert.IsEmpty (DescriptionsFrom (log.eventLogName));
		Assert.IsEmpty (DescriptionsFrom (log.calibrationLogName));
	}

	[Test]
	public void StartingAfterStoppingAllowsLogging ()
	{
		log.Start ();
		log.Stop ();
		log.Start ();

		LogManyThings ();

		Assert.IsNotEmpty (IntsParsedFrom (log.bellowsLogName));
		Assert.IsNotEmpty (DescriptionsFrom (log.eventLogName));
		Assert.IsNotEmpty (DescriptionsFrom (log.calibrationLogName));
	}

	[Test]
	public void StartingAfterStoppingClearsAllLogs ()
	{
		log.Start ();
		LogManyThings ();
		log.Stop ();
		log.Start ();

		Assert.IsEmpty (IntsParsedFrom (log.bellowsLogName));
		Assert.IsEmpty (DescriptionsFrom (log.eventLogName));
		Assert.IsEmpty (DescriptionsFrom (log.calibrationLogName));
	}

	[Test]
	public void CreateFilesCatchesException()
	{
		var nameCollisionFile = "bellowslog_" + Timestamp ();
		File.Create (nameCollisionFile);
		log.Start ();

		// Should not throw an IOException
	}


	private void LogManyThings ()
	{
		List<int[]> datalists = new List<int[]> ();
		for (int i = 0; i < DATA_AMOUNT; i++) {
			datalists.Add (RandomInts (DATA_AMOUNT));
		}
		List<GameEvent> events = RandomEvents (DATA_AMOUNT);
		List<Calibration> calibrations = RandomCalibrations (DATA_AMOUNT);

		foreach (var data in datalists) {
			log.LogBellowsData (data);
		}
		foreach (var gameEvent in events) {
			log.LogEvent (gameEvent);
		}
		foreach (var calibration in calibrations) {
			log.LogCalibration (calibration);
		}
	}

	/*
	 * Returns an array of size amount filled with random integers.
	 */
	private int[] RandomInts (int amount)
	{
		int[] ints = new int[amount];
		for (int i = 0; i < amount; i++) {
			ints [i] = rng.Next ();
		}
		return ints;
	}

	/*
	 * Returns an array of integers parsed from the file specified by path.
	 */
	private int[] IntsParsedFrom (string path)
	{
		List<int> ints = new List<int> ();
		using (BinaryReader reader = new BinaryReader (File.Open (path, FileMode.Open))) {
			while (reader.BaseStream.Position < reader.BaseStream.Length) {
				ints.Add (reader.ReadInt32 ());
			}
		}
		return ints.ToArray ();
	}

	private List<Calibration> RandomCalibrations (int amount)
	{
		List<Calibration> calibrations = new List<Calibration> ();
		for (int i = 0; i < amount; i++) {
			var calibration = new Calibration ();
			calibration.Max = rng.Next ();
			calibration.Min = rng.Next ();
			calibrations.Add (calibration);
		}
		return calibrations;
	}

	private List<GameEvent> RandomEvents (int amount)
	{
		List<GameEvent> events = new List<GameEvent> ();
		for (int i = 0; i < amount; i++) {
			events.Add (new GameEvent (RandomString ()));
		}
		return events;
	}

	/*
	 * Generates a random String of alphanumeric characters (and spaces)
	 * of length 1-15.
	 */
	private string RandomString ()
	{
		var validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";
		var stringChars = new char[rng.Next (15) + 1];
		for (int i = 0; i < stringChars.Length; i++) {
			stringChars [i] = validChars [rng.Next (validChars.Length)];
		}
		return new String (stringChars);
	}

	private List<string> DescriptionsFrom (string path)
	{
		List<string> results = new List<string> ();

		using (StreamReader reader = new StreamReader (File.Open (path, FileMode.Open))) {
			while (reader.Peek () > 0) {
				// Read the event description in the last column (ignore the timestamp)
				var columns = reader.ReadLine ().Split (new char[]{ '\t' }, 2);
				results.Add (columns [columns.Length - 1]);
			}
		}

		return results;
	}

	private List<string> TimestampsFrom (string path)
	{
		List<string> results = new List<string> ();

		using (StreamReader reader = new StreamReader (File.Open (path, FileMode.Open))) {
			while (reader.Peek () > 0) {
				// Read the event description in the last column (ignore the timestamp)
				var columns = reader.ReadLine ().Split (new char[]{ '\t' }, 2);
				results.Add (columns [0]);
			}
		}

		return results;

	}

	private void SleepFor (int time)
	{
		System.Threading.Thread.Sleep (time);
	}

	private string Timestamp ()
	{
		var time = DateTime.Now.ToString ("u");
		int os = (int)Environment.OSVersion.Platform;
		if (os != 4 && os != 6 && os != 128) {
			time = time.Replace (':', '.');
		}
		return time;
	}

}