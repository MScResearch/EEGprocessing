
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using System;
using System.Text;
using Sockets;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LSL;


public class sendTCP : MonoBehaviour
{


    private const int TCP_MATNIC_PORT = 1235;
    private const string TCP_MATNIC_HOST = "127.0.0.1";

    private const int CODE_COMMAND_QUERY_STATUS = 200;

    private const int CODE_STATUS_REMOTE_CONTROL_ALLOWED = 241;
    private const int CODE_STATUS_DEVICE_NOT_PRESENT = 244;
    private const int CODE_COMMAND_START_EEG = 201;
    public const int num_samples = 15000; // 15000 corresponds to 30 seconds of data
    string[] electrodesarray = new string[num_electrodes] { "P7", "CZ", "PZ", "POZ", "P8", "PO7", "PO8", "C4", "F6", "FP2", "FZ", "C3", "F5", "FP1", "AFZ", "E1", "E2", "E3", "E4", "E5" }; // Creating array of electrode names . Sequence of names is important as it corresponds to respective line rendering positions and the sequence in which data is pulled from NIC
    public Renderer[] electrodes = new Renderer[num_electrodes]; // Declaring an array of renderers which later are used to find the respective electrodes
    public GameObject[] DifferentElectrodes = new GameObject[num_electrodes]; // used to find respective electrodes
    public Text ConnectTCP; // Text displayed on GUI
    public Text LogText; // Text displayed on GUI
    public Color orange;

    float[,] buffer = new float[num_samples, num_electrodes]; // Declaring buffer for storing EEG data
    float[,] bufferQuality = new float[10, 20]; // Declaring buffer for storing Impedence Data
    double[] timestamps = new double[num_samples];
    double[] timestampsQuality = new double[10];
    double[,] EEGDisplay = new double[1500, num_electrodes]; // Creating matrix for storing 3 seconds of data which is actually displayed on GUI
    public double[,] EEGDisplay1 = new double[1500, num_electrodes];

    Queue[] dataq; // Declaring array of Queues

    private Process prc = null; // Declaring process to later launch NIC
    private bool Running = false;
    private bool FoundNIC = false;
    private bool inletcreated = false; 
    private bool inlet2created = false;
    private bool NICLaunched = false;

    public byte currentNICStatus;
    public const int num_electrodes = 20; //Sets the number of electrodes to 20
    public double Sum = 0; // Sum later equates to the total EEG values to compute Z-Transform

    public static bool monitoring;
    public static bool Acquisition;
    private static bool Acquisition2;
    public liblsl.StreamInfo[] results;         
    public liblsl.StreamInlet inlet;     // inlet for EEG data acquisition
    public liblsl.StreamInfo[] results2;                    
    public liblsl.StreamInlet inlet2;  // separate inlet for Impedence data 
    public double DynamicValue;// Dynamic Value is the variable which = EEG data stored in an element of a matrix

    int chunksize; // Size of inlet.pull chunk
    double ImpedenceChunk; // Size of inlet2.pull chunk
    object[][] arrayofarrays = new object[num_electrodes][]; // matrix that stores the EEG data stored in the array of queues (Datatype: Object)
    double[][] floatarrayofarrays = new double[num_electrodes][]; // matrix that stores the EEG data stored in array of arrays (Datatype: double)
    double[] mean = new double[num_electrodes]; 
    double[] std = new double[num_electrodes]; //standard deviation array

    public double[,] normalizedvalues = new double[num_electrodes,1500]; // normalized values for 3 second worth of data
    public double[,] normalizedvalues_short = new double[num_electrodes,5]; // normalized values for 3 second worth of data

	
    void Start()
    {
        if (SocketClient.InitSocket(TCP_MATNIC_HOST, TCP_MATNIC_PORT))
            UnityEngine.Debug.Log("[TCP] Socket initialized!");
        else
            UnityEngine.Debug.Log("[ERROR] Could not initialize the socket");
        DynamicValue = 0;
        monitoring = false;
        ConnectNIC(); // Call to ConnectNIC method to Launch NIC application
        NICLaunched = true;
        Acquisition = true;
        Acquisition2 = false;
        inletcreated = false; // inlet has not yet been created
        inlet2created = false; // inlet2 has not yet been created
        dataq = new Queue[num_electrodes]; // initializing array of queues of size 20
        for (int j = 0; j < num_electrodes; j++) 
        {
            dataq[j] = new Queue();

            for (int i = 0; i < num_samples; i++)
            {

                dataq[j].Enqueue(0.0f); // filling all values of queue with zeros
            }
        }


        for (int i = 0; i < num_electrodes; i++)
        {
            electrodes[i] = GameObject.Find(electrodesarray[i]).GetComponent<Renderer>(); // Finding Electrodes from frontend Unity
            DifferentElectrodes[i] = GameObject.Find(electrodesarray[i]); // Finding electrodes from frontend Unity
        }
    }

