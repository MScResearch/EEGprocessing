using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;


namespace Sockets
{
	public delegate void IncomingReadHandler(SocketReader read, byte[] data);
	public delegate void IncomingReadErrorHandler(SocketReader read, Exception exception);
	
	public class SocketReader
	{
		public const int BUFFER_SIZE = 256;
		
		protected Socket socket;
		protected IncomingReadHandler readHandler;
		protected IncomingReadErrorHandler errorHandler;
		protected byte[] buffer = new byte[BUFFER_SIZE];

		
		public Socket Socket
		{
			get
			{
				return socket;
			}
		}
		
		SocketReader(Socket socket, IncomingReadHandler readHandler, IncomingReadErrorHandler errorHandler = null)
		{
			this.socket = socket;
			this.readHandler = readHandler;
			this.errorHandler = errorHandler;
			
			BeginReceive ();
		}
		
		void BeginReceive ()
		{
			socket.BeginReceive(buffer, 0, BUFFER_SIZE, SocketFlags.None, new AsyncCallback (OnReceive), this);
		}

/*		void BeginSend ()
		{
			byte[] bufferOut = Encoding.ASCII.GetBytes("<TRIGGER>123</TRIGGER>");
			socket.BeginSend(bufferOut, 0, bufferOut.Length, SocketFlags.None, new AsyncCallback (OnSend), this);
		}
*/
		/*void BeginSend ()
		{
			//byte[] bufferOut = Encoding.ASCII.GetBytes("<TRIGGER>123</TRIGGER>");
			socket.BeginSend(bufferOut, 0, bufferOut.Length, SocketFlags.None, new AsyncCallback (OnSend), this);
		}*/
		

		public static void BeginSendACK ()
		{
			//BeginSend ();
		}
		
		public static SocketReader Begin (Socket socket, IncomingReadHandler readHandler, IncomingReadErrorHandler errorHandler = null)
		{
			return new SocketReader (socket, readHandler, errorHandler);
		}
		
		void OnReceive (IAsyncResult result)
		{
			try
			{
				if (result.IsCompleted && socket != null && socket.Connected)
				{
					int bytesRead = socket.EndReceive (result);
					
					if (bytesRead > 0)
					{
						byte[] read = new byte[bytesRead];
						Array.Copy (buffer, 0, read, 0, bytesRead);
						
						readHandler (this, read);
						Begin (socket, readHandler, errorHandler);
					}
					else
					{
						// Disconnect
					}
				}
			}
			catch (Exception e)
			{
				if (errorHandler != null)
				{
					errorHandler (this, e);
				}
			}
		}

		void OnSend (IAsyncResult result)
		{
			try
			{
				if (result.IsCompleted)
				{
					Debug.Log("Send successful");
				}
			}
			catch (Exception e)
			{
				if (errorHandler != null)
				{
					errorHandler (this, e);
				}
			}
		}
	}

}
