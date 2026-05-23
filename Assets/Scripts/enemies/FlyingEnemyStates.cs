using System;
using System.Collections;
using UnityEngine;

namespace enemies
{
    public class FlyingIdleState<T> : State<T> where T : MonoBehaviour, IEnemy
    {
        public bool IdleTimeElapsed { get; private set; }
        private Coroutine IdleCoroutine { get; set; }
        private State<T> _previousState;

        public override void Enter()
        {
            owner.GetAnimator().SetTrigger("idle");
            IdleCoroutine = owner.StartCoroutine(IdleCoroutineF());
            _previousState = stateMachine.PreviousState;
        }

        private IEnumerator IdleCoroutineF()
        {
            yield return new WaitForSeconds(owner.GetIdleDuration());
            IdleTimeElapsed = true;
        }

        public override void Exit()
        {
            IdleTimeElapsed = false;
            owner.GetAnimator().ResetTrigger("idle");

            if (IdleCoroutine != null)
            {
                owner.StopCoroutine(IdleCoroutine);
            }
        }

        public State<T> GetPreviousState()
        {
            return _previousState;
        }
    }

    public class FlyingChasePlayerState<T> : State<T> where T : MonoBehaviour, IEnemy
    {
        public bool PlayerNearby { get; private set; }
        private bool _isMoving;
        private Boolean? hasRigidbody = null;

        public override void Enter()
        {
            if (hasRigidbody == null)
            {
                hasRigidbody = owner.GetRigidbody() != null;
            }

            PlayerNearby = false;
            _isMoving = true;
            UpdateAnimation();
            owner.transform.rotation = owner.GetDirection() == 1 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            owner.SetMoveSpeed(owner.GetInitialMoveSpeed());
        }

        public override void Update()
        {
            PlayerNearby = Physics2D.OverlapBox(owner.GetPlayerNearbyCollider().bounds.center,
                owner.GetPlayerNearbyCollider().bounds.size, 0, owner.GetPlayerLayer());
            float direction = Player.Instance.transform.position.x < owner.transform.position.x ? -1 : 1;
            owner.transform.rotation = direction == 1 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;

            bool canMove = IsMovable();
            _isMoving = !PlayerNearby && canMove;

            if (_isMoving && hasRigidbody == false)
            {
                Vector3 newPosition = owner.transform.position;
                newPosition.x += direction * owner.GetMoveSpeed() * Time.deltaTime;
                owner.transform.position = newPosition;
            }

            owner.SetDirection(direction);
            owner.UpdateVelocity();
            UpdateAnimation();
        }

        private void UpdateAnimation()
        {
            if (_isMoving)
            {
                owner.GetAnimator().SetTrigger("walk");
            }

            owner.GetAnimator().SetBool("iswalking", _isMoving);
        }

        private bool IsMovable()
        {
            var collider = owner.GetCollider();
            Collider2D hit;
            if (collider is CapsuleCollider2D capsuleCollider2D)
            {
                hit = Physics2D.OverlapCapsule(capsuleCollider2D.bounds.center, capsuleCollider2D.bounds.size,
                    capsuleCollider2D.direction,
                    0, owner.GetSpaceLayer());
            }
            else
            {
                hit = Physics2D.OverlapBox(owner.GetCollider().bounds.center, owner.GetCollider().bounds.size, 0,
                    owner.GetSpaceLayer());
            }
            
            if (hit == null)
            {
                return true;
            }

            bool isSpaceObjectLeft = hit.transform.position.x < owner.transform.position.x;

            if (Player.Instance.transform.position.x > owner.transform.position.x)
            {
                return isSpaceObjectLeft;
            }

            return !isSpaceObjectLeft;
        }

        public override void Exit()
        {
            PlayerNearby = false;
            _isMoving = false;
            owner.GetAnimator().ResetTrigger("walk");
            owner.GetAnimator().SetBool("iswalking", false);
            owner.SetMoveSpeed(0);
        }
    }

    public class FlyingPatrolState<T> : State<T> where T : MonoBehaviour, IEnemy
    {
        public bool PlayerNearby { get; private set; }
        private Boolean? hasRigidbody = null;

        public override void Enter()
        {
            if (hasRigidbody == null)
            {
                hasRigidbody = owner.GetRigidbody() != null;
            }

            PlayerNearby = false;
            owner.GetAnimator().SetTrigger("walk");
            owner.GetAnimator().SetBool("iswalking", true);

            owner.transform.rotation = owner.GetDirection() == 1 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
            owner.SetMoveSpeed(owner.GetInitialMoveSpeed());
        }

