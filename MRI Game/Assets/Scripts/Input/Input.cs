using System;

public abstract class Input
{
	/*Structure of a "message" from bellows. This must be verified
	 * short SomeData
	 * short SomeData
	 * short SomeData
	 * short BellowsPositionData
	 * short SomeData
	 * ushort checksum
	 * */


	public const string ConfigFilePath = "Assets/Scripts/Input/config.txt";//path to config file
	public const int MessageLength = 2; //The Length one message from the bellows. (6 shorts -> 12 bytes)
	public const int BellowsPositionDataOffset = 0; //Within each message, the Bellows' position data starts at bytes[6]
	public const int BellowsPositionDataLength = 2; //The length of bytes of the Bellows' position data (1 short -> 2 bytes)
	public const int BufferSize = 12 * 100; //This should be a multiple of MessageLength
	public const int Timeout = 200;//the number of milliseconds reads should timing out.

	//Get Input returns the most recent BellowsPositionData
	public abstract int GetInput();
}

