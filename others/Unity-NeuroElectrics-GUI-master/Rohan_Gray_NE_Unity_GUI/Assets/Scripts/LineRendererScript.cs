using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class LineRendererScript : MonoBehaviour
{

    public sendTCP tcp; // access to sendTCP script
    public Lr_IntegerVariable lriv; // access to Lr_IntegerVariable script
//    public ScaleButtons scale_factor; // access to ScaleButtons script to get the scale factor
   

    public LineRenderer[] lines = new LineRenderer[20]; // Declaring Array of Renderers
    private static float x = 1500f; // X is set to the number of vertices of each renderer
    Renderer[] electrodelines = new Renderer[20]; // Later equated to the electrodes from the sendTCP script
    GameObject[] newelectrodes = new GameObject[20]; // Later equated to DifferentElectrodes (Game Objects) from sendTCP script
    int ValueForInteraction; // Value of IntegerVariable from Lr_IntegerVariable Script
    int ValueForScale; //Value of scaleFactor from ScaleButtons.cs

    public Transform origin; // position set to GameObject  LR_GameObject_Start during runtime
    private static Vector3 destination1 = new Vector3(800f+x, 350f, 20f);    
    private static Vector3 destination2 = new Vector3(800f + x, 315f, 20f);
    private static  Vector3 destination3 = new Vector3(800f + x, 280f, 20f);
    private static Vector3 destination4 = new Vector3(800f + x, 245f, 20f);
    private static Vector3 destination5 = new Vector3(800f + x, 210f, 20f);
    private static Vector3 destination6 = new Vector3(800f + x, 175f, 20f);             // Setting Positions for Each of the Renderers
    private static Vector3 destination7 = new Vector3(800f + x, 140f, 20f);
    private static Vector3 destination8 = new Vector3(800f + x, 105f, 20f);
    private static Vector3 destination9 = new Vector3(800f + x, 70f, 20f);
    private static Vector3 destination10 = new Vector3(800f + x, 35f, 20f);
    private static Vector3 destination11 = new Vector3(800f + x, 0f, 20f);
    private static Vector3 destination12 = new Vector3(800f + x, -35f, 20f);
    private static Vector3 destination13 = new Vector3(800f + x,-70f, 20f);
    private static Vector3 destination14 = new Vector3(800f + x,-105f, 20f);
    private static Vector3 destination15 = new Vector3(800f + x,-140f, 20f);
    private static Vector3 destination16 = new Vector3(800f + x,-175f, 20f);
    private static Vector3 destination17 = new Vector3(800f + x,-210f, 20f);
    private static Vector3 destination18 = new Vector3(800f + x,-245f, 20f);
    private static Vector3 destination19 = new Vector3(800f + x,-280f, 20f);
    private static Vector3 destination20 = new Vector3(800f + x,-315f, 20f);


    private Vector3[] positionarray = new Vector3[20] { destination1, destination2 , destination3,destination4,destination5,destination6,destination7,destination8,destination9,destination10,destination11,destination12,destination13,destination14,destination15,destination16,destination17,destination18,destination19,destination20 };
    private Vector3[] Change = new Vector3[20]; // Change Vector equates the normalized values to the Y variable.

    public float lineDrawSpeed = 10f;
    private int scale;
    static double[,] NormalizedValues1 = new double[1500,20];
    private int scalefactor1;
    public Text scale_value;

    void Start()
    {
        scalefactor1 = 0;
        SetScaleFactor();
        for (int i = 0; i < 20; i++)
        {
           
            lines[i].SetPosition(0, origin.position); // Setting Initial Position for each of the renderers to the position of the GameObject Lr_Game0bject_Start
            lines[i].SetVertexCount(1500);
            lines[i].SetWidth(2f, 2f);
        }
    }

    // Update is called once per frame
   void Update()
    {
        SetScaleFactor();
        for (int k = 0; k < 20;k++)
        {
            electrodelines[k] = tcp.electrodes[k]; //I don't know why this line is throwing an error, it appears all the things it points to are functioning properly.  
            ValueForInteraction = this.lriv.IntegerVariable;
            scalefactor1 = this.lriv.ScaleFactor;
            newelectrodes[k] = this.tcp.DifferentElectrodes[k];
            
            for (int i = 0; i < x; i++)
            {
                NormalizedValues1[i,k] = this.tcp.normalizedvalues[k, i]; // Accessing NormalizedValues from the SendTCP script
                if (double.IsNaN(NormalizedValues1[i,k])) // Check Condition for Normalized values because as soon as EEG monitoring starts NormalizedValues is temporarily NaN
                {
                    NormalizedValues1[i,k] = 0;
                }
                else
                {
                    Change[k] = new Vector3(800f + (i / 1.6f), positionarray[k].y + ((float)NormalizedValues1[i, k] * 10 * scalefactor1), 20f); // Change = new position for Renderer
                    lines[k].SetPosition(i, Change[k]);
                    lines[k].SetWidth(1f, 1f); //This is where we set the line weight of the EEG trace. 
                    lines[k].SetColors(electrodelines[k].material.color, electrodelines[k].material.color); // Setting the colors of the renderers to the equivalent electrodes. Note: Sequence of Electrodes is important for correct referencing to respective Renderers
                    lines[ValueForInteraction - 1].SetWidth(3f, 3f); // ValueForInteraction is an integer value from Lr_IntegerVariable script which ranges from 1-20. Whenever ValueForInteraction Changes the respective renderer would increase in width 
                    newelectrodes[k].transform.localScale = new Vector3(1f, 1f, 1f);
                    newelectrodes[ValueForInteraction - 1].transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);// Whenever ValueForInteraction changes the respective electrode would grow in size
                }

            }

           
        }

    }

    void SetScaleFactor()
    {
        scale_value.text = "Scale: " + scalefactor1.ToString();
    }

}