using Mapbox.Unity.Map;
using Mapbox.Unity.MeshGeneration.Data;
using Mapbox.Unity.MeshGeneration.Modifiers;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Epsim
{
    public class TileBuildingsCollectionModifier : GameObjectModifier
    {
        public int TileBuildingsCount => TileBuildings.Count;

        public bool GetTileBuildings(UnityTile tile, out List<VectorEntity> buildings) => TileBuildings.TryGetValue(tile, out buildings);

        public List<KeyValuePair<UnityTile, List<VectorEntity>>> GetAllTilesBuildings() => TileBuildings.ToList();

        public List<VectorEntity> GetAllBuildings() => TileBuildings.SelectMany(x => x.Value).ToList();

        private Dictionary<UnityTile, List<VectorEntity>> TileBuildings;

        public override void Initialize()
        {
            if (TileBuildings == null)
            {
                TileBuildings = new Dictionary<UnityTile, List<VectorEntity>>();
            }
            else
            {
                TileBuildings.Clear();
            }
        }

        public override void Run(VectorEntity ve, UnityTile tile)
        {
            if (TileBuildings.TryGetValue(tile, out var buildings))
            {
                buildings.Add(ve);
            }
            else
            {
                TileBuildings.Add(tile, new List<VectorEntity> { ve });
            }
        }
    }
}
