using Leopotam.Ecs;
using TMPro;
using UnityEngine;

public class TimerSystem : IEcsRunSystem
{
    private SceneData _sceneData;
    private Config _config;
    
    public void Run()
    {
        if (_config.IsCoach) return;
        if (_config.PlayerTurnState == 1)
        {
            if (_config.SecondsPlayer1 > 0)
            {
                _config.SecondsPlayer1 -= Time.deltaTime;
                DisplayTime1(_config.SecondsPlayer1);
            }
        }
        if (_config.PlayerTurnState == 2)
        {
            if (_config.SecondsPlayer2 > 0)
            {
                _config.SecondsPlayer2 -= Time.deltaTime;
                DisplayTime2(_config.SecondsPlayer2);
            }
        }
        void DisplayTime1(float timeToDisplay)
        {
            timeToDisplay += 1;
            float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            _sceneData.Player1Timer.GetComponent<TextMeshProUGUI>().text = $"{minutes:00}:{seconds:00}";
        }
        void DisplayTime2(float timeToDisplay)
        {
            timeToDisplay += 1;
            float minutes = Mathf.FloorToInt(timeToDisplay / 60); 
            float seconds = Mathf.FloorToInt(timeToDisplay % 60);
            _sceneData.Player2Timer.GetComponent<TextMeshProUGUI>().text = $"{minutes:00}:{seconds:00}";
        }
    }
}
