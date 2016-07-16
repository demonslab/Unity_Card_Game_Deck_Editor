using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Card : MonoBehaviour {
	
	//private SpriteRenderer image;
	public Sprite img;

	public int cardID;
	public string nname;
	public int ability1ID;
	public int ability2ID;
	public int numHP;
	public int numAP;
	public int card_type;

	public bool inDeck;
	public GameObject instance;

	public void AssignStats(int id, string cName, int ability1, int ability2, int hp, int ap, int type){
		this.cardID = id;
		this.nname = cName;
		this.ability1ID = ability1;
		this.ability2ID = ability2;
		this.numHP = hp;
		this.numAP = ap;
		this.card_type = type;

		this.newImage (id);
		this.instance = this.gameObject;
		this.inDeck = false;
	}

	void newImage(int ID){
		img = Resources.Load<Sprite>(Init.cardImages [cardID-1]);
		gameObject.GetComponent<Image> ().sprite = img;
	}

	public string toString(){
		return "To String Method:"
		+ "\nId: " + this.cardID
		+ "\nName: " + this.nname
		+ "\nAbility1 Id: " + this.ability1ID
		+ "\nAbility2 Id: " + this.ability2ID
		+ "\nHP: " + this.numHP
		+ "\nAP: " + this.numAP
		+ "\nType: " + this.card_type;
	}

	public void setDisplay(){
		Image display = GameObject.Find ("Image").GetComponent<Image> ();
		display.sprite = img;
	}

	public void displayInfo(){
		GameObject.Find ("CardInfo").GetComponent<DisplayScript> ().AssignValues (this);
	}

	public void addtoDeck(){
		this.inDeck = true;
	}

	public void isSelected(){
		GameObject selected = null;
		if (inDeck) {
			selected = this.gameObject;
		}
		GameObject.Find ("DeckGrid").GetComponent<GridScript> ().setSelected (selected);
	}
}
