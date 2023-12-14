using Fusion;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngineInternal;

namespace Projectiles.ProjectileDataBuffer_Kinematic
{
    // Projectile data buffer can also be used for kinematic projectiles. Projectiles are simply updated
    // based on the initial data up until they hit something or until their life time expires.
    // Although the solution from this example is fully functional it does not cover some advanced topics like
    // projectiles interpolation between actual path from the camera and visible path from the barrel of the gun;
    // or what happens to the projectiles when this weapon gets destroyed.
    // For more advanced features and examples of multiple other types of projectiles check Projectiles Advanced.
    public class KinematicProjectileDataBuffer : ShellStats
    {
        // PRIVATE MEMBERS

        [SerializeField]
        private float _speed = 50f;

        private LayerMask _hitMask => StaticConsts.ShellHitLayers;
        [SerializeField]
        private float _hitImpulse = 50f;
        [SerializeField]
        private float _lifeTime = 6f;
        [SerializeField]
        private float _lifeTimeAfterHit = 2f;
        [SerializeField]
        private DummyProjectile _dummyProjectilePrefab;
        [FormerlySerializedAs("HitEnergy")] [SerializeField]
        public float Energy = 100f;
        [SerializeField]
        public int GlobalIndex = 0;
        
        [Networked]
        private int _fireCount { get; set; }
        [Networked, Capacity(64)]
        private NetworkArray<ProjectileData> _projectileData { get; }

        [Networked]
        [Capacity(64)]
        public NetworkDictionary<int, ProjectileHitInfo> _hitInfos => default;

        [Networked]
        public int rngSeed { get; set; }

        private DummyProjectile[] _projectiles = new DummyProjectile[64];

        private int _visibleFireCount;

        public HashSet<int> DoneProjectileIndexes = new HashSet<int>();

        private TankHitInfo _tankHitInfo;

        // WeaponBase INTERFACE

        public override void Fire()
        {
            _projectileData.Set(_fireCount % _projectileData.Length, new ProjectileData()
            {
                Index = _fireCount,
                FireTick = Runner.Tick,
                FirePosition = FireTransform.position,
                FireVelocity = FireTransform.forward * _speed,
                FinishTick = Runner.Tick + Mathf.RoundToInt(_lifeTime / Runner.DeltaTime),
            });

            _fireCount++;
        }

        public override Vector3? HitTest(Vector3 position, Vector3 direction, ref Vector3[] OutDebug)
        {
            Vector3? ret = null;

            ProjectileData data = new ProjectileData()
            {
                FireTick = 0,
                FirePosition = position,
                FireVelocity = direction * _speed,
            };

            bool debug = OutDebug != null;

            for(int i = 1; i < StaticConsts.MaxShellRaycastTicks; i++)
            {
                var p1 = GetMovePosition(ref data, i - 1);
                var p2 = GetMovePosition(ref data, i);

#if UNITY_EDITOR
                if(debug)
                {
                    OutDebug[i - 1] = p1;
                    OutDebug[i] = p2;
                }
#endif

                Vector3 dir = p2 - p1;
                if(Physics.Raycast(p1, dir, out var hit, dir.magnitude, StaticConsts.ShellHitLayersTest, QueryTriggerInteraction.Ignore))
                {
                    ret = hit.point;
                    break;
                }
            }

            return ret;
        }

        public override void Spawned()
        {
            _visibleFireCount = _fireCount;
            rngSeed = Random.Range(int.MinValue, int.MaxValue);
        }

