using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace PlayerScripts
{
    public class PlayerVignetteController : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerInfection playerInfection;

        [SerializeField] private List<Image> vignetteHealthLevels; // 0 - 4
        [SerializeField] private List<Image> vignetteInfectionLevels; // 0 - 4


        private void Start()
        {
            if (playerHealth == null)
                playerHealth = NetworkPlayer.GetLocalInstance().GetComponent<PlayerHealth>();
            if (playerInfection == null)
                playerInfection = NetworkPlayer.GetLocalInstance().GetComponent<PlayerInfection>();
            playerHealth.OnHealthChanged += UpdateVignetteHealth;
            playerInfection.OnInfectionChanged += UpdateVignetteInfection;

            UpdateVignetteHealth(playerHealth.Value);
            UpdateVignetteInfection(playerInfection.Value);

        }

        private void UpdateVignetteHealth(float currentHealth)
        {
            float percent = currentHealth / playerHealth.MaxValue;

            int indexToShow = 0;
            if (percent >= 0.8f)
                indexToShow = 0;
            else if (percent >= 0.55f)
                indexToShow = 1;
            else if (percent >= 0.4f)
                indexToShow = 2;
            else if (percent >= 0.25f)
                indexToShow = 3;
            else
                indexToShow = 4;

            for (int i = 0; i < vignetteHealthLevels.Count; i++)
            {
                vignetteHealthLevels[i].enabled = (i == indexToShow);
            }
        }
        private void UpdateVignetteInfection(float currentInfection)
        {
            float percent = currentInfection / playerInfection.MaxValue;

            int indexToShow = 0;
            if (percent >= 0.8f)
                indexToShow = 0;
            else if (percent >= 0.55f)
                indexToShow = 1;
            else if (percent >= 0.4f)
                indexToShow = 2;
            else if (percent >= 0.25f)
                indexToShow = 3;
            else
                indexToShow = 4;

            for (int i = 0; i < vignetteInfectionLevels.Count; i++)
            {
                vignetteInfectionLevels[i].enabled = (i == indexToShow);
            }
        }
    }
}