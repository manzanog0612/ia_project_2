﻿using System;

using UnityEngine;
using UnityEngine.UI;

using TanksProject.Game.Data;
using TanksProject.Common.Saving;

namespace TanksProject.Game.UI
{
    public class StartConfigurationScreen : MonoBehaviour
    {
        #region EXPOSED_FIELDS
        [SerializeField] private Text populationCountTxt;
        [SerializeField] private Slider populationCountSlider;
        [SerializeField] private Text turnsPerGenerationTxt;
        [SerializeField] private Slider turnsPerGenerationSlider;
        [SerializeField] private Text turnDurationTxt;
        [SerializeField] private Slider turnDurationSlider;
        [SerializeField] private Text eliteCountTxt;
        [SerializeField] private Text mutationChanceTxt;
        [SerializeField] private Slider mutationChanceSlider;
        [SerializeField] private Text mutationRateTxt;
        [SerializeField] private Slider mutationRateSlider;
        [SerializeField] private Text hiddenLayersCountTxt;
        [SerializeField] private Slider hiddenLayersCountSlider;
        [SerializeField] private Text neuronsPerHLCountTxt;
        [SerializeField] private Slider neuronsPerHLSlider;
        [SerializeField] private Text biasTxt;
        [SerializeField] private Slider biasSlider;
        [SerializeField] private Text sigmoidSlopeTxt;
        [SerializeField] private Slider sigmoidSlopeSlider;
        [SerializeField] private Button startButton;
        [SerializeField] private Button startDefaultButton;
        [SerializeField] private Button loadSimButton;
        [SerializeField] private GameObject simulationScreen;
        #endregion

        #region PRIVATE_FIELDS
        private string populationText;
        private string turnsPerGenerationText;
        private string turnDurationText;
        private string mutationChanceText;
        private string mutationRateText;
        private string hiddenLayersCountText;
        private string biasText;
        private string sigmoidSlopeText;
        private string neuronsPerHLCountText;
        #endregion

        #region ACTIONS
        private Action onStartSimulation = null;
        private Action onStartDefaultSimulation = null;
        private Action<SimData> onStartLoadedSimulation = null;
        #endregion

        #region UNITY_CALLS
        private void Start()
        {
            populationCountSlider.onValueChanged.AddListener(OnPopulationCountChange);
            turnsPerGenerationSlider.onValueChanged.AddListener(OnTurnsPerGenerationChange);
            turnDurationSlider.onValueChanged.AddListener(OnTurnDurationChange);
            mutationChanceSlider.onValueChanged.AddListener(OnMutationChanceChange);
            mutationRateSlider.onValueChanged.AddListener(OnMutationRateChange);
            hiddenLayersCountSlider.onValueChanged.AddListener(OnHiddenLayersCountChange);
            neuronsPerHLSlider.onValueChanged.AddListener(OnNeuronsPerHLChange);
            biasSlider.onValueChanged.AddListener(OnBiasChange);
            sigmoidSlopeSlider.onValueChanged.AddListener(OnSigmoidSlopeChange);

            populationText = populationCountTxt.text;
            turnsPerGenerationText = turnsPerGenerationTxt.text;
            turnDurationText = turnDurationTxt.text;
            mutationChanceText = mutationChanceTxt.text;
            mutationRateText = mutationRateTxt.text;
            hiddenLayersCountText = hiddenLayersCountTxt.text;
            neuronsPerHLCountText = neuronsPerHLCountTxt.text;
            biasText = biasTxt.text;
            sigmoidSlopeText = sigmoidSlopeTxt.text;

            populationCountSlider.value = GameData.Inst.PopulationCount;
            turnsPerGenerationSlider.value = GameData.Inst.TurnsPerGeneration;
            turnDurationSlider.value = GameData.Inst.TurnDuration * 1000.0f;
            mutationChanceSlider.value = GameData.Inst.MutationChance * 100.0f;
            mutationRateSlider.value = GameData.Inst.MutationRate * 100.0f;
            hiddenLayersCountSlider.value = GameData.Inst.HiddenLayers;
            neuronsPerHLSlider.value = GameData.Inst.NeuronsCountPerHL;
            biasSlider.value = -GameData.Inst.Bias;
            sigmoidSlopeSlider.value = GameData.Inst.P;

            startButton.onClick.AddListener(OnStartButtonClick);
            startDefaultButton.onClick.AddListener(OnStartDefaultButtonClick);
            loadSimButton.onClick.AddListener(OnLoadSim);
        }
        #endregion

