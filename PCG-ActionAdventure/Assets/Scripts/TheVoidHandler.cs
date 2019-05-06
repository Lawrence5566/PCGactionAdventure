using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//on interacting with this object, the player will be sucked into 'the void' and the game will end (or just restart in this case)
public class TheVoidHandler : MonoBehaviour
{
	void OnCollisionEnter(Collision collision){
		//Application.LoadLevel(Application.loadedLevel); //restart level
		SceneManager.LoadScene(StaticStrings.mainSceneName); //restart level
	}
}
