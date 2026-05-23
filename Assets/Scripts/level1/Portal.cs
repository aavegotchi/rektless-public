using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace level2
{
    /**
     * Player should be behind of left part. Should be in front of right part.
     * Player starts from left part and goes to right part.
     */
    public class Portal : MonoBehaviour
    {
        [SerializeField] private GameObject leftPart;
        [SerializeField] private GameObject rightPart;
        [SerializeField] private Transform endPosition;
        [SerializeField] private float portalTransitionDuration = 1f;

        private void Start()
        {
           // var animator = GetComponent<Animator>();
           // animator.runtimeAnimatorController = PersistentData.Instance.CurrentLevelConfig.PortalAnimator;

            Vector3 leftPartPosition = leftPart.transform.position;
            Vector3 rightPartPosition = rightPart.transform.position;

            leftPartPosition.z = Player.Instance.transform.position.z - 1;
            rightPartPosition.z = Player.Instance.transform.position.z + 1;

            leftPart.transform.position = leftPartPosition;
            rightPart.transform.position = rightPartPosition;

            StartCoroutine(ChangeTexture());
        }

        IEnumerator ChangeTexture()
        {

            var levelTex = PersistentData.Instance.CurrentLevelConfig.PortalTexture;
            if (levelTex != null)
            {
                leftPart.GetComponent<SpriteRenderer>().material.SetTexture("_SwapTex", levelTex);
                rightPart.GetComponent<SpriteRenderer>().material.SetTexture("_SwapTex", levelTex);
            }
            else
            {
                Debug.LogWarning("Using default texture");
            }


            yield return null;
        }

        public void StartPortalTransition()
        {
            // Calculate target position (only X and Y change)
            Vector3 targetPosition = new Vector3(
                endPosition.position.x,
                endPosition.position.y,
                Player.Instance.transform.position.z // Keep original Z position
            );

            Player.Instance.transform.DOMove(targetPosition, portalTransitionDuration)
                .SetEase(Ease.InOutQuad)
                .OnComplete(() =>
                {
                    // Re-enable player control after transition
                    Player.Instance.Rb.isKinematic = false;
                    Player.Instance.DisableControlsAndColliders = false;
                    Player.Instance.OnStarting = false;
                    Player.Instance.OnStartAction?.Invoke();

                    Vector3 leftPartPosition = leftPart.transform.position;
                    leftPartPosition.z = Player.Instance.transform.position.z + 1;
                    leftPart.transform.position = leftPartPosition;

                    GetComponent<AudioSource>().Stop();
                });
        }
    }
}