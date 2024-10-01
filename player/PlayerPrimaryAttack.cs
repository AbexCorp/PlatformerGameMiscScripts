using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController
{
    [SerializeField]
    private PrimaryAttack primaryAttack = new PrimaryAttack();
    [Serializable]
    public struct PrimaryAttack
    {
        [SerializeField]
        public GameObject projectilePrefab;

        [SerializeField]
        [Range(0.1f, 5)]
        [Tooltip("For how many seconds the player can control the projectile")]
        public float projectileControlTime;

        [SerializeField]
        [Range(0.1f, 5)]
        [Tooltip("How many seconds the player needs to wait before attacking again")]
        public float projectileCooldownTime;

        [SerializeField]
        [Range(0.1f, 5)]
        [Tooltip("How far the projectile spawns from the player")]
        public float projectileSpawnDistance;
    }


    /// <summary>
    /// Reference to the currently controlled projectile
    /// </summary>
    private GameObject currentProjectile;
    private bool isAttacking;
    private bool attackIsOnCooldown;
    private bool attackIsUnderControl;
    private Coroutine projectileControlCoroutine;
    private Coroutine projectileCooldownCoroutine;

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (isPaused) return; // Skip if paused

        if (context.started && !attackIsOnCooldown && !isAttacking && !attackIsUnderControl)
        {
            Shoot();
            projectileControlCoroutine = StartCoroutine(ProjectileControlCoroutine());
            DisableMovement();
        }
        if (context.canceled && isAttacking)
        {
            EnableMovement();
            StopCoroutine(projectileControlCoroutine);
            StopProjectileControl();
        }
    }
    /// <summary>
    /// Called by player projectile when it get's destroyer while under controll
    /// </summary>
    public void OnControlledProjectileDisposed()
    {
        if (!attackIsUnderControl)
            return;

        StopCoroutine(projectileControlCoroutine);
        isAttacking = false;
        attackIsUnderControl = false;

        currentProjectile = null;
        projectileCooldownCoroutine = StartCoroutine(ProjectileCooldownCoroutine());
        EnableMovement();
    }
    /// <summary>
    /// Spawn projectile and start controlling it
    /// </summary>
    private void Shoot()
    {
        isAttacking = true;
        Vector2 spawnPosition = Vector2Tools.ReadMousePosition();
        spawnPosition = controller.collisions.colliderCenter + (spawnPosition - controller.collisions.colliderCenter).normalized * primaryAttack.projectileSpawnDistance;

        currentProjectile = Instantiate(original: primaryAttack.projectilePrefab, position: spawnPosition, rotation: primaryAttack.projectilePrefab.transform.rotation);
        PlayerProjectile projectile = currentProjectile.GetComponent<PlayerProjectile>();
        projectile.stopControlOnDisposed.AddListener(OnControlledProjectileDisposed);

        projectile.StartControlling();
        projectile.SetOrigin(spawnPosition);
        projectile.SetVelocity(spawnPosition - controller.collisions.colliderCenter); //Done to prevent the projectile from not moving
    }
    /// <summary>
    /// Ends the player's control over the projectile direction
    /// </summary>
    private void StopProjectileControl()
    {
        isAttacking = false;
        attackIsUnderControl = false;

        currentProjectile.GetComponent<PlayerProjectile>().StopControlling();
        currentProjectile = null;
        projectileCooldownCoroutine = StartCoroutine(ProjectileCooldownCoroutine());
        EnableMovement();
    }

    /// <summary>
    /// Starts timer to stop projectile control
    /// </summary>
    private IEnumerator ProjectileControlCoroutine()
    {
        YieldInstruction yield = new WaitForFixedUpdate();
        int numberOfFixedFrames = SpecialAbility.SecondsToFixedFrames(primaryAttack.projectileControlTime);
        attackIsUnderControl = true;

        for (int i = 0; i < numberOfFixedFrames; i++)
        {
            yield return yield;
        }

        attackIsUnderControl = false;
        StopProjectileControl();
    }
    /// <summary>
    /// Starts cooldown before player can attack again
    /// </summary>
    private IEnumerator ProjectileCooldownCoroutine()
    {
        YieldInstruction yield = new WaitForFixedUpdate();
        int numberOfFixedFrames = SpecialAbility.SecondsToFixedFrames(primaryAttack.projectileCooldownTime);
        attackIsOnCooldown = true;

        for (int i = 0; i < numberOfFixedFrames; i++)
        {
            yield return yield;
        }

        attackIsOnCooldown = false;
    }
}
