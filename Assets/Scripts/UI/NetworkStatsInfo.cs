using Fusion.Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkStatsInfo : MonoBehaviour
{

    Fusion.NetworkRunner Runner;

    public float UpdateTime = 0.34f;

    private void Awake()
    {
        StartCoroutine(UpdatePing());
    }

    IEnumerator UpdatePing()
    {
        while(true)
        {
            if (Runner && Runner.IsPlayer)
            {
                var t = Runner.GetPlayerRtt(Runner.LocalPlayer);
                t *= 1000d;
                UIManager.SetPingStateText($"Ping: {t.ToString("0.0")} ms");
            }
            else
            {
                Runner = GetComponent<Fusion.NetworkObject>().Runner;
            }
            yield return new WaitForSeconds(UpdateTime);
        }
    }

}
