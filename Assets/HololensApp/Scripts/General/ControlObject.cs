using UnityEngine;
using System.Collections;

//using HoloToolkit.Unity;

public class ControlObject : MonoBehaviour {

    //private WorldAnchorManager worldAnchorManager;

    [SerializeField]
    private bool isWorldAnchored;
    private Vector3 originalScale;
    private Quaternion tempRotation;
    private Vector3 tempPosition;
    private bool changed;
    
    // Use this for initialization
    void Awake ()
    {
        //worldAnchorManager = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<WorldAnchorManager>(); // Add anchor
        isWorldAnchored = false;
        changed = false;
    }

    public void AttachWorldAnchor()
    {
        if(!isWorldAnchored)
        {
            //worldAnchorManager.AttachAnchor(gameObject, gameObject.name);
            gameObject.AddComponent<UnityEngine.XR.WSA.WorldAnchor>();
            isWorldAnchored = true;
        }
    }

    public void RemoveWorldAnchor()
    {
        if (isWorldAnchored)
        {
            //worldAnchorManager.RemoveAnchor(gameObject);
            DestroyImmediate(gameObject.GetComponent<UnityEngine.XR.WSA.WorldAnchor>());
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

    public void ScaleObject(float factor)
    {
        transform.localScale = originalScale * factor;
    }

    public void SetScale(float scale)
    {
        originalScale = new Vector3(scale, scale, scale);
        transform.localScale = originalScale;
    }

    public void HideObject()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        renderer.enabled = false;
    }

    public void ShowObject()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        renderer.enabled = true;
    }
}
