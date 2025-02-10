using System;
using System.Xml.Linq;
using UnityEngine;

[Serializable]
public class TankManager
{
	public Color m_PlayerColor; //Color para el tanque
	public Transform m_SpawnPoint; //Posición y direción en la que se generará
	[HideInInspector] public int m_PlayerNumber; //Especifica con qué jugador
	[HideInInspector] public string m_ColoredPlayerText; //String que reprsent
	[HideInInspector] public GameObject m_Instance; //Refernecia a la instanci
	[HideInInspector] public int m_Wins; //Número de victorias del jugador

	private TankMovement m_Movement; //Referencia al script de movimiento del
    private TankShooting m_Shooting; //Referencia al script de disparo del tan
    private GameObject m_CanvasGameObject;

	public float maxHealth = 100f;
	private float currentHealth;
	public void Setup()
	{
		currentHealth = maxHealth;

		m_Movement = m_Instance.GetComponent<TankMovement>();
		m_Shooting = m_Instance.GetComponent<TankShooting>();
		m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

		// Asignar PlayerNumber según el SpawnPoint
		if (m_SpawnPoint.position == GameObject.Find("SpawnPoint1").transform.position)
		{
			m_PlayerNumber = 1; // Tanque en SpawnPoint1 usa WASD
		}
		else
		{
			m_PlayerNumber = 2; // Otro tanque usa flechas
		}

		// Asignar el número de jugador a los scripts de movimiento y disparo
		m_Movement.m_PlayerNumber = m_PlayerNumber;
		m_Shooting.m_PlayerNumber = m_PlayerNumber;

		// Crear un string con el color del tanque que diga PLAYER 1, etc.
		m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

		// Ajustar el color de los materiales del tanque
		MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();

		for (int i = 0; i < renderers.Length; i++)
		{
			renderers[i].material.color = m_PlayerColor;
		}
	}



	public void DisableControl()
    {
		m_Movement.enabled = false;
		m_Shooting.enabled = false;
		m_CanvasGameObject.SetActive(false);
	}


	public void TakeDamage(float damage)
	{
		currentHealth -= damage;
		Debug.Log("Jugador herido! Vida restante: " + currentHealth);

		if (currentHealth <= 0)
		{
			Die();
		}
	}


	void Die()
	{
		Debug.Log("El jugador ha muerto!");
		// Aquí puedes agregar animación de muerte, respawn o game over
	}
	public void EnableControl()
    {
		m_Movement.enabled = true;
		m_Shooting.enabled = true;
		m_CanvasGameObject.SetActive(true);
	}


    public void Reset()
    {
		m_Instance.transform.position = m_SpawnPoint.position;
		m_Instance.transform.rotation = m_SpawnPoint.rotation;
		m_Instance.SetActive(false);
		m_Instance.SetActive(true);
	}
}
