using UnityEngine;
using Thirdweb.Redcode.Awaiting;

[RequireComponent(typeof(Collider))]
public class Marketplace : MonoBehaviour, IInteractable
{
    private bool _highlighting;
    private bool _interacting;
    private Color _originalColor;
    private Renderer _renderer;

    private void Awake()
    {
        _renderer = GetComponent<Renderer>();
        _originalColor = _renderer.material.color;
    }

    private void Update()
    {
        if (!_interacting && _highlighting && Input.GetKeyDown(KeyCode.E))
        {
            Interact();
        }
    }

    public void HighLight()
    {
        if (_highlighting)
            return;

        Debug.Log("Highlighting Marketplace");
        _highlighting = true;
        _renderer.material.color = Color.yellow;
    }

    public void StopHighLight()
    {
        if (!_highlighting)
            return;

        Debug.Log("Stopping Highlighting Marketplace");
        _highlighting = false;
        _renderer.material.color = _originalColor;
    }

    public void Interact()
    {
        if (_interacting)
            return;

        Debug.Log("Interacting with Marketplace");
        _interacting = true;

        GameManager.Instance.SetGameState(GameState.Trading);
        CharacterManager.Instance.SetFloat("Speed", 0);
    }

    public void StopInteract()
    {
        if (!_interacting)
            return;

        Debug.Log("Stopping Interacting with Marketplace");
        _interacting = false;
        GameManager.Instance.SetGameState(GameState.Moving);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            HighLight();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StopHighLight();
        }
    }
}
