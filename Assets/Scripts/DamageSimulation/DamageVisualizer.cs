using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageVisualizer : MonoBehaviour
{
    
    public ObjectHolder MainShell;

    public ObjectHolder[] ShrapnelModels;

    public ObjectHolder HitPoints;

    DamageSimulator.VisualDamageNode Node;

    PlayerTankController Tank;

    [Tooltip("In seconds")]
    public float SimulationTime = 10f;

    public float ShrapnelLinesScale = 0.5f;

    public bool Offline = false;

    float LongestPath = 0f;

    float Speed = 0f;

    public enum VisualizerState
    {
        Idle,
        Working,
        Done
    }

    public VisualizerState State = VisualizerState.Idle;

    public void VisualizeDamage(DamageSimulator.VisualDamageNode node, PlayerTankController tank)
    {

        float LongestPath(DamageSimulator.VisualDamageNode node)
        {
            node.Dist = Vector3.Distance(node.Start, node.End);
            node.Dir = (node.End - node.Start).normalized;
            float len = node.Dist;
            if (node.Children == null || node.Children.Count <= 0)
            {
                return len;
            }
            float max = LongestPath(node.Children[0]);
            for(int i = 1; i < node.Children.Count; i++)
            {
                max = Mathf.Max(LongestPath(node.Children[i]), max);
            }
            return max + len;
        }

        Node = node;

        this.LongestPath = LongestPath(Node);

        State = VisualizerState.Working;

        Speed = this.LongestPath / SimulationTime;

        Tank = tank;
        if (Tank)
        {
            Tank.SetGhostingEnabled(true);
        }

        RunningTime = 0f;

        CurrentNodes.Add(new VGNode(node));

    }

    void NodeCleanup()
    {
        if (CurrentNodes == null)
            return;
        HitPoints.ReclaimAll();
        for (int i = 0; i < CurrentNodes.Count; i++)
        {
            var node = CurrentNodes[i];
            if (node.shrapnel != null)
            {
                if (node.SpawnerIndex == -1)
                {
                    MainShell.ReturnObject(node.shrapnel.gameObject);
                    node.shrapnel = null;
                }
                else
                {
                    ShrapnelModels[node.SpawnerIndex].ReturnObject(node.shrapnel.gameObject);
                    node.shrapnel = null;
                }
            }
        }
    }

    public void Reset()
    {
        State = VisualizerState.Idle;
        NodeCleanup();
        if (CurrentNodes == null)
        {
            CurrentNodes = new List<VGNode>(32);
        }
        else
        {
            CurrentNodes.Clear();
        }
        RunningTime = 0f;
    }

    float RunningTime = 0f;
    
    private class VGNode
    {
        public DamageSimulator.VisualDamageNode vn;
        public ShrapnelGraphic shrapnel = null;
        public bool done = false;
        public float dist = 0f;
        public int SpawnerIndex = -1;
        public int CurrentSpawnData = 0;
        public int CurrentChildIndex = 0;
        public int CurrentHitPointIndex = 0;

        public VGNode(DamageSimulator.VisualDamageNode node)
        {
            this.vn = node;
        }

    }

    private List<VGNode> CurrentNodes = new List<VGNode>(32);

    private void Update()
    {
        if(State == VisualizerState.Working)
        {
            RunningTime += Time.deltaTime;
            for(int i = 0; i < CurrentNodes.Count; i++)
            {
                var node = CurrentNodes[i];
                if (node.done)
                {
                    if(node.shrapnel != null)
                    {
                        if(node.SpawnerIndex == -1)
                        {
                            MainShell.ReturnObject(node.shrapnel.gameObject);
                            node.shrapnel = null;
                        }
                        else
                        {
                            ShrapnelModels[node.SpawnerIndex].ReturnObject(node.shrapnel.gameObject);
                            node.shrapnel = null;
                        }
                    }
                    continue;
                }
                if(node.shrapnel == null)
                {
                    Quaternion rot = Random.rotation;
                    if (i == 0)
                    {
                        node.shrapnel = MainShell.GetObject().GetComponent<ShrapnelGraphic>();
                        rot = new Quaternion(float.NaN, float.NaN, float.NaN, float.NaN);
                    }
                    if (!node.shrapnel)
                    {
                        node.SpawnerIndex = Random.Range(0, ShrapnelModels.Length);
                        node.shrapnel = ShrapnelModels[node.SpawnerIndex].GetObject().GetComponent<ShrapnelGraphic>();
                    }
                    node.shrapnel.Init(node.vn.Start, node.vn.Dir, rot, 0f);
                }
                node.dist += Speed * Time.deltaTime;

                //for child-shrapnels
                if (node.vn.Children != null && node.vn.SpawnData.Count > 0 && node.CurrentSpawnData < node.vn.SpawnData.Count && node.vn.SpawnData[node.CurrentSpawnData].Distance <= node.dist)
                {
                    for (int j = 0; j < node.vn.SpawnData[node.CurrentSpawnData].Count && j + node.CurrentChildIndex < node.vn.Children.Count; j++)
                    {
                        CurrentNodes.Add(new VGNode(node.vn.Children[j + node.CurrentChildIndex]));
                    }
                    node.CurrentChildIndex += node.vn.SpawnData[node.CurrentSpawnData].Count;
                    node.CurrentSpawnData++;
                }

                //hit point highlight
                if (node.vn.HitPoints != null && node.vn.HitPoints.Count > 0 && node.CurrentHitPointIndex < node.vn.HitPoints.Count)
                {
                    if (node.vn.HitPoints[node.CurrentHitPointIndex].dist <= node.dist)
                    {
                        HitPoints.GetObject().transform.position = node.vn.HitPoints[node.CurrentHitPointIndex++].point;
                    }
                }

                //done playing node
                if (node.dist >= node.vn.Dist)
                {
                    node.shrapnel.SetPosition(node.vn.End, node.dist / node.vn.Dist * ShrapnelLinesScale);
                    
                    node.done = true;
                }
                else
                {
                    node.shrapnel.SetPosition(node.vn.Start + node.vn.Dir * node.dist, 1f * ShrapnelLinesScale);
                }
            }
            if(RunningTime >= SimulationTime)
            {
                //print($"Visualizer done! time: {RunningTime}");
                if (Tank)
                {
                    Tank.SetGhostingEnabled(false, Offline);
                }
                NodeCleanup();
                State = VisualizerState.Done;
            }
        }
    }

}
