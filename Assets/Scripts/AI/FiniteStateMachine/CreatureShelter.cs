using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine {
    [RequireComponent(typeof(BoxCollider))]
    public class CreatureShelter : MonoBehaviour
    {
        public LayerMask creatureHome;
        [SerializeField] List<CreatureInfo> housedCreatures = new();
        public int maxHousingSpace = 10;
        public int numOfRegisteredCreatures = 0;

        void Start()
        {
            BoxCollider box = GetComponent<BoxCollider>();
            box.includeLayers = creatureHome;
            box.isTrigger = true;
        }

        void Update()
        {
            foreach (CreatureInfo creature in housedCreatures)
            {
                if (creature == null) { continue; }
                if (creature.hunger <= 80f) creature.hunger += Time.deltaTime * 2f;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & creatureHome) != 0)
            {
                CreatureInfo creature = other.GetComponent<CreatureInfo>();
                housedCreatures.Add(creature);
                creature.isSheltered = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (((1 << other.gameObject.layer) & creatureHome) != 0)
            {
                CreatureInfo creature = other.GetComponent<CreatureInfo>();
                housedCreatures.Remove(creature);
                creature.isSheltered = false;
            }
        }
    }
}