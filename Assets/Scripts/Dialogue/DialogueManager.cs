using QuestSystem;
using System.Collections;
using TMPro;
using UnityEngine;

namespace DialogueSystem
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Create New Dialogue")]
    public class Dialogue : ScriptableObject
    {
        public string _dialogueID;
        public string[] _lines;
    }

    public class DialogueManager : MonoBehaviour
    {
        [Header("Dialogue Logic")]
        [SerializeField]    private float               _textSpeed;
                            private int                 _index;
                            private Dialogue            _currentDialogue;

        [Header("Dialogue UI")]
        [SerializeField]    private TextMeshProUGUI     _dialogueBox;

        private void Start()
        {
            EventManager.CreateEvent("OnQuestComplete");
            EventManager.CreateEvent("OnQuestStart");
        }

        void FixedUpdate()
        {
            if (Input.GetMouseButtonDown(0) && _currentDialogue != null && _index < _currentDialogue._lines.Length)
            {
                if (_dialogueBox.text == _currentDialogue._lines[_index])
                {
                    NextLine();
                }
                else
                {
                    StopCoroutine(PlayDialogue(_currentDialogue));
                    _dialogueBox.text = _currentDialogue._lines[_index];
                }
            }
        }

        private void OnEnable()
        {
            EventManager.Connect("OnQuestComplete", OnQuestComplete);
            EventManager.Connect("OnQuestStart", OnQuestStart);
        }

        private void OnDisable()
        {
            EventManager.Disconnect("OnQuestComplete", OnQuestComplete);
            EventManager.Disconnect("OnQuestStart", OnQuestStart);
        }

        private void OnQuestComplete(object[] args)
        {
            Quest quest = (Quest)args[0];
            if (quest != null && _currentDialogue != quest._dialogueUponCompletion && quest.GetQuestStatus() == questStatus.COMPLETED)
            {
                _index = 0;
                _dialogueBox.text = string.Empty;
                _currentDialogue = quest._dialogueUponCompletion;
                StartCoroutine(PlayDialogue(quest._dialogueUponCompletion));
            }
        }

        private void OnQuestStart(object[] args)
        {
            Quest quest = (Quest)args[0];
            if (quest != null && _currentDialogue != quest._dialogueUponStart && quest.GetQuestStatus() == questStatus.IN_PROGRESS)
            {
                _index = 0;
                _dialogueBox.text = string.Empty;
                _currentDialogue = quest._dialogueUponStart;
                StartCoroutine(PlayDialogue(quest._dialogueUponStart));
            }
        }

        private IEnumerator PlayDialogue(Dialogue text)
        {
            _dialogueBox.text = string.Empty;
            if (_index <= _currentDialogue._lines.Length - 1)
            {
                foreach (char c in _currentDialogue._lines[_index])
                {
                    _dialogueBox.text += c;
                    yield return new WaitForSeconds(_textSpeed);
                }
            }
        }

        private void NextLine()
        {
            if (_index <= _currentDialogue._lines.Length - 1)
            {
                _index++;
                _dialogueBox.text = string.Empty;
                StartCoroutine(PlayDialogue(_currentDialogue));
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
    }
}