using HololensApp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ModelImport : MonoBehaviour
{
    public float scale = 0.001f;
    public bool isLoaded { get; set; }
    
    public Shader modelShader;
    private SmartObjImporter importer;

    private SceneManager sceneManager;

    void Start()
    {
        importer = new SmartObjImporter();
        sceneManager = GameObject.Find("SceneManager").GetComponent<SceneManager>();

        isLoaded = false;
    }

    public void LoadModel(string filename)
    {
        importer.InitImporter(filename, modelShader);
        importer.sceneGameObject.SetActive(false);

        StartCoroutine(LoadNewModel());
    }

    private IEnumerator LoadNewModel()
    {
        yield return importer.ImportFile();
        importer.sceneGameObject.transform.localPosition = Camera.main.transform.position + new Vector3(0.0f, 0.0f, -0.5f);
        importer.sceneGameObject.transform.localScale = new Vector3(scale, scale, scale);
        importer.sceneGameObject.SetActive(true);
        importer.sceneGameObject.GetComponent<ControlObject>().AttachWorldAnchor();

        sceneManager.EnableObjectControlUIPanel();
        isLoaded = true;
        yield return null;
    }
}
