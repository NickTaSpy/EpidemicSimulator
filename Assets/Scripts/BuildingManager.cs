using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using TMPro;

namespace Epsim
{
    public class BuildingManager : MonoBehaviour
    {
        [SerializeField] private float EntranceMaxDistance = 100f;

        [SerializeField] private AbstractMap Map;
        [SerializeField] private TileBuildingsCollectionModifier TileBuildings;
        [SerializeField] private TMP_Text BuildingCountUI;
        [SerializeField] private Material BuildingMaterial;

        public int EntranceCount => Entrances.Count;

        private readonly Dictionary<VectorEntity, Vector3> Entrances = new Dictionary<VectorEntity, Vector3>();

        private void OnEnable()
        {
            Map.MapVisualizer.OnTileVectorProcessingFinished += OnTileFinished;
        }

        private void OnDestroy()
        {
            Map.MapVisualizer.OnTileVectorProcessingFinished -= OnTileFinished;
        }

        public void UpdateBuildingCountUI()
        {
            BuildingCountUI.text = EntranceCount.ToString();
        }

        public void CalculateEntrances()
        {
            foreach (var building in TileBuildings.GetAllBuildings())
            {
                if (NavMesh.SamplePosition(building.MeshRenderer.bounds.center, out NavMeshHit hit, EntranceMaxDistance, NavMesh.AllAreas))
                    //&& !building.MeshRenderer.bounds.Contains(hit.position))
                {
                    Entrances.Add(building, hit.position);
                }
            }
        }

        public NativeList<int> GetBuildingIDsQueue()
        {
            var list = new NativeList<int>(EntranceCount, Allocator.Persistent);
            for (int i = 0; i < EntranceCount; i++)
            {
                list.Add(i);
            }
            return list;
        }

        public NativeHashMap<int, float2> GetEntrancesAsNativeHashMap()
        {
            var hashMap = new NativeHashMap<int, float2>(EntranceCount, Allocator.Persistent);

            int id = 0;
            foreach (var entrance in Entrances)
            {
                hashMap.Add(id++, new float2(entrance.Value.x, entrance.Value.z));
            }

            return hashMap;
        }

        private void OnTileFinished(UnityTile tile)
        {
            if (TileBuildings.GetTileBuildings(tile, out var buildings))
            {
                CombineBuildings(tile, buildings);
            }
        }

        private void CombineBuildings(UnityTile tile, List<VectorEntity> buildings)
        {
            var combine = new CombineInstance[buildings.Count];

            for (int i = 0; i < buildings.Count; i++)
            {
                buildings[i].MeshFilter.sharedMesh.SetTriangles(buildings[i].MeshFilter.sharedMesh.triangles, 0);
                combine[i].mesh = buildings[i].MeshFilter.sharedMesh;
                combine[i].transform = buildings[i].GameObject.transform.localToWorldMatrix;
                buildings[i].MeshFilter.gameObject.SetActive(false);
            }

            var obj = new GameObject();
            obj.transform.parent = tile.transform;
            obj.transform.position = Vector3.zero;

            var mf = obj.AddComponent<MeshFilter>();
            mf.mesh = new Mesh();
            mf.mesh.CombineMeshes(combine, true, true);

            var mr = obj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = BuildingMaterial;
            mr.allowOcclusionWhenDynamic = true;

#if UNITY_EDITOR
            obj.name = "Buildings";
            mf.mesh.name = "CombinedBuildingsMesh";
#endif
        }
    }
}
