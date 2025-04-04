using UnityEngine;

[RequireComponent(typeof(InputData))]
public class PlayerInputSystem : MonoBehaviour
{
    private InputData inputData;

    void Awake()
    {
        inputData = GetComponent<InputData>();
    }

    void Update()
    {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        inputData.moveInput = new Vector3(moveHorizontal, 0f, moveVertical);

        if (Input.GetButtonDown("Jump"))
        {
            inputData.jumpInput = true;
        }
    }
} 