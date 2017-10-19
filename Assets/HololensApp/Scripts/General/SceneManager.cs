//using HoloToolkit.Unity;
//using HoloToolkit.Unity.InputModule;
//using HoloToolkit.Unity.SpatialMapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;


namespace HololensApp
{
    public class SceneManager : MonoBehaviour
    {
        public bool isBeingPlaced { get; set; }

        private void Awake()
        {
            //gameObject.GetComponent<WorldAnchorManager>();
        }

        // Use this for initialization
        void Start()
        {
            //InputManager.Instance.PushFallbackInputHandler(gameObject); 
        }

        void Update()
        {
            // If the user is in placing mode,
            // update the placement to match the user's gaze.
            if (isBeingPlaced)
            {
               /* // Do a raycast into the world that will only hit the Spatial Mapping mesh.
                Vector3 headPosition = Camera.main.transform.position;
                Vector3 gazeDirection = Camera.main.transform.forward;

                RaycastHit hitInfo;
                if (Physics.Raycast(headPosition, gazeDirection, out hitInfo, 30.0f, GameObject.FindGameObjectWithTag("SpatialMapping").GetComponent<SpatialMappingManager>().LayerMask))
                {
                    // Rotate this object to face the user.
                    Quaternion toQuat = Camera.main.transform.localRotation;
                    toQuat.x = 0;
                    toQuat.z = 0;

                    // Move this object to where the raycast
                    // hit the Spatial Mapping mesh.
                    // Here is where you might consider adding intelligence
                    // to how the object is placed.  For example, consider
                    // placing based on the bottom of the object's
                    // collider so it sits properly on surfaces.
                    ControlObject model = GameObject.Find("").GetComponent<ControlObject>();
                    model.transform.position = hitInfo.point;
                    model.transform.rotation = toQuat;
                }*/
            }
        }
    }
}