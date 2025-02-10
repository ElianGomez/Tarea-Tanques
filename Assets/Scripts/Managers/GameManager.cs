using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
	public int m_NumRoundsToWin = 5;
	public float m_StartDelay = 3f;
	public float m_EndDelay = 3f;
	public float maxGameTime = 240f; // ⏳ Tiempo máximo de 4 minutos

	public CameraControl m_CameraControl;
	public Text m_MessageText;
	public GameObject gameOverScreen;
	public Text winnerText;
	public Text scoreText;
	public Text timeText;
	public Text timerText; // ⏳ Texto de la UI para mostrar el tiempo restante

	private float gameStartTime;
	private float timeRemaining;
	private bool gameEnded = false;

	public GameObject m_TankPrefab;
	public TankManager[] m_Tanks;
	private int m_RoundNumber;
	private WaitForSeconds m_StartWait;
	private WaitForSeconds m_EndWait;
	private TankManager m_RoundWinner;
	private TankManager m_GameWinner;

	public Transform spawnPoint;
	public static GameManager instance;

	
	public GameObject enemyPrefab; // Prefab del enemigo
	public Transform[] enemySpawnPoints; // Puntos de generación de enemigos

	private void SpawnEnemies()
	{
		int numEnemies = Random.Range(6, 9); // Generar entre 3 y 6 enemigos

		for (int i = 0; i < numEnemies; i++)
		{
			Vector3 randomPosition = new Vector3(
				Random.Range(-10f, 10f),  // Rango X ajustado para un área más pequeña
				3f, // Asegurar que los enemigos aparezcan en la altura correcta
				Random.Range(-10f, 10f)   // Rango Z ajustado
			);

			Instantiate(enemyPrefab, randomPosition, Quaternion.identity);
		}
	}


	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	private void Start()
	{
		gameStartTime = Time.time;
		timeRemaining = maxGameTime; // ⏳ Inicializar el temporizador
		m_StartWait = new WaitForSeconds(m_StartDelay);
		m_EndWait = new WaitForSeconds(m_EndDelay);
		SpawnAllTanks();
		SetCameraTargets();
		StartCoroutine(GameLoop());
		StartCoroutine(GameTimer()); // ⏳ Iniciar el temporizador
		SpawnEnemies();
	}

	private IEnumerator GameTimer()
	{
		while (timeRemaining > 0)
		{
			timeRemaining -= Time.deltaTime;
			UpdateTimerUI();

			yield return null; // Espera un frame antes de continuar
		}

		if (!gameEnded) // ⏳ Si el tiempo se agota y no ha terminado el juego, forzar empate
		{
			EndGameDueToTime();
		}
	}
	private string EndMessage()
	{
		string message = "EMPATE!";

		if (m_RoundWinner != null)
		{
			message = m_RoundWinner.m_ColoredPlayerText + " GANA LA RONDA!";
		}

		message += "\n\n\n\n";

		for (int i = 0; i < m_Tanks.Length; i++)
		{
			message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " VICTORIAS\n";
		}

		if (m_GameWinner != null)
		{
			message = m_GameWinner.m_ColoredPlayerText + " GANA EL JUEGO!";
		}

		return message;
	}

	private void SpawnAllTanks()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			m_Tanks[i].m_Instance = Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation);
			m_Tanks[i].m_PlayerNumber = i + 1;
			m_Tanks[i].Setup();
		}
	}


	private void SetCameraTargets()
	{
		Transform[] targets = new Transform[m_Tanks.Length];

		for (int i = 0; i < targets.Length; i++)
		{
			targets[i] = m_Tanks[i].m_Instance.transform;
		}

		m_CameraControl.m_Targets = targets;
	}

	private void UpdateTimerUI()
	{
		if (timerText != null)
		{
			int minutes = Mathf.FloorToInt(timeRemaining / 60);
			int seconds = Mathf.FloorToInt(timeRemaining % 60);
			timerText.text = $"⏳ {minutes:00}:{seconds:00}";
		}
	}

	private void EndGameDueToTime()
	{
		gameEnded = true;
		Debug.Log("⏳ ¡Tiempo agotado! Ambos jugadores pierden.");

		if (gameOverScreen != null)
		{
			gameOverScreen.SetActive(true);
			winnerText.text = "EMPATE: TIEMPO AGOTADO";
			scoreText.text = "NINGÚN JUGADOR GANA";
			timeText.text = "Tiempo máximo alcanzado";
		}
		else
		{
			Debug.Log("No hay UI de Game Over configurada.");
		}

		StartCoroutine(RestartGame());
	}

	private IEnumerator GameLoop()
	{
		yield return StartCoroutine(RoundStarting());
		yield return StartCoroutine(RoundPlaying());
		yield return StartCoroutine(RoundEnding());

		if (!gameEnded)
		{
			if (m_GameWinner != null)
			{
				ShowGameOverScreen(m_GameWinner);
			}
			else
			{
				StartCoroutine(GameLoop());
			}
		}
	}

	private IEnumerator RoundStarting()
	{
		ResetAllTanks();
		DisableTankControl();
		m_CameraControl.SetStartPositionAndSize();
		m_RoundNumber++;
		m_MessageText.text = "ROUND " + m_RoundNumber;
		yield return m_StartWait;
	}

	private void ResetAllTanks()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			m_Tanks[i].Reset();
		}
	}

	private IEnumerator RoundPlaying()
	{
		EnableTankControl();
		m_MessageText.text = string.Empty;

		while (!OneTankLeft() && !gameEnded)
		{
			yield return null;
		}
	}

	private void EnableTankControl()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			if (m_Tanks[i].m_Instance != null)
			{
				m_Tanks[i].EnableControl();
			}
		}
	}

	private void DisableTankControl()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			if (m_Tanks[i].m_Instance != null)
			{
				m_Tanks[i].DisableControl();
			}
		}
	}

	private IEnumerator RoundEnding()
	{
		DisableTankControl();
		m_RoundWinner = GetRoundWinner();

		if (m_RoundWinner != null)
		{
			m_RoundWinner.m_Wins++;
		}

		m_GameWinner = GetGameWinner();
		string message = EndMessage();
		m_MessageText.text = message;

		yield return m_EndWait;
	}

	private void ShowGameOverScreen(TankManager winner)
	{
		gameEnded = true;
		float elapsedTime = Time.time - gameStartTime;
		string formattedTime = FormatTime(elapsedTime);

		if (gameOverScreen != null)
		{
			gameOverScreen.SetActive(true);
			winnerText.text = "🏆 PLAYER " + winner.m_PlayerNumber + " GANA EL JUEGO!";
			scoreText.text = "RONDAS GANADAS: " + winner.m_Wins;
			timeText.text = "TIEMPO TOTAL: " + formattedTime;
		}
		else
		{
			Debug.Log("No hay UI de Game Over configurada.");
		}

		StartCoroutine(RestartGame());
	}

	private IEnumerator RestartGame()
	{
		yield return new WaitForSeconds(5f);
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	private string FormatTime(float time)
	{
		int minutes = Mathf.FloorToInt(time / 60);
		int seconds = Mathf.FloorToInt(time % 60);
		return string.Format("{0:00}:{1:00}", minutes, seconds);
	}

	private bool OneTankLeft()
	{
		int numTanksLeft = 0;
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			if (m_Tanks[i].m_Instance.activeSelf)
			{
				numTanksLeft++;
			}
		}
		return numTanksLeft <= 1;
	}

	private TankManager GetRoundWinner()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			if (m_Tanks[i].m_Instance.activeSelf)
			{
				return m_Tanks[i];
			}
		}
		return null;
	}

	private TankManager GetGameWinner()
	{
		for (int i = 0; i < m_Tanks.Length; i++)
		{
			if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
			{
				return m_Tanks[i];
			}
		}
		return null;
	}
}
