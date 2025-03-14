using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;        
    public float defaultSpeed;      //속도 일시적 증가 후 되돌릴 값
    private bool isSpeedBoosted = false;    //속도가 증가했는지 판단하는 값
    private Vector2 curMovementInput;       //현재 움직임 벡터
    private bool isMoving = false;

    [Header("Jump")]
    public int currentJumps = 0;       //현재 점프 횟수 
    public int maxJumps = 1;            //최대 점프 횟수
    private bool isJumpBoosted = false; //점프 횟수가 늘어났는지 확인
    public float jumpPower = 80f;             //점프 거리
    public LayerMask groundLayerMask;   //어떤 레이어에 닿았는지 확인 위한 변수
    public float jumpStamina = 5f;

    [Header("Dash")]
    public float dashSpeedMultiplier = 2.5f; // 대쉬 속도 배율
    public float dashDuration = 0.2f; // 대쉬 지속 시간
    public float dashCooldown = 1.0f; // 대쉬 쿨타임
    private bool isDashing = false;
    private bool canDash = true;
    public float dashStamina = 10f;

    [Header("Stamina")]
    public float staminaDrain = 10f;        //특정 행동 시 스태미나 감소량
    public float staminaRecovery = 5f;      //초당 스태미나 회복량

    [Header("Look")]
    public Transform cameraContainer;
    public float minXLook;
    public float maxXLook;
    private float camCurXRot;
    public float lookSensitivity;

    private Vector2 mouseDelta;
    PlayerCondition condition;

    [HideInInspector]
    public bool canLook = true;
    private Rigidbody _rigidbody;
    public Action inventory;


    private void Awake()
    {
        
        _rigidbody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        condition = PlayerManager.Instance.Player.condition;
        defaultSpeed = moveSpeed;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        if(!isDashing)
        {
            Move();
        }
    }

    private void LateUpdate()
    {
        if (canLook)
        {
            CameraLook();
        }
    }
    /// <summary>
    /// 카메라 화면
    /// </summary>
    /// <param name="context"></param>
    public void OnLookInput(InputAction.CallbackContext context)
    {
        mouseDelta = context.ReadValue<Vector2>();
    }
    /// <summary>
    /// WASD 이동 확인
    /// </summary>
    /// <param name="context">입력값</param>
    public void OnMoveInput(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
        {
            curMovementInput = context.ReadValue<Vector2>();
            isMoving = true;
        }
        else if (context.phase == InputActionPhase.Canceled)
        {
            curMovementInput = Vector2.zero;
            isMoving = false;
        }
    }




    /// <summary>
    /// Space 입력 시 점프
    /// </summary>
    /// <param name="context"></param>
    public void OnJumpInput(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started && condition.IsUsableStamina(jumpStamina))
        {
            if (IsGrounded())
            {
                currentJumps = 0; // 땅에서 점프할 때 초기화 (필수)
            }

            if (currentJumps < maxJumps)
            {
                currentJumps++;  // 점프 횟수 증가
                _rigidbody.AddForce(Vector3.up * jumpPower, ForceMode.Impulse);
                condition.UseStamina(jumpStamina);
            }
        }
    }
    /// <summary>
    /// shift 입력 시
    /// </summary>
    public void OnDashInput()
    {
        if(canDash)
        {
            StartCoroutine(Dash());
        }
    }
    /// <summary>
    /// WASD 입력 시 이동 벡터 처리
    /// </summary>
    private void Move()
    {
        Vector3 dir = transform.forward * curMovementInput.y + transform.right * curMovementInput.x;
        dir *= moveSpeed;
        dir.y = _rigidbody.velocity.y;

        _rigidbody.velocity = dir;
    }

    /// <summary>
    /// 대쉬 입력 시 이동 벡터 처리 및 쿨다운
    /// </summary>
    /// <returns></returns>
    private IEnumerator Dash()
    {
        if(condition.IsUsableStamina(dashStamina))
        {
            canDash = false;
            isDashing = true;

            Vector3 dashDirection;
            condition.UseStamina(dashStamina);
            if (isMoving)
            {
                dashDirection = (transform.forward * curMovementInput.y + transform.right * curMovementInput.x).normalized;
            }
            else
            {
                dashDirection = transform.forward;
            }

            moveSpeed *= dashSpeedMultiplier;

            float elapsedTime = 0f;
            while (elapsedTime < dashDuration)
            {
                _rigidbody.velocity = dashDirection * moveSpeed; // 대쉬 방향으로 이동
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            moveSpeed = defaultSpeed;
            isDashing = false;

            yield return new WaitForSeconds(dashCooldown); // 대쉬 쿨타임 대기
            canDash = true;
        }
    }

    void CameraLook()
    {
        camCurXRot += mouseDelta.y * lookSensitivity;
        camCurXRot = Mathf.Clamp(camCurXRot, minXLook, maxXLook);
        cameraContainer.localEulerAngles = new Vector3(-camCurXRot, 0, 0);

        transform.eulerAngles += new Vector3(0, mouseDelta.x * lookSensitivity, 0);
    }

    /// <summary>
    /// 땅에 있는지 확인
    /// </summary>
    /// <returns>땅에 있는지 true or false</returns>
    bool IsGrounded()
    {
        bool grounded = false;
        Ray[] rays = new Ray[4]
        {
            new Ray(transform.position + (transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.forward * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (transform.right * 0.2f) + (transform.up * 0.01f), Vector3.down),
            new Ray(transform.position + (-transform.right * 0.2f) +(transform.up * 0.01f), Vector3.down)
        };

        foreach (var ray in rays)
        {
            if (Physics.Raycast(ray, 0.1f, groundLayerMask))
            {
                grounded = true;
                break;
            }
        }

        if (grounded)
        {
            currentJumps = 0;
        }

        return grounded;
    }

    public void OnInventoryButton(InputAction.CallbackContext callbackContext)
    {
        if (callbackContext.phase == InputActionPhase.Started)
        {
            inventory?.Invoke();
            ToggleCursor();
        }
    }
    void ToggleCursor()
    {
        bool toggle = Cursor.lockState == CursorLockMode.Locked;
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }

    public void ToggleCursor(bool toggle)
    {
        Cursor.lockState = toggle ? CursorLockMode.None : CursorLockMode.Locked;
        canLook = !toggle;
    }

    public void OnActionInput(InputAction.CallbackContext context)
    {
        if(context.phase == InputActionPhase.Started)
        {
            StartCoroutine(HandleActionInput());
        }
    }

    private IEnumerator HandleActionInput()
    {
        float holdTime = 0f;

        while(true)
        {
            if (!Input.GetMouseButton(0)) break;

            holdTime += Time.deltaTime;
            yield return null;
        }

        if(holdTime >= 0.5f && PlayerManager.Instance.Player.equip.curEquip == null)
        {
            UseItem();
        }
        else
        {
            Attack();
        }
    }

}
