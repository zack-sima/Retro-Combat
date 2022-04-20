
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Rigidbody), typeof(AudioSource))]
public class PlaneController : MonoBehaviour
{
    [System.Serializable]
    public class PlaneSetUp
    {

        public float currentSpeed = 20f;
        public float maxSpeed = 90f;
        public float minSpeed = 0f;
        public float turnSpeed = 180f;
        public float lookSpeed = 2f;
        public float breakSpeed;
        public float accelerationSpeed;
        public float health;
        public float bulletDamage;

        public GameObject cam;
        public GameObject bullet;
        [HideInInspector]
        public GameObject player;

        public Transform playerSlot;
        public Transform[] firePoints;
        public Transform planeNose;
        public Transform getOutPoint;

        public GameObject particleSystem;
        public GameObject parachute;

    }

    public PlaneSetUp planeSetUp;


    Animator anim;
    Rigidbody rb;

    Quaternion camRot;

    RaycastHit hit;

    public AudioSource planeFlying;
    public AudioSource fireSource;
    public AudioClip fireSound;

    //Audio Parameters
    float min = 5f;
    float max = -5f;
    float newMin = 0.8f;
    float newMax = 1.3f;

    //Input Parameters
    float vertical;
    float horizontal;


    public float groundCheckHeight;
    public float preGroundCheckHeight;
    float audioPitch;


    bool accelerating = false;
    bool startedEngine = false;
    bool canLand;
    public bool bulletExplosion;

    public KeyCode GetOutKey;

    bool hideCursor = true;
    public bool interactingWith, hasFlown;

    public bool canGetOut;

