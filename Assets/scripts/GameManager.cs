using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Globalization;

using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	//Force the generation of a new dictionary
	private bool forceGeneration = false;

	//Scripts & Files

	private ButtonControl buttonControl;
	private string externalDictionaryFileName = "Assets\\datas\\liste_francais.txt";

	//Text Elements

	public Text score;
	public Text bestScore;
	public Text word;

	//Control Elements
	public GameObject reroll;
	public GameObject delete;

	public Button b1;
	public Button b2;
	public Button b3;
	public Button b4;
	public Button b5;
	public Button b6;

	public GameObject submit;


	//Letters <letter> <chance to be selected>
	private string[ , ] vowelRate = new string[,]{
		{"A", "5"},
		{"E", "5"},
		{"I", "5"},
		{"O", "5"},
		{"U", "5"},
		{"Y", "1"}
	};
	private string[ , ] consonantRate = new string[,]{
		{"B", "4"},
		{"C", "4"},
		{"D", "4"},
		{"F", "4"},
		{"G", "4"},
		{"H", "2"},
		{"J", "4"},
		{"K", "1"},
		{"L", "5"},
		{"M", "4"},
		{"N", "5"},
		{"P", "4"},
		{"Q", "2"},
		{"R", "5"},
		{"S", "5"},
		{"T", "5"},
		{"V", "4"},
		{"W", "1"},
		{"X", "1"},
		{"Z", "1"}
	};

	//Letters <letter> <if it can be select 2 times>
	private string[ , ] vowelMultiple = new string[,]{
		{"A", "1"},
		{"E", "1"},
		{"I", "0"},
		{"O", "0"},
		{"U", "0"},
		{"Y", "0"}
	};
	private string[ , ] consonantMultiple = new string[,]{
		{"B", "1"},
		{"C", "1"},
		{"D", "0"},
		{"F", "0"},
		{"G", "0"},
		{"H", "0"},
		{"J", "0"},
		{"K", "0"},
		{"L", "1"},
		{"M", "0"},
		{"N", "1"},
		{"P", "0"},
		{"Q", "0"},
		{"R", "1"},
		{"S", "1"},
		{"T", "1"},
		{"V", "0"},
		{"W", "0"},
		{"X", "0"},
		{"Z", "0"}
	};

	private Button[] buttons;
	private string currentWord = "";
	private List<string> currentLetters;
	private int[] currentButtonsIndexes = new int[]{0, 0, 0, 0, 0, 0};
	private List<string> dictionary = new List<string>();
	private List<string> combinationDictionary = new List<string>();

	public List<string> alpha = new List<string>(new string[] {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"});

	// Use this for initialization
	void Start () {
		
		buttons = new Button[]{b1, b2, b3, b4, b5, b6};

		fillLetters ();

		if (forceGeneration) {
			generateDictionary ();
		}

		setDictionary ();
		word.text = "";

		getPossibleCombination ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public List<string> Shuffle(List<string> lArray)  
	{ 
		for(int i = 0; i < lArray.Count - 1; i++) { 
			int k = Random.Range (0, lArray.Count - 1);  
			string value = lArray[k];  
			lArray[k] = lArray[i];  
			lArray[i] = value;
		}
		return lArray;
	}

	public void fillLetters(){

		currentLetters = Shuffle (getLetters ());

		for(int i = 0; i < buttons.Length; i++) { 
			buttons[i].GetComponent <ButtonControl> ().textContent.text = currentLetters[i];
		}
	}

	public void useLetter(Button action){
		int i = 0;
		int index = - 1;
		while(i < buttons.Length &&  index < 0 ){
			if (buttons [i].GetInstanceID () == action.GetInstanceID ()) {
				index = i;
			}
			i++; 
		}

		currentWord = currentWord + currentLetters [index];
		currentButtonsIndexes [currentWord.Length - 1] = index;

		action.interactable = false;
		word.text = currentWord;

	}

	public void deleteLastLetter(){
		if (currentWord.Length > 0) {
			currentWord = currentWord.Substring (0, currentWord.Length - 1);
			buttons [currentButtonsIndexes [currentWord.Length]].interactable = true;

			word.text = currentWord;
		}
	}

	//Set the list of letters with rates, and call getRandom, return a 6 letters list
	public List<string> getLetters(){
		
		List<string> vowelRated = new List<string>();
		for (int i = 0; i < vowelRate.Length/2; i++) {
			int rate = int.Parse(vowelRate[i, 1]);
			for(int j = 0; j < rate; j++){
				vowelRated.Add (vowelRate [i, 0]);
			}
		}

		List<string> consonantRated = new List<string>();
		for (int i = 0; i < consonantRate.Length/2; i++) {
			int rate = int.Parse(consonantRate[i, 1]);
			for(int j = 0; j < rate; j++){
				consonantRated.Add (consonantRate [i, 0]);
			}
		}

		List<string> vowelSelected = getRandom (vowelRated, 2, vowelMultiple);
		List<string> consonantSelected = getRandom (consonantRated, 4, consonantMultiple);

		// BUG
		//return vowelSelected.AddRange (consonantSelected);

		for (int i = 0; i < consonantSelected.Count; i++) {
			vowelSelected.Add(consonantSelected[i]);
		}

		return vowelSelected;
	}

	// For a list of letter, return <number> of letters
	public List<string> getRandom(List<string> lettersList, int number, string[ , ] letterMultiple){

		List<string> lettersSelection = new List<string>();

		for (int i = 0; i < number; i++) {
			int rand = Random.Range (0, lettersList.Count);
			string currentLetter = lettersList [rand];

			//Select a letter, if it is yet in the list, call the function isMultiple
			if (lettersSelection.Contains (lettersList [rand])) {


				if (letterIsMultiple (lettersList [rand], letterMultiple)) {
					lettersSelection.Add (currentLetter);
					//Debug.Log (currentLetter);
				} else {
					i--;
				}

				//BUG
				//lettersList.Remove(lettersList[rand]); la fonction ne sert pas à filtrer ?

				//Remove all references of the currentLetter
				for (int j = 0; j < lettersList.Count; j++) {
					if (lettersList [j] == currentLetter) {
						lettersList.RemoveAt (j);
						j--;
					}
				}
			} else {
				//If called a first time
				lettersSelection.Add (currentLetter);
				lettersList.RemoveAt (rand);
				//Debug.Log (currentLetter);
			}
		}

		return lettersSelection;
	}

	public bool letterIsMultiple(string letter, string[ , ] letterMultiple){
		int i = 0;
		while(i < letterMultiple.Length/2){
			if (letterMultiple [i, 0] == letter) {
				if (int.Parse (letterMultiple [i, 1]) == 1) {
					return true;
				}else{
					return false;
				}
			}
			i++;
		}
		Debug.Log("letterIsMultiple has not find the letter in the letterMultiple array");
		return false;
	}

	//Read external file, parse and filter words (special chars, spaces and word between 3 and 6 letters), and create a new file
	public void generateDictionary () {

		List<string> tempDictionary = new List<string>();

		TextReader reader;
		reader = new  StreamReader(externalDictionaryFileName);
		string line;
		while (true) {
			
			line = reader.ReadLine ();

			if (line==null) break;

			line = line.Replace ("-", "");
			line = line.Replace (".", "");
			line = line.Replace ("Œ", "OE");
			line = line.Replace (" ", "");


			if (line.Length >= 3 && line.Length <= 6 && !line.Contains("!") && !line.Contains(")")) {

				line = RemoveDiacritics(line.ToUpper ());

				tempDictionary.Add (line);

			}
		}

		Debug.Log (tempDictionary.Count);
		reader.Close();

		File.Delete("Assets\\datas\\dictionary.txt");
		File.Delete("Assets\\datas\\dictionary.txt.meta");

		TextWriter writer;
		string newFileName = "Assets\\datas\\dictionary.txt";
		writer = new StreamWriter(newFileName);
		for (int i = 0; i < tempDictionary.Count; i++) {
			if (i > 0) {
				writer.Write ("\r\n" + tempDictionary [i]);
			} else {
				writer.Write (tempDictionary [i]);
			}
		}
		writer.Close();
	}

	public void setDictionary(){
		TextReader reader;
		if (File.Exists ("Assets\\datas\\dictionary.txt")) {
			reader = new  StreamReader ("Assets\\datas\\dictionary.txt");
			string line;
			while (true) {

				line = reader.ReadLine ();

				if (line == null)
					break;

				dictionary.Add (line);
			}
		} else {
			Debug.Log ("Aucun dictionnaire n'a été trouvé. Entrer un externalDictionaryFileName contenant le fichier externe, et affecter forceGeneration à True pour générer un nouveau fichier");
		}
	}

	public void checkWord(){
		if (currentWord.Length >= 3) {
			bool wordFound = false;
			int i = 0;

			// OPTIMISABLE - en passant par un  predicat (faire une class, chaque mot devient un objet de cette classe, et on peut faire une recherche dans le tableau
			while (i < dictionary.Count && wordFound == false) {
				if (currentWord == dictionary [i]) {
					Debug.Log ("GG !");
					wordFound = true;

					while(currentWord.Length > 0){
						currentWord = currentWord.Substring (0, currentWord.Length - 1);
						buttons [currentButtonsIndexes [currentWord.Length]].interactable = true;

						word.text = currentWord;
					}

					break;
				}
				i++;
			}
			if (wordFound == false) {
				Debug.Log ("Nope !");
			}
		}
	}

	//Found on the web, convert accented character into normalized ones
	static string RemoveDiacritics(string text) 
	{
		var normalizedString = text.Normalize(NormalizationForm.FormD);
		var stringBuilder = new StringBuilder();

		foreach (var c in normalizedString)
		{
			var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
			if (unicodeCategory != UnicodeCategory.NonSpacingMark)
			{
				stringBuilder.Append(c);
			}
		}

		return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
	}

	public void getPossibleCombination(){
		foreach (var word in dictionary){
			
			Debug.Log (word);
			var newCombination = "";
			var wordCombination = word;

			foreach (var lettre in alpha){
				
				var wordLength = wordCombination.Length;

				if (wordLength > 0) {
					for (var i = 0; i < wordLength; i++) {
						if (i == wordCombination.Length) {
							break;
						}
						if (wordCombination [i].ToString () == lettre) {
							Debug.Log (wordCombination [i]);
							Debug.Log ("est dans le mot");
							newCombination = newCombination + wordCombination [i];
							if (i == 0) {
								wordCombination = wordCombination.Substring (1, wordCombination.Length - 1);
							} else if (i == wordCombination.Length - 1) {
								wordCombination = wordCombination.Substring (0, wordCombination.Length - 1);
							} else {
								wordCombination = wordCombination.Substring (0, i) + wordCombination.Substring (i + 1, wordCombination.Length - (i + 1));
							}
							Debug.Log (wordCombination);
							i--;
						}
					}
				}

			}
			combinationDictionary.Add (newCombination);
		}
	}
}
