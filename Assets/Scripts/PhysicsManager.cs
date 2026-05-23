using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsManager : MonoBehaviourSingleton<PhysicsManager>
{
    [SerializeField] LayerMask whatIsGround;
    public LayerMask WhatIsGround => whatIsGround;

    [SerializeField] LayerMask whatIsPlayer;
    public LayerMask WhatIsPlayer => whatIsPlayer;

    [SerializeField] ContactFilter2D enemyContactFilter;
    public ContactFilter2D EnemyContactFilter => enemyContactFilter;
}
