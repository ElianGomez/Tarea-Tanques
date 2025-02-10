using Complete;
using UnityEngine;
using UnityEngine.UI;

public class TankHealth : MonoBehaviour
{
	public float m_StartingHealth = 100f;
	public Slider m_Slider;
	public Image m_FillImage;
	public Color m_FullHealthColor = Color.green;
	public Color m_ZeroHealthColor = Color.red;
	public GameObject m_ExplosionPrefab;

	private AudioSource m_ExplosionAudio;
	private ParticleSystem m_ExplosionParticles;
	private float m_CurrentHealth;
	private bool m_Dead;

	public float maxHealth = 100f;
	private float currentHealth;

	public int vidasPerdidas = 0; // Contador de intentos fallidos
	public int maxIntentos = 5; // Máximo de intentos antes de terminar el juego

	[Range(0.1f, 1f)] // Permite ajustar el daño desde el Inspector de Unity
	public float damageMultiplier = 0.5f; // Reducir daño al 50% por defecto

	private void Awake()
	{
		m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();
		m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

		m_ExplosionParticles.gameObject.SetActive(false);
	}

	private void Start()
	{
		currentHealth = maxHealth;
	}

	private void OnEnable()
	{
		m_CurrentHealth = m_StartingHealth;
		m_Dead = false;

		SetHealthUI();
	}

	public void TakeDamage(float amount)
	{
		// Aplicar el multiplicador de daño
		float damageTaken = amount * damageMultiplier;

		Debug.Log("TakeDamage() en " + gameObject.name + " - Daño recibido: " + amount + " -> Daño aplicado: " + damageTaken);

		m_CurrentHealth -= damageTaken;
		SetHealthUI();

		Debug.Log("Nueva vida de " + gameObject.name + ": " + m_CurrentHealth);

		if (m_CurrentHealth <= 0f && !m_Dead)
		{
			Debug.Log(gameObject.name + " ha sido destruido.");
			OnDeath();
		}
	}

	private void SetHealthUI()
	{
		Debug.Log("Actualizando UI del slider. Vida actual: " + m_CurrentHealth);

		m_Slider.value = m_CurrentHealth;
		m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / maxHealth);
	}

	private void OnDeath()
	{
		m_Dead = true;

		// Colocar el prefab de explosión en la posición actual del tanque y activarlo.
		m_ExplosionParticles.transform.position = transform.position;
		m_ExplosionParticles.gameObject.SetActive(true);

		// Reproducir la explosión
		m_ExplosionParticles.Play();
		m_ExplosionAudio.Play();

		// Desactivar el tanque.
		gameObject.SetActive(false);
	}
}
