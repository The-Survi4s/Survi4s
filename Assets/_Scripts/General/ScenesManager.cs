using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesManager : MonoBehaviour
{
    // Eazy access --------------------------------------------------------
    public static ScenesManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    public void LoadScene(int id)
    {
        SceneManager.LoadScene(id);
    }

    [SerializeField] private GameObject _disconnectedPanel;

    public void SetDisconnectedPanelActive(bool isActive) => _disconnectedPanel.SetActive(isActive);
}
