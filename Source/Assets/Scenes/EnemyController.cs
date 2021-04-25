using System;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;



public class EnemyController : MonoBehaviour
{
    public Transform[] points;
    private int destPoint = 0;
    private NavMeshAgent agent;

    private Rigidbody rb;
    private bool knockBack;
    private Vector3 direction;

    Animator anim;
    Vector2 smoothDeltaPosition = Vector2.zero;
    Vector2 velocity = Vector2.zero;
    
    
    // Use this for initialization
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        // Disabling auto-braking allows for continuous movement
        // between points (ie, the agent doesn't slow down as it
        // approaches a destination point).
        agent.autoBraking = true;
        GotoNextPoint();

        //rb = GetComponent<Rigidbody>();

        knockBack = false;

        anim = GetComponent<Animator>();
        agent.updatePosition = false;

        rb = GetComponent<Rigidbody>();
    }


    void GotoNextPoint()
    {
        // Returns if no points have been set up
        if (points.Length == 0)
            return;

        // Set the agent to go to the currently selected destination.
        agent.destination = points[destPoint].position;

        // Choose the next point in the array as the destination,
        // cycling to the start if necessary.
        destPoint = (destPoint + 1) % points.Length;
    }


    void Update()
    {
        // Choose the next destination point when the agent gets
        // close to the current one.
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GotoNextPoint();


        Vector3 worldDeltaPosition = agent.nextPosition - transform.position;

        // Map 'worldDeltaPosition' to local space
        float dx = Vector3.Dot(transform.right, worldDeltaPosition);
        float dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        Vector2 deltaPosition = new Vector2(dx, dy);

        // Low-pass filter the deltaMove
        float smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        // Update velocity if delta time is safe
        if (Time.deltaTime > 1e-5f)
            velocity = smoothDeltaPosition / Time.deltaTime;

        bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

        // Update animation parameters
        anim.SetBool("Move", shouldMove);
        anim.SetFloat("velX", velocity.x);
        anim.SetFloat("velY", velocity.y);

        
    }

    private void FixedUpdate()
    {
        if (knockBack)
        {
            agent.velocity = -direction.normalized * 8;
        }
        
    }

    void OnTriggerEnter(Collider other)
    {            Debug.Log("triggered");

        direction = other.transform.position - transform.position; //Always knocks ememy in the direction the main character is facing

        if (other.CompareTag("hitbox") && Input.GetMouseButton(0))
        {
            Debug.Log("Hit");
            agent.updatePosition = false;

            StartCoroutine(KnockBack());
        }
        
        agent.updatePosition = false;

        // Pull agent towards character
        if ((agent.nextPosition - transform.position).magnitude > agent.radius)
            agent.nextPosition = transform.position + 0.9f * (agent.nextPosition - transform.position);
    }


    IEnumerator KnockBack()
    {
        knockBack = true;
        agent.speed = 6;
        agent.angularSpeed = 540; //Keeps the enemy facing forwad rther than spinning
        agent.acceleration = 6;


        yield return new WaitForSeconds(0.2f); //Only knock the enemy back for a short time    

        //Reset to default values
        knockBack = false;
        agent.speed = 2;
        agent.angularSpeed = 180;
        agent.acceleration = 2;
        
        
    }

    void OnAnimatorMove()
    {
        // Update postion to agent position
        transform.position = agent.nextPosition;
    }
}