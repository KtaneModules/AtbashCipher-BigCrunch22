using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class AtbashCipherScript : MonoBehaviour {

    public KMAudio Audio;
    public KMBombInfo Bomb;
    public KMBombModule Module;
    
    public KMSelectable[] MainButtons;
    public KMSelectable ResetButton;
    public TextMesh[] ScrambledAlphabet;
    public TextMesh Display;
    
    string TheInput, TheOutput = "";
    bool Loading = false;
    string[] RegularAlphabet = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
    string[] ShuffleThings = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z"};
    
    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool ModuleSolved;
    
    void Awake()
    {
        moduleId = moduleIdCounter++;
        for (int a = 0; a < MainButtons.Count(); a++)
        {
            int Placement = a;
            MainButtons[Placement].OnInteract += delegate
            {
                Press(Placement);
                return false;
            };
        }
        ResetButton.OnInteract += delegate () { ResetInputs(); return false; };
    }
    
    void Start()
    {
        MakeASequence();
        ShuffleBoard();
    }
    
    void Press(int Placement)
    {
        MainButtons[Placement].AddInteractionPunch(.2f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, MainButtons[Placement].transform);
        if (!ModuleSolved && !Loading)
        {
            TheOutput = TheOutput + ShuffleThings[Placement];
            if (TheOutput.Length == 15)    {
            CheckForInconsistencies();
            }    else    {
            ShuffleBoard();    
            }
        }
    }
    
    void CheckForInconsistencies()
    {
        Debug.LogFormat("[Atbash Cipher #{0}] You submitted: {1}", moduleId, TheOutput);
        for (int x = 0; x < 15; x++)
        {
            if(RegularAlphabet[25 - Array.IndexOf(RegularAlphabet, TheOutput[x].ToString())] != TheInput[x].ToString())
            {
                StartCoroutine(IncorrectSequence());
                return;
            }
        }
        CorrectSequence();
    }
    
    void ResetInputs()
    {
        ResetButton.AddInteractionPunch(.2f);
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.BigButtonPress, ResetButton.transform);
        if (!ModuleSolved && !Loading)
        {
            TheOutput = "";
            ShuffleBoard();
        }
    }
    
    void ShuffleBoard()
    {
        ShuffleThings.Shuffle();
        for (int x = 0; x < ScrambledAlphabet.Length; x++)
        {
            ScrambledAlphabet[x].text = ShuffleThings[x];
        }
    }
    
    void MakeASequence()
    {
        string TheRightWay = "";
        for (int x = 0; x < 15; x++)
        {
            int Goldbloom = UnityEngine.Random.Range(0,26);
            Display.text += RegularAlphabet[Goldbloom];
            TheInput += RegularAlphabet[Goldbloom];
            TheRightWay += RegularAlphabet[25-Goldbloom];
            
            if (x == 7) {
                Display.text += "\n";
            }
        }
        Debug.LogFormat("[Atbash Cipher #{0}] The sequence generated: {1}", moduleId, TheInput);
        Debug.LogFormat("[Atbash Cipher #{0}] The correct answer: {1}", moduleId, TheRightWay);
    }
    
    void CorrectSequence()
    {
        Module.HandlePass();
        Debug.LogFormat("[Atbash Cipher #{0}] The sequence submitted was correct. Module solved.", moduleId);
        string Discord = " MODULE    IS    SOLVED   ";
        Display.text = "CORRECT";
        for (int x = 0; x < 26; x++)
        {
            ScrambledAlphabet[x].text = Discord[x].ToString();
        }
        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
        ModuleSolved = true;
    }
    
    IEnumerator IncorrectSequence()
    {
        Loading = true;
        Module.HandleStrike();
        
        Debug.LogFormat("[Atbash Cipher #{0}] The sequence submitted was incorrect. The module performed a reset.", moduleId);
        string Discord = "WHAT YOUTYPED ISNOT GOOD  ";
        Display.text = "INCORRECT";
        for (int x = 0; x < 26; x++)
        {
            ScrambledAlphabet[x].text = Discord[x].ToString();
        }
        yield return new WaitForSecondsRealtime(3f);
        Display.text = "";
        TheInput = "";
        TheOutput = "";
        MakeASequence();
        ShuffleBoard();
        Loading = false;
    }
    
    //twitch plays
    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"To submit a sequence of letters, use !{0} submit [15 letters] (Example: !{0} submit BWIIBXIZWRXLLK)";
    #pragma warning restore 414
    
    IEnumerator ProcessTwitchCommand(string command)
    {
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length != 2)
            {
                yield return "sendtochaterror Parameter length is invalid. Command ignored.";
                yield break;
            }
            
            if (!parameters[1].All(Char.IsLetter))
            {
                yield return "sendtochaterror There is a character submitted that is not a letter. Command ignored.";
                yield break;
            }
            
            if (parameters[1].Length != 15)
            {
                yield return "sendtochaterror Submitted alphabet sequence is not exactly 15 letters. Command ignored.";
                yield break;
            }
            
            if (Loading)
            {
                yield return "sendtochaterror The module can not be interacted currently. Command ignored.";
                yield break;
            }
            
            for (int x = 0; x < parameters[1].Length; x++)
            {
                MainButtons[Array.IndexOf(ShuffleThings, parameters[1][x].ToString().ToUpper())].OnInteract();
                yield return new WaitForSeconds(.1f);
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        while (Loading) yield return true;
        string TheRightWay = "";
        for (int x = 0; x < 15; x++)
            TheRightWay += RegularAlphabet[25 - Array.IndexOf(RegularAlphabet, TheInput[x].ToString())];
        for (int i = 0; i < TheOutput.Length; i++)
        {
            if (TheOutput[i] != TheRightWay[i])
            {
                ResetButton.OnInteract();
                yield return new WaitForSeconds(.1f);
            }
        }
        int start = TheOutput.Length;
        for (int i = start; i < TheRightWay.Length; i++)
        {
            MainButtons[Array.IndexOf(ShuffleThings, TheRightWay[i].ToString())].OnInteract();
            yield return new WaitForSeconds(.1f);
        }
    }
}
