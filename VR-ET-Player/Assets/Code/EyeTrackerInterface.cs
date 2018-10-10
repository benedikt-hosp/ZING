using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using SimpleFileBrowser;
using UnityEngine.UI;


public class EyeTracker
{
    SMI.SMIEyeTrackingUnity smiInstance = null;


    StreamWriter gazeWriter;
    StreamWriter logWriter;
    string logMessage = "";
    bool calibrationDone = false;


    // Use this for initialization
    public EyeTracker(StreamWriter logWriter)
    {

        this.logWriter = logWriter;
        smiInstance = SMI.SMIEyeTrackingUnity.Instance;

        // set callback to receive all gaze samples and save them in file
        smiInstance.smi_SetAsyncGazeSampleCallback(OnHighFrequencyGazeSample);
        this.logWriter.WriteLine(smiInstance.smi_GetTimeStamp() + "\t" + "SMI Eyetracker Started");

    }


    public ulong getSMITimeStamp()
    {
        return smiInstance.smi_GetTimeStamp();

    }

    public void startCalibration()
    {
        smiInstance.smi_StartFivePointCalibration();
        this.logWriter.WriteLine(getSMITimeStamp() + "\t Calibration started\t");
    }

    // Callback function for getting a calibration result
    public void OnCalibrationResult(int returnCode)
    {
        Debug.Log("SMI HMD calibration result: " + SMI.SMIEyeTrackingUnity.ErrorIDContainer.getErrorMessage(returnCode));

        // Return code message:
        //     SMI_SUCCESS: Calibration is successfully done.
        //     SMI_OPERATION_FAILED: Calibration process is aborted.
        logMessage = "Calibration result:\n" + SMI.SMIEyeTrackingUnity.ErrorIDContainer.getErrorMessage(returnCode);
        Debug.LogWarning(logMessage);

        if (returnCode.Equals(0))
        {
            this.logWriter.WriteLine(getSMITimeStamp() + "\t Calibration successfull\t");

            calibrationDone = true;
        }
        else
        {
            this.logWriter.WriteLine(getSMITimeStamp() + "\t Calibration failed\t");
            calibrationDone = false;
        }
       
    }


    public bool isCalibrationDone()
    {
        if(calibrationDone)
        {
            return true;

        }
        else
        {
            return false;
        }
    }



    // Callback function for getting high frequency gaze sample. 
    // It is assured that all values within this callback are computed from the same timestamp images.
    void OnHighFrequencyGazeSample()
    {
        //ulong timeStamp = smiInstance.smi_GetTimeStamp();
        //Vector2 binocularPor = smiInstance.smi_GetBinocularPor();
        //Vector3 cameraRaycast = smiInstance.smi_GetCameraRaycast();
        //float ipd = smiInstance.smi_GetIPD();
        //Vector2 leftPor = smiInstance.smi_GetLeftPor();
        //Vector2 rightPor = smiInstance.smi_GetRightPor();
        //Vector3 leftBasePoint = smiInstance.smi_GetLeftGazeBase();
        //Vector3 rightBasePoint = smiInstance.smi_GetRightGazeBase();
        //Vector3 leftGazeDirection = smiInstance.smi_GetLeftGazeDirection();
        //Vector3 rightGazeDirection = smiInstance.smi_GetRightGazeDirection(); 
    }





    // Update is called once per frame
    public void writeGaze(StreamWriter gazeWriter)
    {
        if (smiInstance == null)
            return;

        // 
        // Note that each get function delivers the value from the latest available gaze sample.
        // The gaze sample may be updated right after/before you call any of those get functions.
        // 

        ulong timeStamp = smiInstance.smi_GetTimeStamp();
        Vector2 binocularPor = smiInstance.smi_GetBinocularPor();
        Vector3 cameraRaycast = smiInstance.smi_GetCameraRaycast();
        float ipd = smiInstance.smi_GetIPD();
        Vector2 leftPor = smiInstance.smi_GetLeftPor();
        Vector2 rightPor = smiInstance.smi_GetRightPor();
        Vector3 leftBasePoint = smiInstance.smi_GetLeftGazeBase();
        Vector3 rightBasePoint = smiInstance.smi_GetRightGazeBase();
        Vector3 leftGazeDirection = smiInstance.smi_GetLeftGazeDirection();
        Vector3 rightGazeDirection = smiInstance.smi_GetRightGazeDirection();
        gazeWriter.Write(
             timeStamp +"\t" +
             cameraRaycast.ToString("G4") + "\t"+
             binocularPor.x + ", "+ binocularPor.y + "\t" +
             SMI.SMIEyeTrackingUnity.smi_IsValid(binocularPor) + "\t"+
             ipd + "\t" +
             SMI.SMIEyeTrackingUnity.smi_IsValid(ipd) +"\t"
            );

        gazeWriter.Write(
            leftPor.x + ", " + leftPor.y + "\t" +
            SMI.SMIEyeTrackingUnity.smi_IsValid(leftPor) + "\t"+
            rightPor.x + ", " + rightPor.y + "\t" +
            SMI.SMIEyeTrackingUnity.smi_IsValid(rightPor) + "\t"
            );

        gazeWriter.Write(
           leftBasePoint.x + ", " + leftBasePoint.y + ", " + leftBasePoint.z + "\t" +
           SMI.SMIEyeTrackingUnity.smi_IsValid(leftPor) + "\t"+
           rightBasePoint.x + ", " + rightBasePoint.y + ", " + rightBasePoint.z + "\t" +
           SMI.SMIEyeTrackingUnity.smi_IsValid(rightPor) + "\t"
           );

        gazeWriter.Write(
            leftGazeDirection.x + ", " + leftGazeDirection.y + ", " + leftGazeDirection.z + "\t" +
            SMI.SMIEyeTrackingUnity.smi_IsValid(leftGazeDirection) + "\t"+
            rightGazeDirection.x + ", " + rightGazeDirection.y + ", " + rightGazeDirection.z + "\t" +
            SMI.SMIEyeTrackingUnity.smi_IsValid(rightGazeDirection)+"\t"
            );

        if(SMI.SMIEyeTrackingUnity.smi_IsValid(binocularPor) &&
            SMI.SMIEyeTrackingUnity.smi_IsValid(ipd) &&
            SMI.SMIEyeTrackingUnity.smi_IsValid(leftPor) &&
            SMI.SMIEyeTrackingUnity.smi_IsValid(rightPor) &&
            SMI.SMIEyeTrackingUnity.smi_IsValid(leftGazeDirection) &&
            SMI.SMIEyeTrackingUnity.smi_IsValid(rightGazeDirection) 
           )
        {
            gazeWriter.Write("true\t\n");
        }
        else
        {
            gazeWriter.Write("false\t\n");
        }

    }

    //Test calibration
    public void TestThreePointCalibration()
    {
        smiInstance.smi_StartThreePointCalibration();
    }

    //Test close validation
    public void TestCloseValidation()
    {
        smiInstance.smi_CloseVisualization();
    }

    //Test reset calibration
    public void TestResetCalibration()
    {
        smiInstance.smi_ResetCalibration();
    }

    //Test quit application
    public void TestQuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    //Test streaming start/stop
    bool isStreaming = true;
    public void TestToggleStreaming()
    {
        if (!isStreaming)
        {
            smiInstance.smi_StartStreaming();
            isStreaming = true;
        }
        else
        {
            smiInstance.smi_StopStreaming();
            isStreaming = false;
        }
    }
}