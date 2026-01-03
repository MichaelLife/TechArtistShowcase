using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LifeHMA.Interaction
{
    public class Interactor : MonoBehaviour
    {
        [SerializeField] private Transform _interactionPoint;
        [SerializeField] private float _interactionPointRadius = 0.5f;
        [SerializeField] private LayerMask _interactionLayerMask;
        [SerializeField] private InteractionPromptUI _interactionPromptUI;

        private readonly Collider[] _colliders = new Collider[3];
        [SerializeField] private int _numberFound;
        [SerializeField] private bool showGizmos;

        public IInteractable _interactable;

        public void CheckForInteractable()
        {
            _numberFound = Physics.OverlapSphereNonAlloc(_interactionPoint.position, _interactionPointRadius, _colliders, _interactionLayerMask);

            if (_numberFound > 0)
            {
                _interactable = _colliders[0].GetComponent<IInteractable>();

                if (_interactable != null)
                {
                    if (!_interactionPromptUI.IsDisplayed)
                    {
                        _interactionPromptUI.SetUp(_interactable.InteractionPrompt);
                    }
                }
            }
            else
            {
                if (_interactable != null) _interactable = null;
                if (_interactionPromptUI.IsDisplayed) _interactionPromptUI.Close();
            }
        }

        private void OnDrawGizmos()
        {
            if (showGizmos)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_interactionPoint.position, _interactionPointRadius);
            }
        }
    }

    public interface IInteractable
    {
        public string InteractionPrompt { get; }
        public bool Interact(Interactor interactor);
    }

}
