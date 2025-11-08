using System;
using System.Collections.Generic;
using UnityEngine;

public class PowerGenerator : Conveyor, IElectricBuilding
{
    [Header("Energy Settings")]
    [SerializeField] private float energyPerSecond = 10f;
    [SerializeField] private float maxEnergy = 100f;
    [SerializeField] private float energyDurationPerResource = 5f; // Durée de production par ressource

    public float CurrentEnergy => currentEnergyRef.Value;
    public float MaxEnergy => maxEnergy;
    public float EnergyDeltaPerSecond => energyPerSecond;
    public Vector3 Position => transform.position;

    #region Refs
    [NonSerialized] public Ref<float> currentEnergyRef;
    [NonSerialized] public Ref<float> maxEnergyRef;
    #endregion

    private bool isConsuming = false;
    private float energyTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        currentEnergyRef = new Ref<float>();
        maxEnergyRef = new Ref<float>(maxEnergy);
    }

    protected override void Update()
    {
        base.Update();
        HandleEnergyProduction();
    }

    private void HandleEnergyProduction()
    {
        // Si on est plein, on arrête tout
        if (currentEnergyRef.Value >= maxEnergyRef.Value)
        {
            isConsuming = false;
            return;
        }

        // Si on n’est pas déjà en train de consommer, on essaie de prendre une ressource
        if (!isConsuming && resources.Count > 0)
        {
            ConsumeResource();
        }

        // Si on est en phase de production
        if (isConsuming)
        {
            energyTimer -= Time.deltaTime;
            currentEnergyRef.Value = Mathf.Min(maxEnergyRef.Value, currentEnergyRef.Value + energyPerSecond * Time.deltaTime);

            // Quand la ressource a fini de produire
            if (energyTimer <= 0f)
            {
                isConsuming = false;
            }
        }
    }

    private void ConsumeResource()
    {
        // Si l’énergie est déjà pleine → on ne consomme pas
        if (currentEnergyRef.Value >= maxEnergyRef.Value) return;

        // Retirer la première ressource de la liste
        Resource r = resources[0];
        resources.RemoveAt(0);
        Destroy(r.gameObject);

        // Démarrer la production
        isConsuming = true;
        energyTimer = energyDurationPerResource;
    }

    protected override void CreateBuildingModules()
    {
        base.CreateBuildingModules();
        modules.Add(CreateSliderModule(currentEnergyRef, maxEnergyRef, false, Color.yellowGreen, Color.yellow));
    }

    public float AddEnergy(float amount)
    {
        float accepted = Mathf.Min(amount, maxEnergyRef.Value - currentEnergyRef.Value);
        currentEnergyRef.Value += accepted;
        return amount - accepted;
    }
}
