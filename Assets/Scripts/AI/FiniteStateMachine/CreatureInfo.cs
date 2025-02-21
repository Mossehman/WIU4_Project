using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    // Temporary storage for entity stats
    public class CreatureInfo : MonoBehaviour
    {
        [Header("Basic Info")]
        private float maxhealth;
        [SerializeField] private float health;
        public float hunger = 80;
        public GameObject target;
        public GameObject segs;
        public FiniteStateMachine fsm;

        public delegate void EntityDeath(CreatureInfo info);
        public delegate void EntityHurt(CreatureInfo info);
        public event EntityDeath OnEntityDeath;
        public event EntityHurt OnEntityHurt;

        public float Health { 
            get { 
                return health; 
            } 
            set { 
                health = value;
                if (value < health)
                {
                    OnEntityHurt?.Invoke(this);
                }
                if (health <= 0f)
                {
                    OnEntityDeath?.Invoke(this);
                }
            } 
        }

        [Header("Advanced Info")]
        public Group CurrentGroup;
        public Vector3 FlockDirection = Vector3.zero;
        private Vector3 currentFlockDirection = Vector3.zero;
        public float FlockSpeed = 0.05f;
        public float FlockTightness = 1.0f;
        public float MaxFlockDistance = 5.0f;
        public float ProductionCooldown = 30.0f;
        private float ProductionDuration = 30.0f;

        public CreatureShelter assignedHome;
        public bool isSheltered = false;
        public bool isDead = false;

        [Header("Audio Info")]
        public string goes = string.Empty;
        public string walk = string.Empty;
        public string rest = string.Empty;
        public string hurt = string.Empty;
        public string dead = string.Empty;

        private CharacterController characterController;
        private Animator animator;
        private AudioSource sfxSource;
        public AudioSource voiceSource;
        
        private void Start()
        {
            fsm = GetComponent<FiniteStateMachine>();
            characterController = GetComponent<CharacterController>();
            sfxSource = gameObject.AddComponent<AudioSource>();
            voiceSource = gameObject.AddComponent<AudioSource>();
            if (transform.childCount > 0)
                animator = transform.GetChild(0).GetComponent<Animator>();
            maxhealth = health;
            ProductionDuration = ProductionCooldown;
            OnEntityHurt += (CreatureInfo _) =>
            {
                AudioManager.Instance.PlayNonSpamAudio(hurt, ref sfxSource, default, true, 1, true);
            };

            OnEntityDeath += (CreatureInfo _) =>
            {
                if (!isDead)
                {
                    isDead = true;
                    AudioManager.Instance.PlayNonSpamAudio(dead, ref sfxSource, default, true, 1, true);
                    if (animator != null) animator.SetBool("isMoving", false);
                }
            };
        }

        private void Update()
        {
            if (hunger <= 0 || isDead)
            {
                Destroy(gameObject, 10f);
                isDead = true;
                characterController.Move(Vector3.zero);
                return;
            }
            if (hunger >= 90 && isSheltered && ProductionDuration <= 0f)
            {
                ProductionDuration = ProductionCooldown;
                hunger -= 30;
                Debug.Log("B");
                Instantiate(segs).GetComponent<CreatureInfo>().hunger = 70;
            }
            ProductionDuration -= Time.deltaTime;
            hunger = Mathf.Clamp(hunger, 0f, 120f);
            health = Mathf.Clamp(health, 0f, maxhealth);

            if (CurrentGroup != null)
            {
                if (fsm.GetCurrentStateName() != "Hunt" || fsm.GetCurrentStateName() != "Run")
                {
                    UpdateFlockingBehaviour();
                    if (FlockDirection.sqrMagnitude > 0)
                    {
                        currentFlockDirection = (CurrentGroup != null ?
                           (CurrentGroup.Leader == this ? Vector3.zero : FlockDirection * FlockSpeed)
                           : Vector3.zero) * 0.01f;
                        if (currentFlockDirection.sqrMagnitude > 0)
                            characterController.Move(currentFlockDirection);
                    }
                }
            }
            if (animator != null)
            {
                if (characterController.velocity.sqrMagnitude > 0f)
                {
                    animator.SetBool("isMoving", true);
                    
                    AudioManager.Instance.PlayNonSpamAudio(walk, ref sfxSource, default, true, characterController.velocity.magnitude, true);
                }
                else
                    animator.SetBool("isMoving", false);

                animator.SetFloat("speedMod", characterController.velocity.magnitude);
            }
        }

        private void UpdateFlockingBehaviour()
        {
            if (CurrentGroup.Leader == null || CurrentGroup.Leader == gameObject) return;

            // Cohesion: Move towards the leader
            Vector3 leaderPosition = CurrentGroup.Leader.transform.position;
            Vector3 cohesion = (leaderPosition - transform.position).normalized;

            // Separation: Avoid crowding local flockmates
            Vector3 separation = Vector3.zero;
            foreach (var member in CurrentGroup.Members)
            {
                if (member != null)
                {
                    if (member != gameObject && Vector3.Distance(transform.position, member.transform.position) < 2f)
                    {
                        separation -= (member.transform.position - transform.position).normalized;
                    }
                }
            }

            FlockDirection = (cohesion + separation).normalized;
        }

        public void Move(Vector3 dir)
        {
            Vector3 overalldir = dir;

            if (overalldir.sqrMagnitude > 0)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(overalldir, Vector3.up), 0.01f);

            characterController.Move(overalldir * 0.01f);
        }

        private void LateUpdate()
        {

            characterController.Move(Vector3.down * 0.05f);
            if (currentFlockDirection.sqrMagnitude > 0)
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(currentFlockDirection, Vector3.up), 0.01f);
        }

        private void OnDestroy()
        {
            if (assignedHome != null) assignedHome.numOfRegisteredCreatures--;
        }

        private void OnDrawGizmos()
        {
            Vector3 upoffset = Vector3.zero;
            if (target != null)
            {
                if (fsm.GetCurrentStateName() == "Run")
                {
                    Gizmos.color = new Color(1, 0, 0, 0.5f);
                    Gizmos.DrawLine(transform.position + upoffset, target.transform.position + upoffset);
                    upoffset += Vector3.up;
                }
                else if (fsm.GetCurrentStateName() == "Hunt")
                {
                    Gizmos.color = new Color(0, 1, 1, 0.5f);
                    Gizmos.DrawLine(transform.position + upoffset, target.transform.position + upoffset);
                    upoffset += Vector3.up;
                }
            }
            if (CurrentGroup != null)
            {
                if (CurrentGroup.Leader != null)
                {
                    Gizmos.color = new Color(0, 1, 0, 0.5f);
                    Gizmos.DrawLine(transform.position + upoffset, CurrentGroup.Leader.transform.position + upoffset);
                    if (CurrentGroup.Leader == this)
                    {
                        Gizmos.color = new Color(0, 0, 1, 0.5f);
                        Gizmos.DrawSphere(transform.position + new Vector3(0, 6, 0), 0.5f);
                    }
                    upoffset += Vector3.up;
                }
            }
            if (assignedHome != null)
            {
                Gizmos.color = new Color(1, 1, 1, 0.25f);
                Gizmos.DrawLine(transform.position + upoffset, assignedHome.transform.position + upoffset);

                upoffset += Vector3.up;

            }
            if (hunger < 100f)
                Gizmos.color = new Color(hunger <= 0f ? 0f : 1 - (hunger * 0.01f), hunger <= 0f ? 0f : hunger * 0.01f, 0, 0.5f);
            else
                Gizmos.color = new Color(0, 1 -((hunger-100f) * 0.01f), ((hunger-100f) * 0.01f), 0.5f);
            Gizmos.DrawCube(transform.position + new Vector3(0, 5, 0), new Vector3(1, 1, 1));


            Gizmos.color = new Color(health <= 0f ? 0f : 1 - (health / maxhealth), health <= 0f ? 0f : health / maxhealth, 0, 0.5f);
            Gizmos.DrawCube(transform.position + new Vector3(0, 4, 0), new Vector3(1, 1, 1));
            Gizmos.color = new Color(0, 0, 1, 0.25f);
            Gizmos.DrawLine(transform.position, (transform.position + transform.forward * 5f));
        }
    }
}