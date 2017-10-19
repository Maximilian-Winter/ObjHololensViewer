using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

#if NETFX_CORE
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
#endif

public class FileImportManager : MonoBehaviour
{
    public string basepath = @"C:\test\";

    public Text loadingUIText;
    private Button loadFileButton;
    public GameObject fileListUIPanel;
    public GameObject fileListScrollViewContentPlane;
    public GameObject fileListEntryPrefab;
    public GameObject loadFileButtonUIGO;

    private List<string> appDataFilePaths;
    private string fileNameWithoutExtension;
    public string FileNameWithoutExtension
    {
        get
        {
            return fileNameWithoutExtension;
        }
        set
        {
            fileNameWithoutExtension = value;
        }
    }

    private bool fileListLoaded = false;

    public delegate void OnLoadFileDelegate(string filePath);
    public event OnLoadFileDelegate OnLoadFile;

    private ModelImport modelImport;

    // Use this for initialization
    void Start()
    {
        loadFileButton = loadFileButtonUIGO.GetComponent<Button>();
        loadFileButton.onClick.AddListener(OnClickLoadFile);
        fileNameWithoutExtension = "";
        appDataFilePaths = new List<string>();
    }

    private void Awake()
    {
        modelImport = GameObject.Find("SceneManager").GetComponent<ModelImport>();
        modelImport.OnFileIsLoaded += DisableLoadingUIText;
    }

    private void OnDisable()
    {
        modelImport.OnFileIsLoaded -= DisableLoadingUIText;
    }

    // Update is called once per frame
    void Update()
    {
        if (fileListLoaded)
        {
            fileListUIPanel.SetActive(true);
            foreach (string fileName in appDataFilePaths)
            {
                GameObject listEntry = Instantiate(fileListEntryPrefab);
                listEntry.transform.SetParent(fileListScrollViewContentPlane.transform, false);
                ListEntry entry = listEntry.GetComponent<ListEntry>();
                // Subsribe to ListEntry event OnClick
                entry.OnClickListEntry += LoadModel;
                entry.SetLoadButtonFileName(fileName);
            }
            fileListLoaded = false;
        }
    }

    public void LoadModel(string fileName)
    {
        loadingUIText.enabled = true;
        loadingUIText.text = "Loading... ";

        fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        OnLoadFile(fileName);
        loadFileButtonUIGO.SetActive(false);

        // Unsubsribe from ListEntry event OnClick
        ListEntry[] entrys = GetComponents<ListEntry>();
        foreach (ListEntry entry in entrys)
        {
            entry.OnClickListEntry -= LoadModel;
        }

        DisableFileListUIPanel();
    }

#if NETFX_CORE
            private void PickHandler(IReadOnlyList<StorageFile> files)
            {
                string objfilePath = "";
                string mtlfilePath = "";
                if (files != null)
                {
                    foreach (IStorageItem item in files)
                    {
                        string fileExtension = Path.GetExtension(item.Path);
                        Debug.Log(item.Path);
                        if (fileExtension == ".obj")
                        {
                            objfilePath = item.Path;
                        }
                        else if (fileExtension == ".mtl")
                        {
                            mtlfilePath = item.Path;
                        }
                    }
                    if (objfilePath != "")
                    {
                        LoadModel(objfilePath);
                    }
                    else
                    {
                        string msg = "";
                        if (objfilePath != "")
                        {
                            msg += "No model file (.obj) selected. ";
                        }
                        if (mtlfilePath != "")
                        {
                            msg += "No material file (.mtl) selected.";
                        }
                        //UserMessage(msg);
                    }
                }
                else
                {
                    //UserMessage("No files selected");
                }
            }

            public void PickAndLoadModel(Action<IReadOnlyList<StorageFile>> selectAction)
            {
                UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
                {
                    FileOpenPicker thePicker = new FileOpenPicker();
                    thePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
                    thePicker.ViewMode = PickerViewMode.List;
                    thePicker.CommitButtonText = "Load File";
                    thePicker.FileTypeFilter.Add(".obj");
                    thePicker.FileTypeFilter.Add(".mtl");
                    IReadOnlyList<StorageFile> files = await thePicker.PickMultipleFilesAsync();
                    UnityEngine.WSA.Application.InvokeOnAppThread(
                        () => { selectAction(files); }, true);
                }, false);
            }

            public void GetStorageFolderContent(string path)
            {
                Task task = new Task(
                 async () =>
                 {
                     StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(path);
                     IReadOnlyList<StorageFile> sortedItems = await folder.GetFilesAsync();
                     foreach (StorageFile file in sortedItems)
                     {
                         if (Path.GetExtension(file.Path) == ".obj")
                         {
                             appDataFilePaths.Add(file.Path);
                         }
                     }
                 });
                task.Start();
                task.Wait();
            }
#endif

    public void GetHardDriveFolderContent(string path)
    {
        string[] filePaths = Directory.GetFiles(@basepath, "*.obj");
        foreach (string file in filePaths)
        {
            appDataFilePaths.Add(file);
        }
        fileListLoaded = true;
    }

    void OnClickLoadFile()
    {
#if NETFX_CORE
                GetStorageFolderContent(ApplicationData.Current.RoamingFolder.Path);
#else
        GetHardDriveFolderContent(@basepath);
#endif
    }

    public void DisableLoadFileUIButton()
    {
        loadFileButton.enabled = false;
    }

    public void EnableLoadFileUIButton()
    {
        loadFileButton.enabled = true;
    }

    public void DisableFileListUIPanel()
    {
        fileListUIPanel.SetActive(false);
    }

    public void EnableFileListUIPanel()
    {
        fileListUIPanel.SetActive(true);
    }

    public void EnableLoadingUIText()
    {
        loadingUIText.enabled = true;
    }

    public void DisableLoadingUIText()
    {
        loadingUIText.enabled = false;
    }
}
