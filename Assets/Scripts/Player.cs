using UnityEngine;
using UnityEngine.InputSystem;
using Yarn.Unity.Shim;

public class Player : MonoBehaviour
{
    // states
    bool shooting;
    bool firedShot;
    bool charging;
    bool meleeActive;
    bool meleeing;

    // ammo and HP management
    [SerializeField] int maxAmmo;
    int currentAmmo;

    [SerializeField] int maxPatience;
    int currentPatience;

    [SerializeField] int maxChargeCount;
    int currentChargeCount;
    [SerializeField] float timeToCharge;
    [SerializeField] float timeToLoseCharge;
    float currentChargeTime;

    [SerializeField] float shootingCooldown;
    float timeSinceLastShot;
    [SerializeField] float shotLifetime;
    float timeSinceShotFired;

    [SerializeField] float meleeCooldown;
    [SerializeField] float meleeLifetime;
    float timeSinceLastMelee;

    // references
    [SerializeField] CircleCollider2D cc;
    [SerializeField] SpriteRenderer sr;

    // unity input system
    InputAction shootAction;
    InputAction chargeAction;
    InputAction meleeAction;
    InputAction cursorPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentAmmo = maxAmmo;
        currentPatience = maxPatience;
        cursorPosition = InputSystem.actions.FindAction("Move");
        shootAction = InputSystem.actions.FindAction("Shoot");
        chargeAction = InputSystem.actions.FindAction("Charge");
        meleeAction = InputSystem.actions.FindAction("Melee");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(firedShot) { timeSinceShotFired += Time.deltaTime; }
        currentChargeTime += Time.deltaTime;
        timeSinceLastShot += Time.deltaTime;
        timeSinceLastMelee += Time.deltaTime;
        StateManagement();
        ChangeCursorColor();
        MoveCursor();
        Shoot();
        Charge();
        Melee();
    }

    void StateManagement()
    {
        if (firedShot && timeSinceShotFired >= shotLifetime) //resets the shot state
        {
            firedShot = false;
            timeSinceShotFired = 0;
        }

        if (!firedShot && shootAction.IsPressed()) //allows player to shoot again
        {
            shooting = true;
        }
        else
        {
            shooting = false;
        }

        if (meleeActive && timeSinceLastMelee >= meleeLifetime) //resets the melee state
        {
            meleeActive = false;
            timeSinceLastMelee = 0;
        }

        if (!meleeActive && meleeAction.IsPressed()) // allows player to melee again
        {
            meleeing = true;
        }
        else
        {
            meleeing = false;
        }

        charging = chargeAction.IsPressed(); //holds charge

    }

    void MoveCursor() //makes cursor follow the mouse
    {
        Vector3 currentPos = Camera.main.ScreenToWorldPoint(cursorPosition.ReadValue<Vector2>());
        currentPos.z = 0;
        transform.position = currentPos;
    }

    void Shoot() // shooting logic
    {
        if(shooting)
        {
            if(currentAmmo - currentChargeCount >= 0 && timeSinceLastShot >= shootingCooldown)
            {
                firedShot = true;
                sr.color = Color.red;
                Debug.Log("shoot");
                //shoot enemy logic
                currentAmmo = currentAmmo - currentChargeCount;
                currentChargeCount = 0;
                timeSinceLastShot = 0;
            }
            else // may need some tweaking later for balancing, current implementation loses ammo while shot is held with no targets. may make it less punishing later
            {
                sr.color = Color.gray;
                currentAmmo = currentAmmo - currentChargeCount;
                Debug.Log("misfire");
                timeSinceLastShot = 0;
            }

        }
    }

    void Charge() // charge logic
    {
        if (charging)
        {
            if(currentChargeTime >= timeToCharge) // increment charge count while held, more charge count = more damage
            {
                currentChargeTime = 0;
                if (currentChargeCount < maxChargeCount) { currentChargeCount++; }
                else { currentChargeCount = maxChargeCount; }
                Debug.Log(currentChargeCount);
            }
        }
        else
        {
            if (currentChargeTime >= timeToLoseCharge) //brings charge count down while the button isnt held
            {
                currentChargeTime = 0;
                if (currentChargeCount > 0) { currentChargeCount--; }
                else { currentChargeCount = 0; }
                Debug.Log(currentChargeCount);
            }
        }
    }

    void Melee() // melee logic, pretty similar to shooting logic except no penalty for missing. only applies to people with the wrong codes though
    {
        if (meleeing)
        {
            if (timeSinceLastMelee >= meleeCooldown && !shooting)
            {
                meleeActive = true;
                sr.color = Color.blue;
                Debug.Log("melee");
                //shoot enemy logic
                timeSinceLastMelee = 0;
            }
        }
    }

    void ChangeCursorColor() // reset cursor color. will probably refactor to make it handle all cursor color changes, but ideally will replace color changing with animations later
    {
        if(!shooting && !meleeing && !charging)
        {
            sr.color = Color.white;
        }
    }
}
