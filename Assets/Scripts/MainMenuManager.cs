using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    public Button startGameBtn;
    // Start is called before the first frame update
    void Start()
    {
        startGameBtn.onClick.AddListener(() =>
        {
            SceneManager.LoadScene(1);
        });
    }

    // Update is called once per frame
    void Update()
    {

    }
}
