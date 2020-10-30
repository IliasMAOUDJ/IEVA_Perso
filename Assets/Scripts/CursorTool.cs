using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System;
using ExitGames.Client.Photon;

namespace WasaaMP
{
    public class CursorTool : MonoBehaviourPun
    {
        float x, previousX = 0;
        float y, previousY = 0;
        float z, lastZ;
        public bool active;

        private bool caught;

        public Interactive interactiveObjectToInstanciate;
        private GameObject target;
        MonoBehaviourPun targetParent;
        MonoBehaviourPun player;

        void Start()
        {
            Cursor.visible = false;
            active = false;
            caught = false;
            player = (MonoBehaviourPun)this.GetComponentInParent(typeof(Navigation));
            name = player.name + "_" + name;
        }

        void Update()
        {
            // control of the 3D cursor
            /*
			float x = Input.mousePosition.x;
			float y = Input.mousePosition.y;
			Vector3 point = Camera.main.ScreenToWorldPoint(new Vector3(x, y, 1));
			transform.position = point;
			*/
            if (player.photonView.IsMine || !PhotonNetwork.IsConnected)
            {
                if (Input.GetButtonDown("Fire1"))
                {
                    Fire1Pressed(Input.mousePosition.x, Input.mousePosition.y);
                }
                if (Input.GetButtonUp("Fire1"))
                {
                    Fire1Released(Input.mousePosition.x, Input.mousePosition.y);
                }
                if (active)
                {
                    Fire1Moved(Input.mousePosition.x, Input.mousePosition.y, Input.mouseScrollDelta.y);
                }
                if (Input.GetKeyDown(KeyCode.C))
                {
                    CreateInteractiveCube();
                }
                if (Input.GetKeyDown(KeyCode.B))
                {
                    Catch();
                }
                if (Input.GetKeyDown(KeyCode.N))
                {
                    Release();
                    target = null;
                }

            }
        }

        public void Fire1Pressed(float mouseX, float mouseY)
        {
            active = true;
            x = mouseX;
            previousX = x;
            y = mouseY;
            previousY = y;
        }

        public void Fire1Released(float mouseX, float mouseY)
        {
            active = false;
        }

        public void Fire1Moved(float mouseX, float mouseY, float mouseZ)
        {
            x = mouseX;
            float deltaX = (x - previousX) / 100.0f;
            previousX = x;
            y = mouseY;
            float deltaY = (y - previousY) / 100.0f;
            previousY = y;
            float deltaZ = mouseZ / 10.0f;
            transform.Translate(deltaX, deltaY, deltaZ);

        }

        // create a dictionary to store the target (key) and its parent (value) in order to find which object is original parent when release object
        // this allows to place the ghost to its original place after releasing
        Dictionary<GameObject, Tuple<GameObject, Vector3, Quaternion>> StoreOrigins = new Dictionary<GameObject, Tuple<GameObject, Vector3, Quaternion>>();
        public void Catch()
        {
            print("B ?");
            if (target != null)
            {
                print("B :");
                var tb = target.GetComponent<Interactive>();
                var originalParent = target.transform.parent;
                if (!StoreOrigins.ContainsKey(target))
                {
                    StoreOrigins.Add(target, new Tuple<GameObject, Vector3, Quaternion>(originalParent.gameObject, target.transform.localPosition, target.transform.localRotation));
                }
                if (tb != null)
                {
                    if (!caught)
                    {
                        if ((this != tb.GetSupport()))
                        { // pour ne pas prendre 2 fois l'objet et lui faire perdre son parent
                            targetParent = tb.GetSupport();
                            photonView.RPC("AddTotalForce", RpcTarget.All, 20, PhotonView.Get(originalParent).ViewID);
                            photonView.RPC("UpdateLastChest", RpcTarget.All, PhotonView.Get(originalParent).ViewID);
                            photonView.RPC("NbOfHandlesCaught", RpcTarget.All, 1, PhotonView.Get(originalParent).ViewID);
                        }
                        photonView.RPC("ChangeSupport", RpcTarget.All, tb.photonView.ViewID, photonView.ViewID);
                        tb.photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
                        caught = true;
                        print("B !");
                        PhotonNetwork.SendAllOutgoingCommands();
                    }
                }
            }
            else
            {
                print("pas B");
            }
        }

