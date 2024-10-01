using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static PlayerController;

public class PlayerProjectile : Projectile
{
    [HideInInspector]
    public UnityEvent stopControlOnDisposed; //Called when projectile is about to be destroyed but is still controled by the player

    [Header("Player Projectile")]

    [SerializeField]
    [Range(0.1f, 15f)]
    private float secondsOfLifetime;

    private Coroutine lifetimeCoroutine;


    void Start()
    {
        lifetimeCoroutine = StartCoroutine(Lifetime());
    }

    /// <summary>
    /// Called by the player right after spawning the projectile
    /// </summary>
    public void StartControlling()
    {
        isHoming = true;
        isControlledByPlayer = true;
    }
    /// <summary>
    /// Called by the player after the homing period ends. Projectile stops homing and will move in the same direction
    /// </summary>
    public void StopControlling()
    {
        isHoming = false;
        isControlledByPlayer = false;
        stopControlOnDisposed.RemoveAllListeners();
    }
    protected override void OnCollisionEnter2D(Collision2D collision)
    {
        IShootable shootable = collision.gameObject.GetComponent<IShootable>();
        if (shootable != null)
        {
            collision.gameObject.GetComponent<IShootable>().GetHitByPlayerProjectile();
            if (isControlledByPlayer)
                DisposedWhileUnderControll();
            Dispose();
        }
        if (stoppingLayers == (stoppingLayers | 1 << collision.gameObject.layer)) //Colide with destroying layers
        {
            if (isControlledByPlayer)
                DisposedWhileUnderControll();
            Dispose();
        }
    }

    protected override void GetHomingPoint()
    {
        if(isControlledByPlayer)
            homingPoint = Vector2Tools.ReadMousePosition() - rigidBody.position;
    }
    /// <summary>
    /// Triggers the stopControlOnDisposed event to notify player that the projectile is no longer under control
    /// </summary>
    private void DisposedWhileUnderControll()
    {
        stopControlOnDisposed?.Invoke();
        stopControlOnDisposed.RemoveAllListeners();
        StopControlling();
    }

    /// <summary>
    /// Destroy projectile after lifetime elapses
    /// </summary>
    private IEnumerator Lifetime()
    {
        yield return null; //Stop after calling from start
        YieldInstruction yield = new WaitForFixedUpdate();
        int numberOfFixedFrames = SpecialAbility.SecondsToFixedFrames(secondsOfLifetime);
        for (int i = 0; i < numberOfFixedFrames; i++)
        {
            yield return yield;
        }

        if (isControlledByPlayer)
                DisposedWhileUnderControll();
        Dispose();
    }
}
