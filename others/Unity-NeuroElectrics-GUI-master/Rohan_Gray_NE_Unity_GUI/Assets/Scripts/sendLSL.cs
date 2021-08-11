using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using LSL;

class sendLSL : MonoBehaviour
{
    public Button b_quality = null;

    private const string source_id = "Unity";
    private const string outlet_id = "NIC_unity";

    //private const int num_channels = 8;
    private const int marker = 10;


    private static liblsl.StreamOutlet outlet = null;

    /*public void PushMarker(GameObject obj) {
		if (outlet == null) return;
		
		int[] data = new int[]{marker};
		Debug.Log ("PushMarker " + data[0].ToString());
		outlet.push_sample(data);
	}*/

    public void PushMarker()
    {
        if (outlet == null) return;

        int[] data = new int[] { marker };
        Debug.Log("PushMarker " + data[0].ToString());
        outlet.push_sample(data);
    }


    public void PullQuality()
    {
        if (outlet == null) return;

        int num_channels = 20;   //openNIC.getNumChannels();
        if (sendTCP.monitoring)
        {
            liblsl.StreamInfo[] results = liblsl.resolve_stream("type", "Quality");

            int k;
            bool found = false;
            for (k = 0; !found && k < results.Length; ++k)
            {
                //Debug.Log("results[" + i + "]: " + results[i].name());
                if (results[k].name().Equals(outlet_id))
                    found = true;
            }

            if (!found)
            {
                Debug.Log("Could not connect to NIC. Please, check the Outlet for Lab Streaming Layer");
                return;
            }

            liblsl.StreamInlet inlet2 = new liblsl.StreamInlet(results[k - 1]);
            float[] sample = new float[num_channels];

            //Debug.Log("PULL SAMPLE");
            inlet2.pull_sample(sample);
            Debug.Log("Quality: ");
            for (int i = 0; i < sample.Length; ++i)
                Debug.Log("# Ch" + (i + 1) + ": " + sample[i]);

            if (sample.Length > 0)
            {
                if (sample[0] <= 0.5F)
                    b_quality.GetComponent<Image>().color = Color.green;
                else if (sample[0] <= 0.8F)
                    b_quality.GetComponent<Image>().color = new Color(1.0F, 0.5F, 0.0F);
                else
                    b_quality.GetComponent<Image>().color = Color.red;
                //button_quality.GetComponent<UIButton>().isEnabled = true;
            }
        }
        else
        {
            Debug.Log("Need to start monitoring EEG");
            //button_quality.GetComponent<UIWidget>().color = Color.gray;
        }
    }


    // Use this for initialization
    void Start()
    {
        liblsl.StreamInfo info = new liblsl.StreamInfo(source_id, "Markers", 1, 0, liblsl.channel_format_t.cf_int32, "myuniquesourceid123456");
        outlet = new liblsl.StreamOutlet(info);

        Debug.Log("[LSL] initialized!");
    }

}