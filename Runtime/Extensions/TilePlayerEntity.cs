using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DGG.Tiles
{

    public class TilePlayerEntity : TileEntity
    {
        Transform cameraDirection;

        public bool Moving => inputingMovment;
        bool inputingMovment = false;

        public Vector3 Direction => direction;
        Vector3 direction;
        public Vector2 Input => input;
        Vector2 input;

        TileDirection currentMoveDirection = TileDirection.Forward;
        TileDirection currentLookDirection = TileDirection.Forward;
        public void MoveInput(Vector2 rawInput)
        {
            input = rawInput;

            inputingMovment = input.sqrMagnitude > 0.2f;
            if (!inputingMovment) return;

            Vector3 playerForwardDir = Vector3.ProjectOnPlane(cameraDirection.forward, transform.up).normalized;
            Vector3 playerRightDir = Vector3.Cross(transform.up, playerForwardDir);
            direction = (playerForwardDir * input.y) + (playerRightDir * input.x);
        }

        public void Interact()
        {
            tile.GetNeighborTile(currentTileDirection).Interact(this);

        }

        virtual protected void Update()
        {
            if (Camera.main) { cameraDirection = Camera.main.transform; }
            else if (cameraDirection == null) { cameraDirection = FindFirstObjectByType<Camera>().transform; }

            if (inputingMovment)
            {
                currentMoveDirection = tile.GetTransformLocalDir(tile.transform.position + direction).ToTileDir();
                currentLookDirection = input.ClosestDir().ToTileDir();

                Rotate(currentLookDirection);
                Move(currentMoveDirection);
            }
        }
    }
}
