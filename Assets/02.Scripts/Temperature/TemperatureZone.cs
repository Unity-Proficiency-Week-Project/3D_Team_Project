using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureZone : MonoBehaviour
{
    public float temp = 20f; //기본 온도

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            PlayerTemperature playerTemp = other.GetComponent<PlayerTemperature>();
            if(playerTemp != null)
            {
                playerTemp.EnterTempZone(this);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerTemperature playerTemp = other.GetComponent<PlayerTemperature>();
            if (playerTemp != null)
            {
                playerTemp.ExitTempZone(this);
            }
        }
    }
}