        public override void FixedUpdateNetwork()
        {
            //if (IsProxy == true)
            //    return;

            int tick = Runner.Tick;

            // Process projectile update
            for (int i = 0; i < _projectileData.Length; i++)
            {
                var data = _projectileData[i];

                if (data.IsActive == false)
                    continue;
                if (data.FinishTick <= tick)
                    continue;

                // For simplicity projectile update is processed directly in the Weapon.
                // It might be more suitable to move this logic to projectile object itself in order
                // to have different projectile behaviours without the need to alter the weapon.
                // See Projectiles Advanced where such approach is used.
                UpdateProjectile(ref data, tick);

                _projectileData.Set(i, data);
            }

            if(_hitInfos.TryGet(Runner.Tick - 1, out var hitInfo)) {
                bool found = Runner.TryFindObject(hitInfo.HitObjectId, out var obj);
                string st = $"hit: {(found ? obj.name : "NO_OBJECT")} at: {hitInfo.HitPosition}, dir: {hitInfo.HitDirection}, tick: {hitInfo.Tick}\n";

                OnProjectileHit(hitInfo);

                if (_tankHitInfo != null)
                {
                    ConsoleLog.Log($"Dmg hash: {_tankHitInfo.DamageableRoot.GetHash()}, sum: {_tankHitInfo.DamageableRoot.HP_Sum()}\n");
                    _tankHitInfo.DamageableRoot.ApplyDamage();
                }

                _tankHitInfo = null;
                //_hitInfos.Remove(Runner.Tick - 1);
            }
            if(_hitInfos.TryGet(Runner.Tick - 2, out hitInfo))
            {
                _hitInfos.Remove(Runner.Tick - 2);
            }
        }

        public override void Render()
        {
            if (_visibleFireCount < _fireCount)
            {
                PlayFireEffect();
            }

            // Instantiate missing projectile objects
            for (int i = _visibleFireCount; i < _fireCount; i++)
            {
                int index = i % _projectileData.Length;
                var data = _projectileData[index];

                var previousProjectile = _projectiles[index];
                if (previousProjectile != null)
                {
                    Destroy(previousProjectile.gameObject);
                }

                var projectile = Instantiate(_dummyProjectilePrefab, data.FirePosition, Quaternion.LookRotation(data.FireVelocity));

                // When using multipeer, move to correct scene
                Runner.MoveToRunnerScene(projectile);

                _projectiles[index] = projectile;
            }

            // For proxies we move projectiles in remote time frame, for input/state authority we use local time frame
            float renderTime = Object.IsProxy == true ? Runner.InterpolationRenderTime : Runner.SimulationRenderTime;
            float floatTick = renderTime / Runner.DeltaTime;

            // Update projectile visuals
            for (int i = 0; i < _projectiles.Length; i++)
            {
                var projectile = _projectileData[i];
                var projectileObject = _projectiles[i];

                if (projectile.IsActive == false || projectile.FinishTick < floatTick)
                {
                    if (projectileObject != null)
                    {
                        Destroy(projectileObject.gameObject);
                    }

                    continue;
                }

                if (projectile.PointingDirection != Vector3.zero)
                    projectileObject.SetPointingDirection(projectile.PointingDirection);

                if (projectile.HitPosition != Vector3.zero)
                {
                    projectileObject.transform.position = projectile.HitPosition;
                    projectileObject.ShowHitEffect();
                }
                else
                {
                    projectileObject.transform.position = GetMovePosition(ref projectile, floatTick);
                }
            }

            _visibleFireCount = _fireCount;
        }

        void OnProjectileHit(ProjectileHitInfo info)
        {

            NetworkObject obj = null;
            PlayerTankController ptc = null;

            if (!info.IsValidTank(Runner, ref obj, ref ptc))
                return;

            _tankHitInfo = new TankHitInfo(ptc, info);

            DamageSimulator.SimulateDamage(info, _tankHitInfo.DamageNode, rngSeed, ref ptc);

        }

        public Dictionary<PlayerRef, (Vector3 t, Vector3 d)> PlayerHits = new Dictionary<PlayerRef, (Vector3 t, Vector3 d)>();

        // PRIVATE METHODS

