using UnityEngine;

public interface IEnemy : IDestroyable
{
    Animator GetAnimator();
    AudioSource GetAudioSource();
    Collider2D GetCollider();
    LayerMask GetPlayerLayer();
    LayerMask GetSpaceLayer();
    float GetMoveSpeed();
    float GetInitialMoveSpeed();
    void SetMoveSpeed(float moveSpeed);
    float GetIdleDuration();
    BoxCollider2D GetPlayerNearbyCollider();
    BoxCollider2D GetAttackCollider();
    Rigidbody2D GetRigidbody();
    void UpdateVelocity();
    AudioClip GetAttackSound();
    AudioClip GetDeathSound();
    AudioClip GetTakeDamageSound();
    GameObject GetGemPrefab();
    Transform GetGemSpawnPoint();
    int GetLives();
    void TakeDamage();
    float GetDirection();
    void SetDirection(float direction);
    bool IsTakingDamage();
    void CompleteTakeDamage();
    void OnAttackTrigger();
    void OnAttackComplete();
    void OnDeathAnimationEnd();
}

public interface IDestroyable
{
    void Die();
}