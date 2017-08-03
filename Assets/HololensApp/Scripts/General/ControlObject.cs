using UnityEngine;
using System.Collections;
using UnityEngine.VR.WSA;
using HoloToolkit.Unity;

public class ControlObject : MonoBehaviour {
    private WorldAnchorManager worldAnchorManager;

    [SerializeField]
    private bool isWorldAnchored;

    private Quaternion tempRotation;
    private Vector3 tempPosition;
    private bool changed;
    
    // Use this for initialization
    void Awake ()
    {
        worldAnchorManager = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<WorldAnchorManager>(); // Add anchor
        isWorldAnchored = false;
        changed = false;
    }

    public void AttachWorldAnchor()
    {
        if(!isWorldAnchored)
        {
            //worldAnchorManager.AttachAnchor(gameObject, gameObject.name);
            gameObject.AddComponent<WorldAnchor>();
            isWorldAnchored = true;
        }
    }

    public void RemoveWorldAnchor()
    {
        if (isWorldAnchored)
        {
            //worldAnchorManager.RemoveAnchor(gameObject);
            DestroyImmediate(gameObject.GetComponent<WorldAnchor>());
            isWorldAnchored = false;
        }
    }

    // Update is called once per frame
    void Update ()
    {
        if(changed && !isWorldAnchored)
        {
            transform.SetPositionAndRotation(tempPosition, tempRotation);
            AttachWorldAnchor();
            changed = false;
        }
    }

    public void TranslateObject(Vector3 trans)
    {
        RemoveWorldAnchor();
        tempPosition = transform.position + trans;
        tempRotation = transform.rotation;
        changed = true;
    }

    public void RotateObject(Vector3 rot)
    {
        RemoveWorldAnchor();
        tempRotation = Quaternion.Euler(rot) * transform.rotation;
        tempPosition = transform.position;
        changed = true;
    }

    public void ScaleObject(float scale)
    {
        transform.localScale = new Vector3(0.001f, 0.001f, 0.001f) * scale;
    }
}
