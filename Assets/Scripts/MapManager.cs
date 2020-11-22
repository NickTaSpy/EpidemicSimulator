using Epsim.Human;
using Epsim.Profile;
using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Utils;
using Reese.Nav;
using Reese.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Scripting;
using UnityEngine.UIElements;

namespace Epsim
{
    public class MapManager : MonoBehaviour
    {
        [SerializeField] private AbstractMap Map;
        [SerializeField] private ProfileManager ProfileManager;
        [SerializeField] private BuildingManager BuildingManager;
        [SerializeField] private TMP_Text HomeWorkPairsCount;
        [SerializeField] private float MaxTravelDistance;

        [SerializeField] private Material SusceptibleMaterial;
        [SerializeField] private Material InfectedMaterial;
        [SerializeField] private Material RecoveredMaterial;

        private NavMeshSurface Surface;

        private EntityManager EntityManager => World.DefaultGameObjectInjectionWorld.EntityManager;

        private void Awake()
        {
            Map.InitializeOnStart = false; // Prevent double initialization of the map.

            // Create nav mesh surface.
            Surface = Map.gameObject.AddComponent<NavMeshSurface>();
            //Surface.buildHeightMesh = true;
            Surface.layerMask = LayerMask.GetMask("Surface");

            // Set not walkable area.
            var extents = Map.Options.extentOptions.defaultExtents.rangeAroundCenterOptions;
            float max = (Mathf.Max(extents.west, Mathf.Max(extents.south, Mathf.Max(extents.east, extents.north))) * 2 + 5) * Map.Options.scalingOptions.unityTileSize;
            var volume = Map.gameObject.AddComponent<NavMeshModifierVolume>();
            volume.size = new Vector3(max, 100f, max);
            volume.center = new Vector3(volume.center.x, 100 / 2 + 0.05f, volume.center.z);
            volume.area = 1;

            // Ground collider.
            var boxCol = Map.gameObject.AddComponent<BoxCollider>();
            boxCol.center = new Vector3(0f, -0.015f, 0f);
            boxCol.size = new Vector3(max, 0.03f, max);
        }

        private void OnEnable()
        {
            Map.MapVisualizer.OnMapVisualizerStateChanged += OnMapVisualizerStateChanged;
        }

        private void OnDisable()
        {
            Map.MapVisualizer.OnMapVisualizerStateChanged -= OnMapVisualizerStateChanged;
        }

        public void StartNewMap(Vector2d location, int zoom = -1)
        {
            CreateMap(location, zoom);
        }

        private void CreateMap(Vector2d location, int zoom = -1)
        {
            if (zoom >= 0)
            {
                Map.Initialize(location, Map.AbsoluteZoom);
            }
            else
            {
                Map.Initialize(location, zoom);
            }
        }

        private void OnMapVisualizerStateChanged(ModuleState state)
        {
            if (state == ModuleState.Finished) // Map finished.
            {
                Surface.BuildNavMesh();
                AddNavSurface();
                DestroyRoads();

                if (!NavMesh.SamplePosition(Vector3.up, out NavMeshHit hit, 100f, NavMesh.AllAreas))
                    Debug.LogError("NavMesh: Could not sample initial position.", this);

                float buildingHeight = hit.position.y;

                BuildingManager.CalculateEntrances();
                BuildingManager.UpdateBuildingCountUI();
                InitHumanSystem();
                EntityManager.World.GetOrCreateSystem<HumanDataAssignmentSystem>().SimProfile = ProfileManager.Profile;
                int pop = ProfileManager.Profile.PopulationProfile.Population;
                int pairCount = InitBuildingAssignmentSystem(pop, buildingHeight);
                SpawnHumans(new float3(hit.position.x, hit.position.y + 1, hit.position.z), math.clamp(pop, 0, pairCount));
            }
        }

        private void SpawnHumans(float3 position, int count)
        {
            var prefabEntity = EntityManager.CreateEntityQuery(typeof(EntityPrefab)).GetSingleton<EntityPrefab>().Value;

            SpawnSystem.Enqueue(new Spawn()
                .WithPrefab(prefabEntity)
                .WithComponentList(
                    new NavAgent
                    {
                        JumpDegrees = 45,
                        JumpGravity = 200,
                        TranslationSpeed = 20,                        
                        TypeID = NavUtil.GetAgentType(NavConstants.HUMANOID),
                        Offset = new float3(0, 1, 0)
                    },
                    new NavNeedsSurface { },
                    new Translation
                    {
                        Value = position
                    }
                ),
                count
            );
        }

