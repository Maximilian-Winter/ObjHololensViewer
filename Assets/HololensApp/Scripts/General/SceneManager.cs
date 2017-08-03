using HoloToolkit.Unity;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.SpatialMapping;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

#if NETFX_CORE
using Windows.Storage;
using Windows.Storage.Pickers;
using System.Threading.Tasks;
#endif
namespace HololensApp
{
    public class SceneManager : MonoBehaviour, IInputClickHandler
    {
        private bool buttonPressed;
        public string basepath = @"C:\test\";
        public ModelImport modelImporter;
        public Text loadingUIText;
        public Text scaleObjectUIText;
        public Text speedObjectUIText;
        public GameObject fileListUIPanel;
        public GameObject fileListScrollViewContentPlane;
        public GameObject fileListEntryPrefab;
        public GameObject objectControlUIPanel;

        bool isLoaded = false;

        public GameObject loadFileButtonUIGO;
        private Button loadFileButton;

        public Button moveForwardButton;
        public Button moveBackwardButton;
        public Button moveLeftButton;
        public Button moveRightButton;
        public Button moveUpButton;
        public Button moveDownButton;
        public Button rotateLeftButton;
        public Button rotateRightButton;
        public Button speedLarger;
        public Button speedSmaller;
        public Button scaleLarger;
        public Button scaleSmaller;

        private List<string> appDataFilePaths;
        private float scale;
        private string fileNameWithoutExtension;

        // Use the delta time to calculate a fps dependent speed modifier.
        float speedModifier;
        private float speedModScale;

        public bool isBeingPlaced { get; set; }

        private void Awake()
        {
            gameObject.GetComponent<WorldAnchorManager>();
        }

        // Use this for initialization
        void Start()
        {
            InputManager.Instance.PushFallbackInputHandler(gameObject);
            buttonPressed = false;
            speedModifier = 1.0f;
            speedModScale = 3.0f;
            scale = 1.0f;
            fileNameWithoutExtension = "";
            loadFileButton = loadFileButtonUIGO.GetComponent<Button>();

            appDataFilePaths = new List<string>();
            loadFileButton.onClick.AddListener(OnClickLoadFile);
            moveForwardButton.onClick.AddListener(OnClickMoveForward);
            moveBackwardButton.onClick.AddListener(OnClickMoveBackward);
            moveLeftButton.onClick.AddListener(OnClickMoveLeft);
            moveRightButton.onClick.AddListener(OnClickMoveRight);
            moveUpButton.onClick.AddListener(OnClickMoveUp);
            moveDownButton.onClick.AddListener(OnClickMoveDown);
            rotateLeftButton.onClick.AddListener(OnClickRotateLeft);
            rotateRightButton.onClick.AddListener(OnClickRotateRight);
            speedLarger.onClick.AddListener(OnClickSpeedLarger);
            speedSmaller.onClick.AddListener(OnClickSpeedSmaller);
            scaleLarger.onClick.AddListener(OnClickScaleLarger);
            scaleSmaller.onClick.AddListener(OnClickScaleSmaller);
        }

        void Update()
        {
            if(buttonPressed)
            {
                buttonPressed = false;
            }

            // If the user is in placing mode,
            // update the placement to match the user's gaze.
            if (isBeingPlaced)
            {
                // Do a raycast into the world that will only hit the Spatial Mapping mesh.
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
                    ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                    model.transform.position = hitInfo.point;
                    model.transform.rotation = toQuat;
                }
            }

            if (isLoaded)
            {
                fileListUIPanel.SetActive(true);
                foreach (string fileName in appDataFilePaths)
                {
                    GameObject listEntry = Instantiate(fileListEntryPrefab);
                    listEntry.transform.SetParent(fileListScrollViewContentPlane.transform, false);
                    ListEntry entry = listEntry.GetComponent<ListEntry>();
                    entry.SetLoadButtonFileName(fileName);
                }
                isLoaded = false;
            }

            if (modelImporter.isLoaded)
            {
                loadingUIText.enabled = false;
            }
        }

