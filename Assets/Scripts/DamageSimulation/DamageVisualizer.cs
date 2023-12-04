using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageVisualizer : MonoBehaviour
{
    
    public ObjectHolder MainShell;

    public ObjectHolder[] ShrapnelModels;

    DamageSimulator.VisualDamageNode Node;

    GhostEffectObject Ghost;

    [Tooltip("In seconds")]
    public float SimulationTime = 10f;

    public float ShrapnelLinesScale = 0.5f;

    float LongestPath = 0f;

    float Speed = 0f;

    public enum VisualizerState
    {
        Idle,
        Working,
        Done
    }

    public VisualizerState State = VisualizerState.Idle;

    public void VisualizeDamage(DamageSimulator.VisualDamageNode node, GhostEffectObject ghost)
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

        Ghost = ghost;

        Ghost.SetMaterial(GhostEffectObject.MaterialType.Ghost);

        RunningTime = 0f;

        CurrentNodes.Add(new VGNode(node));

    }

    public void Reset()
    {
        State = VisualizerState.Idle;
        if(CurrentNodes == null)
            CurrentNodes = new List<VGNode>(32);
        else
            CurrentNodes.Clear();
        RunningTime = 0f;
    }

    float RunningTime = 0f;
    
    public class VGNode
    {
        public DamageSimulator.VisualDamageNode vn;
        public ShrapnelGraphic shrapnel = null;
        public bool done = false;
        public float dist = 0f;
        public int SpawnerIndex = -1;
        public int CurrentSpawnData = 0;
        public int CurrentChildIndex = 0;

        public VGNode(DamageSimulator.VisualDamageNode node)
        {
            this.vn = node;
        }

    }

    public List<VGNode> CurrentNodes = new List<VGNode>(32);

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

                if (node.vn.Children != null && node.vn.SpawnData.Count > 0 && node.CurrentSpawnData < node.vn.SpawnData.Count && node.vn.SpawnData[node.CurrentSpawnData].Distance <= node.dist)
                {
                    for (int j = 0; j < node.vn.SpawnData[node.CurrentSpawnData].Count && j + node.CurrentChildIndex < node.vn.Children.Count; j++)
                    {
                        CurrentNodes.Add(new VGNode(node.vn.Children[j + node.CurrentChildIndex]));
                    }
                    node.CurrentChildIndex += node.vn.SpawnData[node.CurrentSpawnData].Count;
                    node.CurrentSpawnData++;
                }

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
                Ghost.SetMaterial(GhostEffectObject.MaterialType.Original);
                State = VisualizerState.Done;
            }
        }
    }

}
