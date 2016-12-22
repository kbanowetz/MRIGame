using NUnit.Framework;
using UnityTest;
using System.Linq;
using System;
using NSubstitute;

[TestFixture]
public class CalibrationControllerTest
{
	private const int DATASIZE = 200;

	Input input;
	CalibrationController controller;

	[SetUp]
	public void Setup() {
		// Mock the input to allow control over GetInput return values
		 input = Substitute.For<Input>();

		 controller = new CalibrationController(input);
	}

    [Test]
    public void CalibrationControllerCorrectlySetsCalibrationMinAndMax()
    {
		// Create test data
		int[] inputs = randomInputs (DATASIZE);
		int MAX = inputs.Max ();
		int MIN = inputs.Min ();

        // Simulate a series of input values
        for (int i = 0; i < DATASIZE; i++)
        {
            input.GetInput().Returns(inputs[i]);
            controller.Update();
        }

		// Ensure calibration set correctly
		Calibration calibration = controller.Calibration;

        Assert.AreEqual(MIN, calibration.Min);
        Assert.AreEqual(MAX, calibration.Max);
    }

	// Returns an array of random integers in range [0, 1000]
	private int[] randomInputs(int amount) {
		Random rng = new Random ();
		int[] inputs = new int[amount];
		for (int i = 0; i < inputs.Length; i++) {
			inputs [i] = rng.Next (1000);
		}
		return inputs;
	}
}
