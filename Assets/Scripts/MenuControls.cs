/* Copyright (C) 2024 Christopher Buer */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuControls : MonoBehaviour
{
    public void OnPlayButtonClicked()
    {        
        SceneManager.LoadScene("TestLevel");
    }
}
