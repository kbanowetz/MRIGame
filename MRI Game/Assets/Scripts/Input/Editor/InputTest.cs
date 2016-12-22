using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

[TestFixture]
public class InputUnitTests
{
	Input inputModule;

	[SetUp]
	public void initialize()
	{
		this.inputModule = new TcpInput();
	}

	[Test]
	public void GetSimulatedInputReturnsValidInteger()
	{
		for (int x = 0; x < 10000; x++) 
		{
			int data = this.inputModule.GetInput();
			Assert.Less(data, 10000);
			Assert.Greater(data, 0);
		}
	}

}