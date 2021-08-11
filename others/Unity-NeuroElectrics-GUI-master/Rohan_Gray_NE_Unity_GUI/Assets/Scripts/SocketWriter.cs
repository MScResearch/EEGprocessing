using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;


namespace Sockets
{
	public delegate void IncomingWriteHandler(SocketWriter read, byte[] data);
	public delegate void IncomingWriteErrorHandler(SocketWriter read, Exception exception);
	
	public class SocketWriter
	{
		public const int BUFFER_SIZE = 256;
		
		protected Socket socket;
		protected IncomingWriteHandler writeHandler;
		protected IncomingWriteErrorHandler errorHandler;
		protected byte[] buffer = new byte[BUFFER_SIZE];
		
		
		public Socket Socket
		{
			get
			{
				return socket;
			}
		}

		/*SocketWriter(string code, Socket socket, IncomingWriteHandler writeHandler, IncomingWriteErrorHandler errorHandler = null)
		{
			this.socket = socket;
			this.writeHandler = writeHandler;
			this.errorHandler = errorHandler;
			
			BeginSend (code);
		}*/
		SocketWriter(byte[] code, Socket socket, IncomingWriteHandler writeHandler, IncomingWriteErrorHandler errorHandler = null)
		{
			this.socket = socket;
			this.writeHandler = writeHandler;
			this.errorHandler = errorHandler;
			
			BeginSend (code);
		}
		SocketWriter(int code, Socket socket, IncomingWriteHandler writeHandler, IncomingWriteErrorHandler errorHandler = null)
		{
			this.socket = socket;
			this.writeHandler = writeHandler;
			this.errorHandler = errorHandler;
			
			BeginSend (code);
		}
		
		/*void BeginSend (string code)
		{
			byte[] bufferOut = Encoding.ASCII.GetBytes("<TRIGGER>"+code+"</TRIGGER>");
			//byte[] bufferOut = Encoding.ASCII.GetBytes(code);
			Debug.Log("Sending " + code.ToString() + " (string)\n");
			socket.BeginSend(bufferOut, 0, bufferOut.Length, SocketFlags.None, new AsyncCallback (OnSend), this);
		}
		void BeginSend (string code)
		{
			//byte[] bufferOut = Encoding.ASCII.GetBytes("<TRIGGER>"+code+"</TRIGGER>");
			//byte[] bufferOut = Encoding.ASCII.GetBytes(code);
			byte[] bufferOut = Encoding.UTF8.GetBytes(code);
			Debug.Log("Sending " + bufferOut[0] + " (string) " + code + "\n");
			//socket.BeginSend(bufferOut, 0, bufferOut.Length, SocketFlags.None, new AsyncCallback (OnSend), this);
			socket.BeginSend(bufferOut, 0, 1, SocketFlags.None, new AsyncCallback (OnSend), this);
		}*/
		void BeginSend (byte[] code)
		{
			Debug.Log("Sending " + System.Text.Encoding.UTF8.GetString(code) + " (byte[])\n");
			socket.BeginSend(code, 0, code.Length, SocketFlags.None, new AsyncCallback (OnSend), this);
		}
		void BeginSend (int code)
		{
			byte codeByte = (byte) code;
			byte[] bufferOut = {codeByte};
//			Debug.Log("Sending " + bufferOut[0] + " (int)\n");
			//socket.BeginSend(bufferOut, 0, bufferOut.Length, SocketFlags.None, new AsyncCallback (OnSend), this);
			socket.BeginSend(bufferOut, 0, 1, SocketFlags.None, new AsyncCallback (OnSend), this);
		}

		/*public static SocketWriter Begin (string code, Socket socket, IncomingWriteHandler writeHandler, IncomingWriteErrorHandler errorHandler = null)
		{
			Debug.Log("Sending " + code + " to NIC (string)");
			return new SocketWriter (code,socket, writeHandler, errorHandler);
		}*/
		public static SocketWriter Begin (byte[] code, Socket socket, IncomingWriteHandler writeHandler, IncomingWriteErrorHandler errorHandler = null)
		{
			Debug.Log("Sending " + 	System.Text.Encoding.UTF8.GetString(code) + " to NIC (string)");
			return new SocketWriter (code,socket, writeHandler, errorHandler);
		}
		public static SocketWriter Begin (int code, Socket socket, IncomingWriteHandler writeHandler, IncomingWriteErrorHandler errorHandler = null)
		{
//			Debug.Log("Sending " + code + " to NIC (int)");
			return new SocketWriter (code,socket, writeHandler, errorHandler);
		}
		
		
		void OnSend (IAsyncResult result)
		{
			try
			{
				if (result.IsCompleted)
				{
					//int bytesRead =  
					socket.EndSend (result);
					//Debug.Log("Send successful");
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
