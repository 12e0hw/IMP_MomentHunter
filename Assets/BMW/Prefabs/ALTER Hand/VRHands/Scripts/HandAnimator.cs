using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

[RequireComponent(typeof(Animator))]
public class HandAnimator : MonoBehaviour
{
    /// <summary>
    /// Parameters for animaiton on action performed | Method 1
    /// </summary>
    [SerializeField] private InputActionReference controllerActionGrip;
    [SerializeField] private InputActionReference controllerActionTrigger;
    [SerializeField] private InputActionReference controllerActionFirstPrimary;
    [SerializeField] private InputActionReference controllerActionSecondPrimary;
    [SerializeField] private InputActionReference controllerActionJoystick;
    [SerializeField] private bool isRightHand = true;
    private bool isInitSet = true;

    private GameObject vrGloveObject;
    private bool isVisual;

    private WristUIManager wristUIManager;

    #region Method 2 Parameters

    ///// <summary>
    ///// Parameters for realtime animation | Method 2
    ///// </summary>
    //[SerializeField] private XRInputValueReader<Vector2> m_StickInput = new XRInputValueReader<Vector2>("Thumbstick");

    //[SerializeField] private XRInputValueReader<float> m_TriggerInput = new XRInputValueReader<float>("Trigger");
    //[SerializeField] private XRInputValueReader<float> m_GripInput = new XRInputValueReader<float>("Grip");

    #endregion Method 2 Parameters

    private Animator handAnimator = null;

    /// <summary>
    /// List of fingers animated when grabbing / using grab action
    /// </summary>
    private readonly List<Finger> grippingFingers = new List<Finger>()
    {
        new Finger(FingerType.Middle),
        new Finger(FingerType.Ring),
        new Finger(FingerType.Pinky)
    };

    /// <summary>
    /// List of fingers animated when pointing / using trigger action
    /// </summary>
    private readonly List<Finger> pointingFingers = new List<Finger>()
    {
        new Finger(FingerType.Index)
    };

    /// <summary>
    /// List of fingers animated when locomtion / using thumbstick
    /// </summary>
    private readonly List<Finger> primaryFingers = new List<Finger>()
    {
        new Finger(FingerType.Thumb)
    };

    /// <summary>
    /// Add your own hand animation here. For example a fist.
    /// </summary>
    //private readonly List<Finger> fistFingers = new List<Finger>()
    //{
    //    new Finger(FingerType.Thumb),
    //    new Finger(FingerType.Index)
    //    new Finger(FingerType.Middle),
    //    new Finger(FingerType.Ring),
    //    new Finger(FingerType.Pinky)
    //};

    private void Start()
    {
        this.handAnimator = GetComponent<Animator>();
        wristUIManager = FindAnyObjectByType<WristUIManager>();
        vrGloveObject = transform.Find("vr_glove_right_slim")?.gameObject;
        isVisual = true;
    }

    #region Method 1

    private void OnEnable()
    {
        controllerActionGrip.action.performed += GripAction_performed;
        controllerActionTrigger.action.performed += TriggerAction_performed;
        controllerActionFirstPrimary.action.performed += PrimaryAction_performed;
        controllerActionSecondPrimary.action.performed += PrimaryAction_performed;
        controllerActionJoystick.action.performed += PrimaryAction_performed;

        controllerActionGrip.action.canceled += GripAction_canceled;
        controllerActionTrigger.action.canceled += TriggerAction_canceled;
        controllerActionFirstPrimary.action.canceled += PrimaryAction_canceled;
        controllerActionSecondPrimary.action.canceled += PrimaryAction_canceled;
        controllerActionJoystick.action.canceled += PrimaryAction_canceled;
    }

    private void OnDisable()
    {
        controllerActionGrip.action.performed -= GripAction_performed;
        controllerActionTrigger.action.performed -= TriggerAction_performed;
        controllerActionFirstPrimary.action.performed -= PrimaryAction_performed;
        controllerActionSecondPrimary.action.performed -= PrimaryAction_performed;
        controllerActionJoystick.action.performed -= PrimaryAction_performed;

        controllerActionGrip.action.canceled -= GripAction_canceled;
        controllerActionTrigger.action.canceled -= TriggerAction_canceled;
        controllerActionFirstPrimary.action.canceled -= PrimaryAction_canceled;
        controllerActionSecondPrimary.action.canceled -= PrimaryAction_canceled;
        controllerActionJoystick.action.canceled -= PrimaryAction_canceled;
    }

    public void YGrap_performed()
    {
        if (isRightHand) {
            SetFingerAnimationValues(pointingFingers, 1.0f); 
            SetFingerAnimationValues(grippingFingers, 0.0f);
            SetFingerAnimationValues(primaryFingers, 1.0f);

            AnimateActionInput(pointingFingers);
            AnimateActionInput(grippingFingers);
            AnimateActionInput(primaryFingers);
        }
    }

