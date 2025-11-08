using UnityEngine;

public interface IElectricBuilding
{
    /// <summary>Quantité actuelle d’électricité stockée (en unités).</summary>
    float CurrentEnergy { get; }

    /// <summary>Capacité maximale d’énergie que le bâtiment peut contenir.</summary>
    float MaxEnergy { get; }

    /// <summary>Production nette par seconde (positive = produit, négative = consomme).</summary>
    float EnergyDeltaPerSecond { get; }

    /// <summary>Position du bâtiment dans le monde (utile pour les connexions visuelles).</summary>
    Vector3 Position { get; }

    /// <summary>Ajoute ou retire de l’énergie. Retourne l’énergie restante non acceptée.</summary>
    float AddEnergy(float amount);
}