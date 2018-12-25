using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TankGame
{
    public class UserControl : MonoBehaviour {

        protected Tank target;


	    void Start () {
		
	    }
    
	    // Update is called once per frame
	    void Update () {
            if (Input.GetKey(KeyCode.W)) target.SetMove(1f * Time.deltaTime);
            if (Input.GetKey(KeyCode.S)) target.SetMove(-1f * Time.deltaTime);
            if (Input.GetKey(KeyCode.A)) target.SetRotate(-1f * Time.deltaTime);
            if (Input.GetKey(KeyCode.D)) target.SetRotate(1f * Time.deltaTime);
            if (Input.GetKey(KeyCode.Space)) target.Shoot();
        }
    }
}