    public void YGrap_canceled()
    {
        if (isRightHand)
        {
            SetFingerAnimationValues(pointingFingers, 0.25f);
            AnimateActionInput(pointingFingers);
            SetFingerAnimationValues(grippingFingers, 0.25f);
            AnimateActionInput(grippingFingers);
            SetFingerAnimationValues(primaryFingers, 0.0f);
            AnimateActionInput(primaryFingers);
        }
    }

    private void GripAction_performed(InputAction.CallbackContext obj)
    {
        if (!isRightHand)
        {
            SetFingerAnimationValues(pointingFingers, 1.0f);
            AnimateActionInput(pointingFingers);
            SetFingerAnimationValues(grippingFingers, 1.0f);
            AnimateActionInput(grippingFingers);
            SetFingerAnimationValues(primaryFingers, 1.0f);
            AnimateActionInput(primaryFingers);
        }
        else
        {
            SetFingerAnimationValues(grippingFingers, 1.0f);
            AnimateActionInput(grippingFingers);
            if (isVisual && !wristUIManager.isYGrap) Invisualization();
        }
    }

    private void TriggerAction_performed(InputAction.CallbackContext obj)
    {
        if (isRightHand)
        {
            SetFingerAnimationValues(pointingFingers, 1.0f);
            AnimateActionInput(pointingFingers);
        }
        
    }

    private void PrimaryAction_performed(InputAction.CallbackContext obj)
    {
        if (isRightHand) SetFingerAnimationValues(primaryFingers, 0.5f);
        else SetFingerAnimationValues(primaryFingers, 1.0f);
        AnimateActionInput(primaryFingers);
    }

    private void GripAction_canceled(InputAction.CallbackContext obj)
    {
        if (isRightHand)
        {
            SetFingerAnimationValues(grippingFingers, 0.25f);
            AnimateActionInput(grippingFingers);
            if (!isVisual) Visualization();
        }
        else
        {
            SetFingerAnimationValues(pointingFingers, 0.0f);
            AnimateActionInput(pointingFingers);
            SetFingerAnimationValues(grippingFingers, 0.0f);
            AnimateActionInput(grippingFingers);
            SetFingerAnimationValues(primaryFingers, 0.0f);
            AnimateActionInput(primaryFingers);
        }
    }

    private void TriggerAction_canceled(InputAction.CallbackContext obj)
    {
        if (isRightHand)
        {
            SetFingerAnimationValues(pointingFingers, 0.25f);
            // SetFingerAnimationValues(pointingFingers, 0.0f);
            AnimateActionInput(pointingFingers);
        }
    }

    private void PrimaryAction_canceled(InputAction.CallbackContext obj)
    {
        SetFingerAnimationValues(primaryFingers, 0.0f);
        AnimateActionInput(primaryFingers);
    }

    #endregion Method 1

    #region Method 2

    private void Update()
    {
        //if (m_StickInput != null)
        //{
        //    var stickVal = m_StickInput.ReadValue();
        //    SetFingerAnimationValues(primaryFingers, stickVal.y);
        //    AnimateActionInput(primaryFingers);
        //}

        //if (m_TriggerInput != null)
        //{
        //    var triggerVal = m_TriggerInput.ReadValue();
        //    SetFingerAnimationValues(pointingFingers, triggerVal);
        //    AnimateActionInput(pointingFingers);
        //}

        //if (m_GripInput != null)
        //{
        //    var gripVal = m_GripInput.ReadValue();
        //    SetFingerAnimationValues(grippingFingers, gripVal);
        //    AnimateActionInput(grippingFingers);
        //}

        if (isRightHand && isInitSet)
        {
            SetFingerAnimationValues(pointingFingers, 0.25f);
            AnimateActionInput(pointingFingers);
            SetFingerAnimationValues(grippingFingers, 0.25f);
            AnimateActionInput(grippingFingers);

            isInitSet = !isInitSet;
        }

    }

    #endregion Method 2

    public void SetFingerAnimationValues(List<Finger> fingersToAnimate, float targetValue)
    {
        foreach (Finger finger in fingersToAnimate)
        {
            finger.target = targetValue;
        }
    }

    public void AnimateActionInput(List<Finger> fingersToAnimate)
    {
        foreach (Finger finger in fingersToAnimate)
        {
            var fingerName = finger.type.ToString();
            var animationBlendValue = finger.target;
            handAnimator.SetFloat(fingerName, animationBlendValue);
        }
    }
    public void Visualization()
    {
        if (vrGloveObject != null) vrGloveObject.SetActive(true);
        isVisual = true;
    }

    public void Invisualization()
    {
        if (vrGloveObject != null) vrGloveObject.SetActive(false);
        isVisual = false;
    }
}
