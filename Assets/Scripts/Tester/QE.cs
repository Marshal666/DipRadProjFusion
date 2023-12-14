using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QE : MonoBehaviour
{

    public PlayerTankController Tank;

    public Hitbox Box;

    private void OnDrawGizmos()
    {
        if(Tank && Box)
        {
            var t = Tank.GetHitboxDamageModel(Box);
            var v = Utils.TransfromFromObjectCoords(transform.position, Box.transform, t);
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.1f);
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(v, 0.1f);
        }
    }

}
