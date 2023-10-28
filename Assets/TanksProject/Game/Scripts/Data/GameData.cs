using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using TanksProject.Game.Entity.PopulationController;

namespace TanksProject.Game.Data
{
    public class GameData : MonoBehaviour
    {
        #region SINGLETON
        private static GameData instance = null;

        public static GameData Inst 
        {
            get 
            { 
                if (instance == null)
                {
                    instance = FindObjectOfType<GameData>();
                }

                return instance;
            }
        }
        #endregion

        #region PUBLIC_FIELDS
        [Header("Population")]
        public int PopulationCount = 0;

        public float GenerationDuration = 0;
        public int IterationCount = 0;

        public int EliteCount = 0;
        public float MutationChance = 0;
        public float MutationRate = 0;

        public int InputsCount = 0;
        public int HiddenLayers = 0;
        public int OutputsCount = 0;
        public int NeuronsCountPerHL = 0;
        public float Bias = 0;
        public float P = 0;

        public int TestIndex = 0;
        #endregion

        #region EXPOSED_FIELDS
        [SerializeField] private float[] fitnessTillNewTest = null;
        #endregion

        #region PRIVATE_FIELDS
        private Dictionary<TEAM, PopulationManager> populationManagers = new Dictionary<TEAM, PopulationManager>();
        #endregion

        #region PROPERTIES
        public float[] FitnessTillNewTest { get => fitnessTillNewTest; }
        public int MinesCount { get => PopulationCount * 2; }

        public PopulationManager PM(TEAM team)
        {
            if (!populationManagers.ContainsKey(team))
            {
                populationManagers.Add(team, FindObjectsOfType<PopulationManager>().ToList().Find(pm => pm.Team == team));
            }

            return populationManagers[team];
        }
        #endregion


    }
}
