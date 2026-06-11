using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using SettlersClone.Map;
using SettlersClone.Pathfinding;

namespace SettlersClone.Settlers
{
    public enum SettlerType  { Carrier, Worker, Soldier, Builder }
    public enum SettlerState { Idle, MovingTo, Working, Returning }

    [RequireComponent(typeof(NavMeshAgent))]
    public class Settler : MonoBehaviour
    {
        [SerializeField] protected SettlerType settlerType;

        public SettlerType  Type  => settlerType;
        public SettlerState State { get; protected set; } = SettlerState.Idle;
        public HexCell      CurrentCell { get; protected set; }

        protected NavMeshAgent agent;

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed        = 3.5f;
            agent.acceleration = 8f;
            agent.stoppingDistance = 0.5f;
        }

        protected void MoveTo(Vector3 destination)
        {
            agent.SetDestination(destination);
            State = SettlerState.MovingTo;
        }

        protected bool HasArrived() =>
            !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance;

        protected void UpdateCurrentCell(HexGrid grid)
        {
            CurrentCell = grid?.GetCellAt(transform.position);
        }

        protected IEnumerator WaitSeconds(float seconds)
        {
            yield return new WaitForSeconds(seconds);
        }
    }
}
