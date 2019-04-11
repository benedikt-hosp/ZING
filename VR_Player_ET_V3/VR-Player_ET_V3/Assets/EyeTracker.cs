using UnityEngine;
using System.IO;



public class EyeTracker
{
    SMI.SMIEyeTrackingUnity smiInstance = null;


    StreamWriter gazeWriter;
    StreamWriter logWriter;
    string logMessage = "";
    bool calibrationDone = false;


    // Use this for initialization
    public EyeTracker(StreamWriter logWriter, StreamWriter gazeWriter)
    {
        this.smiInstance = SMI.SMIEyeTrackingUnity.Instance;

        while(!this.smiInstance)
        { }

        this.logWriter = logWriter;
        this.gazeWriter = gazeWriter;






        gazeWriter.WriteLine("timestamp \t" +
           "cameraRaycast \t" +
           "binocularPor \t" +
           "binocularPor_isValid \t" +
           "ipd (m) \t" +
           "ipd_isValid \t" +
           "leftPor \t " +
           "leftPor_isValid \t  " +
           "rightPor \t  " +
           "rightPor_isValid \t  " +
           "leftBasePoint \t " +
           "leftBasePoint_isValid \t " +
           "rightBasePoint \t  " +
           "rightBasePoint_isValid \t  " +
           "leftGazeDirection \t " +
           "leftGazeDirection_isValid \t  " +
           "rightGazeDirection \t  " +
           "rightGazeDirection_isValid\tvalid");



        // set callback to receive all gaze samples and save them in file
        // smiInstance.smi_SetAsyncGazeSampleCallback(OnHighFrequencyGazeSample);
        this.logWriter.WriteLine(this.smiInstance.smi_GetTimeStamp() + "\t" + "SMI Eyetracker Started");



    }


    public ulong getSMITimeStamp()
    {
        return this.smiInstance.smi_GetTimeStamp();

    }

    public void startCalibration()
    {
        Debug.LogWarning("Started Calibration");
        this.logWriter.WriteLine(getSMITimeStamp() + "\t Calibration started\t");
        this.smiInstance.smi_StartFivePointCalibration();
        this.smiInstance.smi_SetCalibrationReturnCallback(OnCalibrationResult);


    }

    // Callback function for getting a calibration result
    public void OnCalibrationResult(int returnCode)
    {

        Debug.LogWarning("Calibration returned");
        Debug.LogError("SMI HMD calibration result: " + SMI.SMIEyeTrackingUnity.ErrorIDContainer.getErrorMessage(returnCode));

        // Return code message:
        //     SMI_SUCCESS: Calibration is successfully done.
        //     SMI_OPERATION_FAILED: Calibration process is aborted.
        logMessage = "Calibration result:\n" + SMI.SMIEyeTrackingUnity.ErrorIDContainer.getErrorMessage(returnCode);
        Debug.LogError(logMessage);

        if (returnCode.Equals(0))
        {
            this.logWriter.WriteLine(getSMITimeStamp() + "\t Calibration successfull\t");
            Debug.LogWarning("Calibration successfull");


            calibrationDone = true;
        }
        else
        {
            this.logWriter.WriteLine(getSMITimeStamp() + "\t Calibration failed\t");
            Debug.LogWarning("Calibration failed");
            calibrationDone = false;
        }

    }

