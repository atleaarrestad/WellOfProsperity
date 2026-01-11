using Assets.Scripts.Combat.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.Traps.Conditions
{
    public class ChargesCondition : MonoBehaviour, ITrapCondition
    {
        [Header("Charges")]
        [Min(0)]
        [SerializeField] private int maxCharges = 10;

        [Tooltip("If true, charges refill over time.")]
        [SerializeField] private bool autoRecharge = true;

        [Tooltip("Seconds per 1 charge (only used if Auto Recharge is enabled).")]
        [Min(0.01f)]
        [SerializeField] private float secondsPerCharge = 5f;

        [Header("Debug / Runtime")]
        [SerializeField] private int currentCharges;

        private float nextRechargeTime;

        private void Awake() {
            if (currentCharges <= 0 && maxCharges > 0) {
                currentCharges = maxCharges;
            }
        }

        public bool CanActivate(TrapContext context, IReadOnlyList<ITargetable> targets) {
            if (autoRecharge)
                TryRecharge();

            return currentCharges > 0;
        }

        public void OnFired() {
            if (currentCharges <= 0)
                return;

            currentCharges--;

            if (autoRecharge && currentCharges < maxCharges)
                ScheduleNextRecharge();
        }

        private void TryRecharge() {

            if (maxCharges <= 0) {
                return;
            }
            if (currentCharges >= maxCharges) {
                return;
            }

            if (nextRechargeTime <= 0f) {
                ScheduleNextRecharge();
            }

            while (Time.time >= nextRechargeTime && currentCharges < maxCharges) {
                currentCharges++;
                nextRechargeTime += Mathf.Max(0.01f, secondsPerCharge);
            }

            if (currentCharges >= maxCharges) {
                nextRechargeTime = 0f;
            }
        }

        private void ScheduleNextRecharge() {
            nextRechargeTime = Time.time + Mathf.Max(0.01f, secondsPerCharge);
        }

        public void AddCharges(int amount) {

            if (amount <= 0) {
                return;
            }

            currentCharges = Mathf.Min(maxCharges, currentCharges + amount);

            if (autoRecharge && currentCharges < maxCharges && nextRechargeTime <= 0f) {
                ScheduleNextRecharge();
            }
        }

        public void RefillToMax() {
            currentCharges = maxCharges;
            nextRechargeTime = 0f;
        }

#if UNITY_EDITOR
        private void OnValidate() {
            if (maxCharges < 0) {
                maxCharges = 0;
            }

            if (secondsPerCharge < 0.01f) {
                secondsPerCharge = 0.01f;
            }

            
            if (!Application.isPlaying) {
                if (currentCharges < 0) {
                    currentCharges = 0;
                }

                if (currentCharges > maxCharges) {
                    currentCharges = maxCharges;
                }
            }
        }
#endif
    }
}
