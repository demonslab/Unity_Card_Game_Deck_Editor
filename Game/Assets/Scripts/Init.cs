using UnityEngine;
using System.Collections;

public class Init : MonoBehaviour {
	private static int numCards = 6;
	public static string[] cardImages = new string[numCards];
	public static GameObject[] allCards = new GameObject[numCards];

	public static void insertImage(int id, string path){
		cardImages [id-1] = path; 
	}

	public static void newCard(int id, string name, int ab1id, int ab2id, int hp, int ap, int type){
		GameObject newCard = (GameObject) Instantiate (Resources.Load("Prefab/AddCard"));
		newCard.GetComponent<Card>().AssignStats (id, name, ab1id, ab2id, hp, ap, type);

		newCard.SetActive (false);
		allCards [id - 1] = newCard;
		//Debug.Log (newCard.GetComponent<Card>().toString());
		if (id == numCards) sendArr();
	}

	private static void sendArr(){
		GameObject.Find ("CardGrid").GetComponent<GridScript> ().assignCards (allCards);
	}

}