        public override void Update()
        {
            PlayerNearby = Physics2D.OverlapBox(owner.GetPlayerNearbyCollider().bounds.center,
                owner.GetPlayerNearbyCollider().bounds.size, 0, owner.GetPlayerLayer());

            bool shouldChangeDirection = !IsMovable();
            if (shouldChangeDirection)
            {
                owner.SetDirection(-owner.GetDirection());
                owner.transform.rotation =
                    owner.GetDirection() == 1 ? Quaternion.Euler(0, 180, 0) : Quaternion.identity;
                owner.UpdateVelocity();
            }

            if (hasRigidbody == false)
            {
                Vector3 newPosition = owner.transform.position;
                newPosition.x += owner.GetDirection() * owner.GetMoveSpeed() * Time.deltaTime;
                owner.transform.position = newPosition;
            }
        }

        private bool IsMovable()
        {
            var collider = owner.GetCollider();
            Collider2D hit;
            if (collider is CapsuleCollider2D capsuleCollider2D)
            {
                hit = Physics2D.OverlapCapsule(capsuleCollider2D.bounds.center, capsuleCollider2D.bounds.size,
                    capsuleCollider2D.direction,
                    0, owner.GetSpaceLayer());
            }
            else
            {
                hit = Physics2D.OverlapBox(owner.GetCollider().bounds.center, owner.GetCollider().bounds.size, 0,
                    owner.GetSpaceLayer());
            }

            if (hit == null)
            {
                return true;
            }

            bool isSpaceObjectRight = hit.transform.position.x > owner.transform.position.x;
            return owner.GetDirection() == -1 ? isSpaceObjectRight : !isSpaceObjectRight;
        }

        public override void Exit()
        {
            owner.GetAnimator().ResetTrigger("walk");
            owner.GetAnimator().SetBool("iswalking", false);
            owner.SetMoveSpeed(0);
        }
    }

    public class FlyingAttackState<T> : State<T> where T : MonoBehaviour, IEnemy
    {
        public bool AttackComplete { get; private set; }
        private State<T> _previousState;

        public override void Enter()
        {
            AttackComplete = false;
            owner.GetAnimator().SetTrigger("attack");
            _previousState = stateMachine.PreviousState;
        }

        public override void Exit()
        {
            AttackComplete = false;
        }

        public void OnAttackComplete()
        {
            AttackComplete = true;
        }

        public State<T> GetPreviousState()
        {
            return _previousState;
        }
    }

    public class FlyingTakeDamageState<T> : State<T> where T : MonoBehaviour, IEnemy
    {
        public bool DamageAnimationComplete { get; protected set; }
        public State<T> PreviousState;

        public override void Enter()
        {
            DamageAnimationComplete = false;

            owner.TakeDamage();
            if (owner.GetLives() <= 0 && stateMachine.HasState<FlyingDeathState<T>>())
            {
                stateMachine.ChangeState<FlyingDeathState<T>>();
                return;
            }

            if (stateMachine.PreviousState is FlyingTakeDamageState<T> previousTakeDamageState)
            {
                owner.GetAnimator().Play("takedamage", -1, 0);
                PreviousState = previousTakeDamageState.GetPreviousState();
            }
            else
            {
                owner.GetAnimator().SetTrigger("takedamage");
                PreviousState = stateMachine.PreviousState;
            }

            AudioSource audioSource = owner.GetAudioSource();
            AudioClip takeDamageSound = owner.GetTakeDamageSound();
            if (audioSource != null && takeDamageSound != null)
            {
                owner.GetAudioSource().PlayOneShot(takeDamageSound);
            }
        }

        public override void Exit()
        {
            DamageAnimationComplete = false;
        }

        public void OnTakeDamageComplete()
        {
            DamageAnimationComplete = true;
        }

        public State<T> GetPreviousState()
        {
            return PreviousState;
        }
    }

    public class FlyingDeathState<T> : State<T> where T : MonoBehaviour, IEnemy
    {
        public override void Enter()
        {
            AudioSource audioSource = owner.GetAudioSource();
            AudioClip deathSound = owner.GetDeathSound();
            if (audioSource != null && deathSound != null)
            {
                audioSource.PlayOneShot(deathSound);
            }

            owner.GetCollider().enabled = false;
            owner.GetAnimator().SetTrigger("death");

            Rigidbody2D rb = owner.GetRigidbody();
            
            if (rb != null)
                rb.gravityScale = 1;

            if (owner.GetLives() <= 0)
                Player.Instance.KillsStatistic++;
        }

        public override void Exit()
        {
        }
    }
}