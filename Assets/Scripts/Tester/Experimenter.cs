using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Experimenter : MonoBehaviour
{

    public Vector3 GlobalPoint = Vector3.one;

    //public Transform Cube1;
    //public Transform Cube2;

    public Transform[] Tank1Parts;

    public Transform[] Tank2Parts;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (Tank1Parts == null || Tank2Parts == null || Tank1Parts.Length != Tank2Parts.Length)
            return;
        for(int i = 0; i < Tank1Parts.Length; i++)
        {
            if (Tank1Parts[i] == null || Tank2Parts[i] == null)
                continue;
            Tank2Parts[i].localRotation = Tank1Parts[i].localRotation;
        }
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(GlobalPoint, 0.2f);

        //if(Cube1 && Cube2)
        //{

        //    Vector3 transformed = Utils.TransfromFromObjectCoords(GlobalPoint, Cube1.transform, Cube2.transform);

        //    Gizmos.color = Color.green;
        //    Gizmos.DrawSphere(transformed, 0.2f);
        //}

        if (Tank1Parts == null || Tank2Parts == null || Tank1Parts.Length != Tank2Parts.Length ||Tank1Parts.Length < 1)
            return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(GlobalPoint, 0.2f);
        //Vector3 tp = Utils.TransfromFromObjectCoords(GlobalPoint, Tank1Parts[0], Tank2Parts[0]);
        Vector3 tp = Utils.TransfromFromObjectCoords(GlobalPoint, Tank1Parts[0].position, Tank1Parts[0].rotation, Tank2Parts[0]);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(tp, 0.2f);
    }

}
