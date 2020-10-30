using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using WasaaMP;

public class InstructionScript : MonoBehaviour
{

    public TMP_Text instruction;
    public GameObject chest;

    public int GetValue()
    {
        return chest.GetComponent<ChestScript>().getTotalForce();
    }

    public void InstructionType()
    {
        chest = CursorTool.lastChest;
        if (chest)
        {
            var chestScript = chest.GetComponent<ChestScript>();
            if (chestScript.canCarryChest)
            {
                instruction.text = "You can move the chest";
            }
            else if (chestScript.getNumberOfHandlesCaught() == 2 && !chestScript.canCarryChest)
            {
                instruction.text = "This chest is too heavy !";
            }
            else if (chestScript.getNumberOfHandlesCaught() > 0 && !chestScript.canCarryChest)
            {
                instruction.text = "Waiting for a player to catch the other handle";
            }
            else
            {
                instruction.text = "";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        InstructionType();
    }
}