        private void InitHumanSystem()
        {
            var humanSystem = EntityManager.World.GetOrCreateSystem<HumanSystem>();
            humanSystem.SusceptibleMaterial = SusceptibleMaterial;
            humanSystem.InfectedMaterial = InfectedMaterial;
            humanSystem.RecoveredMaterial = RecoveredMaterial;
            humanSystem.Enabled = true;
        }

        private int InitBuildingAssignmentSystem(int population, float buildingHeight)
        {
            var buildingAssignmentSystem = EntityManager.World.GetOrCreateSystem<BuildingAssignmentSystem>();
            buildingAssignmentSystem.BuildingToPosition = BuildingManager.GetEntrancesAsNativeHashMap();
            buildingAssignmentSystem.BuildingHeight = buildingHeight;

            //FilterOutSimilar(buildingAssignmentSystem.BuildingToPosition, 0.1f);

            var houses = new List<int>();
            var houseCapacity = new List<int>();
            var jobs = new List<int>();
            var jobCapacity = new List<int>();
            for (int i = 0; i < BuildingManager.EntranceCount / 2; i++)
            {
                houses.Add(i);
                houseCapacity.Add(UnityEngine.Random.Range(5, 11));
                jobs.Add(BuildingManager.EntranceCount - i);
                jobCapacity.Add(UnityEngine.Random.Range(5, 20));
            }

            var pairs = new NativeList<int2>(population, Allocator.Persistent);

            for (int i = 0; i < houses.Count; i++)
            {
                if (pairs.Length == population)
                    break;

                float2 house = buildingAssignmentSystem.BuildingToPosition[i];

                if (houseCapacity[i] == 0)
                    continue;

                for (int j = 0; j < jobs.Count; j++)
                {
                    float2 job = buildingAssignmentSystem.BuildingToPosition[j];

                    if (jobCapacity[j] == 0)
                        continue;

                    if (math.distance(house, job) > MaxTravelDistance)
                        continue;

                    //if (!IsPathValid(new Vector3(house.x, buildingHeight, house.y), new Vector3(job.x, buildingHeight, job.y)))
                    //    continue;

                    houseCapacity[i] -= 1;
                    jobCapacity[j] -= 1;
                    pairs.Add(new int2(i, j));

                    if (houseCapacity[i] == 0)
                        break;

                    if (pairs.Length == population)
                        break;
                }
            }

            Shuffle(pairs); // Randomize order.

            buildingAssignmentSystem.Pairs = pairs;
            HomeWorkPairsCount.text = pairs.Length.ToString();

            return pairs.Length;
        }

        private void AddNavSurface()
        {
            var surfEnt = Surface.gameObject.AddComponent<NavSurfaceAuthoring>();
            surfEnt.JumpableSurfaces = new List<NavSurfaceAuthoring>();
            var cte = surfEnt.gameObject.AddComponent<ConvertToEntity>();
            cte.ConversionMode = ConvertToEntity.Mode.ConvertAndInjectGameObject;
        }

        private void DestroyRoads()
        {
            int layer = LayerMask.NameToLayer("Surface");
            var renderers = FindObjectsOfType<MeshRenderer>(false);
            foreach (var renderer in renderers)
            {
                if (renderer.gameObject.layer == layer)
                {
                    Destroy(renderer.gameObject);
                }
            }
        }

        private void Shuffle<T>(INativeList<T> list, int times = 1) where T : struct
        {
            for (int i = 0; i < times; i++)
            {
                int n = list.Length;
                while (n > 1)
                {
                    n--;
                    int k = UnityEngine.Random.Range(0, n + 1);
                    T value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
            }
        }

        private bool IsPathValid(Vector3 pos1, Vector3 pos2)
        {
            NavMeshPath path = new NavMeshPath();
            NavMesh.CalculatePath(pos1, pos2, NavMesh.AllAreas, path);

            return path.status == NavMeshPathStatus.PathComplete;
        }

        private void FilterOutSimilar(NativeHashMap<int, float2> keyValues, float minDistance)
        {
            var keyValuesArray = keyValues.GetKeyValueArrays(Allocator.Temp);

            for (int i = 0; i < keyValuesArray.Length; i++)
            {
                for (int j = 0; j < keyValuesArray.Length; j++)
                {
                    if (keyValuesArray.Keys[j] == -1 || keyValuesArray.Keys[i] == keyValuesArray.Keys[j])
                        continue;

                    if (math.distance(keyValuesArray.Values[j], keyValuesArray.Values[j]) <= minDistance)
                    {
                        keyValuesArray.Keys[j] = -1;
                        keyValues.Remove(keyValuesArray.Keys[j]);
                    }
                }
            }

            keyValuesArray.Dispose();
        }
    }
}