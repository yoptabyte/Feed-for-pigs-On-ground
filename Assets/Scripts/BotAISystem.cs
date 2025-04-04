using UnityEngine;

[RequireComponent(typeof(InputData))]
public class BotAISystem : MonoBehaviour
{
    private InputData inputData;

    void Awake()
    {
        inputData = GetComponent<InputData>();
    }

    void Update()
    {
        Vector3 targetDirection = transform.forward;
        bool shouldJump = Random.value < 0.01f;

        inputData.moveInput = targetDirection;
        if (shouldJump)
        {
            inputData.jumpInput = true;
        }
    }
} 