						//UnityEngine.Debug.Log("");      << Gray Text for invoking debug log.
	
    void Update()
    {
        if(FoundNIC == true && NICLaunched == true) // If ConnectNIC method is implemented and NIC is launched then connect() method is implemented with a delay of 2 seconds to avoid server based exceptions
        {
			Invoke("connect", 2f); 
            NICLaunched = false; // declared as false so that connect() method is not updated every time
        }
        byte status = queryStatus(); // regular status calls for proper functioning of interface
        if (LogText.text == "EEG monitoring started")
        {
			switch (status)
            {
                case (244): // if Enobio disconnects while the interface is on then display the following error on the interface
                    LogText.text = "Device Not Present. Restart your Enobio and Launch the application again.";
                    break;
            }
        }
        //print("0");
        //print(System.DateTime.Now.Hour + "-"+ System.DateTime.Now.Minute + "-" + System.DateTime.Now.Second + "-" + System.DateTime.Now.Millisecond);

        if (monitoring == true && Acquisition == true) // after the connection to enobio is established the startEEG() method is invoked within the connect() method and EEG monitoring is started
        {
			results = liblsl.resolve_stream("type", "EEG");
            inlet = new liblsl.StreamInlet(results[0]); // create inlet for EEG acquisition
			Acquisition = false; // declared false to prevent inlet to be repetitively created
            inletcreated = true;
        }
        if (monitoring == true && inletcreated == true) // after inlet is created, if it succeeded
        {
            chunksize = inlet.pull_chunk(buffer, timestamps);
            //UnityEngine.Debug.Log("Gray Waypoint4.1 - chunksize assigned.");
			for (int j = 0; j < num_electrodes; j++) // amount of new data per electrode
            {
                //UnityEngine.Debug.Log("Gray Waypoint4.2 - did we get here?");
				for (int i = 0; i < chunksize; i++) // size of queue array
                {
                    //UnityEngine.Debug.Log("Gray Waypoint4.3 did we get here?");
					dataq[j].Dequeue();  // Dequeuing the first value
                    dataq[j].Enqueue(buffer[i, j]); // Enqueuing to the end (FIFO concept)
                }

            }
            for (int j = 0; j < num_electrodes; j++)
            {

                arrayofarrays[j] = dataq[j].ToArray(); // Coneverting array of queues to an array of arrays of object datatype
                floatarrayofarrays[j] = new double[num_samples]; 
                for (int i = 0; i < num_samples; i++)
                {
                    floatarrayofarrays[j][i] = System.Convert.ToDouble(arrayofarrays[j][i]); // converting object arrayofarrays to double arrayofarrays
                }

                mean[j] = floatarrayofarrays[j].Sum() / floatarrayofarrays[j].Length; // computing mean for indvidual electrodes 
            }
            for (int k = 0; k < num_electrodes; k++) // num_electrodes = 20
            {
                for (int i = 0; i < num_samples; i++) // num of eeg samples = 15000
                {
                    DynamicValue = Math.Pow((floatarrayofarrays[k][i] - mean[k]), 2); // Dynamic Value is the variable which = EEG data stored in an element of a matrix

                }
                Sum = Sum + DynamicValue; // Sigma component


                std[k] = Math.Sqrt(Sum / (floatarrayofarrays[k].Length - 1)); // standard deviation of data
            }
            for (int i = 0; i < num_electrodes; i++)
            {
                for (int k = 0; k < 1500; k++)
                {
                    EEGDisplay[k, i] = floatarrayofarrays[i][floatarrayofarrays[i].Length - (1500 - k)]; // creating a matrix of the most recent 3 second of data
                    normalizedvalues[i,k] = (EEGDisplay[k, i] - mean[i]) / (std[i]); // computing Z transform on the 3 second matrix and storing it in normalizedvalues (matrix)
				}
                for (int j = 0; j < 5; j++)
                {
                    EEGDisplay[j, i] = floatarrayofarrays[i][floatarrayofarrays[i].Length - (5 - j)]; // creating a matrix of the most recent 3 second of data
                    normalizedvalues_short[i,j] = (EEGDisplay[j, i] - mean[i]) / (std[i]); // computing Z transform on the 3 second matrix and storing it in normalizedvalues (matrix)
                    EEGDisplay1[j, i] = floatarrayofarrays[i][floatarrayofarrays[i].Length - (5 - j)];
                    //UnityEngine.Debug.Log(normalizedvalues_short[i,j]);  
                }				
				//UnityEngine.Debug.Log("Gray Waypoint7.9 - one electrode's worth of data printed (5 samples)."); 
				

            }         

            if (monitoring == true && inletcreated == true && inlet2created == false) // Creating separate inlet2 for pulling Impedence data only if monitoring is on and the previous inlet for pulling EEG values is created
            {
                results2 = liblsl.resolve_stream("type", "Quality");
                inlet2 = new liblsl.StreamInlet(results2[0]);
                Acquisition2 = false;
                inlet2created = true;
            }

            if (inlet2created == true) // if inlet2 for pulling impedence values created then carry out impedence checks
            {
				ImpedenceChecks(); 
            }

        }
	//	other = LineRenderer.GetComponent("LineRendererScript");
	//	UnityEngine.Debug.Log("Gray Waypoint10.9 - Called lineRendererScript and returned to sendTCP");

    }