        public virtual void OnInputClicked(InputClickedEventData eventData)
        {
            // On each tap gesture, toggle whether the user is in placing mode.
            isBeingPlaced = !isBeingPlaced;

            // If the user is in placing mode, display the spatial mapping mesh.
            if (isBeingPlaced)
            {
                GameObject.FindGameObjectWithTag("SpatialMapping").GetComponent<SpatialMappingManager>().DrawVisualMeshes = true;
                ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                model.RemoveWorldAnchor();
            }
            // If the user is not in placing mode, hide the spatial mapping mesh.
            else
            {
                GameObject.FindGameObjectWithTag("SpatialMapping").GetComponent<SpatialMappingManager>().DrawVisualMeshes = false;
                ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                model.AttachWorldAnchor();
            }
        }

        public void LoadModel(string fileName)
        {
            loadingUIText.enabled = true;
            loadingUIText.text = "Loading... ";

            fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

            modelImporter.LoadModel(fileName);
            loadFileButtonUIGO.SetActive(false);
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
                     isLoaded = true;
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
            isLoaded = true;
        }

        void OnClickLoadFile()
        {
            #if NETFX_CORE
                GetStorageFolderContent(ApplicationData.Current.RoamingFolder.Path);
            #else
                GetHardDriveFolderContent(@basepath);
            #endif
        }

        void OnClickMoveForward()
        {
            if(!buttonPressed)
            {
                ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                model.TranslateObject((Vector3.forward * 0.01f) * speedModScale);
                buttonPressed = true;
            } 
        }

        void OnClickMoveBackward()
        {
            if (!buttonPressed)
            {
                ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                model.TranslateObject((Vector3.back * 0.01f) * speedModScale);
                buttonPressed = true;
            }
        }

        void OnClickMoveLeft()
        {
            if (!buttonPressed)
            {
                ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                model.TranslateObject((Vector3.left * 0.01f) * speedModScale);
                buttonPressed = true;
            }
        }

        void OnClickMoveRight()
        {
            if (!buttonPressed)
            {
                ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                model.TranslateObject((Vector3.right * 0.01f) * speedModScale);
                buttonPressed = true;
            } 
        }

        void OnClickMoveUp()
        {
            if (!buttonPressed)
            {
                ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                model.TranslateObject((Vector3.up * 0.01f) * speedModScale);
                buttonPressed = true;
            }
        }

        void OnClickMoveDown()
        {
            if (!buttonPressed)
            {
                ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                model.TranslateObject((Vector3.down * 0.01f) * speedModScale);
                buttonPressed = true;
            }
        }

        void OnClickRotateLeft()
        {
            if (!buttonPressed)
            {
                ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                model.RotateObject((Vector3.up * 1.0f) * speedModScale);
                buttonPressed = true;
            }
        }

        void OnClickRotateRight()
        {
            if (!buttonPressed)
            {
                ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                model.RotateObject((Vector3.down * 1.0f) * speedModScale);
                buttonPressed = true;
            }
        }

        void OnClickSpeedLarger()
        {
            if (!buttonPressed)
            {
                if (speedModScale < 100000)
                {
                    if (speedModScale < 1)
                        speedModScale += 0.25f;
                    else
                        speedModScale += 0.50f;
                    speedObjectUIText.text = "Speed: " + speedModScale;
                    buttonPressed = true;
                }
            } 
        }

        void OnClickSpeedSmaller()
        {
            if (!buttonPressed)
            {
                if (speedModScale > 0)
                {
                    if (speedModScale <= 1)
                        speedModScale -= 0.25f;
                    else
                        speedModScale -= 0.50f;
                    speedObjectUIText.text = "Speed: " + speedModScale;
                    buttonPressed = true;
                }
            }
        }

        void OnClickScaleLarger()
        {
            if (!buttonPressed)
            {
                if (scale < 100000)
                {
                    scale += (0.05f * speedModScale);
                    scaleObjectUIText.text = "Scale: " + scale;
                    ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                    model.ScaleObject(scale);
                    buttonPressed = true;
                }
            }
        }

        void OnClickScaleSmaller()
        {
            if (!buttonPressed)
            {
                if (scale > 0)
                {
                    scale -= (0.05f * speedModScale);
                    scaleObjectUIText.text = "Scale: " + scale;
                    ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                    model.ScaleObject(scale);
                    buttonPressed = true;
                }
            }
            
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

        public void DisableObjectControlUIPanel()
        {
            objectControlUIPanel.SetActive(false);
        }

        public void EnableObjectControlUIPanel()
        {
            objectControlUIPanel.SetActive(true);
        }
    }
}