        #region PUBLIC_METHODS
        public void Init(Action onStartSimulation, Action onStartDefaultSimulation, Action<SimData> onStartLoadedSimulation)
        {
            this.onStartSimulation = onStartSimulation;
            this.onStartDefaultSimulation = onStartDefaultSimulation;
            this.onStartLoadedSimulation = onStartLoadedSimulation;
        }
        #endregion

        #region PRIVATE_FIELDS
        private void OnLoadSim()
        {
            SimData sim = SaveLoadSystem.LoadSimFile();

            if (sim == null)
            {
                return;
            }

            ConfigurationData config = sim.config;

            populationCountSlider.onValueChanged.Invoke(config.population_count);
            turnsPerGenerationSlider.onValueChanged.Invoke(config.turnsPerGeneration);
            turnDurationSlider.onValueChanged.Invoke(config.turnDuration * 1000.0f);
            mutationChanceSlider.onValueChanged.Invoke(config.mutation_chance * 100.0f);
            mutationRateSlider.onValueChanged.Invoke(config.mutation_rate * 100.0f);
            hiddenLayersCountSlider.onValueChanged.Invoke(config.hidden_layers_count);
            neuronsPerHLSlider.onValueChanged.Invoke(config.neurons_per_hidden_layers);
            biasSlider.onValueChanged.Invoke(-config.bias);
            sigmoidSlopeSlider.onValueChanged.Invoke(config.sigmoid);

            onStartLoadedSimulation.Invoke(sim);

            gameObject.SetActive(false);
            simulationScreen.SetActive(true);
        }

        private void OnPopulationCountChange(float value)
        {
            GameData.Inst.PopulationCount = (int)value;

            populationCountTxt.text = string.Format(populationText, GameData.Inst.PopulationCount);
        }

        private void OnTurnsPerGenerationChange(float value)
        {
            GameData.Inst.TurnsPerGeneration = (int)value;

            turnsPerGenerationTxt.text = string.Format(turnsPerGenerationText, GameData.Inst.TurnsPerGeneration);
        }
        private void OnTurnDurationChange(float value)
        {
            GameData.Inst.TurnDuration = value / 1000.0f;

            turnDurationTxt.text = string.Format(turnDurationText, (int)(GameData.Inst.TurnDuration * 1000f));
        }

        private void OnMutationChanceChange(float value)
        {
            GameData.Inst.MutationChance = value / 100.0f;

            mutationChanceTxt.text = string.Format(mutationChanceText, (int)(GameData.Inst.MutationChance * 100));
        }

        private void OnMutationRateChange(float value)
        {
            GameData.Inst.MutationRate = value / 100.0f;

            mutationRateTxt.text = string.Format(mutationRateText, (int)(GameData.Inst.MutationRate * 100));
        }

        private void OnHiddenLayersCountChange(float value)
        {
            GameData.Inst.HiddenLayers = (int)value;


            hiddenLayersCountTxt.text = string.Format(hiddenLayersCountText, GameData.Inst.HiddenLayers);
        }

        private void OnNeuronsPerHLChange(float value)
        {
            GameData.Inst.NeuronsCountPerHL = (int)value;

            neuronsPerHLCountTxt.text = string.Format(neuronsPerHLCountText, GameData.Inst.NeuronsCountPerHL);
        }

        private void OnBiasChange(float value)
        {
            GameData.Inst.Bias = -value;

            biasTxt.text = string.Format(biasText, GameData.Inst.Bias.ToString("0.00"));
        }

        private void OnSigmoidSlopeChange(float value)
        {
            GameData.Inst.P = value;

            sigmoidSlopeTxt.text = string.Format(sigmoidSlopeText, GameData.Inst.P.ToString("0.00"));
        }


        private void OnStartButtonClick()
        {
            onStartSimulation.Invoke();

            gameObject.SetActive(false);
            simulationScreen.SetActive(true);
        }

        private void OnStartDefaultButtonClick()
        {
            onStartDefaultSimulation.Invoke();

            gameObject.SetActive(false);
            simulationScreen.SetActive(true);
        }
        #endregion
    }
}