        private void UpdateProjectile(ref ProjectileData projectileData, int tick)
        {
            if (projectileData.HitPosition != Vector3.zero)
                return;

            var previousPosition = GetMovePosition(ref projectileData, tick - 1f);
            var nextPosition = GetMovePosition(ref projectileData, tick);
            //_projectiles[index].SetPointingDirection(nextPosition - previousPosition);
            projectileData.PointingDirection = (nextPosition - previousPosition).normalized;

            var direction = nextPosition - previousPosition;

            float distance = direction.magnitude;
            direction /= distance; // Normalize

            var hitOptions = HitOptions.IncludePhysX | HitOptions.IgnoreInputAuthority;

            if (Runner.LagCompensation.Raycast(previousPosition, direction, distance,
                    Object.InputAuthority, out var hit, _hitMask, hitOptions) == true)
            {
                projectileData.HitPosition = hit.Point;
                projectileData.HitDirection = direction;
                projectileData.FinishTick = tick + Mathf.RoundToInt(_lifeTimeAfterHit / Runner.DeltaTime);

                NetworkId id = new NetworkId();
                NetworkObject no = null;
                Hitbox hi = hit.Hitbox;

                Vector3 HitboxPosition = Vector3.zero;
                Quaternion HitboxRotation = Quaternion.identity;

                if (hi)
                {
                    print($"hit Hitbox: {hit.Hitbox.name}");
                    no = hi.transform.root.GetComponent<NetworkObject>();

                    Runner.LagCompensation.PositionRotation(hi, no.InputAuthority, out HitboxPosition, out HitboxRotation);
                } else
                {
                    throw new System.Exception("No Hitbox on hit object! This should not be the case!!!");
                }

                PlayerTankController tank = no.GetComponent<PlayerTankController>();
                HitboxRoot root = hi.Root;

                Utils.SetTankInnerColliderTargets(Runner, tank, root, no.InputAuthority);

                ProjectileHitInfo info = new ProjectileHitInfo()
                {
                    Tick = tick,
                    HitPosition = hit.Point,
                    HitDirection = direction,
                    ProjectileIndex = projectileData.Index,
                    //HitObject = hit.Collider.gameObject.GetComponent<NetworkObject>(),
                    HitObjectId = no.Id,
                    Processed = false,
                    Energy = Energy,
                    HitboxId = hit.Hitbox.HitboxIndex,
                    HitboxPosition = HitboxPosition,
                    HitboxRotation = HitboxRotation,
                };

                Vector3 s = info.HitPosition;
                Vector3 b = Utils.TransfromFromObjectCoords(s, hi.transform, tank.GetHitboxDamageModel(hi));
                PlayerHits[no.InputAuthority] = (s, b);

                info.HitPosition = b;

                _hitInfos.Set(tick, info);
                //OnProjectileHit(info);

                //if (hit.Collider != null && hit.Collider.attachedRigidbody != null)
                //{
                //    hit.Collider.attachedRigidbody.AddForce(direction * _hitImpulse, ForceMode.Impulse);
                //}
                //string st = $"hit: {hit.Collider.gameObject.name} at: {hit.Point}, dir: {direction}, tick: {tick}\n";
                //print(st);
                //ConsoleLog.Log(st);
            }
        }

        private Vector3 GetMovePosition(ref ProjectileData data, float currentTick)
        {
            float time = (currentTick - data.FireTick) * Runner.DeltaTime;

            if (time <= 0f)
                return data.FirePosition;

            return data.FirePosition + data.FireVelocity * time + time * time * Physics.gravity;
        }

        private void OnDrawGizmos()
        {
            
            foreach(var hit in PlayerHits)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(hit.Value.d, 0.1f);
                Gizmos.color = Color.blue;
                Gizmos.DrawSphere(hit.Value.t, 0.1f);
            }
        }

        // DATA STRUCTURES

        private struct ProjectileData : INetworkStruct
        {
            public bool IsActive => FireTick > 0;

            public int FireTick;
            public int FinishTick;
            public int Index;

            public Vector3 FirePosition;
            public Vector3 FireVelocity;
            
            [Networked]
            public Vector3 HitPosition { get; set; }
            [Networked]
            public Vector3 HitDirection { get; set; }

            public Vector3 PointingDirection;
        }

        [System.Serializable]
        public struct ProjectileHitInfo : INetworkStruct
        {
            public int Tick;
            public int ProjectileIndex;
            public Vector3 HitPosition;
            public Vector3 HitDirection;

            public Vector3 HitboxPosition;
            public Quaternion HitboxRotation;

            public int HitboxId;
            
            public float Energy;
            
            //Causes big weaver errors
            //public NetworkObject HitObject;
            public NetworkId HitObjectId;
            public bool Processed;

            public bool IsValidTank(NetworkRunner Runner, ref NetworkObject obj, ref PlayerTankController ptc)
            {
                if(!HitObjectId.IsValid)
                    return false;
                Runner.TryFindObject(HitObjectId, out obj);
                if (!obj)
                    return false;
                ptc = obj.GetComponent<PlayerTankController>();
                if (!ptc)
                    return false;
                return true;
            }

        }

    }
}
