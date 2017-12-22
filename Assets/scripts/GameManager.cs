using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text;
using System.Globalization;
using System.Linq;
using UnityEngine.SceneManagement;

using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	//Force the generation of a new dictionary
	private bool forceGeneration = false;

	//Scripts & Files

	private ButtonControl buttonControl;
	private string externalDictionaryFileName = "Assets/datas/liste_francais.txt";

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
	private int[] currentButtonsIndexes = new int[]{9, 9, 9, 9, 9, 9};
	private List<string> dictionary = new List<string>();
	private List<string> combinationDictionary = new List<string>();

	private string internalDictionaryPath = "Assets/datas/dictionary.txt";
	private string internalDictionaryMetaPath = "Assets/datas/dictionary.txt.meta";

	public List<string> alpha = new List<string>();

	public bool dev = true;

	// Use this for initialization
	void Start () {
		
		for(char c = 'A'; c <= 'Z'; c++){
			alpha.Add("" + c);
		}
		
		//Init interface elements
		buttons = new Button[]{b1, b2, b3, b4, b5, b6};
		word.text = "";

		//Generate dictionnary file
		if (forceGeneration || !File.Exists(externalDictionaryFileName)) {
			generateDictionary ();
		}

		//Get dictionnary from file
		setDictionary ();

		setCombinations ();


		//Fil buttons with letters
		fillLetters ();

		/*SceneManager.LoadScene ();
			DontDestroyOnLoad (); // pour un element
		SceneManager.LoadScene ("eorgj",LoadSceneMode.Additive);//Additive pour ne pas supprimer la scene deja chargée
		*/

	}

	// Update is called once per frame
	void Update () {
		
	}

	public List<string> Shuffle(List<string> lArray, int[] keepPositions = null)  
	{
		List<string> lettersGenerated = new List<string> ();
		List<string> lettersToShuffle = new List<string> ();


		if (keepPositions != null) {
			//Select the new letters

			List<int> positionToKeep = new List<int> { 0, 1, 2, 3, 4, 5 };
			//faire un nouveau tableau T (1 2 3 4 5 6)
			//virer les valeurs présentes dans keepositions

			for (int i = 0; i < keepPositions.Length - 1; i++) {
				if (keepPositions [i] != 9) {
					positionToKeep.Remove (keepPositions [i]);
				}
			}

			for (int i = 0; i < keepPositions.Length; i++) {
				//If letter has to be kept
				if (positionToKeep.IndexOf (i) >= 0) {
					lettersGenerated.Add (lArray [i]);
				} else {
					lettersGenerated.Add ("");
					lettersToShuffle.Add (lArray [i]);
				}
			}
		} else {
			lettersToShuffle = lArray;
		}

		for(int i = 0; i < lettersToShuffle.Count - 1; i++) { 
			int k = Random.Range (0, lettersToShuffle.Count - 1);  
			string value = lettersToShuffle[k];
			lettersToShuffle[k] = lettersToShuffle[i];
			lettersToShuffle[i] = value;
		}

		//Faire une fonction pour remplir les cases vides d'un tableau par un autre tableau fillEmpty(tableau avec vide, tableau pour copmleter)
		int lettersGeneratedCount = lettersGenerated.Count;
		for (int i = 0; i < lettersToShuffle.Count; i++) {
			if (lettersGeneratedCount != 0) {
				int j = 0;
				while (lettersGenerated [j] != "") {
					j++;
				}
				lettersGenerated [j] = lettersToShuffle [i];
			} else {
				lettersGenerated.Add (lettersToShuffle [i]);
			}
		}

		return lettersGenerated;
	}

	public void fillLetters(int[] keepPositions = null){
		List<string> newCombination;
		//Do a random comb while not valid
		do {
			newCombination = getLetters (keepPositions);
		} while(!checkCombination (newCombination));

		currentLetters = Shuffle (newCombination, keepPositions);

		for(int i = 0; i < buttons.Length; i++) { 
			buttons[i].GetComponent <ButtonControl> ().textContent.text = currentLetters[i];
		}

	}

	//Action on LetterButton - Add letter in currentWord, and disable button
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

	//Action on DeleteLetter
	public void deleteLastLetter(){
		if (currentWord.Length > 0) {
			currentWord = currentWord.Substring (0, currentWord.Length - 1);
			buttons [currentButtonsIndexes [currentWord.Length]].interactable = true;

			word.text = currentWord;
		}
	}

	//Set the list of letters with rates, and call getRandom, return a 6 letters list
	public List<string> getLetters(int[] keepPositions = null){

		//ismultiple est vérifié quand keepPositions ? tester
		List<string> lettersGenerated = new List<string> ();
		List<string> vows = new List<string> (new string[] { "A", "E", "I", "O", "U", "Y" });
		int vowsToKeep = 0, consToKeep = 0;

		if (keepPositions != null) {
			lettersGenerated = currentLetters.Select(item => (string)item.Clone()).ToList();

			//Remove the used letters
			for (int i = 0; i < keepPositions.Length;i++){
				if (keepPositions[i] != 9) {
					lettersGenerated [keepPositions [i]] = "";
				}
			}

			//Check amount of vows and cons
			for (int i = 0; i < keepPositions.Length;i++){
				if(lettersGenerated[i] != ""){
					consToKeep++;
					if (vows.IndexOf (lettersGenerated [i].ToString()) >= 0) {
						vowsToKeep++;
					}
				}
			}
			for (int i = 0; i < lettersGenerated.Count; i++) {
				Debug.Log (lettersGenerated [i]);
			}
			consToKeep = consToKeep - vowsToKeep;
		}
		
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
		int minVows = 1;
		if (vowsToKeep > 0) {
			minVows = 0;
		}
		int randomVowsAndCons = Random.Range (minVows, 4 - vowsToKeep);

		List<string> vowelSelected = getRandom (vowelRated, randomVowsAndCons, vowelMultiple);
		List<string> consonantSelected = getRandom (consonantRated, 6 - randomVowsAndCons - consToKeep -vowsToKeep, consonantMultiple);

		if (true) {
			foreach (var ccc in lettersGenerated) {
				Debug.Log (ccc);
			}
		}
		//Fill empty values with vows
		int lettersGeneratedCount = lettersGenerated.Count;
		for (int i = 0; i < vowelSelected.Count; i++) {
			if (lettersGeneratedCount != 0) {
				int j = 0;
				while (lettersGenerated [j] != "") {
					j++;
				}
				lettersGenerated [j] = vowelSelected [i];
			} else {
				lettersGenerated.Add (vowelSelected [i]);
			}
		}
		//Fill empty values with cons
		for (int i = 0; i < consonantSelected.Count; i++) {
			if (lettersGeneratedCount != 0) {
				int j = 0;
				while (lettersGenerated [j] != "") {
					j++;
				}
				lettersGenerated [j] = consonantSelected [i];
			} else {
				lettersGenerated.Add (consonantSelected [i]);
			}
		}
		//vowelSelected.Sort();

		if (false) {
			foreach (var ccc in lettersGenerated) {
				Debug.Log (ccc);
			}
		}
		return lettersGenerated;
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
			line = line.Replace ("Œ", "OE"); // marche pas ?
			line = line.Replace (" ", "");


			if (line.Length >= 3 && line.Length <= 6 && !line.Contains("!") && !line.Contains(")")) {

				line = RemoveDiacritics(line.ToUpper ());

				tempDictionary.Add (line);

			}
		}

		Debug.Log (tempDictionary.Count);
		reader.Close();

		File.Delete(internalDictionaryPath);
		File.Delete(internalDictionaryMetaPath);

		TextWriter writer;
		string newFileName = internalDictionaryPath;
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

	//Convert internal generated file in list
	public void setDictionary(){
		TextReader reader;
		if (File.Exists (internalDictionaryPath)) {
			reader = new  StreamReader (internalDictionaryPath);
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

	public void setCombinations(){
		foreach (var word in dictionary) {
			var temp = word.ToList();
			temp.Sort();
			string tempstring = "";
			foreach (var letter in temp) {
				tempstring = tempstring + letter;
			}
			combinationDictionary.Add (tempstring);
		}
		combinationDictionary = combinationDictionary.Distinct().ToList();
	}

	//Check if word exist in dictionnary
	public void checkWord(){
		if (currentWord.Length >= 3) {
			bool wordFound = false;
			int i = 0;

			// OPTIMISABLE - en passant par un  predicat (faire une class, chaque mot devient un objet de cette classe, et on peut faire une recherche dans le tableau
			while (i < dictionary.Count && wordFound == false) {
				if (currentWord == dictionary [i]) {
					if (dev) {
						Debug.Log ("GG !");
					}
					wordFound = true;

					fillLetters (currentButtonsIndexes);
					while(currentWord.Length > 0){
						currentWord = currentWord.Substring (0, currentWord.Length - 1);
						buttons [currentButtonsIndexes [currentWord.Length]].interactable = true;

						word.text = currentWord;
					}
					currentButtonsIndexes = new int[]{9, 9, 9, 9, 9, 9};

					break;
				}
				i++;
			}
			if (wordFound == false && dev) {
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

	//Check if unless one word (one combination) can be done
	public bool checkCombination(List<string> combinationLetters){
		int combinationDictionaryLength = combinationDictionary.Count;
		List<string>  tempCombinationLetters = new List<string> ();
		//For all words
		for(int i = 0; i < combinationDictionaryLength; i++){
			int wordLength = combinationDictionary[i].Length;
			//Set a tempCombination to remove letters and handdle multiple letters
			tempCombinationLetters = combinationLetters.Select(item => (string)item.Clone()).ToList();

			//For all letters in the word
			for (int j = 0; j < wordLength; j++) {
				//Get position of letter
				int posInWord = tempCombinationLetters.IndexOf(combinationDictionary[i][j].ToString());
				//If not in, next word
				if(posInWord < 0){
					break;
				}else{
					tempCombinationLetters.RemoveAt (posInWord);
					//If last letter is ok, this word can be done
					if(j == wordLength - 1){
						if (dev) {
							Debug.Log(tempCombinationLetters.Count);
							for (int k = 0; k < wordLength; k++) {
								Debug.Log (combinationDictionary[i][k]);
							}
						}
						return true;
					}
				}
			}
		}

		return false;
	}
}

/*
 	faire une fonction pour completer les valeurs vide dans un tableau avec les valeurs d'un autre

	is multiple testé quand keepPosition != null ?

	fix OE in dictionnary generated, tous les mots à accents vires ?

	bouton refresh : avoir au moins 3 lettres différentes en sortie (donc stocker ancienne)

	supprimer mot du dictionnaire (pas du fichier, du dico généré) pour pas pouvoir le refaire et régénerer combinationDictionnary (lourd, optimisale ?)
	
	ajouter un caractère espace entre les lettres (et supprimer avec la lettre sur bouton effacer)
	utiliser fonction sinus et cosinus pour position de l'image vague, pour faire des variations
	loadscene additive pour le jeu, et décharger accueil et rejouer en additive (pour pas reprendre du temps pour le screen)
	prendre composant pour afficher score/font/bouton rejouer plutot que scene ?
	refaire images pour qu'elles matchent mieux avec les bords (+ cercle rose du compteur + vagues + fond blanc compteur + fond blanc chrono/refresh

	ajouter points sur les lettres, compter les points, faire les timers, faire les combos

	bonus:animation de meileur score, faire bouger les éléments indépendamment
	trouver dictionnaire plus complet (pluriels, accords... et le mot flute)
	en js faire un compteur de mots, quels caractères utilisés (spéciaux surtout), combien de caractère max d'une lettre, combien de fois ce nb de caractère max
	faire rercherche de combinaison possible via Linq (Link.where(_ => _.truc = valeur))

*/
