using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public class CharacterMovementHandler : NetworkBehaviour
{
    Vector2 viewInput;

    //Rotation
    float cameraRotationX = 0;

    //Other components
    NetworkCharacterController networkCharacterController;
    Camera localCamera;

    private void Awake()
    {
        networkCharacterController = GetComponent<NetworkCharacterController>();
        localCamera = GetComponentInChildren<Camera>();
    }
    
    public override void FixedUpdateNetwork()
    {
        //Get the input from the network
        if (GetInput(out NetworkInputData networkInputData))
        {
            

            Vector3 moveDirection = transform.forward * networkInputData.movementInput.y + transform.right * networkInputData.movementInput.x;
            moveDirection.Normalize();

            networkCharacterController.Move(moveDirection);
            
        }
    }
    
}
