using UnityEngine;
using System.Collections;
//This script executes commands to change character animations
[RequireComponent (typeof (Animator))]
public class TB1_Actions : MonoBehaviour {




	private Animator animator,animatorWeap;
 
	void Awake () {
		animator = GetComponent<Animator> ();
    }
 


	public void Idle()
	{
		animator.SetBool ("Idle", true);
	}
	public void OpenDoor()
	{
		animator.SetBool ("OpenDoor", true);
	}
	public void Destroy1()
	{
		animator.SetBool ("Destroy1", true);
	}
	public void Destroy2()
	{
		animator.SetBool ("Destroy2", true);
	}
 

 
}
