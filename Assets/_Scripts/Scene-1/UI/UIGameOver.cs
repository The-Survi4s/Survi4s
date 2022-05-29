using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOver : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Text _nameTextbox;
    [SerializeField] private Text _killCountTextbox;

    public void Start()
    {
        GameManager.GameOver += GameOverEventHandler;
        _gameOverPanel.gameObject.SetActive(false);
        _nameTextbox.text = "";
        _killCountTextbox.text = "";
    }

    private void GameOverEventHandler()
    {
        _gameOverPanel.gameObject.SetActive(true);

        // Tampilkan nama semua player dan score masing2 player
        foreach (var player in UnitManager.Instance.players)
        {
            _nameTextbox.text += player.name + "\n";
            _killCountTextbox.text += player.KillCount.ToString() + "\n";
        }
    }
}
