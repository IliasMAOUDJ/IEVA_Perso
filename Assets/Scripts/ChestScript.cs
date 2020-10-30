using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WasaaMP;

public class ChestScript : MonoBehaviourPun
{
    public GameObject chest;
    public GameObject leftHandleGhost;
    public GameObject rightHandleGhost;
    public bool canCarryChest = false;
    private int totalForce = 0;
    private int numberOfHandlesCaught = 0;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        CheckChestState();
        if (canCarryChest)
            ComputeChestPosition(PhotonView.Get(leftHandleGhost), PhotonView.Get(rightHandleGhost));
    }

    public void CheckChestState()
    {

        if (totalForce > chest.GetComponent<Rigidbody>().mass && (numberOfHandlesCaught == 2))
        {
            chest.GetComponentInChildren<Renderer>().material.color = Color.green;
            canCarryChest = true;
        }
        else if (totalForce > 0)
        {
            chest.GetComponentInChildren<Renderer>().material.color = Color.yellow;
            canCarryChest = false;
        }
        else
        {
            chest.GetComponentInChildren<Renderer>().material.color = Color.red;
            canCarryChest = false;
        }
    }

    [PunRPC]
    public void ComputeChestPosition(PhotonView LeftHandle, PhotonView RightHandle)
    {
        var x = (LeftHandle.transform.position.x + RightHandle.transform.position.x) / 2.0f;
        var y = (LeftHandle.transform.position.y + RightHandle.transform.position.y) / 2.0f - 0.3f;
        var z = (LeftHandle.transform.position.z + RightHandle.transform.position.z) / 2.0f;
        var rot = Quaternion.Slerp(LeftHandle.transform.rotation, RightHandle.transform.rotation, 0.5f);
        rot = Quaternion.Euler(new Vector3(rot.eulerAngles.x, rot.eulerAngles.y, 90f));
        chest.transform.SetPositionAndRotation(new Vector3(x, y, z), rot);
    }


    public int getTotalForce()
    {
        return totalForce;
    }

    public void AddTotalForce(int force)
    {
        totalForce += force;
    }

    public void SubTotalForce(int force)
    {
        totalForce -= force;
    }

    public int getNumberOfHandlesCaught()
    {
        return numberOfHandlesCaught;
    }

    public void setNumberOfHandlesCaught(int n)
    {
        numberOfHandlesCaught = n;
    }

}



