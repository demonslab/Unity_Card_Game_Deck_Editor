using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DisplayScript : MonoBehaviour {

	public GameObject card;
	public GameObject displayName;
	public GameObject cardType;
	public GameObject ability;
	public GameObject abilityDesc;
	public GameObject HP;
	public GameObject AP;
	public GameObject nextAbButton;

	private bool ab2;
	private int currentAb;

	void Awake(){
		card = null;
		setButton (false);
	}

	public void ResetDisplay(){
		if (card != null) {
			if (card.GetComponent<Card> ().inDeck == false)
				return;
		}
		card = null;
		GameObject.Find ("Image").GetComponent<Image> ().sprite = null;
		displayName.GetComponent<Text> ().text = "Select a card to display its information";
		cardType.GetComponent<Text> ().text = "Card Type";
		ability.GetComponent<Text> ().text = "Ability: ";
		abilityDesc.GetComponent<Text> ().text = "";
		HP.GetComponent<Text> ().text = "HP: ";
		AP.GetComponent<Text> ().text = "AP: ";
		setButton (false);
	}

	public void AssignValues(Card cardScript){
		this.card = cardScript.instance;

		displayName.GetComponent<Text> ().text = cardScript.nname;

		cardType.GetComponent<Text>().text =
			getTypeID(false, cardScript.card_type);	//Card Type

		if (cardScript.ability2ID != 0)
			setButton (true);
		else setButton (false);
		swapAbility (currentAb);

			
		HP.GetComponent<Text>().text = "HP: " + cardScript.numHP;
		AP.GetComponent<Text>().text = "AP: " + cardScript.numAP;
	}

	private string getTypeID(bool ability, int typeID){
		TestDB dbScript = GameObject.Find ("InitOb").GetComponent<TestDB>();
		//Debug.Log ("Getting info, ability is " + ability);
		return dbScript.getCardType (ability, typeID);
	}

	private string getAbility(int abID){
		TestDB dbScript = GameObject.Find ("InitOb").GetComponent<TestDB>();
		return dbScript.getAbility (abID);
	}

	public void swap(){
		if (ab2){
			currentAb = currentAb==1?2:1;
			swapAbility(currentAb);
		}
	}

	private void setButton(bool avail){
		currentAb = 1;
		ab2 = avail;
		nextAbButton.GetComponent<Button> ().interactable = ab2;	
	}

	private void swapAbility(int id){
		if (card==null) return;
		if (id == 1) {
			Card cardScript = card.GetComponent<Card> ();
			ability.GetComponent<Text> ().text = "Ability: " +
			getTypeID (true, cardScript.ability1ID);	//Ability Type

			//Ability Description
			abilityDesc.GetComponent<Text> ().text = 
			getAbility (cardScript.ability1ID);
		}
		else if (id == 2) {
			Card cardScript = card.GetComponent<Card> ();
			ability.GetComponent<Text> ().text = "Ability: " +
				getTypeID (true, cardScript.ability2ID);	//Ability Type

			//Ability Description
			abilityDesc.GetComponent<Text> ().text = 
				getAbility (cardScript.ability2ID);
		}
	}
}
