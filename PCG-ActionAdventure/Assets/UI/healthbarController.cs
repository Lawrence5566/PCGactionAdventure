using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthbarController : MonoBehaviour
{
	Transform bar;

    // Start is called before the first frame update
    void Start()
    {
		bar = transform.Find ("bar");
    }

	public void SetSize(float sizeNormalised){
		bar.localScale = new Vector3 (sizeNormalised, 1f);
		if (sizeNormalised < 0) {
			bar.localScale = new Vector3 (0, 1f);
		}
	}
		
}
