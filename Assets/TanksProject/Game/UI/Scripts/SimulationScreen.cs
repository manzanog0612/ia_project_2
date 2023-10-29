using System;

using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField] private Button saveBtn;
        [SerializeField] private GameObject startConfigurationScreen;

        [SerializeField] private SimulationStateView[] simulationStateViews = null;
        #endregion

        #region PRIVATE_FIELDS
        private string timerText;
        #endregion

        #region ACTIONS
        private Action onStopSimulation = null;
        private Action onPauseSimulation = null;
        private Action onSaveData = null;
        #endregion

        #region UNITY_CALLS
        private void Start()
        {
            timerSlider.onValueChanged.AddListener(OnTurnDurationChangeChange);
            timerText = timerTxt.text;

            timerTxt.text = string.Format(timerText, GameData.Inst.TurnDuration * 100);

            pauseBtn.onClick.AddListener(OnPauseButtonClick);
            stopBtn.onClick.AddListener(OnStopButtonClick);
            saveBtn.onClick.AddListener(OnSaveData);
        }
        #endregion

        #region PUBLIC_METHODS
        public void Init(Action onPauseSimulation, Action onStopSimulation, Action onSaveData)
        {
            this.onPauseSimulation = onPauseSimulation;
            this.onStopSimulation = onStopSimulation;
            this.onSaveData = onSaveData;
        }
        #endregion

        #region PRIVATE_FIELDS
        private void OnTurnDurationChangeChange(float value)
        {
            GameData.Inst.TurnDuration = value / 100f;
            timerTxt.text = string.Format(timerText, (int)(GameData.Inst.TurnDuration * 100));
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

        private void OnSaveData()
        {
            onSaveData.Invoke();
        }
        #endregion
    }
}