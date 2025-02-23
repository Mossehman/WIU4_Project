//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace Assets.Scripts.AI.FiniteStateMachine
//{
//    public class TimeManager : MonoBehaviour
//    {
//        static public TimeManager Instance { get; private set; }

//        [Range(0f, 24f)]
//        public float worldtime = 5f;
//        public float worldtimescale = 1f;

//        public TimeOfTheDay timeOfTheDay;
//        int divider;

//        private void Start()
//        {
//            if (Instance == null)
//                Instance = this;

//            divider = (int)(24f / (int)TimeOfTheDay.NumOfPeriods);
//        }

//        void Update()
//        {
//            worldtime += Time.deltaTime * 0.016f * worldtimescale;
//            if (worldtime > 24f) { worldtime = 0f; }

//            timeOfTheDay = (TimeOfTheDay)(worldtime / divider);
//        }

//        public bool IsWithinCurrentTimePeriod(TimeOfTheDay start, TimeOfTheDay end)
//        {
//            if (start <= end)
//                return timeOfTheDay >= start && timeOfTheDay <= end;
//            else
//                return timeOfTheDay >= start || timeOfTheDay <= end;
//        }

//        public bool IsWithinCurrentTimePeriod(MinMaxEnum<TimeOfTheDay> range)
//        {
//            return IsWithinCurrentTimePeriod(range.start, range.end);
//        }

//        private void OnDrawGizmos()
//        {
//            Gizmos.color = Color.yellow;
//            float rad = -((360 * worldtime / 24f) - 90f) * Mathf.Deg2Rad;
//            Gizmos.DrawLine(transform.position, transform.position + (new Vector3(50f * Mathf.Cos(rad), 50f * Mathf.Sin(rad), 0)));
//        }
//    }

//    public enum TimeOfTheDay
//    {
//        Midnight,
//        Morning,
//        Afternoon,
//        Night,
//        NumOfPeriods,
//    }
//}