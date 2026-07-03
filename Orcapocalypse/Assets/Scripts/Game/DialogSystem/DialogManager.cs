using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class DialogManager : MonoBehaviour
{
    // Make this a Singleton so we can easily call it from anywhere (like tutorial triggers)
    public static DialogManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject dialogPanel; // The main dark gray box
    public TextMeshProUGUI dialogText;
    public TextMeshProUGUI nameText;
    public Image portraitLeft;
    public Image portraitRight;

    [Header("Typewriter Settings")]
    public float typingSpeed = 0.05f;

    [Header("Prompt UI & Icons")]
    public Image continuePromptImage;
    public Sprite xboxIcon;       // A Button
    public Sprite playstationIcon; // Cross Button

    [Header("Audio")]
    public AudioSource audioSource;
    [Range(0.8f, 1.2f)] public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.1f;
    public int charsPerBlip = 2; // Plays a sound every 2 characters so it isn't deafening

    private Queue<DialogLine> linesQueue;
    private bool isTyping = false;
    private bool isDialogOpen = false;

    private void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        linesQueue = new Queue<DialogLine>();
        dialogPanel.SetActive(false); // Hide the UI when the game starts
    }

    private void Update()
    {
        // If the dialog is not open, do nothing
        if (!isDialogOpen) return;

        // Check for the "South" Button press. 
        // "Submit" is mapped to the South Button (A on Xbox, Cross on PS) in Unity's old Input Manager by default.
        // If you are using the New Input System, you will replace this line with your specific input call.
        if (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
        {
            if (isTyping)
            {
                // Optional: We can ignore input while typing so they can't skip, 
                // OR we could force the text to finish instantly here.
                // For now, as requested, we do nothing while typing so it forces them to read.
                return;
            }
            else
            {
                // The line is done typing, load the next one
                DisplayNextLine();
            }
        }
    }

    public void StartDialog(DialogSequence sequence)
    {
        isDialogOpen = true;
        dialogPanel.SetActive(true);
        linesQueue.Clear();

        // Load all lines from the Scriptable Object into our queue
        foreach (DialogLine line in sequence.lines)
        {
            linesQueue.Enqueue(line);
        }

        DisplayNextLine();
    }

    private void UpdatePromptIcon()
    {
        if (continuePromptImage == null) return;

        // 1. Check if a Gamepad is connected and active
        if (Gamepad.current != null)
        {
            // Unity's New Input System identifies PlayStation controllers (DualShock 4 / DualSense)
            // either by their specific type or by name
            if (Gamepad.current is UnityEngine.InputSystem.DualShock.DualShockGamepad ||
                Gamepad.current.displayName.Contains("PlayStation") ||
                Gamepad.current.displayName.Contains("DualSense") ||
                Gamepad.current.displayName.Contains("DualShock"))
            {
                continuePromptImage.sprite = playstationIcon;
            }
            else
            {
                // Default all other controllers (Xbox, generic PC gamepads) to the Xbox icon
                continuePromptImage.sprite = xboxIcon;
            }
        }
        else
        {
            // No controller detected, default to Xbox
            continuePromptImage.sprite = xboxIcon;
        }
    }

    public void DisplayNextLine()
    {
        if (continuePromptImage != null) continuePromptImage.gameObject.SetActive(false); // Hide immediately on press

        // If there are no lines left, end the conversation
        if (linesQueue.Count == 0)
        {
            EndDialog();
            return;
        }

        DialogLine nextLine = linesQueue.Dequeue();

        // 1. Setup the Nameplate
        if (string.IsNullOrEmpty(nextLine.speakerName))
        {
            nameText.gameObject.SetActive(false);
        }
        else
        {
            nameText.gameObject.SetActive(true);
            nameText.text = nextLine.speakerName;
        }

        // 2. Setup the Portraits
        portraitLeft.gameObject.SetActive(false);
        portraitRight.gameObject.SetActive(false);

        if (nextLine.portraitPlacement == DialogLine.PortraitPosition.Left)
        {
            portraitLeft.gameObject.SetActive(true);
            portraitLeft.sprite = nextLine.speakerPortrait;
        }
        else if (nextLine.portraitPlacement == DialogLine.PortraitPosition.Right)
        {
            portraitRight.gameObject.SetActive(true);
            portraitRight.sprite = nextLine.speakerPortrait;
        }

        // 3. Start the Typewriter
        StopAllCoroutines(); // Stop any current typing before starting a new one
        StartCoroutine(TypeSentence(nextLine));
    }

    private IEnumerator TypeSentence(DialogLine line)
    {
        isTyping = true;
        if (continuePromptImage != null) continuePromptImage.gameObject.SetActive(false); // Hide prompt while typing

        dialogText.text = "";
        int charCount = 0;

        foreach (char letter in line.text.ToCharArray())
        {
            dialogText.text += letter;

            if (line.voiceBlip != null && charCount % charsPerBlip == 0 && letter != ' ')
            {
                audioSource.pitch = Random.Range(minPitch, maxPitch);
                audioSource.PlayOneShot(line.voiceBlip);
            }

            charCount++;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;

        // Sentence is done! Detect the controller and show the prompt
        UpdatePromptIcon();
        if (continuePromptImage != null) continuePromptImage.gameObject.SetActive(true);
    }

    private void EndDialog()
    {
        isDialogOpen = false;
        dialogPanel.SetActive(false);
    }
}