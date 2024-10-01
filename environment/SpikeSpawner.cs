using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.XR.Interaction;

public class SpikeSpawner : MonoBehaviour
{
    [SerializeField]
    private GameObject spikePrefab;
    [SerializeField]
    private BoxCollider2D triggerCollider;
    [SerializeField]
    private BoxCollider2D warningCollider;

    [SerializeField]
    private bool drawGizmos = true;

    [Header("Spawning")]

    [SerializeField]
    private Vector2 size;

    [SerializeField]
    [Range(0.01f, 15)]
    private float spawnSpeed;

    [SerializeField]
    private bool useSpawnSpeedVariation;

    [SerializeField]
    [Range(0, 10)]
    private float spawnSpeedVariation;

    [Header("Trigger")]

    [SerializeField]
    private bool autoTrigger;

    [SerializeField]
    private Vector2 triggerSize;
    [SerializeField]
    private Vector2 triggerOffset;

    [SerializeField]
    private Vector2 warningSize;
    [SerializeField]
    private Vector2 warningOffset;

    [Header("Drop patterns, use only one")]

    [SerializeField]
    private bool dropInCenter;

    [SerializeField]
    private bool random;



    private bool isReadyToDrop;
    private bool willDrop;

    private GameObject nextSpike;
    private SpikeProjectile nextSpikeSpikeProjectile;
    private Vector2 spawnPosition;

    private Vector2 spawnRangeMin;
    private Vector2 spawnRangeMax;

    private bool playerInTrigger;
    private bool playerInWarning;

    private Coroutine dropDelayCoroutine;
    private Coroutine checkIfSpikePresentCoroutine; 


    private void Start()
    {
        size = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
        spawnRangeMin.x = transform.position.x - (size.x / 2);
        spawnRangeMin.y = transform.position.y;
        spawnRangeMax.x = transform.position.x + (size.x / 2);
        spawnRangeMax.y = transform.position.y;

        triggerSize = new Vector2(Mathf.Abs(triggerSize.x), Mathf.Abs(triggerSize.y));
        warningSize = new Vector2(Mathf.Abs(warningSize.x), Mathf.Abs(warningSize.y));
        triggerCollider.size = triggerSize;
        triggerCollider.offset = triggerOffset;
        warningCollider.size = warningSize;
        warningCollider.offset = warningOffset;

        checkIfSpikePresentCoroutine = StartCoroutine(CheckIfSpikePresent());
    }

    void Update()
    {
        if (isReadyToDrop)
        {
            if (autoTrigger)
                StartCoroutine(TriggerDrop());
            else if (playerInTrigger)
                StartCoroutine(TriggerDrop());
        }
        else if (!isReadyToDrop && !willDrop)
            dropDelayCoroutine = StartCoroutine(DropDelay());
    }
    private IEnumerator DropDelay()
    {
        willDrop = true;

        YieldInstruction yield = new WaitForFixedUpdate();
        int seconds = SpecialAbility.SecondsToFixedFrames(spawnSpeed);
        seconds += SpeedVariation();
        for(int i = 0; i < seconds; i++)
        {
            yield return yield;
        }

        PrepareSpike();
        isReadyToDrop = true;
        willDrop = false;
    }
    private int SpeedVariation()
    {
        if (!useSpawnSpeedVariation)
            return 0;

        return SpecialAbility.SecondsToFixedFrames(UnityEngine.Random.Range(0, spawnSpeedVariation));
    }
    private void PrepareSpike()
    {
        SetNextSpawnPoint();
        nextSpike = Instantiate(original:spikePrefab, position:spawnPosition, rotation:Quaternion.Euler(0,0,0));
        nextSpikeSpikeProjectile = nextSpike.GetComponentInChildren<SpikeProjectile>();

        try
        {
            if(playerInWarning & !playerInTrigger)
                nextSpikeSpikeProjectile.StartWarningPlayer();
        }
        catch (NullReferenceException) { return; } //Could be caused if spike instantly collides
        catch (MissingReferenceException) { return; }
    }
    private void SetNextSpawnPoint()
    {
        if(dropInCenter)
            spawnPosition = transform.position;
        else if(random)
        {
            spawnPosition.y = spawnRangeMax.y;
            spawnPosition.x = UnityEngine.Random.Range(spawnRangeMin.x, spawnRangeMax.x);
        }
    }
    private IEnumerator TriggerDrop()
    {
        isReadyToDrop = false;
        yield return new WaitForSeconds(0);
        Drop();
    }
    private void Drop()
    {
        try
        {
            nextSpikeSpikeProjectile.StopWarningPlayer();
            nextSpikeSpikeProjectile.projectile.StartMoving();
            nextSpike = null;
            nextSpikeSpikeProjectile = null;
        }
        catch (NullReferenceException) { nextSpike = null; nextSpikeSpikeProjectile = null; }
        catch (MissingReferenceException) { nextSpike = null; nextSpikeSpikeProjectile = null; }
    }
    /// <summary>
    /// Check and replace spike every couple of frames if spike was triggered by player or collided with something before trigger
    /// </summary>
    private IEnumerator CheckIfSpikePresent()
    {
        yield return null; //Don't execute right after call in start
        YieldInstruction yield = new WaitForSeconds(0.3f + UnityEngine.Random.Range(-0.1f, 0.1f));

        while (yield != null)
        {
            if (isReadyToDrop)
            {
                if (nextSpikeSpikeProjectile == null || nextSpike == null)
                {
                    isReadyToDrop = false;
                    dropDelayCoroutine = StartCoroutine(DropDelay());
                }
            }
            yield return yield;
        }
    }


    public void PlayerEnterTrigger()
    {
        playerInTrigger = true;
    }
    public void PlayerExitTrigger()
    {
        playerInTrigger = false;
    }

    public void PlayerEnterWarning()
    {
        playerInWarning = true;
        if (nextSpikeSpikeProjectile is null || nextSpike is null || !isReadyToDrop)
            return;

        try
        {
            nextSpikeSpikeProjectile.StartWarningPlayer();
        }
        catch (NullReferenceException) { return; }
        catch (MissingReferenceException) { return; }
    }
    public void PlayerExitWarning()
    {
        playerInWarning = false;
        if (nextSpikeSpikeProjectile is null || nextSpike is null || !isReadyToDrop)
            return;

        try
        {
            nextSpikeSpikeProjectile.StopWarningPlayer();
        }
        catch (NullReferenceException) { return; }
        catch (MissingReferenceException) { return; }
    }





    private void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, size);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((Vector2)transform.position + warningOffset, warningSize);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube((Vector2)transform.position + triggerOffset, triggerSize);
    }
}