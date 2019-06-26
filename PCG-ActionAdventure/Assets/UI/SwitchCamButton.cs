using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//switches between 2 cameras
[RequireComponent(typeof(Button))]
public class SwitchCamButton : MonoBehaviour
{
    private Button button;
    public Camera camera1;
    public Camera camera2;

    void Start() {
        button = GetComponent<Button>();
        button.onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick() {
        if (camera1.isActiveAndEnabled == true) {
            camera1.enabled = false;
            camera2.enabled = true;
        }
        else {
            camera1.enabled = true;
            camera2.enabled = false;
        }
            
        
    }
}
