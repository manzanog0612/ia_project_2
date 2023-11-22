using System;

using UnityEngine;
using UnityEngine.UI;

using TanksProject.Game.Data;
using TanksProject.Game.Entity.MineController;

namespace TanksProject.Game.UI
{
    public class SimulationScreen : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private Text timerTxt;
        [SerializeField] private Text minesTotalTxt;
        [SerializeField] private Text minesLeftTxt;
        [SerializeField] private Text minesBestTxt;
        [SerializeField] private Text minesWorstTxt;
        [SerializeField] private Slider timerSlider;
        [SerializeField] private Button pauseBtn;
        [SerializeField] private Button stopBtn;
        [SerializeField] private Button saveBtn;
        [SerializeField] private GameObject startConfigurationScreen;

        [SerializeField] private SimulationStateView[] simulationStateViews = null;
        #endregion

        #region PRIVATE_FIELDS
        private string timerText;

        private int bestMines = int.MaxValue;
        private int worstMines = 0;
        #endregion

        #region ACTIONS
        private Action onStopSimulation = null;
        private Action onPauseSimulation = null;
        private Action onSaveData = null;
        #endregion

        #region UNITY_CALLS
        private void OnEnable()
        {            
            if (timerText == null)
            {
                timerText = timerTxt.text;
            }

            timerTxt.text = string.Format(timerText, GameData.Inst.TurnDuration * 1000f);
            timerSlider.value = GameData.Inst.TurnDuration * 1000.0f;
            OnSetWorstMines(0);
            OnSetBestMines(int.MaxValue);
        }

        private void Start()
        {
            timerSlider.onValueChanged.AddListener(OnTurnDurationChangeChange);

            pauseBtn.onClick.AddListener(OnPauseButtonClick);
            stopBtn.onClick.AddListener(OnStopButtonClick);
            saveBtn.onClick.AddListener(OnSaveData);
        }

        private void Update()
        {
            minesTotalTxt.text = GameData.Inst.MinesCount.ToString();
        }
        #endregion

        #region PUBLIC_METHODS
        public void Init(Action onPauseSimulation, Action onStopSimulation, Action onSaveData)
        {
            this.onPauseSimulation = onPauseSimulation;
            this.onStopSimulation = onStopSimulation;
            this.onSaveData = onSaveData;
        }

        public void SetMinesAmountLeft(int mines)
        {
            if (mines < bestMines)
            {
                OnSetBestMines(mines);
            }

            minesLeftTxt.text = mines.ToString();
        }

        public void SetMinesAmountLeftAfterEpoch(int mines)
        {
            if (mines > worstMines)
            {
                OnSetWorstMines(mines);
            }
        }
        #endregion

        #region PRIVATE_FIELDS
        private void OnSetBestMines(int mines)
        {
            bestMines = mines;
            minesBestTxt.text = mines.ToString();
        }

        private void OnSetWorstMines(int mines)
        {
            worstMines = mines;
            minesWorstTxt.text = mines.ToString();
        }

        private void OnTurnDurationChangeChange(float value)
        {
            GameData.Inst.TurnDuration = value / 1000f;
            timerTxt.text = string.Format(timerText, (int)(GameData.Inst.TurnDuration * 1000));
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