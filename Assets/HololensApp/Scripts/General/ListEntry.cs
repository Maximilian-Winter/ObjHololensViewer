using HololensApp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ListEntry : MonoBehaviour {

    private SceneManager sceneManager;
    public Button loadElementButton;
    public string filePath { get; set; }
    // Use this for initialization
    void Start ()
    {
        loadElementButton.onClick.AddListener(OnClickLoad);
        sceneManager = GameObject.Find("SceneManager").GetComponent<SceneManager>();
    }

    public void SetLoadButtonFileName(string filePath)
    {
        this.filePath = filePath;
        loadElementButton.GetComponentInChildren<Text>().text = Path.GetFileName(filePath);
    }

    void OnClickLoad()
    {
        sceneManager.LoadModel(filePath);
        sceneManager.DisableFileListUIPanel();
    }

    // Update is called once per frame
    void Update () {
		
	}
}
