using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;



/// <summary>
/// Base class for all special abilities
/// </summary>
[RequireComponent(typeof(PlayerController))]
[RequireComponent(typeof(CollisionController2D))]
public abstract class SpecialAbility : MonoBehaviour
{
    protected PlayerController playerController;
    protected CollisionController2D collisionController;

    protected bool specialAbilityInUse = false;
    public abstract string AbilityName { get; }

    protected virtual void Start()
    {
        playerController = GetComponent<PlayerController>();
        collisionController = GetComponent<CollisionController2D>();
    }
    protected abstract void Update();

    /// <summary>
    /// Main functionality of the item
    /// </summary>
    /// <param name="context"></param>
    public abstract void UseSpecialAbility(InputAction.CallbackContext context); //Method called by playerController


    /// <summary>
    /// Converts seconds to fixed time intervals, for use in coroutines to provide accurate time
    /// </summary>
    /// <param name="seconds"></param>
    /// <returns></returns>
    public static int SecondsToFixedFrames(float seconds)
    {
        return Mathf.RoundToInt(seconds / Time.fixedDeltaTime); //1s ~ 50 fixedFrams with default settings
    }
}

