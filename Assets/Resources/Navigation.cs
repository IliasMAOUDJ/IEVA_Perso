using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


namespace WasaaMP {
    public class Navigation : MonoBehaviourPunCallbacks {
     
        #region Public Fields

        // to be able to manage the offset of the camera
        public Vector3 cameraPositionOffset = new Vector3 (0, 1.6f, 0) ;
        public Quaternion cameraOrientationOffset = new Quaternion () ;
        Camera theCamera;
        [Tooltip("The local player instance. Use this to know if the local player is represented in the Scene")]
        public static GameObject LocalPlayerInstance;

        #endregion
        void Awake () {
            // #Important
            // used in GameManager.cs: we keep track of the localPlayer instance to prevent instantiation when levels are synchronized
            if (photonView.IsMine) {
                LocalPlayerInstance = this.gameObject;
            }
            // #Critical
            // we flag as don't destroy on load so that instance survives level synchronization, thus giving a seamless experience when levels load.
            //DontDestroyOnLoad (this.gameObject) ;
        }

        Transform cameraTransform;
        // Start is called before the first frame update
        void Start () {
            if (photonView.IsMine) {// || ! PhotonNetwork.IsConnected) {
                // attach the camera to the navigation rig
                theCamera = (Camera)GameObject.FindObjectOfType (typeof(Camera)) ;
                cameraTransform = theCamera.transform ;
                cameraTransform.SetParent (transform) ;
                cameraTransform.localPosition = cameraPositionOffset ;
                cameraTransform.localRotation = cameraOrientationOffset ;
            }
        }

        // Update is called once per frame
        void Update () {
            if (photonView.IsMine) {// || ! PhotonNetwork.IsConnected) {
                var x = Input.GetAxis ("Horizontal") * Time.deltaTime * 6.0f ;
                var z = Input.GetAxis("Vertical") * Time.deltaTime * 6.0f ;
                var rot = Time.deltaTime * 150.0f;

                transform.Translate(new Vector3(x, 0, z));
                
                //handle watching right and left
                if (Input.GetKey("f"))
                {
                    transform.Rotate(new Vector3(0, -rot, 0), Space.World);
                }

                if (Input.GetKey("h"))
                {
                    transform.Rotate(new Vector3(0, rot, 0), Space.World);
                }

                // handle watching above and below               
                if (Input.GetKey("t"))
                {
                    cameraTransform.transform.Rotate(new Vector3(-rot, 0, 0), Space.Self);
                }

                if (Input.GetKey("g"))
                {
                    cameraTransform.transform.Rotate(new Vector3(rot, 0, 0), Space.Self);
                }
                
            }

        }

    }

}