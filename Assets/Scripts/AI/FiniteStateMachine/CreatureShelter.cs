using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine {
    [RequireComponent(typeof(BoxCollider))]
    public class CreatureShelter : MonoBehaviour
    {
        [SerializeField] public LayerMask creatureHome;
        [SerializeField] List<CreatureInfo> housedCreatures = new();

        // Start is called before the first frame update
        void Start()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            box.includeLayers = creatureHome;
            box.isTrigger = true;
        }

        // Update is called once per frame
        void Update()
        {
            foreach (CreatureInfo creature in housedCreatures)
            {
                if (creature == null) continue;
                if (creature.hunger <= 80f) creature.hunger += Time.deltaTime * 2f;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            CreatureInfo creature = other.GetComponent<CreatureInfo>();
            housedCreatures.Add(creature);
            creature.isSheltered = true;
        }

        private void OnTriggerExit(Collider other)
        {
            CreatureInfo creature = other.GetComponent<CreatureInfo>();
            housedCreatures.Remove(creature);
            creature.isSheltered = false;
        }
    }
}