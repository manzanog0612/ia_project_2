using System;

using UnityEngine;
using UnityEngine.UI;

using TanksProject.Game.Entity.PopulationController;
using TanksProject.Game.Data;

namespace TanksProject.Game.UI
{
    public class SimulationScreen : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private Text timerTxt;
        [SerializeField] private Slider timerSlider;
        [SerializeField] private Button pauseBtn;
        [SerializeField] private Button stopBtn;
        [SerializeField] private GameObject startConfigurationScreen;

        [SerializeField] private SimulationStateView[] simulationStateViews = null;
        #endregion

        #region PRIVATE_FIELDS
        private string timerText;
        #endregion

        #region ACTIONS
        private Action onStopSimulation = null;
        private Action onPauseSimulation = null;
        #endregion

        #region UNITY_CALLS
        private void Start()
        {
            timerSlider.onValueChanged.AddListener(OnTimerChange);
            timerText = timerTxt.text;

            timerTxt.text = string.Format(timerText, GameData.Inst.IterationCount);

            pauseBtn.onClick.AddListener(OnPauseButtonClick);
            stopBtn.onClick.AddListener(OnStopButtonClick);
        }
        #endregion

        #region PUBLIC_METHODS
        public void Init(Action onPauseSimulation, Action onStopSimulation)
        {
            this.onPauseSimulation = onPauseSimulation;
            this.onStopSimulation = onStopSimulation;
        }
        #endregion

        #region PRIVATE_FIELDS
        private void OnTimerChange(float value)
        {
            GameData.Inst.IterationCount = (int)value;
            timerTxt.text = string.Format(timerText, GameData.Inst.IterationCount);
        }

        private void OnPauseButtonClick()
        {
            onPauseSimulation.Invoke();
        }

        private void OnStopButtonClick()
        {
            onStopSimulation.Invoke();

            gameObject.SetActive(false);
            startConfigurationScreen.SetActive(true);

            for (int i = 0; i < simulationStateViews.Length; i++)
            {
                simulationStateViews[i].OnStopButtonClick();
            }
        }
        #endregion
    }
}