using HoloToolkit.Unity.InputModule;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using HoloToolkit.Unity.SpatialMapping;
using HoloToolkit.Unity;

public class Placeholder : MonoBehaviour, IInputClickHandler
{
    public Transform prefab;

    private void Start()
    {
        InputManager.Instance.PushFallbackInputHandler(
          this.gameObject);
    }
    private void Update()
    {
        if (!this.loaded && (WorldAnchorManager.Instance.AnchorStore != null))
        {
            var ids = WorldAnchorManager.Instance.AnchorStore.GetAllIds();

            // NB: I'm assuming that the ordering here is either preserved or
            // maybe doesn't matter.
            foreach (var id in ids)
            {
                var instance = Instantiate(this.prefab);
                WorldAnchorManager.Instance.AttachAnchor(instance.gameObject, id);
            }
            this.loaded = true;
            this.count = ids.Length;
        }
    }
    public void OnInputClicked(InputClickedEventData eventData)
    {
        var instance = Instantiate(this.prefab);

        instance.gameObject.transform.position =
          GazeManager.Instance.GazeOrigin +
          GazeManager.Instance.GazeNormal * 1.5f;

        var tapToPlace = instance.gameObject.AddComponent<TapToPlace>();
        tapToPlace.SavedAnchorFriendlyName = (++this.count).ToString();
    }

    bool loaded;
    int count;
}
