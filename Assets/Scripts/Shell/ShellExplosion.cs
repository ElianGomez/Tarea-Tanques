using UnityEngine;
using System.Collections;

public class ShellExplosion : MonoBehaviour
{
	public LayerMask m_TankMask;
	public ParticleSystem m_ExplosionParticles;
	public AudioSource m_ExplosionAudio;
	public float m_MaxDamage = 100f;
	public float m_ExplosionForce = 1000f;
	public float m_MaxLifeTime = 2f;
	public float m_ExplosionRadius = 5f;

	private void Awake()
	{
		if (m_ExplosionAudio == null)
		{
			m_ExplosionAudio = GetComponent<AudioSource>();
		}

		if (m_ExplosionAudio != null)
		{
			m_ExplosionAudio.enabled = true;  // Asegurar que el AudioSource está habilitado
		}

		if (m_ExplosionParticles == null)
		{
			Debug.LogError("m_ExplosionParticles no está asignado en ShellExplosion.");
		}

		if (m_ExplosionAudio == null)
		{
			Debug.LogError("m_ExplosionAudio no está asignado en ShellExplosion.");
		}
	}

	private void Start()
	{
		Destroy(gameObject, m_MaxLifeTime);
	}

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("Bala impactó con: " + other.name);

		Collider[] colliders = Physics.OverlapSphere(transform.position, m_ExplosionRadius, m_TankMask);

		for (int i = 0; i < colliders.Length; i++)
		{
			Rigidbody targetRigidbody = colliders[i].GetComponent<Rigidbody>();

			if (!targetRigidbody)
			{
				Debug.Log("No se encontró Rigidbody en: " + colliders[i].name);
				continue;
			}

			TankHealth targetHealth = targetRigidbody.GetComponent<TankHealth>();

			if (!targetHealth)
			{
				Debug.Log("No se encontró TankHealth en: " + targetRigidbody.name);
				continue;
			}

			Debug.Log("Se encontró TankHealth en: " + targetHealth.gameObject.name);

			float damage = CalculateDamage(targetRigidbody.position);
			targetHealth.TakeDamage(damage);
		}
	}


	private IEnumerator ExplodeAndDestroy()
	{
		// Activar partículas de explosión
		if (m_ExplosionParticles != null)
		{
			m_ExplosionParticles.transform.parent = null;
			m_ExplosionParticles.Play();
		}

		// Reproducir sonido de explosión
		if (m_ExplosionAudio != null)
		{
			m_ExplosionAudio.Play();
			yield return new WaitForSeconds(m_ExplosionAudio.clip.length); // Esperar a que termine el sonido
		}

		// Destruir el proyectil y la explosión
		if (m_ExplosionParticles != null)
		{
			Destroy(m_ExplosionParticles.gameObject, m_ExplosionParticles.main.duration);
		}

		Destroy(gameObject);
	}

	private float CalculateDamage(Vector3 targetPosition)
	{
		Vector3 explosionToTarget = targetPosition - transform.position;
		float explosionDistance = explosionToTarget.magnitude;

		float relativeDistance = (m_ExplosionRadius - explosionDistance) / m_ExplosionRadius;
		float damage = relativeDistance * m_MaxDamage;

		// Asegurar que el daño mínimo no sea cero
		damage = Mathf.Max(10f, damage);

		return damage;
	}
}
