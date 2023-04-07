using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Movement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float speed;
    [SerializeField] float rotationSmoothTime;

    [Header("Gravity")]
    [SerializeField] float gravity = 9.8f;
    [SerializeField] float gravityMultiplier = 2;
    [SerializeField] float groundedGravity = -0.5f;
    [SerializeField] float jumpHeight = 3f;
    float velocityY;
    public AudioSource audioSource;
    public AudioClip bark;
    public Text textPrvi;
    public Text gameOverText;
    public Text scoreText;
    public GameObject primjerPeseka;
    public Transform[] spawnPoints;


    CharacterController controller;
    Camera cam;

    float currentAngle;
    float currentAngleVelocity;
    private float beginSpeed;
    private float sprintSpeed;
    private GameObject[] psici;
    private MaliPas maliPas;
    private string difficulty= null;
    private float vrijeme;
    private int brojSpawnova=0;
    private float pocetnoVrijeme;
    private float score;
    private bool drown;
    private bool shown;
    private readonly Dictionary<string, float> dificultyDatabase = new Dictionary<string, float>()
{
    { "Easy", 3000.0f },
    { "Normal", 240.0f },
    { "Hard", 180.0f },
    {"Very Hard",120.0f },
    {"Impossible",60.0f}
};

    private void Start()
    {
        score = StateNameController.totalScore;
        drown = false;
        shown = true;
        gameOverText.enabled = false;
        if(StateNameController.difficulty != null)
        {
            difficulty = StateNameController.difficulty;
            vrijeme = dificultyDatabase[difficulty];
            pocetnoVrijeme = vrijeme;
            switch (difficulty)
            {
                case "Easy":
                    brojSpawnova = 1;break;
                case "Normal":
                    brojSpawnova = 2; break;
                case "Hard":
                    brojSpawnova = 3; break;
                case "Very Hard":
                    brojSpawnova = 4; break;
                case "Impossible":
                    brojSpawnova = 5; break;
            }
        }
        controller = GetComponent<CharacterController>();
        cam = Camera.main;
        beginSpeed = speed;
        if(brojSpawnova != 0)
        {
            for(int i=0; i<brojSpawnova; i++)
            {
                int spawnPointIndex = UnityEngine.Random.Range(0,spawnPoints.Length);
                Instantiate(primjerPeseka, spawnPoints[spawnPointIndex].position, spawnPoints[spawnPointIndex].rotation);
                var dest = new List<Transform>(spawnPoints);
                dest.RemoveAt(spawnPointIndex);
                spawnPoints= dest.ToArray();
            }
        }
        psici= GameObject.FindGameObjectsWithTag("BebaPas");
    }

    private void Update()
    {
        scoreText.text = "Score: " + score;
        if (vrijeme > 0 && psici.Length != 0)
        {
            vrijeme -= Time.deltaTime;
            
        }
        if (difficulty != null && difficulty != "Easy") textPrvi.text = "Time left: "+vrijeme.ToString() + "\nPups remaning: " + psici.Length;
        else textPrvi.text = "";
        if(vrijeme <= 0 && difficulty != "Easy")
        {
            score = 0f;
            gameOverText.enabled = true;
            Cursor.visible = true;
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }

        if (!gameOverText.enabled || (winner() && SceneManager.GetActiveScene().name=="NormalnaSuma"))
        {
            HandleMovement();
            HandleGravityAndJump();
        }

        float solution = Mathf.Sqrt(Mathf.Pow(Input.GetAxis("Horizontal"),2) + Mathf.Pow(Input.GetAxis("Vertical"),2));
        gameObject.GetComponent<Animator>().SetFloat("MoveValue",Mathf.Abs(solution));

        if (Input.GetMouseButtonDown(0))
        {
            audioSource.PlayOneShot(bark);
            foreach (GameObject psic in psici)
            {
                maliPas= psic.GetComponent<MaliPas>();
                maliPas.GlavniLaj = true;
            }
        }

        winner();

        if (drown)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
    }

    private void HandleMovement()
    {
        Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = beginSpeed * 2;
            gameObject.GetComponent<Animator>().SetBool("Pokrenuo",true);

        }
        else
        {
            speed = beginSpeed;
            gameObject.GetComponent<Animator>().SetBool("Pokrenuo", false);
        }

        if (movement.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            currentAngle = Mathf.SmoothDampAngle(currentAngle, targetAngle, ref currentAngleVelocity, rotationSmoothTime);
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);

            Vector3 rotatedMovement = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
            controller.Move(rotatedMovement * speed * Time.deltaTime);
        }
    }

    void HandleGravityAndJump()
    {
        if (controller.isGrounded && velocityY < 0f)
            velocityY = groundedGravity;

        if (controller.isGrounded && Input.GetKeyDown(KeyCode.Space))
        {
            velocityY = Mathf.Sqrt(jumpHeight * 2f * gravity);
            gameObject.GetComponent<Animator>().SetTrigger("Skok");
        }

        velocityY -= gravity * gravityMultiplier * Time.deltaTime;
        controller.Move(Vector3.up * velocityY * Time.deltaTime);
    }

    bool winner()
    {
        if(vrijeme > 0 && psici.Length == 0)
        {
            Cursor.visible = true;
            StateNameController.totalScore = score;
            if (SceneManager.GetActiveScene().name == "NormalnaSuma")
            {
                GameObject.FindWithTag("Vrata").GetComponent<Collider>().isTrigger = true;
                if(shown) StartCoroutine(korutina());
                shown = false;
                return true;
            }
            else
            {
                gameOverText.enabled = true;
                gameOverText.text = "Winner!\nPress Enter to return to main menu";
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    SceneManager.LoadScene("MainMenu");
                }
                return true;
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "BebaPas")
        {
            score += (pocetnoVrijeme / vrijeme) * 50.0f * ((float)brojSpawnova);
            Destroy(other.gameObject);
            int makni=psici.Length;
            for(int j=0; j < psici.Length; j++)
            {
                if (psici[j].transform == other.gameObject.transform) makni = j;
            }
            var dest = new List<GameObject>(psici);
            dest.RemoveAt(makni);
            psici = dest.ToArray();
        }
        if(other.gameObject.tag == "Respawn")
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        if(other.gameObject.tag == "Voda")
        {
            gameOverText.enabled=true;
            gameOverText.text = "You drowned\nPress Enter to return to main menu";
            drown = true;
        }
        if(other.gameObject.tag == "Vrata")
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    IEnumerator korutina()
    {
        gameOverText.enabled = true;
        gameOverText.text = "Winner!\nGo to the door.";
        yield return new WaitForSeconds(3f);
        gameOverText.enabled = false;
    }
}
