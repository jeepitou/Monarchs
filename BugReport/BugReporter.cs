using System.Collections;
using System.IO;
using Monarchs.Client;
using TMPro;
using UnityEngine;

namespace Monarchs.BugReport
{
    public class BugReporter : MonoBehaviour
    {
        public TMP_Text title;
        public TMP_Text description;
        public TMP_Text invalidFormText;
        private string _path = "Screenshots";
        private string _filename = "bug.png";
        private string _fullPath;
        private CanvasGroup _canvasGroup;
        
        public void Start()
        {
            _fullPath = Path.Combine(Application.persistentDataPath, _path, _filename);
            _path = Path.Combine(Application.persistentDataPath, _path);
            _canvasGroup = GetComponent<CanvasGroup>();
            
            Hide();
        }
        
        public void SubmitBug()
        {
             bool invalidTitle = string.IsNullOrWhiteSpace(title.text) || title.text.Length == 1;
             bool invalidDescription = string.IsNullOrWhiteSpace(description.text) || description.text.Length == 1;
             
            if (invalidTitle || invalidDescription)
            {
                Debug.Log("Didn't submit bug report. Invalid form data.", this);
                invalidFormText.gameObject.SetActive(true);
                SetInvalidFormText(invalidTitle, invalidDescription);
                return;
            }
            
            if (!System.IO.Directory.Exists(_path))
            {
                System.IO.Directory.CreateDirectory(_path);
            }
            
            if (System.IO.File.Exists(_fullPath))
            {
                System.IO.File.Delete(_fullPath);
            }
            Hide();
            StartCoroutine(TakeScreenshotWithDelay());
        }
        
        IEnumerator TakeScreenshotWithDelay()
        {
            Debug.Log($"Saving screenshot to: {_fullPath}", this);
            ScreenCapture.CaptureScreenshot(_fullPath);

            // Wait until the screenshot file is created
            while (!System.IO.File.Exists(_fullPath))
            {
                yield return null;
            }

            Debug.Log($"Screenshot saved to: {_fullPath}", this);

            GameClient.Get().SendBugReport(title.text, description.text, _fullPath);
        }

        private void SetInvalidFormText(bool invalidTitle, bool invalidDescription)
        {
            if (invalidTitle)
            {
                invalidFormText.text = invalidDescription ? "Please give a title and a description to the bug" : "Please give a title to the bug";
            }
            else 
            {
                invalidFormText.text = "Please give a description to the bug";
            }
        }
        
        public void Show()
        {
            _canvasGroup.alpha = 1;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
        }
        
        public void Hide()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            invalidFormText.gameObject.SetActive(false);
        }
    }
}