using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Shield : SpecialAbility
{
    public GameObject visualShield;
    public override string AbilityName { get { return "Shield"; } }

    [SerializeField]
    [Range(0.1f, 7)]
    [Tooltip("How big the shield bubble is, low values may leave the player sticking out")] //Distance of the shield from the player
    private float shieldRadius;

    [SerializeField]
    [Range(10, 360)]
    [Tooltip("Degrees of protection, how wide the shield is")] //Width of the shield in degres
    private float shieldSize;

    /// <summary>
    /// Layers of all objects that shield will block
    /// </summary>
    [SerializeField]
    private LayerMask layersToBlock;

    [SerializeField]
    public bool shieldCooldownEnabled;

    [SerializeField]
    [Range(0, 10)]
    [Tooltip("How many seconds the shield will be active for")]
    private float shieldTime; //Ignored if cooldown disabled.

    [SerializeField]
    [Range(0, 10)]
    [Tooltip("How many seconds the shield will need to recharge")]
    private float shieldCooldown; //Ignored if cooldown disabled.


    //Display
    [SerializeField]
    private bool isOnCooldown = false;
    [SerializeField]
    private float remainingShieldTime = 0;
    [SerializeField]
    private float remainingCooldown = 0;


    //Private variables
    /// <summary>
    /// Center of the shield
    /// </summary>
    private Vector2 shieldOrigin;
    /// <summary>
    /// Direction where the shield is pointing
    /// </summary>
    private Vector2 shieldPosition;
    /// <summary>
    /// Leftmost point of the shield arc
    /// </summary>
    private Vector2 shieldMin;
    /// <summary>
    /// Rightmost point of the shield arc
    /// </summary>
    private Vector2 shieldMax;
    /// <summary>
    /// Normalized shield position used to calculate projectile angle;
    /// </summary>
    private Vector2 shieldPositionAngle;


    //Coroutines
    private Coroutine shieldTimeCoroutine;
    private Coroutine shieldCooldownCoroutine;


    protected override void Start()
    {
        playerController = GetComponent<PlayerController>();
        collisionController = GetComponent<CollisionController2D>();

        shieldTimeCoroutine = StartCoroutine(ShieldTimeCoroutine()); //Prevents null reference later in code
        StopCoroutine(shieldTimeCoroutine);
        shieldCooldownCoroutine = StartCoroutine(ShieldCooldownCoroutine());
        StopCoroutine(shieldCooldownCoroutine);
        isOnCooldown = false; //Vars need to be reset after stopping Coroutines
    }

    protected override void Update()
    {
        if (!specialAbilityInUse || isOnCooldown)
            return;
        CalculateShield();
    }

    public override void UseSpecialAbility(InputAction.CallbackContext context)
    {
        if (context.started) { }
        if (context.performed)
        {
            PlayerController.Instance.inputBuffer.Add(new PlayerController.InputBufferAction("OnUseSpecialAbilityStart", 0.1f, 0, OnUseSpecialAbilityStart));
        }
        if (context.canceled && !isOnCooldown)
        {
            PlayerController.Instance.inputBuffer.Add(new PlayerController.InputBufferAction("OnShieldEnd", 0f, 0, OnUseSpecialAbilityEnd));
        }
    }
    public bool OnUseSpecialAbilityStart()
    {
        if (collisionController.collisions.below && !isOnCooldown)
        {
            specialAbilityInUse = true;
            playerController.DisableMovement();
            if(visualShield != null)
                visualShield.SetActive(true);

            if (shieldCooldownEnabled)
                shieldTimeCoroutine = StartCoroutine(ShieldTimeCoroutine());
            return true;
        }
        return false; 
    }
    public bool OnUseSpecialAbilityEnd()
    {
        if (shieldCooldownEnabled && specialAbilityInUse)
        {
            StopCoroutine(shieldTimeCoroutine);
            shieldCooldownCoroutine = StartCoroutine(ShieldCooldownCoroutine());
        }

        specialAbilityInUse = false;
        playerController.EnableMovement();
        if(visualShield != null)
            visualShield.SetActive(false);
        return true;
    }


    /// <summary>
    /// Calculates all nessesery variables, and detects projectiles from every layer
    /// </summary>
    private void CalculateShield()
    {
        shieldOrigin = collisionController.collisions.colliderCenter;
        shieldPosition = ReadShieldPosition();
        shieldMin = Vector2Tools.RotatePointAroundPivot(shieldPosition, shieldOrigin, -shieldSize / 2); //Only for debug for now
        shieldMax = Vector2Tools.RotatePointAroundPivot(shieldPosition, shieldOrigin, shieldSize / 2); //Only for debug for now
        shieldPositionAngle = (shieldPosition - shieldOrigin).normalized; //Used to calculate angle of the incoming projectile

        RaycastHit2D[] hitArray = Physics2D.CircleCastAll(origin: shieldOrigin, radius: shieldRadius, direction: shieldOrigin, distance: 0, layerMask: layersToBlock.value);
        Block(hitArray);

        DrawDebugShield();
        if(visualShield != null)
            UpdateVisualShield();
    }

    /// <summary>
    /// Blocks all projectiles that are within the shield radius and the shield blocking cone
    /// </summary>
    /// <param name="hitArray"></param>
    /// <param name="shieldPositionAngle"></param>
    private void Block(RaycastHit2D[] hitArray)
    {
        if (hitArray.Length == 0)
            return;

        foreach (RaycastHit2D hit in hitArray)
        {
            if (Vector2.Angle(-hit.normal, shieldPositionAngle) > shieldSize / 2) //Ignore objects outside the blocked angle
                continue;

            //Destroy bullet code here
            Debug.DrawLine(shieldOrigin, hit.transform.position, Color.red);
            IBlockable toBlock = hit.transform.GetComponent<IBlockable>();
            if (toBlock is null)
            {
                Debug.LogWarning("Object did not implement IBlockable");
                continue;
            }
            toBlock.GetBlocked();
        }
    }
    private Vector2 ReadShieldPosition() //Temporary function to read mouse position
    {
        return shieldOrigin + ((Vector2)(Camera.main.ScreenToWorldPoint(Mouse.current.position.value)) - shieldOrigin).normalized * shieldRadius;
        //shieldOrigin + (new Vector2(shieldPositionX, shieldPositionY) - shieldOrigin).normalized * shieldRadius //Version that uses Vector2 as input instead of reading mouse
    }

    private void UpdateVisualShield()
    {
        visualShield.transform.position = shieldPosition;
        visualShield.transform.rotation = Quaternion.identity;
        visualShield.transform.Rotate(new Vector3(0, 0, Vector2.SignedAngle(Vector2.up, shieldPositionAngle)));
    }

    private void DrawDebugShield()
    {
        Debug.DrawLine(shieldOrigin, shieldPosition, Color.yellow);
        Debug.DrawLine(shieldOrigin, shieldMin, Color.magenta);
        Debug.DrawLine(shieldOrigin, shieldMax, Color.magenta);
        Debug.DrawLine(shieldMin, shieldPosition, Color.magenta);
        Debug.DrawLine(shieldMax, shieldPosition, Color.magenta);
        DebugHelper.DrawDebugBox(shieldPosition, Color.yellow);
        DebugHelper.DrawDebugBox(shieldMin, Color.magenta);
        DebugHelper.DrawDebugBox(shieldMax, Color.magenta);
    }

    /// <summary>
    /// Ends block after the max allowed block time passed
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShieldTimeCoroutine()
    {
        YieldInstruction fixedUpdate = new WaitForFixedUpdate();
        int numberOfFixedFrames = SecondsToFixedFrames(shieldTime);
        for (int i = 0; i < numberOfFixedFrames; i++)
        {
            remainingShieldTime = (numberOfFixedFrames - i) * Time.fixedDeltaTime;
            yield return fixedUpdate;
        }
        isOnCooldown = true;
        shieldCooldownCoroutine = StartCoroutine(ShieldCooldownCoroutine());
        playerController.EnableMovement();
        if(visualShield != null)
            visualShield.SetActive(false);
    }

    /// <summary>
    /// Starts the shield cooldown
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShieldCooldownCoroutine()
    {
        isOnCooldown = true;
        specialAbilityInUse = false;
        YieldInstruction fixedUpdate = new WaitForFixedUpdate();
        int numberOfFixedFrames = SecondsToFixedFrames(shieldCooldown);
        for (int i = 0; i < numberOfFixedFrames; i++)
        {
            remainingCooldown = (numberOfFixedFrames - i) * Time.fixedDeltaTime;
            yield return fixedUpdate;
        }
        isOnCooldown = false;
    }
}