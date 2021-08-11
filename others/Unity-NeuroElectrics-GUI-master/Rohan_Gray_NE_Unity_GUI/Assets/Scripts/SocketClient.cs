using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Sockets
{
	public static class SocketClient
	{
		private static int unansweredKeepAlives;
		public static float connTimeoutSecs;
		public static bool Connected 
		{
			get
			{
				bool isConnected = socket != null;
				if(isConnected)
				{
					//Debug.Log("unansweredKeepAlives: " + unansweredKeepAlives);
					isConnected = isConnected && socket.Connected && unansweredKeepAlives < 5;
				}
				return isConnected;	
			}
		}
		private static int port;
		public static Socket socket; 
		public static EndPoint server;
		private static IPAddress ip;
		private static GameObject gameObject;
		//private static string message;
		private static string lastErrorMsg;

		//private static List<string> packets;
		private static List<byte[]> packets;
		
		// Initialize the server connecion
		public static bool InitSocket(string ipstr, int port)
		{
			if(SocketClient.socket != null && SocketClient.socket.Connected)
				return true;
			unansweredKeepAlives = 0;
			SocketClient.port = port;
			if(ipstr.Length == 0) //if empty, use Host IP
			{
				SocketClient.ip = GetHostIP();
				connTimeoutSecs = 5.0f;
			}
			else
			{
				connTimeoutSecs = 5.0f;				
				if(!SetIP(ipstr))
					return false;
			}
			server = new IPEndPoint (ip, port);
			
			//SocketClient.ConnectToServer();
			Debug.Log("Conn keepalive timeout set to:" +connTimeoutSecs);
			//message = "";
			return true;
		}

		public static bool ChangeServerIP(string serverIP)
		{
			if(serverIP.Length >= 7 && serverIP.Length <= 15) //if empty, use Host IP
			{
				if(SetIP(serverIP))
				{
					server = new IPEndPoint (ip, port);
					Debug.Log("Server IP changed to:" + serverIP + ":" + port);
					return true;
				}
			}
			return false;
		}
		// Connect the client to the server
		public static bool ConnectToServer()
		{	
			bool ok = false;
			lastErrorMsg = "";
			try	
			{
				if(SocketClient.socket == null)
				{
					SocketClient.socket = new Socket (AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
					//socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 1000);
					// The socket will linger for 1 seconds after Socket.Close is called.
					//LingerOption lingerOption = new LingerOption (true, 1);
					//socket.SetSocketOption (SocketOptionLevel.Socket, SocketOptionName.Linger, lingerOption);
					//SocketClient.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
				
					//SocketClient.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
					//if(!SocketClient.socket.Connected)
					{
						lastErrorMsg = "Attempting connection to NIC [" + ip + ":" + port +"]";
						Debug.Log(lastErrorMsg);
						SocketClient.socket.Connect (server);
					}
					//else
						//Debug.Log("ConnectToServer()::Connected: true");
					//ok = true;
				}
				else
				{
					if(!SocketClient.socket.Connected) //either this side is not connected						
					{//let's try again
						Debug.Log("Socket disconnected. Cleaning up for a retry");
						//Debug.Log("Attempting connection to: "+ip + " " + port);
						//SocketClient.socket.Shutdown(SocketShutdown.Both);
						//SocketClient.socket.Disconnect (true);
						
						SocketClient.socket = null;
						//SocketClient.socket.Connect (server);
						lastErrorMsg = "Socket is connected, but server did not answer";
					}
					else if(unansweredKeepAlives > 5) //or the other one does not answer yet
					{
						SocketClient.socket.Shutdown(SocketShutdown.Both);
						//Debug.Log("shutd");
						SocketClient.socket.Disconnect(true);
						//SocketClient.socket.Disconnect(false);
						Debug.Log("Socket Shutdown and Dced");
						SocketClient.socket = null;
						ok = false;
						lastErrorMsg = "Server at " + ip + ":" + port + " not responding";
					}
				}
			}
			catch(SocketException s)
			{
				Debug.Log("Error connecting to: "+ip + " " + port + " (" + s.ErrorCode +")");
				if(lastErrorMsg.Length == 0)
					lastErrorMsg = "{" + ip + ":" + port + "} "+ s.Message;
				//SocketClient.socket.Shutdown(SocketShutdown.Both);
				//Debug.Log("shutd");
				//SocketClient.socket.Disconnect(true);
				//SocketClient.socket.Disconnect(false);
				//SocketClient.socket = null;
				return false;
			}
			if(SocketClient.socket != null && SocketClient.socket.Connected)
			{//everything went better than expected =)
				Debug.Log("Connected to " + ip + " on port " + port
				+ " using port " + ((IPEndPoint)SocketClient.socket.LocalEndPoint).Port.ToString());
				//lastErrorMsg = "";
				//packets = new List<string>();
				packets = new List<byte[]>();
				// Start listening to the port
				SocketReader.Begin (SocketClient.socket, OnReceive, OnError);
				ok = true;
				unansweredKeepAlives = 0;
			}
			return ok;
		}

		public static IEnumerator TestConnected()
		{
			for(;;)
			{		
				yield return new WaitForSeconds(connTimeoutSecs);
				//Debug.Log("SocketClient.TestConnected()");
				if(SocketClient.socket != null)
				{
					if(SocketClient.socket.Connected)
					{
						//Debug.Log("SocketClient.TestConnected():: Socket seems connected. Try Send");
						bool block = SocketClient.socket.Blocking;
						//byte [] tmp = Encoding.ASCII.GetBytes("<TRIGGER>"+7+"</TRIGGER>");
						SocketClient.socket.Blocking = false;
						try
						{
							//nftypes.h : GAME_CONN_CHECK = 7
							SendData('7');
							unansweredKeepAlives++;
							//Debug.Log(System.DateTime.Now.ToString("[HH:mm:ss] ") + "Sent keepalive:"+unansweredKeepAlives);
						}
						catch(SocketException e)
						{
							/*
							if (e.NativeErrorCode.Equals(10035))
							{//if this exception is thrown the socket is connected, but blocking
								connected = true;
							}
							*/
							lastErrorMsg = e.Message;
						}
						finally
						{
							SocketClient.socket.Blocking = block;
						}
					}
				}
				//Debug.Log("Waiting " + connTimeoutSecs + " unansweredKeepAlives:" + unansweredKeepAlives);
			}
		}
		
		public static bool SetIP(string ipString)
		{
			IPAddress ip;
			if(IPAddress.TryParse(ipString, out ip))
			{
				SocketClient.ip = ip;
				return true;
			}
			return false;
		}
		
		// Get the IP from the HOST	
		public static IPAddress GetHostIP()
		{
			return (from entry in Dns.GetHostEntry (Dns.GetHostName ()).AddressList where entry.AddressFamily == AddressFamily.InterNetwork 
			      select entry).FirstOrDefault ();
		}
	
		public static string GetLastErrMessage()
		{
			if(lastErrorMsg != null && lastErrorMsg.Length > 0)
			{
				string s = System.String.Copy(lastErrorMsg);
				lastErrorMsg = "";
				return s;
			}
			return "";
		}

		// Initialize the server connexion
		public static byte GetData()
		{
			//Debug.Log ("Hello from " + SocketClient.ip);
			//string aux = "";
			byte[] aux = {0};
			if (SocketClient.socket == null || !SocketClient.socket.Connected)
				return aux[0];

			if (packets.Count>0)
			{
				//aux=packets[0];
	//Debug.Log("PACKETS: " + packets.First() + " (" + packets.Count + ")");
				//aux = Encoding.UTF8.GetBytes(packets[0]);
				aux = packets[0];
				packets.RemoveAt(0);
			}
			return aux[0];
		}

		public static void SendData(byte[] dataToSend)
		{			
			SocketWriter.Begin (dataToSend,socket, OnSend, OnErrorWriting);
		}		

		public static void SendData(int dataToSend)
		{			
			SocketWriter.Begin (dataToSend,socket, OnSend, OnErrorWriting);
		}

		public static void KeepAliveReceived()
		{
			unansweredKeepAlives = 0;
		}
		
		//Mensaje recibido del server
		private static void OnReceive (SocketReader read, byte[] data)
		{
			//Debug.Log("[OnRecive]: " + data[0] + " [0]\n");
			packets.Add (data);			
			
			KeepAliveReceived();
			SocketReader.Begin (SocketClient.socket, OnReceive, OnError);
		}

		//Mensaje recibido del server
		private static void OnSend (SocketWriter write, byte[] data)
		{			
			//Debug.Log ("Mensaje recibido del serverr: " + message + " length of packets " + packets.Count);
		}
		
		//Error recibiendo datos del server
		private static void OnError (SocketReader read, System.Exception exception)
		{
			lastErrorMsg = "Receive error: " + exception;
		}

		//Error mandando datos del server
		private static void OnErrorWriting (SocketWriter read, System.Exception exception)
		{
			lastErrorMsg = "Sending error: " + exception;			
		}
	}
}

