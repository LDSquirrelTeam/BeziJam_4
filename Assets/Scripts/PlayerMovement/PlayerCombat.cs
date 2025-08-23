using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(SlashAttack))]
public class PlayerCombat : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] private SlashAttack slashAttack;
    
    [Header("Input")]
    [SerializeField] private bool useInputSystem = true;
    [SerializeField] private KeyCode attackKey = KeyCode.X;
    
    private void Awake()
    {
        if (slashAttack == null)
            slashAttack = GetComponent<SlashAttack>();
    }

    private void Update()
    {
        if (!useInputSystem)
        {
            HandleLegacyInput();
        }
    }

    private void HandleLegacyInput()
    {
        if (Input.GetKeyDown(attackKey))
        {
            PerformAttack();
        }
    }

    public void OnAttack(InputValue value)
    {
        if (useInputSystem && value.isPressed)
        {
            PerformAttack();
        }
    }

    public void PerformAttack()
    {
        if (slashAttack != null && slashAttack.CanAttack)
        {
            slashAttack.PerformSlash();
        }
    }

    public bool IsAttacking()
    {
        return slashAttack != null && slashAttack.IsAttacking;
    }

    public bool CanAttack()
    {
        return slashAttack != null && slashAttack.CanAttack;
    }
}