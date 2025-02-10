using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
	public Transform[] patrolPoints; // Puntos de patrullaje
	public float patrolSpeed = 3f; // Velocidad de patrullaje
	public float detectionRange = 7f; // Rango en el que detecta tanques
	public float pushForce = 600f; // Fuerza con la que empuja a los tanques

	private Rigidbody rb;
	private Transform targetTank;
	private int currentPatrolIndex = 0;
	private bool isChasing = false;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		GoToNextPatrolPoint();
	}

	private void GoToNextPatrolPoint()
	{
		if (patrolPoints.Length == 0) return; // Si no hay puntos de patrulla, salir

		Transform targetPoint = patrolPoints[currentPatrolIndex]; // Selecciona el siguiente punto
		Vector3 direction = (targetPoint.position - transform.position).normalized;
		rb.velocity = direction * patrolSpeed; // Mueve al enemigo hacia el punto

		if (Vector3.Distance(transform.position, targetPoint.position) < 1f) // Si llega al punto
		{
			currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length; // Cambia al siguiente punto
		}
	}


	private void Update()
	{
		DetectTanks();

		if (isChasing && targetTank != null)
		{
			ChaseTank();
		}
		else
		{
			Patrol();
		}
	}

	private void DetectTanks()
	{
		GameObject[] tanks = GameObject.FindGameObjectsWithTag("Tank");

		foreach (GameObject tank in tanks)
		{
			float distance = Vector3.Distance(transform.position, tank.transform.position);

			if (distance < detectionRange)
			{
				targetTank = tank.transform;
				isChasing = true;
				return;
			}
		}

		targetTank = null;
		isChasing = false;
	}

	private void ChaseTank()
	{
		if (targetTank != null)
		{
			Vector3 direction = (targetTank.position - transform.position).normalized;
			rb.velocity = direction * patrolSpeed * 1.5f; // Se mueve más rápido al perseguir
		}
	}

	private void Patrol()
	{
		if (patrolPoints.Length == 0) return;

		Transform targetPoint = patrolPoints[currentPatrolIndex];
		Vector3 direction = (targetPoint.position - transform.position).normalized;
		rb.velocity = direction * patrolSpeed;

		if (Vector3.Distance(transform.position, targetPoint.position) < 1f)
		{
			currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.gameObject.CompareTag("Tank"))
		{
			Rigidbody tankRb = collision.gameObject.GetComponent<Rigidbody>();

			if (tankRb != null)
			{
				Vector3 pushDirection = (collision.transform.position - transform.position).normalized;
				tankRb.AddForce(pushDirection * pushForce);
			}
		}
	}
	// Start is called before the first frame update
	
}
