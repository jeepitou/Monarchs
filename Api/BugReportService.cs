using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;

public class BugReportService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    
    public BugReportService(string apiUrl)
    {
        _httpClient = new HttpClient();
        _apiUrl = apiUrl;
    }

    public async Task SubmitBugAsync(string title, string description, string imagePath)
    {
        try
        {
            using (var formData = new MultipartFormDataContent())
            {
                // Add the title and description to the form data
                formData.Add(new StringContent(title), "title");
                formData.Add(new StringContent(description), "description");

                // Add the image file to the form data (if provided)
                if (!string.IsNullOrWhiteSpace(imagePath) && File.Exists(imagePath))
                {
                    var imageBytes = await File.ReadAllBytesAsync(imagePath);
                    var imageContent = new ByteArrayContent(imageBytes);

                    // Adding additional headers for the image
                    imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // Adjust for your file type
                    imageContent.Headers.Add("Content-Disposition", $"form-data; name=\"file\"; filename=\"{Path.GetFileName(imagePath)}\"");

                    formData.Add(imageContent, "file", Path.GetFileName(imagePath));
                }
                else
                {
                    Console.WriteLine("No valid image path provided or file not found.");
                }

                // Send the POST request to the API
                var response = await _httpClient.PostAsync(_apiUrl + "/bugreport/submit", formData);

                if (response.IsSuccessStatusCode)
                {
                    Debug.Log("Bug report submitted successfully!");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    Debug.Log($"Server Response: {responseContent}");
                }
                else
                {
                    Debug.Log($"Failed to submit bug report. Status Code: {response.StatusCode}");
                    var errorContent = await response.Content.ReadAsStringAsync();
                    Debug.Log($"Error Details: {errorContent}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while submitting the bug report: {ex.Message}");
        }
    }
}
