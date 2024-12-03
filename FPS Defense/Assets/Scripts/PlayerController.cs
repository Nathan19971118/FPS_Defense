using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private DefaultInput defaultInput;
    public Vector2 inputMovement;
    public Vector2 inputView;


    private void Awake()
    {
        defaultInput = new DefaultInput();

        defaultInput.Player.Movement.performed += e => inputMovement = e.ReadValue<Vector2>();
        defaultInput.Player.Movement.performed += e => inputView = e.ReadValue<Vector2>();

        defaultInput.Enable();
    }

    private void Update()
    {
        CalculateView();
        CalculateMovement();
    }

    private void CalculateView()
    {

    }

    private void CalculateMovement()
    {

    }
}