    void Start()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
        //rb.useGravity = false;
        anim = GetComponentInChildren<Animator>();
        planeSetUp.cam.transform.GetChild(0).GetComponent<Camera>().enabled = false;
        rb.isKinematic = true;
    }
    float shootDelay = 0.1f;
    void Update()
    {
        if ((interactingWith && planeSetUp.player == null) || health <= 0d) { //player quit
            BlowUp();
        }
        shootDelay -= Time.deltaTime;
        if (interactingWith && planeSetUp.player.GetComponent<SoldierAnimator>().isPlayer && !planeSetUp.player.GetComponent<SoldierAnimator>().masterController.isMobile && !planeSetUp.player.GetComponent<SoldierAnimator>().masterController.paused)

        {
            hasFlown = true;

            var inputHorizontal = Input.GetAxis("Mouse X");
            var inputVertical = Input.GetAxis("Mouse Y");
            //MouseInput
            vertical = inputVertical;
            horizontal = inputHorizontal;

            if (Input.GetMouseButton(0)) {

                FireGun();
            }

            

        }
    }

    void Sound()
    {
        if (interactingWith)
        {
            if (!startedEngine)
            {
                planeFlying.Play();
                startedEngine = true;
            }

            if (startedEngine)
            {
                audioPitch = Mathf.Abs(horizontal) - vertical;

                planeFlying.pitch = audioPitch.Remap(min, max, newMin, newMax);
            }
        }
    }





    public void GetOutOfPlane()
    {
        planeFlying.Stop();
        
        planeSetUp.player.GetComponent<SoldierAnimator>().animator.gameObject.SetActive(true);
        

        planeSetUp.player.transform.parent = null;
        planeSetUp.player.transform.position = planeSetUp.getOutPoint.position;
        //The player will reset to 0,0,0 rotation
        planeSetUp.player.transform.rotation = Quaternion.Euler(Vector3.zero);

        planeSetUp.player.SetActive(true);
        interactingWith = false;
        planeSetUp.cam.transform.GetChild(0).GetComponent<Camera>().enabled = false;
    }

    IEnumerator DestroyPlane()
    {
        yield return new WaitForSeconds(1f);

        Destroy(this.gameObject);
    }
    public float health;
    public float maxHealth;
    public void FireGun()
    {
        if (shootDelay < 0f)
            shootDelay = 0.0782f;
        else {
            return;
        }
        if (planeSetUp.player.GetComponent<SoldierAnimator>().isPlayer) {
            planeSetUp.player.GetComponent<SoldierAnimator>().shootingCumulationDelta++;
        } else {
            planeSetUp.player.GetComponent<SoldierAnimator>().shootingCumulatedStorage--;
        }
        foreach (Transform item in planeSetUp.firePoints)
        {
            GameObject BulletClone = Instantiate(planeSetUp.bullet, item.position, item.rotation);
            BulletClone.GetComponent<Bullets>().owner = planeSetUp.player.GetComponent<SoldierAnimator>();
            if (!BulletClone.GetComponent<Bullets>())
            {
                BulletClone.AddComponent<Bullets>();
            }

            BulletClone.GetComponent<Bullets>().instantiateParticles = bulletExplosion;

            if (!BulletClone.GetComponent<Rigidbody>())
            {
                BulletClone.AddComponent<Rigidbody>();
            }

            BulletClone.GetComponent<Rigidbody>().AddForce(item.transform.forward * 170f, ForceMode.Impulse);

            BulletClone.transform.parent = null;
            BulletClone.transform.localScale = new Vector3(1, 1, 1);



            BulletClone.AddComponent<Bullets>();

            BulletClone.GetComponent<Bullets>().instantiateParticles = bulletExplosion;

            if (!BulletClone.GetComponent<Rigidbody>())
            {
                BulletClone.AddComponent<Rigidbody>();
            }

            BulletClone.GetComponent<Rigidbody>().AddForce(item.transform.forward * 100, ForceMode.Impulse);

            BulletClone.transform.parent = null;
            BulletClone.transform.localScale = new Vector3(1, 1, 1);


            fireSource.PlayOneShot(fireSound);
        }
    }

    void FixedUpdate()
    {
        if (interactingWith && planeSetUp.player != null && planeSetUp.player.GetComponent<SoldierAnimator>().isPlayer && !planeSetUp.player.GetComponent<SoldierAnimator>().masterController.isMobile)
        {
            float rollDeg = 0f;
            if (Input.GetKey(KeyCode.A)) {
                rollDeg += 2.5f;
            }
            if (Input.GetKey(KeyCode.D)) {
                rollDeg -= 2.5f;
            }
            Move(horizontal, vertical, rollDeg);
        }

        if (interactingWith) {
            anim.SetFloat("Speed", planeSetUp.currentSpeed.Remap(0, planeSetUp.maxSpeed, .1f, 2f));

            Sound();
            if (planeSetUp.player.GetComponent<SoldierAnimator>().isPlayer)
                Move();
        }
       
    }

    void Tricks()
    {
        if (horizontal >= 0.2f && Input.GetKeyDown(KeyCode.E))
        {
            anim.SetTrigger("barrelRollL");
        }

        if (horizontal <= -0.2f && Input.GetKeyDown(KeyCode.Q))
        {
            anim.SetTrigger("barrelRollR");
        }
    }

    void Crash()
    {
        planeSetUp.currentSpeed -= Time.deltaTime * planeSetUp.accelerationSpeed;
        planeSetUp.currentSpeed = Mathf.Clamp(planeSetUp.currentSpeed, planeSetUp.minSpeed, planeSetUp.maxSpeed);
        Vector3 movement = transform.forward * planeSetUp.currentSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + movement);

        if (!startTimer)
        {
            startTimer = true;
            StartCoroutine(CrashTime());
        }

    }

    bool startTimer = false;

    void OnCollisionEnter(Collision collision) {
        if (blowUpCoroutine == null &&
            (planeSetUp.currentSpeed > 20f) && !collision.gameObject.CompareTag("Gun")) {

            blowUpCoroutine = StartCoroutine(DelayedBlowUp(0.1f));
        }
    }
    Coroutine blowUpCoroutine;
    IEnumerator DelayedBlowUp(float seconds) {
        for (float i = 0f; i < seconds; i += Time.deltaTime)

            yield return null;
        blowUpCoroutine = null;
        BlowUp();
    }
    public int planeId;
    IEnumerator CrashTime()
    {
        var parachute = Instantiate(planeSetUp.parachute, transform.position, Quaternion.identity);
        parachute.GetComponent<ParachuteController>().SortPlayer(planeSetUp.player);
        parachute.GetComponent<ParachuteController>().CollidersEnabled(false);


        yield return new WaitForSeconds(Random.Range(2, 5));

        if (planeSetUp.particleSystem != null)
        {
            Instantiate(planeSetUp.particleSystem, transform.position, Quaternion.identity);
        }

        Destroy(this.gameObject);
    }
    float myHorizontal = 0f, myVertical=0f, myRolling = 0f;
    void Move() {
        var turnSpeed = planeSetUp.currentSpeed.Remap(planeSetUp.minSpeed, planeSetUp.maxSpeed, planeSetUp.turnSpeed / 4.2f, planeSetUp.turnSpeed);

        float turn = myHorizontal * turnSpeed *
            Time.deltaTime * planeSetUp.currentSpeed / 190f;
        float vertTurn = -myVertical * turnSpeed *
            Time.deltaTime * planeSetUp.currentSpeed / 250f;
        float rolling = myRolling * turnSpeed *
            Time.deltaTime * planeSetUp.currentSpeed / 170f;
            rb.isKinematic = false;
            rb.useGravity = false;
            if (planeSetUp.currentSpeed < planeSetUp.maxSpeed)
                planeSetUp.currentSpeed += Time.deltaTime * planeSetUp.accelerationSpeed;
            
        Vector3 newvec = Vector3.up * Time.deltaTime * planeSetUp.currentSpeed * 0.0225f;
        Vector3 movement = transform.forward * planeSetUp.currentSpeed * Time.deltaTime * 0.7f;
        rb.MovePosition(rb.position + movement + newvec);

        
        if (planeSetUp.currentSpeed < 20f) {
            if (vertTurn > 0f)
                vertTurn *= planeSetUp.currentSpeed / 20f;
        }
        Quaternion turnRot = Quaternion.Euler(vertTurn, turn, rolling);
        rb.MoveRotation(rb.rotation * turnRot);

        canLand = GroundCheck(groundCheckHeight);
        if (interactingWith)
            planeSetUp.player.transform.position = planeSetUp.playerSlot.position;
    }
    public void Move(float myHorizontal, float myVertical, float myRolling)
    {
        this.myHorizontal = myHorizontal;
        this.myVertical = myVertical;
        this.myRolling = myRolling;
    }
    Vector3 originalPosition;
    Quaternion originalRotation;
    void HideCursor()
    {
        if (!hideCursor)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (Input.GetMouseButtonUp(0))
            {
                hideCursor = true;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (Input.GetKeyUp(KeyCode.Escape))

            {
                hideCursor = false;
            }
        }
    }

    public void TakeDamage(float ammount)
    {
        if (planeSetUp.health <= 0)
        {
            BlowUp();
        }

        planeSetUp.health -= ammount;
    }

    public void BlowUp()
    {
        startedEngine = false;
        rb.isKinematic = true;
        anim.SetFloat("Speed", 0f);
        planeSetUp.currentSpeed = 0.01f;

        if (planeSetUp.player != null) {
            GameObject exp = Instantiate(planeSetUp.bullet.GetComponent<Bullets>().particles, transform.position, transform.rotation);

            planeSetUp.player.GetComponent<SoldierAnimator>().ExitVehicle(true);
            planeSetUp.player.transform.rotation = Quaternion.identity;
            planeSetUp.player.GetComponent<Rigidbody>().velocity = Vector3.zero;
            planeSetUp.player.GetComponent<SoldierAnimator>().health = -1f;
            planeSetUp.player.GetComponent<SoldierAnimator>().diedFromVehicle = true;

            exp.GetComponent<Explosion>().owner = planeSetUp.player.GetComponent<SoldierAnimator>();
            exp.GetComponent<Explosion>().damage = 999f;
        }
        
        health = maxHealth;
transform.position = originalPosition;


        interactingWith = false;
        transform.rotation = originalRotation;
    }

    public void SortPlayer(GameObject player)
    {
        planeSetUp.player = player;
        player.transform.parent = planeSetUp.playerSlot;
        player.transform.position = planeSetUp.playerSlot.position;
        
        planeSetUp.player.GetComponent<SoldierAnimator>().animator.gameObject.SetActive(false);

        if (player.GetComponent<SoldierAnimator>().isPlayer) {
            planeSetUp.cam.transform.GetChild(0).GetComponent<Camera>().enabled = true;
        }

        interactingWith = true;
        //rb.isKinematic = true;
        StartCoroutine(CoolDownGetInButton());
    }

    bool GroundCheck(float height)
    {
        RaycastHit hit;

        Debug.DrawRay((transform.position + transform.forward * 2), Vector3.down * height);

        if (Physics.Raycast((transform.position + transform.forward * 2), Vector3.down, out hit, height))
        {
            return true;
        }
        else
        {
            return false;

        }
    }

    IEnumerator CoolDownGetInButton()
    {
        yield return new WaitForSeconds(.2f);
        canGetOut = true;
    }
}

