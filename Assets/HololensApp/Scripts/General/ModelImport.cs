using HololensApp;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class ModelImport : MonoBehaviour
{
   /* private float scale = 0.001f;
    public float Scale
    {
        get
        {
            return scale;
        }
        set
        {
            scale = value;
        }
    }*/

    public Shader modelShader;
    private SmartObjImporter importer;

    public delegate void OnFileIsLoadedDelegate();
    public event OnFileIsLoadedDelegate OnFileIsLoaded;

    private FileImportManager fileImportManager;

    void Start()
    {
        importer = new SmartObjImporter();
    }
 
    private void Awake()
    {
        fileImportManager = GameObject.Find("SceneManager").GetComponent<FileImportManager>();
        fileImportManager.OnLoadFile += LoadModel;
    }

    private void OnDisable()
    {
        fileImportManager.OnLoadFile -= LoadModel;
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
        importer.sceneGameObject.SetActive(true);
        importer.sceneGameObject.GetComponent<ControlObject>().AttachWorldAnchor();
        OnFileIsLoaded();
        yield return null;
    }
}
