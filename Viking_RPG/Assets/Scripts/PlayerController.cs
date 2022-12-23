using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController instance;

    public float moveSpeed;

    [HideInInspector]
    public Rigidbody2D theRB;

    private Animator anim;

    public SpriteRenderer theSR;
    public Sprite[] playerDirectionSprites;

    public Animator wpnAnim;

    private bool isKnockingBack;
    public float knockbackTime, knockbackForce;
    private float knockbackCounter;
    private Vector2 knockDir;

    public GameObject hitEffect;

    public float dashSpeed, dashLength, dashStamCost;
    private float dashCounter, activeMoveSpeed;

    public float totalStamina, stamRefillSpeed;
    [HideInInspector]
    public float currentStamina;

    private bool isSpinning;
    public float spinCost, spinCooldown;
    private float spinCounter;

    public bool canMove;

    public SpriteRenderer swordSR;
    public Sprite[] allSwords;
    public DamageEnemy swordDmg;
    public int currentSword;

    private Vector3 respawnPos;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        transform.position = SaveManager.instance.activeSave.sceneStartPosition;

        currentSword = SaveManager.instance.activeSave.currentSword;
        swordSR.sprite = allSwords[currentSword];
        swordDmg.damageToDeal = SaveManager.instance.activeSave.swordDamage;

        totalStamina = SaveManager.instance.activeSave.maxStamina;

        theRB = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        activeMoveSpeed = moveSpeed;
        currentStamina = totalStamina;

        UIManager.instance.UpdateStamina();
    }

    // Update is called once per frame
    void Update()
    {
        if (canMove && !GameManager.instance.dialogActive)
        {
            if (!isKnockingBack)
            {
                //transform.position = new Vector3(transform.position.x + (Input.GetAxisRaw("Horizontal") * moveSpeed * Time.deltaTime), transform.position.y + (Input.GetAxisRaw("Vertical") * moveSpeed * Time.deltaTime), transform.position.z);

                theRB.velocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized * activeMoveSpeed;

                anim.SetFloat("Speed", theRB.velocity.magnitude);

                if (theRB.velocity != Vector2.zero)
                {
                    if (Input.GetAxisRaw("Horizontal") != 0)
                    {
                        theSR.sprite = playerDirectionSprites[1];

                        if (Input.GetAxisRaw("Horizontal") < 0)
                        {
                            theSR.flipX = true;
                            wpnAnim.SetFloat("dirX", -1f);
                            wpnAnim.SetFloat("dirY", 0f);

                        }
                        else
                        {
                            theSR.flipX = false;
                            wpnAnim.SetFloat("dirX", 1f);
                            wpnAnim.SetFloat("dirY", 0f);
                        }
                    }
                    else
                    {
                        if (Input.GetAxisRaw("Vertical") < 0)
                        {
                            theSR.sprite = playerDirectionSprites[0];

                            wpnAnim.SetFloat("dirX", 0f);
                            wpnAnim.SetFloat("dirY", -1f);
                        }
                        else
                        {
                            theSR.sprite = playerDirectionSprites[2];

                            wpnAnim.SetFloat("dirX", 0f);
                            wpnAnim.SetFloat("dirY", 1f);
                        }
                    }
                }

                if (Input.GetMouseButtonDown(0) && !isSpinning)
                {
                    wpnAnim.SetTrigger("Attack");

                    AudioManager.instance.PlaySFX(0);
                }

                if (dashCounter <= 0)
                {
                    if (Input.GetKeyDown(KeyCode.Space) && currentStamina >= dashStamCost)
                    {
                        activeMoveSpeed = dashSpeed;
                        dashCounter = dashLength;

                        currentStamina -= dashStamCost;
                    }
                }
                else
                {
                    dashCounter -= Time.deltaTime;

                    if (dashCounter <= 0)
                    {
                        activeMoveSpeed = moveSpeed;
                    }
                }

                if (spinCounter <= 0)
                {
                    if (Input.GetMouseButtonDown(1) && currentStamina >= spinCost)
                    {
                        wpnAnim.SetTrigger("SpinAttack");

                        currentStamina -= spinCost;

                        spinCounter = spinCooldown;

                        isSpinning = true;

                        AudioManager.instance.PlaySFX(0);
                    }
                }
                else
                {
                    spinCounter -= Time.deltaTime;

                    if (spinCounter <= 0)
                    {
                        isSpinning = false;
                    }
                }

                currentStamina += stamRefillSpeed * Time.deltaTime;
                if (currentStamina > totalStamina)
                {
                    currentStamina = totalStamina;
                }

                UIManager.instance.UpdateStamina();

            }
            else
            {
                knockbackCounter -= Time.deltaTime;
                theRB.velocity = knockDir * knockbackForce;

                if (knockbackCounter <= 0)
                {
                    isKnockingBack = false;
                }
            }
        } else
        {
            theRB.velocity = Vector2.zero;

            anim.SetFloat("Speed", 0f);
        }
    }

    public void KnockBack(Vector3 knockerPosition)
    {
        knockbackCounter = knockbackTime;
        isKnockingBack = true;

        knockDir = transform.position - knockerPosition;
        knockDir.Normalize();

        Instantiate(hitEffect, transform.position, transform.rotation);
    }

    public void DoAtLevelStart()
    {
        canMove = true;

        respawnPos = transform.position;
    }

    public void UpgradeSword(int newDamage, int newSwordRef)
    {
        swordDmg.damageToDeal = newDamage;
        currentSword = newSwordRef;
        swordSR.sprite = allSwords[newSwordRef];

        SaveManager.instance.activeSave.currentSword = currentSword;
        SaveManager.instance.activeSave.swordDamage = newDamage;
    }

    public void ResetOnRespawn()
    {
        transform.position = respawnPos;

        canMove = false;

        gameObject.SetActive(true);
        currentStamina = totalStamina;
        knockbackCounter = 0f;
        PlayerHealthController.instance.currentHealth = PlayerHealthController.instance.maxHealth;
    }
}
