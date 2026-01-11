using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Assets.Scripts.Combat.Interfaces;

namespace Assets.Scripts.Traps.Detectors
{
    public class TimedPulseDetector : MonoBehaviour, ITrapDetector
	{
        public event Action<IReadOnlyList<ITargetable>> OnDetection;

        public void Disable()
        {
            throw new NotImplementedException();
        }

        public void Enable()
        {
            throw new NotImplementedException();
        }

        // Use this for initialization
        void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}