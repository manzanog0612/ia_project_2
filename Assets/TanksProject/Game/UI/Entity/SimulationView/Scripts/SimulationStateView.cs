using UnityEngine;
using UnityEngine.UI;

using TanksProject.Game.Entity.PopulationController;
using TanksProject.Game.Data;

namespace TanksProject.Game.UI
{
    public class SimulationStateView : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private TEAM team = default;

        [Header("Entities")]
        [SerializeField] private Text teamTxt;
        [SerializeField] private Text generationsCountTxt;
        [SerializeField] private Text bestFitnessTxt;
        [SerializeField] private Text avgFitnessTxt;
        [SerializeField] private Text worstFitnessTxt;
        [SerializeField] private Button saveSimBtn;
        #endregion

        #region PRIVATE_FIELDS
        private PopulationManager populationManager = null;

        private string generationsCountText;
        private string bestFitnessText;
        private string avgFitnessText;
        private string worstFitnessText;
        private int lastGeneration = 0;
        #endregion

        #region UNITY_CALLS
        private void Start()
        {
            populationManager = GameData.Inst.PM(team);

            teamTxt.text = team == TEAM.RED ? "RED" : "BLUE";
            teamTxt.color = team == TEAM.RED ? Color.red : Color.blue;

            if (string.IsNullOrEmpty(generationsCountText))
            {
                generationsCountText = generationsCountTxt.text;
            }
            if (string.IsNullOrEmpty(bestFitnessText))
            {
                bestFitnessText = bestFitnessTxt.text;
            }
            if (string.IsNullOrEmpty(avgFitnessText))
            {
                avgFitnessText = avgFitnessTxt.text;
            }
            if (string.IsNullOrEmpty(worstFitnessText))
            {
                worstFitnessText = worstFitnessTxt.text;
            }

            saveSimBtn.onClick.AddListener(OnSaveSim);
        }

        private void OnEnable()
        {
            if (string.IsNullOrEmpty(generationsCountText))
            {
                generationsCountText = generationsCountTxt.text;
            }
            if (string.IsNullOrEmpty(bestFitnessText))
            {
                bestFitnessText = bestFitnessTxt.text;
            }
            if (string.IsNullOrEmpty(avgFitnessText))
            {
                avgFitnessText = avgFitnessTxt.text;
            }
            if (string.IsNullOrEmpty(worstFitnessText))
            {
                worstFitnessText = worstFitnessTxt.text;
            }

            generationsCountTxt.text = string.Format(generationsCountText, 0);
            bestFitnessTxt.text = string.Format(bestFitnessText, 0);
            avgFitnessTxt.text = string.Format(avgFitnessText, 0);
            worstFitnessTxt.text = string.Format(worstFitnessText, 0);
        }

        private void LateUpdate()
        {
            if (lastGeneration != populationManager.Generation)
            {
                lastGeneration = populationManager.Generation;
                generationsCountTxt.text = string.Format(generationsCountText, populationManager.Generation);
                bestFitnessTxt.text = string.Format(bestFitnessText, populationManager.BestFitness);
                avgFitnessTxt.text = string.Format(avgFitnessText, populationManager.AvgFitness);
                worstFitnessTxt.text = string.Format(worstFitnessText, populationManager.WorstFitness);
            }
        }
        #endregion

        #region PUBLIC_METHODS
        public void OnStopButtonClick()
        {
            lastGeneration = 0;
        }
        #endregion

        #region PRIVATE_METHODS
        private void OnSaveSim()
        {
            populationManager.SaveCurrentSim();
        }
        #endregion
    }
}
