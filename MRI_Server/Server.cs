using System;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.IO.Ports;

public class Server
{
    //Port is hardcoded
    Int32 port = 5355;
    //String the client sends when input is requested
    String clientCode = "data";
    //TODO
    //Find active socket instead of hardcoding
    String serialSocket = "/home/fremaan/workspace/ints.txt";
    Socket clientSocket;

    /**
    *Initializes server and waits for TCP connection with client
    */
    public Server()
	{

        //Open listener and connect to client
        try
        {

            Console.Write("Initializing server on port: " + port + "\n");
            //Listens for connections to local IP address
            TcpListener server = new TcpListener(IPAddress.Parse("0.0.0.0"), port);
            server.Start();
            Console.Write("Server started\n");
            Console.Write("Waiting for client connection...\n");
            clientSocket = server.AcceptSocket();
            Console.Write("Client connected\n");

        }catch(Exception e)
        {

            Console.Write("Server instantiation failed.\n");
            Console.Write(e.StackTrace);
			Environment.Exit (1);

        }

    }

    /**
    *Workhorse of the Server class. Enters a loop to listen for requests from
    *the client. Once client sends a request for input, the socket is read from
    *and send via TCP, then the server begins waiting for another request
    */
    public void ListenAndReport()
    {

        //Label to avoid Magic Numbers and keep changing this data structure easy
        int sizeOfRequestBuffer = 100;
        //Sets up buffer for client incoming request
        byte[] request = new byte[sizeOfRequestBuffer];

        //Open Bellow Serial Socket
        //The permissions allow non-exclusive privileges in order to keep the socket open
        //for the bellows to write to
        //XXX
        //If streamreader can be opened inclusively so the socket remains open, it will be more robust for reading data
        //FileStream bellowsInput = new FileStream("/dev/ttyS0", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
		StreamReader bellowsInputStream = null;
		try{
                    if(!File.Exists(serialSocket)){
                        throw new FileNotFoundException();
                    }

		    bellowsInputStream = new StreamReader(File.OpenRead(serialSocket));
                
	            Console.WriteLine ("Opened file");
		}catch(Exception e){
			Console.WriteLine ("Failed to open serial socket...exiting");
			Environment.Exit(1);
		}

        //TODO message the client can use to close the server connection
        //currently is waiting for requests until the process is killed
		while (clientSocket.Connected)
        {
            //Receive() sends the number of bytes the client sent, we can use this
            //for iterating through the data structure
			try
			{
            clientSocket.Receive(request);
			

            //Could have it read in a byte array, but I'm not sure yet how
            //the bellows socket works
			SendBellowsData(true, bellowsInputStream.ReadLine().Trim(), clientSocket);
			
			}catch(Exception e){
				Console.WriteLine ("Connection between server and client terminated");
				clientSocket.Close ();
			}

        }

    }

    /**
    *Verifies code given during TCP client request
    *
    *Returns 
    *true - code matches
    *false - otherwise
    */
    private Boolean CheckRequestValidity(Byte[] request)
    {
		
		if ((Encoding.ASCII.GetString (request).CompareTo (clientCode)) == 0) {
			Array.Clear (request, 0, request.Length);
			return true;
		}
		return false;
    }

    /**
    *Handles the encoding of the string message read from the serial socket
    *to a byte array and sends data across TCP socket
    */
    private void SendBellowsData(Boolean confirmation, String transferLine, Socket transferSocket)
    {

        if (confirmation) 
        {
			
            transferSocket.Send(Encoding.ASCII.GetBytes(transferLine));
            Console.WriteLine("Sending: " + transferLine); 
        }

    }

}
