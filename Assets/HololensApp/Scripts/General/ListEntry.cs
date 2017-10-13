using HololensApp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ListEntry : MonoBehaviour
{
    public Button loadElementButton;
    private string filePath;
    public string FilePath
    {
        get
        {
            return filePath;
        }
        set
        {
            filePath = value;
        }
    }

    public delegate void OnClickListEntryDelegate(string filePath);
    public event OnClickListEntryDelegate OnClickListEntry;

    // Use this for initialization
    void Start ()
    {
        loadElementButton.onClick.AddListener(OnClickLoad);
    }

    public void SetLoadButtonFileName(string filePath)
    {
        this.filePath = filePath;
        loadElementButton.GetComponentInChildren<Text>().text = Path.GetFileName(filePath);
    }

    void OnClickLoad()
    {
        OnClickListEntry(this.filePath);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
