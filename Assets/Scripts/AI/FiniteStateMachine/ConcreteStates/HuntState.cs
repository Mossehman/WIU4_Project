using System.Collections;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    [CreateAssetMenu(fileName = "HuntState", menuName = "AI/HuntState")]
    public class HuntState : BaseState
    {
        [SerializeField] float statetime = 20.0f; // Time before switching to another state
        [SerializeField] float attacktime = 0.7f; // Cooldown between attacks
        [SerializeField] float attackRange = 2f; // Range at which the creature can attack
        [SerializeField] float huntRange = 35f; // Range at which the creature will stop hunting
        [SerializeField] int damage = 20; // Damage dealt per attack
        [SerializeField] float hungerGainOnKill = 15f; // Hunger gained when killing a target
        [SerializeField] float hungerGainMultiplier = 0.25f; // Multiplier for hunger gained from target's hunger
        [SerializeField] float speedmod = 1.2f; // Speed multiplier while hunting

        public bool usesPathfinder;
        [ConditionalHide("usesPathfinder", true)]
        [SerializeField] float pathUpdateCooldown = 1.0f;

        private float currenttime;
        private float currentattacktime;
        private float currentPathUpdateTime;
        private CreatureInfo stats;
        Vector3 currentdirection;
        private bool hasAttackParam;
        public override void OnInit(FiniteStateMachine fsm)
        {
            base.OnInit(fsm);
            stats = fsm.GetComponent<CreatureInfo>();
            currenttime = statetime;
            currentattacktime = 0f;
            currentPathUpdateTime = 0f;

            hasAttackParam = false;
            if (stats.transform.childCount > 0)
            {
                foreach (AnimatorControllerParameter param in stats.transform.GetChild(0).GetComponent<Animator>().parameters)
                {
                    if (param.type == AnimatorControllerParameterType.Trigger && param.name == "Attack")
                        hasAttackParam = true;
                }
            }
        }

        public override void OnStateEnter(FiniteStateMachine fsm)
        {
            currenttime = statetime;
            currentattacktime = 0f;
            currentPathUpdateTime = 0f;
        }

        public override void OnStateLeave(FiniteStateMachine fsm)
        {
        }

        public override void WhileStateActive(FiniteStateMachine fsm)
        {
            if (stats.target == null)
            {
                fsm.SwapState("Idle");
                return;
            }

            if (currenttime > 0)
            {
                Vector3 dir = stats.target.transform.position - fsm.transform.position;

                if (usesPathfinder)
                {
                    if (currentPathUpdateTime <= 0f)
                    {
                        stats.pathfinder.GeneratePath(stats.target.transform.position);
                        currentPathUpdateTime = pathUpdateCooldown;
                    }
                    else
                    {
                        currentPathUpdateTime -= Time.deltaTime;
                    }
                    stats.Move(speedmod);
                }
                else
                {
                    currentdirection = Vector3.Slerp(currentdirection, dir.normalized, 0.1f);
                    stats.Move(currentdirection, speedmod);
                }
                // Handle attack logic
                if (currentattacktime > 0) currentattacktime -= Time.deltaTime;
                if (dir.sqrMagnitude <= attackRange && currentattacktime <= 0f)
                {
                    currentattacktime = attacktime;
                    if (hasAttackParam) stats.animator.SetTrigger("Attack");
                    AudioEventSystem.PlaySoundSmart(stats.attk, ref stats.voiceSource, default, default, true, true, 1, true);
                    if (stats.target.TryGetComponent<CreatureInfo>(out var creaturestats))
                    {
                        creaturestats.Health -= damage;
                        //creaturestats.fsm.ForceSwapState("Run", stats.gameObject);
                        AIBlackboardMediator.Instance.Notify(fsm.gameObject, "Im gonna kill you rahh", new object[] { fsm.gameObject });
                        if (creaturestats.Health <= 0f)
                        {
                            stats.hunger += creaturestats.hunger * hungerGainMultiplier;
                            stats.hunger += hungerGainOnKill;
                            stats.CurrentGroup?.ShareFood(hungerGainOnKill);
                        }
                        return;
                    }
                    else if (stats.target.TryGetComponent<PlayerStats>(out var playerstats))
                    {
                        playerstats.DecreaseStat(PlayerStats.StatType.Health, damage);
                        return;
                    }
                    else
                    {
                        Destroy(stats.target);
                        stats.hunger += 15;
                        stats.CurrentGroup?.ShareFood(15);
                    }
                }
                // Give up if target is too far
                else if (dir.sqrMagnitude >= huntRange * huntRange)
                {
                    fsm.SwapState("Search");
                }

                currenttime -= Time.deltaTime;
            }
            else
            {
                fsm.SwapState("Patrol");
            }
        }
    }
}
