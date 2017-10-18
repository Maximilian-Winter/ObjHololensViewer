using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class ControlObjectManager : MonoBehaviour
{
    private bool buttonPressed;
    


    // UI elements to modify the object.
    public GameObject objectControlUIPanel;

    public Button moveForwardButton;
    public Button moveBackwardButton;
    public Button moveLeftButton;
    public Button moveRightButton;
    public Button moveUpButton;
    public Button moveDownButton;
    public Button rotateLeftButton;
    public Button rotateRightButton;
    public Text speedUIText;
    public Button speedLarger;
    public Button speedSmaller;
    public Text objectScaleUIText;
    public Button scaleLarger;
    public Button scaleSmaller;

    // UI elements to scale the object.
    public GameObject objectUnitScaleUIPanel;
    public Button micrometerButton;
    public Button millimeterButton;
    public Button centimeterButton;
    public Button meterButton;
    
    private string fileNameWithoutExtension;
    private float inputSpeedModifier;
    private float scaleFactor;

    private FileImportManager fileImportManager;
    private ModelImport modelImport;

    // Use this for initialization
    void Start ()
    {
        scaleFactor = 1.00f;
        inputSpeedModifier = 1.0f;
        fileNameWithoutExtension = "";

        buttonPressed = false;
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
        micrometerButton.onClick.AddListener(OnClickUnitScaleMicrometer);
        millimeterButton.onClick.AddListener(OnClickUnitScaleMillimeter);
        centimeterButton.onClick.AddListener(OnClickUnitScaleCentimeter);
        meterButton.onClick.AddListener(OnClickUnitScaleMeter);

        fileImportManager = GameObject.Find("SceneManager").GetComponent<FileImportManager>();
    }

    private void Awake()
    {
        modelImport = GameObject.Find("SceneManager").GetComponent<ModelImport>();
        modelImport.OnFileIsLoaded += StartUpControl;
    }

    private void OnDisable()
    {
        modelImport.OnFileIsLoaded -= StartUpControl;
    }

    // Update is called once per frame
    void Update ()
    {
        if (buttonPressed)
        {
            buttonPressed = false;
        }
    }

    void OnClickMoveForward()
    {
        if (!buttonPressed)
        {
            ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
            model.TranslateObject((Vector3.forward * 0.01f) * inputSpeedModifier);
            buttonPressed = true;
        }
    }

    void OnClickMoveBackward()
    {
        if (!buttonPressed)
        {
            ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
            model.TranslateObject((Vector3.back * 0.01f) * inputSpeedModifier);
            buttonPressed = true;
        }
    }

    void OnClickMoveLeft()
    {
        if (!buttonPressed)
        {
            ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
            model.TranslateObject((Vector3.left * 0.01f) * inputSpeedModifier);
            buttonPressed = true;
        }
    }

    void OnClickMoveRight()
    {
        if (!buttonPressed)
        {
            ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
            model.TranslateObject((Vector3.right * 0.01f) * inputSpeedModifier);
            buttonPressed = true;
        }
    }

    void OnClickMoveUp()
    {
        if (!buttonPressed)
        {
            ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
            model.TranslateObject((Vector3.up * 0.01f) * inputSpeedModifier);
            buttonPressed = true;
        }
    }

    void OnClickMoveDown()
    {
        if (!buttonPressed)
        {
            ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
            model.TranslateObject((Vector3.down * 0.01f) * inputSpeedModifier);
            buttonPressed = true;
        }
    }

    void OnClickRotateLeft()
    {
        if (!buttonPressed)
        {
            ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
            model.RotateObject((Vector3.up * 1.0f) * inputSpeedModifier);
            buttonPressed = true;
        }
    }

    void OnClickRotateRight()
    {
        if (!buttonPressed)
        {
            ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
            model.RotateObject((Vector3.down * 1.0f) * inputSpeedModifier);
            buttonPressed = true;
        }
    }

    void OnClickSpeedLarger()
    {
        if (!buttonPressed)
        {
            if (inputSpeedModifier < 100000)
            {
                if (inputSpeedModifier < 1)
                    inputSpeedModifier += 0.25f;
                else
                    inputSpeedModifier += 0.50f;
                speedUIText.text = "Speed: " + inputSpeedModifier;
                buttonPressed = true;
            }
        }
    }

    void OnClickSpeedSmaller()
    {
        if (!buttonPressed)
        {
            if (inputSpeedModifier > 0)
            {
                if (inputSpeedModifier <= 1)
                    inputSpeedModifier -= 0.25f;
                else
                    inputSpeedModifier -= 0.50f;
                speedUIText.text = "Speed: " + inputSpeedModifier;
                buttonPressed = true;
            }
        }
    }

    void OnClickScaleLarger()
    {
        if (!buttonPressed)
        {
            if (scaleFactor < 100000)
            {
                scaleFactor += (0.05f * inputSpeedModifier);
                objectScaleUIText.text = "Scale: " + scaleFactor;
                ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                model.ScaleObject(scaleFactor);
                buttonPressed = true;
            }
        }
    }

    void OnClickScaleSmaller()
    {
        if (!buttonPressed)
        {
            if (scaleFactor > 0.05f)
            {
                scaleFactor -= (0.05f * inputSpeedModifier);
                objectScaleUIText.text = "Scale: " + scaleFactor;
                ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
                model.ScaleObject(scaleFactor);
                buttonPressed = true;
            }
        }
    }

    void OnClickUnitScaleMeter()
    {
        if (!buttonPressed)
        {
            ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
            model.SetScale(1);
            buttonPressed = true;
            DisableObjectUnitScaleUIPanel();
            EnableObjectControlUIPanel();
        }
    }

    void OnClickUnitScaleCentimeter()
    {
        if (!buttonPressed)
        {
            ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
            model.SetScale(0.1f);
            buttonPressed = true;
            DisableObjectUnitScaleUIPanel();
            EnableObjectControlUIPanel();
        }
    }

    void OnClickUnitScaleMillimeter()
    {
        if (!buttonPressed)
        {
            ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
            model.SetScale(0.01f);
            buttonPressed = true;
            DisableObjectUnitScaleUIPanel();
            EnableObjectControlUIPanel();
        }
    }

    void OnClickUnitScaleMicrometer()
    {
        if (!buttonPressed)
        {
            ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
            model.SetScale(0.001f);
            buttonPressed = true;
            DisableObjectUnitScaleUIPanel();
            EnableObjectControlUIPanel();
        }
    }

    public void DisableObjectUnitScaleUIPanel()
    {
        objectUnitScaleUIPanel.SetActive(false);
    }

    public void EnableObjectUnitScaleUIPanel()
    {
        ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
        model.HideObject();
        objectUnitScaleUIPanel.SetActive(true);
    }

    public void DisableObjectControlUIPanel()
    {
        objectControlUIPanel.SetActive(false);
    }

    public void EnableObjectControlUIPanel()
    {
        ControlObject model = GameObject.Find(fileNameWithoutExtension).GetComponent<ControlObject>();
        model.ShowObject();
        objectControlUIPanel.SetActive(true);
    }

    private void StartUpControl()
    {
        fileNameWithoutExtension = fileImportManager.FileNameWithoutExtension;
        EnableObjectUnitScaleUIPanel();
    }
}
