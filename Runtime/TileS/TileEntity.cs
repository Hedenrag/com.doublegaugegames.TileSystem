using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

namespace DGG.Tiles
{

    public class TileEntity : TileObjects
    {
        [SerializeField] public float moveSpeed;

        public UnityEvent<TileEntity> OnEndMove;
        public UnityEvent<TileEntity> OnEndRotationMove;

        protected bool blockInput = false;

        public TileDirection CurrentTileDirection => currentTileDirection;
        protected TileDirection currentTileDirection;

        protected override void Awake()
        {
            base.Awake();
        }

        public void Rotate(TileDirection direction)
        {
            if (blockInput == true) return;
            if (direction == currentTileDirection) return;

            currentTileDirection = direction;
            blockInput = true;
            Invoke(TimeSpan.FromSeconds(0.1f), () => { blockInput = false; });
        }

        /// <summary>
        /// Tries to move Entity towards the specified direction.
        /// </summary>
        /// <returns>Returns true if command could be executed.</returns>
        public bool Move(TileDirection dir)
        {
            if (blockInput) return false;
            //animator.SetTrigger(dir.ToString());
            var targetTile = tile.GetNeighborTile(dir);
            if (targetTile == null) return false;
            if (targetTile.tileEntity != null) return false;
            blockInput = true;

            StartCoroutine(MovementWithRotation(targetTile));
            return true;
        }
        IEnumerator MovementWithRotation(Tile target)
        {
            Vector3 startPosition = transform.position;
            Vector3 targetPosition = target.transform.position;
            Quaternion originalRotation = transform.rotation;
            Quaternion targetRotation;
            {
                var axis = target.transform.ClosestAxis(transform.GetPlaneForward());
                targetRotation = Quaternion.LookRotation(target.GetLocalAxis(axis), target.transform.up);
            }

            float distance = Vector3.Distance(transform.position, target.transform.position);

            m_tile.tileEntity = null;
            if (m_tile.tileObject == this) m_tile.tileObject = null;

            m_tile.OnTileExit.Invoke(this);
            m_tile = target;

            m_tile.tileEntity = this;
            if (m_tile.tileObject == null) m_tile.tileObject = this;

            float targetTime = distance/moveSpeed;
            float time = 0f;
            while (time < targetTime)
            {
                yield return null;
                time += Time.deltaTime;
                transform.position = Vector3.Lerp(startPosition, targetPosition, time/targetTime);
                transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, time/targetTime);
            }

            transform.position = targetPosition;
            transform.rotation = targetRotation;

            blockInput = false;
            //animator.SetBool("Moving", false);
            OnEndMove.Invoke(this);
        }
        

        static void Invoke(TimeSpan dueTime, Action action)
        {
            Timer timer = null;
            timer = new Timer(_ => { timer.Dispose(); action(); });
            timer.Change(dueTime, TimeSpan.FromMilliseconds(-1));
        }
    }

}