using UnityEngine;
using System.Collections;

public class ScaleButtons : MonoBehaviour {

    public int ScaleFactor = 1;

    public void ArrowUpClicked() // Function attached to ArrowUp button: the IntegerVariable value decreases by 1 and from the LineRenderer Script the previous Renderer in series gets higHlighted with the corresponfing electrodes
    {
        if(ScaleFactor < 21 && ScaleFactor > 1)
        {
            ScaleFactor = ScaleFactor - 1;
        }
    
    }
    public void ArrowDownClicked() // Function attached to ArrowDown button: The next renderer in series gets highlighted with corresponding electrode
    {
        if(ScaleFactor > 0 && ScaleFactor < 20)
        {
        ScaleFactor = ScaleFactor + 1;
        }
    }

    
}