        public void Release()
        {
            if (target != null)
            {
                print("N :");
                var tb = target.GetComponent<Interactive>();
                if (tb != null)
                {
                    if (caught)
                    {
                        if (targetParent != null)
                        {
                            photonView.RPC("ChangeSupport", RpcTarget.All, tb.photonView.ViewID, targetParent.photonView.ViewID);
                            targetParent = null;
                        }
                        else
                        {
                            photonView.RPC("RemoveSupport", RpcTarget.All, tb.photonView.ViewID);
                        }

                        print("N !");

                        Tuple<GameObject, Vector3, Quaternion> origins;
                        StoreOrigins.TryGetValue(target, out origins); // returns Item1: original parent, Item2: LocalPosition (relative to originalParent), Item3: LocalRotation (relative to originalParent)
                        photonView.RPC("NbOfHandlesCaught", RpcTarget.All, -1, PhotonView.Get(origins.Item1).ViewID);
                        tb.photonView.transform.parent = origins.Item1.transform;
                        tb.photonView.transform.localPosition = origins.Item2;
                        tb.photonView.transform.localRotation = origins.Item3;
                        photonView.RPC("SubTotalForce", RpcTarget.All, 20, PhotonView.Get(origins.Item1).ViewID);

                    }

                    caught = false;
                }
            }
            else
            {
                print("pas N");
            }
        }

        public void CreateInteractiveCube()
        {
            var objectToInstanciate = PhotonNetwork.Instantiate(interactiveObjectToInstanciate.name, transform.position, transform.rotation, 0);
        }

        void OnTriggerEnter(Collider other)
        {
            print(name + " : OnTriggerEnter");
            target = other.gameObject;
        }

        void OnTriggerExit(Collider other)
        {
            print(name + " : OnTriggerExit");
            target = null;
        }

        [PunRPC]
        public void ChangeSupport(int interactiveID, int newSupportID)
        {
            Interactive go = PhotonView.Find(interactiveID).gameObject.GetComponent<Interactive>();
            MonoBehaviourPun s = PhotonView.Find(newSupportID).gameObject.GetComponent<MonoBehaviourPun>();
            print("ChangeSupport of object " + go.name + " to " + s.name);
            go.SetSupport(s);
        }

        [PunRPC]
        public void RemoveSupport(int interactiveID)
        {
            Interactive go = PhotonView.Find(interactiveID).gameObject.GetComponent<Interactive>();
            print("RemoveSupport of object " + go.name);
            go.RemoveSupport();
        }

        [PunRPC]
        public void AddTotalForce(int force, int chestViewID)
        {
            ChestScript chest = PhotonView.Find(chestViewID).gameObject.GetComponent<ChestScript>();
            chest.AddTotalForce(force);
        }

        [PunRPC]
        public void SubTotalForce(int force, int chestViewID)
        {
            ChestScript chest = PhotonView.Find(chestViewID).gameObject.GetComponent<ChestScript>();
            chest.SubTotalForce(force);
        }


        public static GameObject lastChest;

        [PunRPC]

        public void UpdateLastChest(int chestViewID)
        {
            GameObject chest = PhotonView.Find(chestViewID).gameObject;
            lastChest = chest;
        }

        [PunRPC]

        public void NbOfHandlesCaught(int number, int chestViewID)
        {
            ChestScript chest = PhotonView.Find(chestViewID).gameObject.GetComponent<ChestScript>();
            chest.setNumberOfHandlesCaught(chest.getNumberOfHandlesCaught() + number);
        }
    }
}