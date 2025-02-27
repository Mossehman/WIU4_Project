using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueSystem
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Create New Dialogue")]
    public class Dialogue : ScriptableObject
    {
        public string _dialogueID;
        public string[] _lines;
    }
}