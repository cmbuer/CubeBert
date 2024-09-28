/* Copyright (C) 2024 Christopher Buer */

using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/* The Game class uses a variant of the Singleton pattern.
 * Note that the game object and component are placed in the scene manually
 * rather than being instantiated on first access.
 * To enforce a single instance, DisallowMultipleComponent is used to
 * prevent multiple instances of the component on a single GameObject and
 * a special handler is used in Awake() to destroy any Game objects awakened
 * after one already exists.
 */
[DisallowMultipleComponent]
public class Game : MonoBehaviour
{
    public TextMeshProUGUI healthText;

    private struct CubeBertComponents
    {
        public GameObject gameObject;
        public CubeBert cubeBert;
        public CubeBertMove cubeBertMove;
    }

    private static Game instance;
    private bool gameOver;
    private float gameOverTime;
    private CubeBertComponents cbComponents;
    private const float restartDelay = 2.0f;

    public static Game GetInstance()
    {
        if (instance == null)
        {
            instance = FindObjectOfType<Game>();
        }
        return instance;
    }

    public static Color GetHealthStatus()
    {
        return instance.healthText.color;
    }

    public Vector3 GetCubertTargetPosition()
    {
        return GetCubeBertComponents().cubeBertMove.GetChestPosition();
    }

    public void HandleCubertCollision(CubeBert cubeBert, Collider other)
    {
        if (other.gameObject.tag == "Laser")
        {
            cubeBert.Damage(5);
        }
        if (other.gameObject.tag == "HealthCube")
        {
            cubeBert.Heal(10);
        }
    }

    private CubeBertComponents GetCubeBertComponents()
    {
        if (cbComponents.gameObject == null)
        {
            cbComponents.cubeBert = FindObjectOfType<CubeBert>();
            cbComponents.gameObject = cbComponents.cubeBert.gameObject;
            cbComponents.cubeBertMove = cbComponents.cubeBert.gameObject.GetComponent<CubeBertMove>();
        }
        return cbComponents;
    }

    private void GenerateLevel()
    {

    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        if (gameOver)
        {
            gameOverTime += Time.deltaTime;
            if (gameOverTime > restartDelay)
            {
                Restart();
            }
        }
        else
        {
            CubeBertComponents cbc = GetCubeBertComponents();

            // Check if cubebert has fallen to his death
            if (cbc.gameObject.transform.position.y < -20)
            {
                cbc.cubeBert.Kill();
            }

            int cubeBertHealth = cbc.cubeBert.GetHealth();
            if (cubeBertHealth <= 0)
            {
                // Cubebert is dead (TODO death effect)
                cbc.gameObject.SetActive(false);
                healthText.text = "DEAD";
                healthText.color = Color.red;
                EndGame();
            }
            else
            {
                // Cubebert is still alive: update the HUD
                healthText.text = string.Format("HEALTH: {0}", cubeBertHealth);
                if (cubeBertHealth < 40)
                {
                    healthText.color = Color.red;                    
                }
                else if (cubeBertHealth < 65)
                {
                    healthText.color = Color.yellow;
                }
                else if (cubeBertHealth < 90)
                {
                    healthText.color = Color.green;
                }
                else
                {
                    healthText.color = Color.cyan;
                }
            }
        }
    }

    private void EndGame()
    {
        gameOver = true;
        gameOverTime = 0;
    }

    private void Restart()
    {
        gameOver = false;
        gameOverTime = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