    public void ConnectNIC()
    {

        String os = SystemInfo.operatingSystem;
        UnityEngine.Debug.Log("Info:" + os);

        prc = new Process();

        string path32 = "C:/Program Files (x86)/NeuroElectrics/NIC/NIC.exe";   // establishing path of NIC file
        string path64 = "C:/Program Files (x86)/NeuroElectrics/COREGUI/COREGUI.exe"; 


        if (File.Exists(path32))
        {
            UnityEngine.Debug.Log("_32 Bin Installation Found.");
            prc.StartInfo.FileName = path32; // starting NIC application
            FoundNIC = true;
        }
        else if (File.Exists(path64))
        {
            UnityEngine.Debug.Log("_64 Bin Installation Found.");
            prc.StartInfo.FileName = path64; // starting NIC application
            FoundNIC = true;
        }
        else
        {
            UnityEngine.Debug.Log("Your NIC file has not been found in the specified paths OR probably NIC is not downloaded in your Surface."); 
        }

        if (FoundNIC == true)
        {
            prc.StartInfo.Arguments = "disableMatNICConfirmation systemtray wifi 192.168.1.1"; // minimizing NIC application to taskbar, this is where I'm (gray) adding args so we can connect to wifi.  
            prc.Start();
            Running = true;
            ConnectTCP.text = "Connect"; // if NIC launched then application is ready to connect to enobio
        }
    }
    public void checkForConnection()
    {
        currentNICStatus = queryStatus();

        string s = SocketClient.GetLastErrMessage();
        s = System.DateTime.Now.ToString("HH:mm:ss - ") + s;
        //Debug.Log(s + "currentNICStatus: |" + currentNICStatus.ToString() + "|\n");

        if (currentNICStatus != 0)
        {
            UnityEngine.Debug.Log("Something Received\n");

            if (currentNICStatus == (byte)CODE_STATUS_REMOTE_CONTROL_ALLOWED)
            {
                UnityEngine.Debug.Log("REMOTE CONTROL ALLOWED\n");
                CancelInvoke("checkForConnection");
                ConnectTCP.text = "Disconnect";
                LogText.text = "Connected to Enobio!";
                UnityEngine.Debug.Log("Gray Waypoint2.5 - about to invoke startEEG");
				startEEG(); // Start EEG monitoring once connection to Enobio is established
            }
            else
            {
                UnityEngine.Debug.Log("Not code expected...");
            }
        }
        else
        {
            UnityEngine.Debug.Log("...waitting response\n");
        }

        UnityEngine.Debug.Log("Please wait...");


    }

