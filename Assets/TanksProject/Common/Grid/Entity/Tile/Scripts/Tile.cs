using System.Collections.Generic;

using UnityEngine;

namespace TanksProject.Common.Grid.Entity.Tile
{
    public class Tile
    {
        public int x;
        public int y;
        public GameObject go;
        public List<Tile> neighbours;
    }
}
