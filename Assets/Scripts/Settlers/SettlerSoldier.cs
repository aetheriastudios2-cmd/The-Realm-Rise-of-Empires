using System.Collections;
using UnityEngine;
using SettlersClone.Buildings;
using SettlersClone.Map;

namespace SettlersClone.Settlers
{
    public class SettlerSoldier : Settler
    {
        [SerializeField] private float attackRange  = 2f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float maxHp = 100f;

        public float Hp { get; private set; }

        private Building       garrisonBuilding;
        private SettlerSoldier attackTarget;
        private float          attackTimer;

        protected override void Awake()
        {
            base.Awake();
            settlerType = SettlerType.Soldier;
            Hp = maxHp;
        }

        public void Garrison(Building barracks)
        {
            garrisonBuilding = barracks;
            State = SettlerState.Idle;
            MoveTo(barracks.transform.position);
        }

        public void AttackTarget(SettlerSoldier target)
        {
            attackTarget = target;
            StopAllCoroutines();
            StartCoroutine(AttackLoop());
        }

        private IEnumerator AttackLoop()
        {
            while (attackTarget != null && attackTarget.Hp > 0 && Hp > 0)
            {
                float dist = Vector3.Distance(transform.position, attackTarget.transform.position);
                if (dist > attackRange)
                {
                    MoveTo(attackTarget.transform.position);
                }
                else
                {
                    agent.ResetPath();
                    attackTimer += Time.deltaTime;
                    if (attackTimer >= attackCooldown)
                    {
                        attackTimer = 0f;
                        attackTarget.TakeDamage(attackDamage);
                    }
                }
                yield return null;
            }
            attackTarget = null;
            State = SettlerState.Idle;
        }

        public void TakeDamage(float amount)
        {
            Hp -= amount;
            if (Hp <= 0f) Die();
        }

        private void Die()
        {
            SettlerManager.Instance?.OnSoldierKilled(this);
            Destroy(gameObject);
        }
    }
}
