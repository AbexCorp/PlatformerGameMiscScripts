using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PolygonCollider2D))]
public class SpikeProjectile : MonoBehaviour, IBlockable, IShootable
{
    public Projectile projectile;
    public LayerMask ground;
    public LayerMask player;
    public GameObject spikeSpriteObject;

    [SerializeField]
    private float ShakeRangeX;
    [SerializeField]
    private float ShakeRangeY;
    [SerializeField]
    private float ShakeAmount;

    private bool isTrembling;
    private Vector2 spikeSpriteCenter;
    private Vector2 currentSpikePosition;

    void Start()
    {
        spikeSpriteCenter = spikeSpriteObject.transform.localPosition;
        currentSpikePosition = spikeSpriteCenter;
    }
    void Update()
    {
        if (isTrembling)
            Tremble();
    }

    public void Trigger()
    {
        if (projectile.IsMoving)
            return;
        isTrembling = false;
        spikeSpriteObject.transform.localPosition = spikeSpriteCenter;
        projectile.StartMoving();
    }
    public void StartWarningPlayer()
    {
        if (projectile.IsMoving)
            return;
        isTrembling = true;
    }
    public void StopWarningPlayer()
    {
        if (projectile.IsMoving)
            return;
        isTrembling = false;
        spikeSpriteObject.transform.localPosition = spikeSpriteCenter;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Boss"))
        {
            HealthSystem health = collision.gameObject.GetComponent<HealthSystem>();
            if (health != null) health.ReceiveDamage(1, gameObject);
            else Debug.LogWarning("Boss doesn't implement health");

            projectile.Dispose();
        }
        if (collision.gameObject.tag == "Player")
        {
            HealthSystem health = collision.gameObject.GetComponent<HealthSystem>();
            if (health != null) health.ReceiveDamage(1, gameObject);
            else Debug.LogWarning("Player doesn't implement health");

            projectile.Dispose();
        }
    }
    private void Tremble()
    {
        currentSpikePosition -= spikeSpriteCenter;
        currentSpikePosition += Random.insideUnitCircle * ShakeAmount * Time.deltaTime;
        currentSpikePosition.x = Vector2.ClampMagnitude(currentSpikePosition, ShakeRangeX).x;
        currentSpikePosition.y = Vector2.ClampMagnitude(currentSpikePosition, ShakeRangeY).y;
        currentSpikePosition += spikeSpriteCenter;
        spikeSpriteObject.transform.localPosition = currentSpikePosition;
    }

    public void GetBlocked()
    {
        projectile.GetBlocked();
    }
    public void GetHitByPlayerProjectile()
    {
        Trigger();
    }
}