    public bool exists()
    {
        if (smiInstance)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    public bool isCalibrationDone()
    {
        if (calibrationDone)
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
        /*
        if (smiInstance == null)
        {
            Debug.LogWarning("SMI NOT SET. Cant write gaze");
            return;
        }


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
             timeStamp + "\t" +
             cameraRaycast.ToString("G4") + "\t" +
             binocularPor.x + ", " + binocularPor.y + "\t" +
             SMI.SMIEyeTrackingUnity.smi_IsValid(binocularPor) + "\t" +
             ipd + "\t" +
             SMI.SMIEyeTrackingUnity.smi_IsValid(ipd) + "\t"
            );

        gazeWriter.Write(
            leftPor.x + ", " + leftPor.y + "\t" +
            SMI.SMIEyeTrackingUnity.smi_IsValid(leftPor) + "\t" +
            rightPor.x + ", " + rightPor.y + "\t" +
            SMI.SMIEyeTrackingUnity.smi_IsValid(rightPor) + "\t"
            );

        gazeWriter.Write(
           leftBasePoint.x + ", " + leftBasePoint.y + ", " + leftBasePoint.z + "\t" +
           SMI.SMIEyeTrackingUnity.smi_IsValid(leftPor) + "\t" +
           rightBasePoint.x + ", " + rightBasePoint.y + ", " + rightBasePoint.z + "\t" +
           SMI.SMIEyeTrackingUnity.smi_IsValid(rightPor) + "\t"
           );

        gazeWriter.Write(
            leftGazeDirection.x + ", " + leftGazeDirection.y + ", " + leftGazeDirection.z + "\t" +
            SMI.SMIEyeTrackingUnity.smi_IsValid(leftGazeDirection) + "\t" +
            rightGazeDirection.x + ", " + rightGazeDirection.y + ", " + rightGazeDirection.z + "\t" +
            SMI.SMIEyeTrackingUnity.smi_IsValid(rightGazeDirection) + "\t"
            );

        if (SMI.SMIEyeTrackingUnity.smi_IsValid(binocularPor) &&
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
        */
    }





    // Update is called once per frame
    public void writeGaze( )
    {
        if (smiInstance == null)
        {
            Debug.LogWarning("SMI NOT SET. Cant write gaze");
            return;
        }
            

        // 
        // Note that each get function delivers the value from the latest available gaze sample.
        // The gaze sample may be updated right after/before you call any of those get functions.
        // 

        ulong timeStamp = this.smiInstance.smi_GetTimeStamp();
        Vector2 binocularPor = this.smiInstance.smi_GetBinocularPor();
        Vector3 cameraRaycast = this.smiInstance.smi_GetCameraRaycast();
        float ipd = this.smiInstance.smi_GetIPD();
        Vector2 leftPor = this.smiInstance.smi_GetLeftPor();
        Vector2 rightPor = this.smiInstance.smi_GetRightPor();
        Vector3 leftBasePoint = this.smiInstance.smi_GetLeftGazeBase();
        Vector3 rightBasePoint = this.smiInstance.smi_GetRightGazeBase();
        Vector3 leftGazeDirection = this.smiInstance.smi_GetLeftGazeDirection();
        Vector3 rightGazeDirection = this.smiInstance.smi_GetRightGazeDirection();

        gazeWriter.Write(
             timeStamp + "\t" +
             cameraRaycast.ToString("G4") + "\t" +
             binocularPor.x + ", " + binocularPor.y + "\t" +
             SMI.SMIEyeTrackingUnity.smi_IsValid(binocularPor) + "\t" +
             ipd + "\t" +
             SMI.SMIEyeTrackingUnity.smi_IsValid(ipd) + "\t"
            );

        gazeWriter.Write(
            leftPor.x + ", " + leftPor.y + "\t" +
            SMI.SMIEyeTrackingUnity.smi_IsValid(leftPor) + "\t" +
            rightPor.x + ", " + rightPor.y + "\t" +
            SMI.SMIEyeTrackingUnity.smi_IsValid(rightPor) + "\t"
            );

        gazeWriter.Write(
           leftBasePoint.x + ", " + leftBasePoint.y + ", " + leftBasePoint.z + "\t" +
           SMI.SMIEyeTrackingUnity.smi_IsValid(leftPor) + "\t" +
           rightBasePoint.x + ", " + rightBasePoint.y + ", " + rightBasePoint.z + "\t" +
           SMI.SMIEyeTrackingUnity.smi_IsValid(rightPor) + "\t"
           );

        gazeWriter.Write(
            leftGazeDirection.x + ", " + leftGazeDirection.y + ", " + leftGazeDirection.z + "\t" +
            SMI.SMIEyeTrackingUnity.smi_IsValid(leftGazeDirection) + "\t" +
            rightGazeDirection.x + ", " + rightGazeDirection.y + ", " + rightGazeDirection.z + "\t" +
            SMI.SMIEyeTrackingUnity.smi_IsValid(rightGazeDirection) + "\t"
            );

        if (SMI.SMIEyeTrackingUnity.smi_IsValid(binocularPor) &&
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