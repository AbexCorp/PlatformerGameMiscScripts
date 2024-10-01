using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerController : IPlayerCheckpoints
{
    [Header("Checkpoints")]

    [SerializeField]
    [Tooltip("Ground that will be detected as a safe place to respawn, otherwise will be ignored")]
    private LayerMask checkPointSafeGround;

    [SerializeField]
    [Tooltip("Seconds before player returns to safepoint after touching teleport")]
    private float teleportDelay = 0;

    [SerializeField]
    [Tooltip("Should the player heal after returning to the same Milestone")]
    private bool healOnReenter;


    private Vector2? safepoint = null;
    private Vector2? milestone = null;

    const float groundSearchMaxDistance = 20;

    private bool willReturn = false;

    public void OnCheckpointCollisionEnter(Collider2D collider)
    {
        if (collider.gameObject.tag == "Safepoint" || collider.gameObject.tag == "Milestone" || collider.gameObject.tag == "SafepointReturn")
        {
            Vector2? newPosition = FindSafeGroundBelowCheckpoint(collider.transform.position);
            if (newPosition is null || newPosition == null)
                return;

            switch (collider.gameObject.tag)
            {
                case "Safepoint":
                    UpdateSafepoint((Vector2)newPosition);
                    return;

                case "Milestone":
                    Vector2? oldMilestone = milestone;
                    UpdateMilestone((Vector2)newPosition);
                    UpdateSafepoint((Vector2)newPosition);
                    if (!healOnReenter && oldMilestone != null && oldMilestone is not null && milestone != null && milestone is not null && milestone == oldMilestone)
                        return;
                    healthSystem.SetCurrentHealth(healthSystem.GetMaxHealth());
                    return;

                case "SafepointReturn":
                    if (!willReturn)
                        StartCoroutine(ReturnToSafePointDelay());
                    return;
            }
            return;
        }
        Debug.LogWarning("Detected checkpoint has no required tag. Assign either \"Checkpoint\" or \"Milestone\"");
    }

    public void GoToSafepoint()
    {
        if (safepoint is null)
        {
            Debug.LogWarning("Player has no active Safepoint");
            return;
        }
        StopAllMovement();
        transform.position = (Vector3)safepoint;
    }

    public void UpdateSafepoint(Vector2 newPos)
    {
        if (newPos == null)
            return;
        safepoint = newPos;
    }

    public void GoToMilestone()
    {
        if (milestone is null)
        {
            Debug.LogWarning("Player has no active Milestone");
            return;
        }
        StopAllMovement();
        transform.position = (Vector3)milestone;
    }

    public void UpdateMilestone(Vector2 newPos)
    {
        if (newPos == null)
            return;
        milestone = newPos;
    }

    private Vector2? FindSafeGroundBelowCheckpoint(Vector2 center)
    {
        Vector2? checkPointPosition;
        RaycastHit2D hit = Physics2D.Raycast(origin: center, direction: Vector2.down, distance: groundSearchMaxDistance, layerMask: checkPointSafeGround);
        if (hit)
        {
            if (hit.distance == 0) //Under ground
            {
                checkPointPosition = null;
                Debug.LogWarning("Checkpoint is under ground or stuck in terrain. Try moving it to an unoccupied space");
                return checkPointPosition;
            }
            float playerHeightHalf = transform.position.y - controller.collider.bounds.min.y;
            checkPointPosition = new Vector2(center.x, hit.point.y + playerHeightHalf);
        }
        else //Found no ground
        {
            checkPointPosition = null;
            Debug.LogWarning("Checkpoint couldn't find safe ground. Try placing a ground under it, or making it smaller");
        }
        return checkPointPosition;
    }

    private IEnumerator ReturnToSafePointDelay()
    {
        YieldInstruction yield = new WaitForFixedUpdate();
        int numberOfFixedFrames = SpecialAbility.SecondsToFixedFrames(teleportDelay);

        willReturn = true;
        for (int i = 0; i < numberOfFixedFrames; i++)
        {
            yield return yield;
        }
        willReturn = false;
        GoToSafepoint();
    }
}
