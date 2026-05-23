using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabDataExporter : MonoBehaviour
{
    public int startCount;
    // Call this method when you want to export data to Google Sheets
    public void ExportDataToGoogleSheet(int dataCount)
    {
        // Create the request to execute the cloud script
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "exportDataToGoogleSheet", // Cloud Script method name
            FunctionParameter = new { startPosition = dataCount }, // Pass your int parameter
            GeneratePlayStreamEvent = true // Optional: if you want to generate events for PlayStream
        };

        PlayFabClientAPI.ExecuteCloudScript(request, OnCloudScriptSuccess, OnCloudScriptError);
    }
    public void ExportDataToGoogleSheet()
    {
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "exportDataToGoogleSheet" // The name of the Cloud Script function
        };

        PlayFabClientAPI.ExecuteCloudScript(request, OnCloudScriptSuccess, OnCloudScriptError);
    }
    private void OnCloudScriptSuccess(ExecuteCloudScriptResult result)
    {
        Debug.Log("Data export successful: " + result.FunctionResult);
    }

    private void OnCloudScriptError(PlayFabError error)
    {
        Debug.LogError("Error exporting data: " + error.GenerateErrorReport());
    }
}