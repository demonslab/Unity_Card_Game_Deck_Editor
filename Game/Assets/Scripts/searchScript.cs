using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class searchScript : MonoBehaviour {

	public GameObject infTog;
	public GameObject supTog;
	public GameObject leadTog;

	private int inf;
	private int sup;
	private int lead;

	void Start(){
		setAll ();
	}

	public void setAll(){
		inf = 1;
		sup = 1;
		lead = 1;
	}

	public void togInf(){
		inf = infTog.GetComponent<Toggle> ().isOn == true?1:0;
	}

	public void togSup(){
		sup = supTog.GetComponent<Toggle> ().isOn == true?1:0;
	}

	public void togLead(){
		lead = leadTog.GetComponent<Toggle> ().isOn == true?1:0;
	}

	public string searchLimits(){
		int limit = (inf*100) + (sup*10) + lead;
		switch (limit) {
			case 001: return "=3";
			case 010: return "=2";
			case 100: return "=1";
			case 110: return "!=3";
			case 101: return "!=2";
			case 011: return "!=1";
			case 111: return "!=0";
			default: return "=0";
		}
	}
}
