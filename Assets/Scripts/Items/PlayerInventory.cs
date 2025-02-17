using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace Player.Inventory
{
    enum SortingType
    {
        RECENTLY_ADDED,
        ALPHABETICAL,
        HEAVIEST,
        LIGHTEST,
    }

    public class PlayerInventory : MonoBehaviour
    {
        [SerializeField] private List<BaseItem> _items;
        [SerializeField] public float _baseWeight;
        [SerializeField] public float _baseMaxWeight;
        [SerializeField] private float _currentWeight;

        void Start()
        {
            _items = new List<BaseItem>();
        }

        void Update()
        {

        }

        private void RenderInventory()
        {

        }

        private void SortInventory(SortingType type)
        {

        }
    }
}
