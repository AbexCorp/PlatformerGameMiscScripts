using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour, IBlockable
{
    protected Rigidbody2D rigidBody;
    protected Vector2 velocity; //Used to assign velocity to rigidbody
    protected Vector2 position; //Used to assign position to rigidbody
    protected float currentSpeed;
    protected Vector2 homingPoint;


    [SerializeField]
    protected bool damagesPlayer;

    [SerializeField]
    protected bool damagesEnemies;

    [SerializeField]
    [Tooltip("Direction of initial movement of the projectile, will get overwritten if projectile is homing")]
    protected Vector2 destination;

    [SerializeField]
    [Range(0, 30)]
    [Tooltip("How fast the projectile will be moving. Max speed if acceleration is turned on")]
    protected float speed;

    [SerializeField]
    [Tooltip("Layers that will destroy or stop the projectile")]
    protected LayerMask stoppingLayers;

    [SerializeField]
    [Tooltip("True - will move on start, False - will not move until triggered")]
    protected bool startsMoving;

    [SerializeField]
    [Tooltip("DevOnly. If true projectile will be destroyed on collision, otherwise will be deactivated")]
    protected bool oneTimeOnly;

    [Header("Acceleration")]

    [SerializeField]
    [Tooltip("True - speed can change, False - constant speed")]
    protected bool usesAcceleration;

    [SerializeField]
    [Range(0, 30)]
    [Tooltip("Initial speed")]
    protected float startingSpeed;

    [SerializeField]
    [Range(0.01f, 15)]
    [Tooltip("How fast the projectile will change speed")]
    protected float acceleration;

    [Header("Homing")]

    [SerializeField]
    [Tooltip("Should the projectile turn to reach a tracked point")]
    protected bool isHoming;

    [SerializeField]
    [Tooltip("Should the projectile follow the player mouse")]
    protected bool isControlledByPlayer;
    //protected bool followsPlayer; //NEED PLAYER POSITION AND TURNING SCRIPT

    [SerializeField]
    [Range(1, 180)]
    [Tooltip("How fast should the projectile turn")]
    protected float turningSpeed;


    /// <summary>
    /// True - is in use, False - can be reused
    /// </summary>
    public bool IsActive { get; protected set; } //For object pooling
    /// <summary>
    /// True - already moving, False - waiting to be triggered
    /// </summary>
    public bool IsMoving { get; protected set; }


    protected void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        rigidBody.bodyType = RigidbodyType2D.Kinematic;
        rigidBody.useFullKinematicContacts = true;

        IsActive = true;
        ResetProjectile();
    }
    protected virtual void Update()
    {
        if (!IsMoving) //Stop movement if the bullet is not ready to move yet
            return;

        if(isHoming)
            ChangeDirection();
        if(usesAcceleration)
            UpdateSpeed();

        velocity = destination.normalized * currentSpeed;
        rigidBody.velocity = velocity;

        if (isHoming)
        {
            Debug.DrawLine(rigidBody.position, rigidBody.velocity + rigidBody.position, Color.white);
            DebugHelper.DrawDebugBox(homingPoint + rigidBody.position, Color.red);
        }
    }

    public virtual void GetBlocked()
    {
        Dispose();
    }
    /// <summary>
    /// Primary method of getting rid of projectiles
    /// </summary>
    public void Dispose()
    {
        if(oneTimeOnly)
            Destroy(gameObject);
        else
            Deactivate();
    }
    /// <summary>
    /// Trigger the projectile movement
    /// </summary>
    public void StartMoving()
    {
        IsMoving = true;
    }
    /// <summary>
    /// Set new origin of the projectile
    /// </summary>
    /// <param name="origin"></param>
    public void SetOrigin(Vector2 origin)
    {
        SetOrigin(origin.x, origin.y);
    }
    /// <summary>
    /// Set new origin of the projectile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void SetOrigin(float x, float y)
    {
        position.x = x;
        position.y = y;
        rigidBody.position = position;
    }
    /// <summary>
    /// Set starting destination when creating or reusing the projectile
    /// </summary>
    /// <param name="destination"></param>
    public void SetDestination(Vector2 destination)
    {
        SetDestination(destination.x, destination.y);
    }
    /// <summary>
    /// Set starting destination when creating or reusing the projectile
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void SetDestination(float x, float y)
    {
        destination.x = x;
        destination.y = y;
    }
    /// <summary>
    /// Set starting velocity. Useful only for homing projectiles
    /// </summary>
    /// <param name="velocity"></param>
    public void SetVelocity(Vector2 velocity)
    {
        SetVelocity(velocity.x, velocity.y);
    }
    /// <summary>
    /// Set starting velocity. Useful only for homing projectiles
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void SetVelocity(float x, float y)
    {
        velocity.x = x;
        velocity.y = y;
        rigidBody.velocity = velocity;
    }
    /// <summary>
    /// Deactivate the projectile to reuse it later
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        gameObject.SetActive(false);
    }
    /// <summary>
    /// Activate the object and allow reusing it
    /// </summary>
    public void Activate()
    {
        ResetProjectile();
        IsActive = true;
        gameObject.SetActive(true);
    }
    /// <summary>
    /// Accelerate or Decelerate
    /// </summary>
    protected void UpdateSpeed()
    {
        if(currentSpeed != speed)
            currentSpeed = Mathf.MoveTowards(currentSpeed, speed, acceleration * Time.deltaTime);
    }
    /// <summary>
    /// Go towards the followed point
    /// </summary>
    protected void ChangeDirection()
    {
        GetHomingPoint();
        float velocityAngle = Vector2.SignedAngle(Vector2.up, rigidBody.velocity.normalized) * -1;
        velocityAngle = (velocityAngle + 360) % 360; //Change range to 0-360
        float homingAngle = Vector2.SignedAngle(Vector2.up, homingPoint.normalized) * -1;
        homingAngle = (homingAngle + 360) % 360;

        float rotationAngle = (homingAngle - velocityAngle + 360) % 360;
        rotationAngle = rotationAngle <= 180 ? rotationAngle : -360 + rotationAngle; //Pick rotation direction
        rotationAngle = Mathf.MoveTowards(0, rotationAngle, turningSpeed * Time.deltaTime) * -1;

        destination = Vector2Tools.RotatePointAroundPivot(rigidBody.velocity.normalized, Vector2.zero, rotationAngle);
    }
    /// <summary>
    /// Set the point that the projectile will try to get to
    /// </summary>
    protected virtual void GetHomingPoint()
    {
        if(isControlledByPlayer)
            homingPoint = Vector2Tools.ReadMousePosition() - rigidBody.position;
        //else if(isFollowingPlayer)...
        else
            homingPoint = Vector2.zero - rigidBody.position;
    }
    /// <summary>
    /// Reset vars before using again
    /// </summary>
    protected void ResetProjectile()
    {
        if(!usesAcceleration)
        {
            startingSpeed = speed;
            currentSpeed = speed;
        }
        else
            currentSpeed = startingSpeed;
        if (startsMoving)
            StartMoving();
        else
            IsMoving = false;

        //if(isHoming) //Prevent homing projectiles from not moving because starting velocity is 0
            //rigidBody.velocity = Vector2.up;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (stoppingLayers == (stoppingLayers | 1 << collision.gameObject.layer)) //Colide with destroying layers
        {
            Dispose();
        }
        if(collision.gameObject.tag == "Player" && damagesPlayer)
        {
            //DamagePlayer
            Dispose();
        }
        //Damages enemies script here
    }
}
