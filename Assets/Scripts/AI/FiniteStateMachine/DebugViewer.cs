using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.AI.FiniteStateMachine
{
    public class DebugViewer : MonoBehaviour
    {
        List<List<Vector3>> graphpoints = new();

        public float length = 10f;
        public float height = 5f; 
        bool started = false;

        void Start()
        {
            graphpoints.Add(new List<Vector3>());
            graphpoints.Add(new List<Vector3>());
            InvokeRepeating(nameof(PlotGraph), 1f, 1f); // Plot the graph every second.
            started = true;
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(0, height, 0));
            Gizmos.DrawLine(transform.position, transform.position + new Vector3(length, 0, 0));
            if (!started) return;

            float increment = length / Mathf.Max(graphpoints[0].Count, 1);
            Gizmos.color = new Color (1,0,0,0.5f);
            Vector3 previous = transform.position;
            for (int i = 0; i < graphpoints[0].Count; i++)
            {
                Vector3 currentPos = transform.position + new Vector3(increment * i, Mathf.Clamp(graphpoints[0][i].y, 0, height), 0);
                Gizmos.DrawLine(previous, currentPos);
                previous = currentPos;
            }

            Gizmos.color = new Color(0, 0, 1, 0.5f);
            previous = transform.position;
            for (int i = 0; i < graphpoints[1].Count; i++)
            {
                Vector3 currentPos = transform.position + new Vector3(increment * i, Mathf.Clamp(graphpoints[1][i].y, 0, height), 0);
                Gizmos.DrawLine(previous, currentPos);
                previous = currentPos;
            }
        }

        void PlotGraph()
        {
            graphpoints[0].Add(new Vector3(0, AIBlackboardMediator.Instance.AITypeCounts[LayerMask.NameToLayer("Hostile")], 0));
            graphpoints[1].Add(new Vector3(0, AIBlackboardMediator.Instance.AITypeCounts[LayerMask.NameToLayer("Passive")], 0));
        }
    }
}
