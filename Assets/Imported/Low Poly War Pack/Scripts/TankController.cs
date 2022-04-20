using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class TankController : MonoBehaviour {
    [System.Serializable]
    public class TankSetUp {
        public float reloadTime = 2.5f;
        public float speed = 3.5f;
        public float turnSpeed = 75f;
        public float headRot = 2f;
        public float turretRot = 2f;
        public GameObject tankHead;
        public GameObject tankTurret;
        public GameObject bullet;

        [HideInInspector]
        public GameObject player;
        public Transform playerSlot;

        public Transform getOutPoint;
        public Camera Cam;


        public bool particleBullets;

        public KeyCode getOutOfTank;

    }

    public TankSetUp tankSetUp;

    public GameObject tankBoxTracer;

    Rigidbody rb;

    [System.Serializable]
    public class AudioParams {
        //Audio Parameters
        public float min = 1f;
        public float max = -1f;
        public float newMin = 0.8f;
        public float newMax = 1.3f;
    }

    public AudioParams audioParams;


    Vector3 originalPosition;
    Quaternion originalRotation;

    //Input Parameters
    float vertical;
    float horizontal;



    public float health;
    public float maxHealth;

    [HideInInspector]
    public SoldierAnimator requestedBot;

    Quaternion HeadRot;
    Animator anim;
    Vector3 lastPosition = Vector3.zero;

    public bool interactingWith = false;
    bool startedEngine = false;
    public AudioSource tankMovingSource;
    public AudioSource fireSource;
    public AudioClip fireSound;
    //wrecks
    public GameObject tankWreck;
    public int tankId;

    // Use this for initialization
    void Start() {
        GetComponent<Rigidbody>().centerOfMass = new Vector3(0, -0.9f, 0);
        Assignments();
        originalMass = rb.mass;
        interactingWith = false;
        tankSetUp.Cam.enabled = false;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }
    void Assignments() {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        HeadRot = tankSetUp.tankHead.transform.rotation;
    }
    float shootDelay;
    public Transform rotateTarget;
    public float rotateTargetDelay;

    public bool canGetOut;
    // Update is called once per frame
    void Update() {
        if (rotateTargetDelay > 0f) {
            rotateTargetDelay -= Time.deltaTime;
            if (rotateTargetDelay <= 0f)
                rotateTarget = null;
        }
        if (health < 0f)
            BlowUp();
        if (interactingWith) {
            if (!tankSetUp.player.GetComponent<SoldierAnimator>().isPlayer && !tankSetUp.player.GetComponent<SoldierAnimator>().masterController.isSinglePlayer) {
                rb.mass = originalMass * 10;
            } else {
                rb.mass = originalMass;
            }

            if (tankSetUp.player == null) {
                BlowUp();
                return;
            } else {
                tankSetUp.player.transform.position = tankSetUp.playerSlot.position;
            }
            requestedBot = null;
        }
        if (interactingWith && tankSetUp.player != null && tankSetUp.player.GetComponent<SoldierAnimator>().isPlayer && !tankSetUp.player.GetComponent<SoldierAnimator>().masterController.paused) {
            vertical = -Input.GetAxis("Vertical");
            horizontal = Input.GetAxis("Horizontal");

            if (Input.GetKeyDown(KeyCode.Mouse0) && !tankSetUp.player.GetComponent<SoldierAnimator>().masterController.isMobile) {
                FireGun();
            }

            Sound();
        }
    }

    public void GetOutOfTank(bool playerDisconnect=false) {
        rotateTarget = null;
        startedEngine = false;
        if (!playerDisconnect) {
            tankSetUp.player.transform.parent = null;
            tankSetUp.player.transform.position = tankSetUp.getOutPoint.position;
            //The player will reset to world 0,0,0 rotation
            tankSetUp.player.transform.rotation = Quaternion.Euler(Vector3.zero);
            tankSetUp.player.GetComponent<SoldierAnimator>().animator.gameObject.SetActive(true);
        }
        interactingWith = false;
        anim.SetFloat("Speed", 0);
        
        tankMovingSource.Stop();
        canGetOut = false;
    }

    IEnumerator CoolDownGetInButton() {
        yield return new WaitForSeconds(.2f);
        canGetOut = true;
    }

    public void FireGun() {
        if (shootDelay <= 0f) {
            if (tankSetUp.player.GetComponent<SoldierAnimator>().isPlayer) {
                tankSetUp.player.GetComponent<SoldierAnimator>().shootingCumulationDelta++;
            } else {
                tankSetUp.player.GetComponent<SoldierAnimator>().shootingCumulatedStorage--;
            }
            GetComponent<AudioSource>().PlayOneShot(fireSound);
            ///anim.SetTrigger("Fire");
            GameObject BulletClone = Instantiate(tankSetUp.bullet, tankSetUp.tankTurret.transform.GetChild(0).transform.position, tankSetUp.tankTurret.transform.GetChild(0).transform.rotation);

            BulletClone.AddComponent<Rigidbody>();
            BulletClone.GetComponent<Rigidbody>().useGravity = false;
            BulletClone.GetComponent<Bullets>().instantiateParticles = tankSetUp.particleBullets;
            BulletClone.GetComponent<Rigidbody>().AddForce(tankSetUp.tankTurret.transform.GetChild(0).transform.forward * 53f, ForceMode.Impulse);
            BulletClone.GetComponent<Bullets>().sender = tankSetUp.player.GetComponent<SoldierAnimator>().playerId;
            BulletClone.GetComponent<Bullets>().owner = tankSetUp.player.GetComponent<SoldierAnimator>();

            BulletClone.transform.parent = null;
            shootDelay = tankSetUp.reloadTime;
        }
    }
    void FixedUpdate() {
        if (shootDelay >= 0f)
            shootDelay -= Time.deltaTime;
        if (rotateTarget != null) {
            Quaternion targetRotationFlat = Quaternion.LookRotation(new Vector3(rotateTarget.position.x, 0f, rotateTarget.position.z) - new Vector3(tankSetUp.tankHead.transform.position.x, 0f, tankSetUp.tankHead.transform.position.z));
            float targetRotationHeight = Mathf.Acos((tankSetUp.tankTurret.transform.position.y - (rotateTarget.position.y + 0.82f)) / Vector3.Distance(tankSetUp.tankTurret.transform.position, rotateTarget.transform.position)) * 180 / Mathf.PI;

            if (Quaternion.Angle(tankSetUp.tankHead.transform.rotation, Quaternion.Euler(targetRotationFlat.eulerAngles.x, targetRotationFlat.eulerAngles.y + 180f, targetRotationFlat.eulerAngles.z)) > 0.72f)
                tankSetUp.tankHead.transform.rotation = Quaternion.RotateTowards(tankSetUp.tankHead.transform.rotation, Quaternion.Euler(targetRotationFlat.eulerAngles.x, targetRotationFlat.eulerAngles.y + 180f, targetRotationFlat.eulerAngles.z), 75f * Time.deltaTime);
            tankSetUp.tankTurret.transform.localRotation = Quaternion.Euler(targetRotationHeight - 90f, 0f, 0f);
            if (Quaternion.Angle(tankSetUp.tankTurret.transform.rotation, Quaternion.Euler(targetRotationFlat.eulerAngles.x, targetRotationFlat.eulerAngles.y + 180f, targetRotationFlat.eulerAngles.z)) < 7.5f) {
                FireGun();
            }
        }
        if (interactingWith && tankSetUp.player != null && tankSetUp.player.GetComponent<SoldierAnimator>().isPlayer && !tankSetUp.player.GetComponent<SoldierAnimator>().masterController.isMobile && !tankSetUp.player.GetComponent<SoldierAnimator>().masterController.paused) {
            Move(vertical);
            Turn(horizontal);
            RotateXTankHead(Input.GetAxis("Mouse X"));
            RotateYTankTurret(Input.GetAxis("Mouse Y"));
        }
    }
    float myHorizontal = 0f, myVertical = 0f;
    void Sound() {
        //if (interactingWith) {
            if (!startedEngine) {
                tankMovingSource.Play();
                startedEngine = true;
            }

            if (startedEngine) {
                var audioPitch = myVertical - Mathf.Abs(myHorizontal);

                tankMovingSource.pitch = audioPitch.Remap(audioParams.min, audioParams.max, audioParams.newMin, audioParams.newMax);
            }
        //}
    }

    public void Move(float myVertical) {
        Vector3 movement = transform.forward * myVertical * tankSetUp.speed * Time.deltaTime;
        this.myVertical = myVertical;
        rb.MovePosition(rb.position + movement);

        float currentSpeed = Mathf.Abs((((transform.position - lastPosition).magnitude) / Time.deltaTime));

        lastPosition = transform.position;
        currentSpeed.Remap(0, tankSetUp.speed, 0, 2);

        if (myVertical < 0.1) {
            anim.SetFloat("Speed", currentSpeed);
        }

        if (myVertical > -0.1) {
            anim.SetFloat("Speed", -currentSpeed);
        }

        if (myHorizontal >= 0.1) {
            anim.SetFloat("Speed", -1);
        }

        if (myHorizontal <= -0.1) {
            anim.SetFloat("Speed", 1);
        }
        tankSetUp.player.transform.position = tankSetUp.playerSlot.position;
    }

    public void Turn(float myHorizontal) {
        float turn = myHorizontal * tankSetUp.turnSpeed * Time.deltaTime;
        this.myHorizontal = myHorizontal;
        Quaternion turnRot = Quaternion.Euler(0f, turn, 0f);
        rb.MoveRotation(rb.rotation * turnRot);
    }

    public void RotateXTankHead(float xValue) {
        float xRot = xValue * 0.17f * tankSetUp.headRot;
        HeadRot = Quaternion.Euler(0, xRot, 0f);

        tankSetUp.tankHead.transform.rotation *= HeadRot;
    }
    public void RotateYTankTurret(float yValue) {
        float yRot = yValue * 0.1f * tankSetUp.turretRot;
        HeadRot = Quaternion.Euler(yRot, 0f, 0f);
        tankSetUp.tankTurret.transform.rotation *= HeadRot;

        float normalX = tankSetUp.tankTurret.transform.localEulerAngles.x;
        if (normalX < -252f)
            normalX += 360f;
        else if (normalX > 190f)
            normalX -= 360f;
        if (normalX > 15f) {
            tankSetUp.tankTurret.transform.localEulerAngles = new Vector3(15f, tankSetUp.tankTurret.transform.localEulerAngles.y, tankSetUp.tankTurret.transform.localEulerAngles.z);
        }
        if (normalX <  - 17f) {
            tankSetUp.tankTurret.transform.localEulerAngles = new Vector3(-017f, tankSetUp.tankTurret.transform.localEulerAngles.y, tankSetUp.tankTurret.transform.localEulerAngles.z);
        }
    }


    public void TakeDamage(float ammount) {
        if (health <= 0) {
            BlowUp();
        }

        Debug.Log(" Take Damage");
        health -= ammount;
    }
    float originalMass = 100;
    public void BlowUp() {
        

        rotateTarget = null;
        requestedBot = null;
        if (tankSetUp.player != null) {
            GameObject exp = Instantiate(tankSetUp.bullet.GetComponent<Bullets>().particles, transform.position, transform.rotation);
            tankSetUp.player.GetComponent<SoldierAnimator>().ExitVehicle(true);
            tankSetUp.player.GetComponent<SoldierAnimator>().nearbyTank = null;

            exp.GetComponent<Explosion>().owner = tankSetUp.player.GetComponent<SoldierAnimator>();
            exp.GetComponent<Explosion>().damage = 350;
            tankSetUp.player.GetComponent<SoldierAnimator>().diedFromVehicle = true;
        } else {
            GetOutOfTank(true);
        }
        health = maxHealth;
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        tankSetUp.tankHead.transform.localRotation = Quaternion.identity;
        tankSetUp.tankTurret.transform.localRotation = Quaternion.identity;
    }

    

    public void SortPlayer(GameObject player)
    {
        if (!interactingWith) {
            if (player.GetComponent<SoldierAnimator>().isPlayer) {
                tankSetUp.Cam.enabled = true;
            }

            tankSetUp.player = player;
            tankSetUp.player.transform.parent = tankSetUp.playerSlot;
            tankSetUp.player.transform.position = tankSetUp.playerSlot.position;
            tankSetUp.player.GetComponent<SoldierAnimator>().animator.gameObject.SetActive(false);


            interactingWith = true;
            StartCoroutine(CoolDownGetInButton());
        }
    }
}
