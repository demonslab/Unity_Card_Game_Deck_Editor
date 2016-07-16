using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class GridScript : MonoBehaviour {
	private RectTransform t;
	private float width;

	public int imageSize;		//Cell Size + Spacing (Y)
	public int columns;
	public int topPadding;

	private GameObject[] cards;
	private bool clear;
	public int numObjs;

	private GameObject selected;

	void Start () {
		this.t = gameObject.GetComponent<RectTransform> ();
		this.width = t.rect.width;
		selected = null;
		resize ();
	}

	private int calcHeight(){
		numObjs = numActive();
		int offset = (int) Math.Ceiling((double) numObjs / columns);
		return (offset * imageSize) + topPadding;
	}

	private void resize(){
		RectTransform t = gameObject.GetComponent<RectTransform> ();
		t.sizeDelta = new Vector2 (this.width, calcHeight ());
	}

	/// <summary>
	/// Fill the grid with an array of card gameobjects
	/// </summary>
	/// <param name="newCards">New cards.</param>
	public void assignCards(GameObject[] newCards){
		cards = newCards;
		foreach (GameObject temp in newCards){
			temp.SetActive (true);
			temp.transform.SetParent (this.transform, false);
		}
		resize ();
	}

	/// <summary>
	/// Shows All Cards in Main Grid
	/// </summary>
	public void resetGrid(){
		assignCards (cards);
	}

	/// <summary>
	/// Clears the grid.
	/// </summary>
	public void clearGrid(){
		foreach (Transform child in transform) {
			//child.gameObject.SetActive (false);
			Destroy(child.gameObject);
		}
		transform.DetachChildren ();
		selected = null;
		gameObject.transform.position = new Vector3(gameObject.transform.position.x, 0, 0);
		resize ();
	}

	/// <summary>
	/// Number of active child objects
	/// </summary>
	/// <returns>The active.</returns>
	private int numActive(){
		int i = 0;			//number of active child objects
		foreach (Transform child in transform) {
			if (child.gameObject.activeSelf)
				i++;
		}
		if (this.gameObject.Equals (GameObject.Find ("DeckGrid"))) {
			GameObject.Find ("CardTotal").GetComponent<Text> ().text = "Count: " + i;
		}
		return i;
	}
		
	public void addtoGrid(){
		DisplayScript obj = GameObject.Find ("CardInfo").GetComponent<DisplayScript> ();
		if (obj.card == null) return;
		if (obj.card.GetComponent<Card> ().card_type != 3) {				//if not a leader card
			GameObject copy = Instantiate (obj.card);
			copy.GetComponent<Card> ().addtoDeck ();
			copy.transform.SetParent (GameObject.Find ("DeckGrid").transform);
			copy.transform.localScale = new Vector3 (1, 1, 1);
			copy.SetActive (true);
		}
		else if (obj.card.GetComponent<Card> ().card_type == 3) {			//if a leader card
			GameObject leaderBlock = GameObject.Find ("LeaderBlock");
			GameObject copy = Instantiate (obj.card);
			removeLeader ();
			leaderBlock.transform.GetChild (0).gameObject.SetActive (false);
			copy.transform.SetParent (leaderBlock.transform);
			copy.transform.localScale = new Vector3 (1, 1, 1);
			copy.SetActive (true);
		}
		resize ();
	}

	public void removeLeader(){
		GameObject leaderBlock = GameObject.Find ("LeaderBlock");
		if (leaderBlock.transform.childCount > 1) {
			Destroy (leaderBlock.transform.GetChild (1).gameObject);
		}
		leaderBlock.transform.GetChild (0).gameObject.SetActive (true);
	}

	public void removeCard(){
		if (selected != null && this.gameObject.Equals (GameObject.Find ("DeckGrid"))) {
			selected.transform.SetParent(null);
			Destroy (selected.gameObject);
			selected = null;
			resize ();
		}
	}

	public void searchCards(){
		string feed = GameObject.Find ("InputField").transform.GetChild (2).GetComponent<Text> ().text;
		string limit = GameObject.Find ("SearchStuff").GetComponent<searchScript> ().searchLimits ();

		foreach (Transform child in transform) {
			child.gameObject.SetActive (false);
		}
		GameObject.Find ("InitOb").GetComponent<TestDB> ().searchDB (feed,limit);
		resize();
	}

	public void setActiveChild(int i){
		transform.GetChild (i - 1).gameObject.SetActive (true);
	}


	public void setSelected(GameObject theCard){
		selected = theCard;
	}
}
