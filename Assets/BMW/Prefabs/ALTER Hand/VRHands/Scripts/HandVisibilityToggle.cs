using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class HandVisibilityToggle : MonoBehaviour
{
    [SerializeField] private NearFarInteractor handInteractor;

    private SkinnedMeshRenderer handModel;
    public bool isVisual;
    private bool isGrabbed = false;

    private void Start()
    {
        handModel = GetComponentInChildren<SkinnedMeshRenderer>();
        if (handInteractor != null) handInteractor.selectEntered.AddListener(OnGrab);
        if (handInteractor != null) handInteractor.selectExited.AddListener(OnLetGo);
        isVisual = true;
    }

    private void Update()
    {
        if (isGrabbed)
        {
            if (handInteractor != null && handInteractor.selectionRegion.Value == NearFarInteractor.Region.Near)
            {
                if (handModel.enabled && isVisual)
                {
                    Invisualization();
                }
            }
        }
        else
        {
            if (!handModel.enabled && !isVisual)
            {
                Visualization();
            }
        }
    }

    public void Visualization()
    {
        handModel.enabled = true;
        isVisual = true;
    }

    public void Invisualization()
    {
        handModel.enabled = false;
        isVisual = false;
    }

    private void OnLetGo(SelectExitEventArgs arg0)
    {
        isGrabbed = false;
    }

    private void OnGrab(SelectEnterEventArgs arg0)
    {
        isGrabbed = true;
    }
}