    public void connect()
    {
        if (ConnectTCP.text == "Connect")
        {
            if (SocketClient.ConnectToServer())
                UnityEngine.Debug.Log("[TCP] Connected to the Server");
            else
                UnityEngine.Debug.Log("[TCP] Could not connect to the Server");


            if (!SocketClient.Connected && SocketClient.InitSocket(TCP_MATNIC_HOST, TCP_MATNIC_PORT))
            {
                SocketClient.ConnectToServer();
                string s = SocketClient.GetLastErrMessage();
                if (s.Length > 0)
                {
                    s = System.DateTime.Now.ToString("HH:mm:ss - ") + s;
                    UnityEngine.Debug.Log(s);

                }

            }
            //StartCoroutine(SocketClient.TestConnected());
            //StartCoroutine(attemptReconnection());		

            UnityEngine.Debug.Log("Connection DONE!");

            InvokeRepeating("checkForConnection", 1, 1); // Constant  status checks for querystatus() of the device
            ConnectTCP.text = "Connecting...";
            LogText.text = "Connecting to Enobio...If your Enobio doesnt connect within 30 seconds, make sure your Enobio is charged and restart your Enobio.";

        }
		else if (ConnectTCP.text == "Connecting...") {
			CancelInvoke("checkForConnection");	
			UnityEngine.Debug.Log ("REMOTE CONTROL CANCELLED\n");
			ConnectTCP.text = "Connect";
		} 
		else {
			SocketClient.socket.Close();
			UnityEngine.Debug.Log("Socket CLOSED");
			ConnectTCP.text = "Connect";
		}

    }


    public byte queryStatus()
    {
        byte current_status = (byte)'0';

        if (ConnectTCP.text != "Connect")
        {
            SocketClient.SendData(CODE_COMMAND_QUERY_STATUS); 
            current_status = SocketClient.GetData(); // Getting status calls of whether the device is functioning
        }

        return current_status;
    }

    private void RecordEEG(string patientName, string recordEasy, string recordEDF) // Method to create a .easy/.edf file of the data monitored 
    {

        byte current_status = queryStatus();
        byte b_cmd = (byte)CODE_COMMAND_START_EEG;
        byte[] b_pat = Encoding.UTF8.GetBytes(patientName);
        byte comma = (byte)';';
        byte[] b_easy = Encoding.UTF8.GetBytes(recordEasy);
        byte[] b_edf = Encoding.UTF8.GetBytes(recordEDF);

        //int size = b_cmd(1) + b_pat.Length + comma(1) + b_easy.Length + comma(1) + b_edf.Length + comma(1);	
        int size = b_pat.Length + b_easy.Length + b_edf.Length + 4;
        byte[] send_code = new byte[size];

        int index = 0;
        send_code[index++] = b_cmd;

        for (int i = 0; i < b_pat.Length; ++i)
            send_code[index++] = b_pat[i];
        send_code[index++] = comma;

        for (int i = 0; i < b_easy.Length; ++i)
            send_code[index++] = b_easy[i];
        send_code[index++] = comma;

        for (int i = 0; i < b_edf.Length; ++i)
            send_code[index++] = b_edf[i];

        send_code[index++] = (byte)'\0';

        //Debug.Log("-> send_data: |" + System.Text.Encoding.UTF8.GetString(send_code) + "|\n");
        SocketClient.SendData(send_code);
        monitoring = true;

        current_status = SocketClient.GetData();
        LogText.text = "EEG monitoring started";

    }

    public void startEEG()
    {
        RecordEEG("", "true", "true"); // Setting Boolean "false" , "false" starts monitoring the EEG but does not store any file of the data. Only if both bools are set true a .edf, .easy file of recorded EEG data would be created
    }

    public void ImpedenceChecks()
    {
        
            ImpedenceChunk = inlet2.pull_chunk(bufferQuality,timestampsQuality); 
            
            if (ImpedenceChunk > 0)
        {
            print(ImpedenceChunk);
                for (int i = 0; i < num_electrodes; i++)
            {
                    if (bufferQuality[0,i] <= 0.6F)  // Threshold Values for Impedence 
                        electrodes[i].material.color = Color.red; // Change the color of the respective electrode
                    else if (bufferQuality[0,i] <= 0.9F)
                        electrodes[i].material.color = this.orange;
                    else if (bufferQuality[0,i] <= 1F)
                    {
                        electrodes[i].material.color = Color.green;
                    }
            }

        }
       
    }

    void OnApplicationQuit()
    {
        if (prc != null && Running) // If NIC Process is Running while user quits the application
        {
            prc.Kill(); // Kill instances of NIC
            prc.WaitForExit();
        }
    }

}