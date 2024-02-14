using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    [SerializeField]
    private CharacterController _character;

    [SerializeField]
    private Animator _characterAnimator;

    [SerializeField]
    private float _speed = 5.0f;

    [SerializeField]
    private float _turnSmoothTime = 0.1f;
    private float _turnSmoothVelocity;

    [SerializeField]
    private Transform _cameraTransform;

    public static CharacterManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (GameManager.Instance.CurrentGameState != GameState.Moving)
            return;

        MoveCharacter();
    }

    private void MoveCharacter()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(_character.transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
            _character.transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            _character.Move(moveDir.normalized * _speed * Time.deltaTime);

            SetFloat("Speed", _speed);
        }
        else
        {
            SetFloat("Speed", 0);
        }

        // Handle gravity
        if (!_character.isGrounded)
        {
            _character.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
    }

    internal void SetTrigger(string triggerName)
    {
        _characterAnimator.SetTrigger(triggerName);
    }

    internal void SetFloat(string floatName, float value)
    {
        _characterAnimator.SetFloat(floatName, value);
    }
}
