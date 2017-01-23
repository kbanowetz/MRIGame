/*
 * This test class opens a serial port, establishes a tcp connection with a client, and then sends the client all of the data that is receives from the serial port.
 * Data is read from the serial port asynchronously, so all you need to do is instantiate this class and then loop while data is being received.
 * */
using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;

public class DemoServer
{
	private SerialPort Port;
	private TcpClient Client;
	private NetworkStream Stream;
	private int BaudRate = 115200;
	private int MessageLength = 12;
	private short TcpPort = 5355;
	private string PortName = "/dev/ttyS0";
	private string IPString;
	private TcpListener Listener;

	public static void Main ()
	{
		DemoServer MyServer = new DemoServer ();
		MyServer.MainLoop ();
	}

	/**
    *Initializes server and waits for TCP connection with client
    */
	public DemoServer ()
	{
		IPString = "192.168.0.2";
		Port = new SerialPort ();
		Port.PortName = PortName;
		Port.StopBits = StopBits.One;
		Port.BaudRate = BaudRate;
		Port.DataBits = 8;
	}
	/*
	MainLoop provides a CLI which allows users to configure and run the server.
	*/
	private void MainLoop ()
	{
		string Prompt = "Enter a command (use \"commands\" to see a list of commands\n";
		string Tildy = "> ";
		string Commands = "Valid commands:\n"
		                        + "run\n    (Starts the server, make sure)\n\n"
		                        + "setserial [argument]\n    (Sets the name of the serial port)\n\n"
		                        + "tcpport [argument]\n    (Sets the server's tcp port number)\n\n"
		                        + "setip [argument]\n    (Sets the server's IP)\n\n"
		                        + "commands\n    (Prints the list of commands)\n\n"
		                        + "loopback\n    (Toggles the Loopback variable, when true the server's IP address is the loopback address)\n\n"
		                        + "quit\n    (Terminates the program)\n";
		bool Loop = true;
		while (Loop) {
			Console.Write (Prompt);
			Console.Write (Tildy);
			string[] Input = Console.ReadLine ().Split (null);
			if (Input.Length <= 0) {
				continue;
			}
			switch (Input [0]) {
			case "setserial":
				if (Input.Length != 2) {
					Console.WriteLine ("Please provide the name of the port, example usage is: portname /dev/ttyS0");
					break;
				}
				Port.PortName = Input [1];
				Console.WriteLine ("Set port name to: " + Port.PortName);
				break;
			case "tcpport":
				if (Input.Length != 2) {
					Console.WriteLine ("Please provide the tcp port number, example usage is: tcpport 5355");
					break;
				}
				try {
					short InputPort = Convert.ToInt16 (Input [1]);
					if (InputPort < 1) {
						Console.WriteLine ("Invalid argument, port number must be greater than 0 and less then 65536");
						break;
					}
					TcpPort = InputPort;	
					Console.WriteLine ("Set port to: " + TcpPort);
				} catch (Exception) {
					Console.WriteLine ("Invalid argument");
				}
				break;
			case "setip":
				if (Input.Length != 2) {
					Console.WriteLine ("Please provide the ip address, example usage is: setip 192.168.0.1");
					break;
				}
				IPString = Input [1];
				Console.WriteLine ("Set IP to: " + IPString);
				break;
			case "run":
				if (Input.Length != 1) {
					Console.WriteLine ("You provided too many arguments, example usage is: run");
					break;
				}
				Run ();
				break;
			case "commands":
				Console.Write (Commands);
				break;
			case "loopback":
				IPString = "0.0.0.0";
				Console.WriteLine ("Set IP to " + IPString);
				break;
			case "quit":
				Console.WriteLine ("Exiting program");
				Loop = false;
				break;
			case "default":
				Console.WriteLine ("Invalid command, use \"commands\" to see the list of valid commands\n" + Prompt);
				break;
			}
		}
	}
	/*
	Run() attempts to open a serial port and establish a tcp connection. If either fail, it will wait half a second and try again. After ~30 seconds
	without succeeding, Run() will return to the prompt loop
	*/
	public void Run ()
	{
		Listener = new TcpListener (IPAddress.Parse (IPString), TcpPort);
		if (!TcpConnect ()) {
			Console.WriteLine ("Unable to connect to Input");
			return;
		}
		if (!this.SerialOpen ()) {
			Console.WriteLine ("Unable to open serial port");
			return;
		}
		while (Client.Connected) {
			short Data = this.ReceiveData ();
			if (Data == -2) {
				break;
			}
			if (Data == -1) {
				Task.Delay (3);
				continue;
			}

			byte[] ToSend = BitConverter.GetBytes (Data);
			if (!SendData (ToSend)) {
				Task.Delay (3);
				continue;
			}	
					
		}
		Port.Close ();
		Console.WriteLine ("Disconnected");
	}
	/*
	ReceiveData attempts to read data from the serial port
	Returns:	0 if a full message of data is not available but the port is open
				1 if a full message was read
			   -1 if the read fails or the port is closed
	*/
	private short ReceiveData ()
	{
		if (this.Port.BytesToRead < 12) {
			return -1;
		}
		try {
			byte[] Buffer = new byte[this.MessageLength];
			this.Port.Read (Buffer, 0, this.MessageLength);
			//pull the bellows position data out of the Serial "packet"
			short ActualData = (short)(Buffer [7] << 8 | Buffer [6]);
			return ActualData;
			Console.WriteLine (ActualData);
		} catch (Exception) {
			return -2;
		}
	}
	/*
	SendData attempts to send data over the network via the servers tcp connection
		Data is sent by writing a byte array to the tcp connection's network stream
	returns:	0 on success
			   -1 on failure
	*/
	public bool SendData (byte[] Data)
	{
		try {
			this.Stream.Write (Data, 0, Data.Length);
		} catch (Exception) {
			return false;
		}
		return true;
	}
	/*
	SerialOpen attempts to open the serial port specified by the user supplied serial port name
	returns:	0 on success
			   -1 on failure
	*/
	public bool SerialOpen ()
	{
		try {
			this.Port.Open ();
			return true;
		} catch (Exception e) {
			Console.WriteLine (e.Message);
			return false;
		}
	
	}
	/*
	TcpConnect creates a tcp listener and waits for a connection to be established by a tcp client
	returns: 0 if the connection is successfully established
			-1 if established the connection fails
	*/
	public bool TcpConnect ()
	{ 
		try {
			//Passive open TCP connection
			Listener.Start ();
			Console.WriteLine ("Waiting for client to establish connection...");
			Client = Listener.AcceptTcpClient ();
			Listener.Stop ();
			Stream = Client.GetStream ();
			Console.WriteLine ("Connection established");
		} catch (Exception e) {
			Console.WriteLine (e.Message);
			return false;
		}
		return true;
	}
	/*
	GetLocalIPAddress gets the local IP of the executing host
	returns:	The local IP address of the executing host in string format
	*/
	private string GetLocalIPAddress ()
	{
		var host = Dns.GetHostEntry (Dns.GetHostName ());
		foreach (var ip in host.AddressList) {
			if (ip.AddressFamily == AddressFamily.InterNetwork) {
				return ip.ToString ();

			}
		}
		Console.WriteLine ("Unable to find IP address, please set manually using the setip command");
		return "0.0.0.0";
	}
}




