using UnityEngine;
using Thirdweb.Redcode.Awaiting;

[RequireComponent(typeof(Collider))]
public class Herb : MonoBehaviour, IInteractable
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

        Debug.Log("Highlighting Herb");
        _highlighting = true;
        _renderer.material.color = Color.yellow;
    }

    public void StopHighLight()
    {
        if (!_highlighting)
            return;

        Debug.Log("Stopping Highlighting Herb");
        _highlighting = false;
        _renderer.material.color = _originalColor;
    }

    public async void Interact()
    {
        if (_interacting)
            return;

        Debug.Log("Interacting with Herb");
        _interacting = true;

        GameManager.Instance.SetGameState(GameState.Gathering);
        CharacterManager.Instance.SetTrigger("Gather");
        await new WaitForSeconds(2f);
        GameManager.Instance.AddHerb();
        StopInteract();
    }

    public void StopInteract()
    {
        if (!_interacting)
            return;

        Debug.Log("Stopping Interacting with Herb");
        _interacting = false;
        GameManager.Instance.SetGameState(GameState.Moving);
        Destroy(gameObject);
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
