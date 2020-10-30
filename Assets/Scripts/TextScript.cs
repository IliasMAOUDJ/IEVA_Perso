using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using WasaaMP;

public class TextScript : MonoBehaviour
{

    public TMP_Text force;
    public GameObject chest; 
    // Start is called before the first frame update
    public int GetValue()
    {
        return chest.GetComponent<ChestScript>().getTotalForce();
    }


    // Update is called once per frame
    void Update()
    {
        chest = CursorTool.lastChest;
        if (chest)
            force.text = "Force Applied: " + GetValue() + "/" + chest.GetComponent<Rigidbody>().mass;
            
    }
}
