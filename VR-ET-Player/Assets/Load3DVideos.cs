using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using SimpleFileBrowser;
using UnityEngine.UI;


public enum StimulusType
{
    VIDEO,
    IMAGE
}

public enum Dimension
{
    TWO_D,
    THREE_D
}




//[InitializeOnLoad]
public class Load3DVideos : MonoBehaviour
{

    // Images
    public GameObject imageCanvas; //Parent Panel you want the new Images to be children of

    // Utilities
    float canvasStartX = 0.0f;
    float canvasStartY = 0.0f;
    float canvasStartZ = 20.0f;
    Vector3 startPositionCanvas;

    GameObject NewObj;
    Image NewImage;

    // Videos
    private int videoIndex = 0;
    public Material skyboxThreeDmat;
    public Material skyboxTwoDmat;
    public Material skyboxBlack;
    bool finished = false;
    bool start = true;
    string experimentPath;
    private long systemStartTime;
    EyeTracker et;
    StreamWriter et_logWriter;
    StreamWriter player_logWriter;
    StreamWriter et_gazeWriter;

    FileInfo experimentFile;

    List<StimulusDescriptionObject> stimuli;

  
    void Start()
    {
        createWriters();
        createEyeTrackerObject();
     
        Resources.UnloadUnusedAssets();
        setupGUIElements();
        skyboxThreeDmat = Resources.Load("mat_3D360") as Material;
        skyboxTwoDmat = Resources.Load("mat_2D360") as Material;
        skyboxBlack = Resources.Load("BlackMat") as Material;

        stimuli = new List<StimulusDescriptionObject>();
        experimentPath = createExperimentFolder();
    
        ReadCSVFile(experimentPath);
        CreatePlaylist();
        startEyeTrackerCalibration();

    }

    private void startEyeTrackerCalibration()
    {
        et.startCalibration();
        StartCoroutine(waitForSuccessfullCalibration());
    }

    IEnumerator waitForSuccessfullCalibration()
    {
        bool done = false;
        while (!done) // essentially a "while true", but with a bool to break out naturally
        {
            if (et.isCalibrationDone())
            {
                done = true; // breaks the loop
                Debug.LogWarning("Calibration finished");
                yield break;
            }
            yield return null; // wait until next frame, then continue execution from here (loop continues)

        }

        // now this function returns
    }

    private void createWriters()
    {

        string resultFolder = Path.Combine(Application.dataPath, "..");
        resultFolder = Path.Combine(resultFolder, "Results");
        if(!Directory.Exists(resultFolder))
        {
            Directory.CreateDirectory(resultFolder);
        }
        Debug.Log("Saving into Resultsfolder: " + resultFolder);

        string player_logFilePath = Path.Combine(resultFolder, "player-log.txt");
        player_logWriter = new StreamWriter(player_logFilePath);
        Debug.Log("Results/player-log.txt created.");

        player_logWriter.WriteLine("timestamp\t" +
            "event\t"+
            "name\t\n");

        string et_gazeFilePath = Path.Combine(resultFolder, "et-gaze.txt");
        et_gazeWriter = new StreamWriter(et_gazeFilePath);
        Debug.Log("Results/et-gaze created.");


        et_gazeWriter.WriteLine("timestamp \t"+ 
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

        string et_logFilePath = Path.Combine(resultFolder, "et-log.txt");
        et_logWriter = new StreamWriter(et_logFilePath);
        Debug.Log("Results/et-log.txt created.");



        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        long cur_time = (long)(System.DateTime.UtcNow - epochStart).TotalMilliseconds;
        systemStartTime = cur_time;

        Debug.Log("System start(ms): "+ systemStartTime);

    }

    private void createEyeTrackerObject()
    {
       
        et = new EyeTracker(et_logWriter);
    }

    private string createExperimentFolder()
    {
        string path = Path.Combine(Application.dataPath, "..");
        path = Path.Combine(path, "Experiment");
        Directory.CreateDirectory(path);

        Debug.Log("Using Experimentfolder: " + path);
        

        return path;

    }

    private void Update()
    {
         // TODO: Wait for key to start playing
         if (start && Input.GetKeyDown(KeyCode.Space))
         {
             start = false;
             StartCoroutine(playStimuliList());
         }

         et.writeGaze(et_gazeWriter);

        if (Input.GetKey("escape"))
        {

            player_logWriter.Write(getCurrentTime() + "\tSystem shutdown.\t\n");
            et_logWriter.WriteLine(et.getSMITimeStamp() + "\t" + "SMI Eyetracker Stopped");

            et.TestQuitApplication();
            player_logWriter.Close();
            et_gazeWriter.Close();
            et_logWriter.Close();
            Application.Quit();
        }


    }
    
    private void setupGUIElements()
    {
        NewObj = new GameObject(); //Create the GameObjects
        NewImage =  NewObj.AddComponent<Image>(); //Add the Image Component script

        // find Canvas for Images
        imageCanvas = GameObject.FindGameObjectWithTag("ImageCanvas");

        // set starting position of canvas to center
        startPositionCanvas = new Vector3(canvasStartX, canvasStartY, canvasStartZ);

        // assign starting position to canvas
        imageCanvas.GetComponent<RectTransform>().SetPositionAndRotation(startPositionCanvas, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f));
        NewImage.GetComponent<RectTransform>().SetPositionAndRotation(startPositionCanvas, new Quaternion(0.0f, 0.0f, 0.0f, 0.0f));



        Debug.LogWarning("UI canvas has been setup");


    }

    private void ReadCSVFile(string filePath)
    {

        DirectoryInfo dir = new DirectoryInfo(filePath);
        FileInfo[] info = dir.GetFiles("*.experiment");

       if(info.Length <1)
        {
            Debug.Log("No experiment file found! Stopping.");
        }
        else
        {
            filePath = Path.Combine(filePath, info[0].FullName);

            // Handle any problems that might arise when reading the text
            try
            {
                string line;
                // Create a new StreamReader, tell it which file to read and what encoding the file
                // was saved as
                StreamReader theReader = new StreamReader(filePath, Encoding.Default);
                // Immediately clean up the reader after this block of code is done.
                // You generally use the "using" statement for potentially memory-intensive objects
                // instead of relying on garbage collection.
                // (Do not confuse this with the using directive for namespace at the 
                // beginning of a class!)
                using (theReader)
                {
                    // While there's lines left in the text file, do this:
                    do
                    {
                        line = theReader.ReadLine();

                        if (line != null)
                        {
                            // Do whatever you need to do with the text line, it's a string now
                            // In this example, I split it into arguments based on comma
                            // deliniators, then send that array to DoStuff()
                            string[] entries = line.Split(';');
                            if (entries.Length > 0)
                            {
                                CreatePlayableObject(entries);
                            
                            }
                                //DoStuff(entries);
                        }
                    }
                    while (line != null);
                    // Done reading, close the reader and return true to broadcast success    
                    theReader.Close();
                    return;
                }
            }
            // If anything broke in the try block, we throw an exception with information
            // on what didn't work
            catch (Exception e)
            {
                Console.WriteLine("{0}\n", e.Message);
                return;
            }
         }
    }

    private long getCurrentTime()
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        long cur_time = (long)(System.DateTime.UtcNow - epochStart).TotalMilliseconds;
        
        return (cur_time-systemStartTime );
    }

    private void CreatePlayableObject(string[] entries)
    {
        StimulusType stimulusType;
        string path;
        Dimension dimension;
        int resolution_x, resolution_y;
//        string name;

        //Type type, Dimension dimension, int resolution_x, int resolution_y, string path


// 0. Type: Video | Image
        if(entries[0].Equals("Video"))
        {
            stimulusType = StimulusType.VIDEO;
    
            // 1. Dimension
            if (entries[1].Equals("NULL"))
            {
                return;
            }
            else
            {
                if (entries[1].Equals("3D"))
                {
                    dimension = Dimension.THREE_D;
                }
                else
                {
                    dimension = Dimension.TWO_D;
                }
            }

        }
        else
        {
            stimulusType = StimulusType.IMAGE;
            dimension = Dimension.TWO_D;
        }

        // TODO when we want to have different resolutions possible
        // 2. Resolution => split 
        string[] axis = entries[2].Split('x');
        resolution_x = Int32.Parse(axis[0]); 
        resolution_y = Int32.Parse(axis[1]);

        
        // 3. Path to files
        path = entries[3];

        StimulusDescriptionObject obj = new StimulusDescriptionObject(stimulusType, dimension, resolution_x,resolution_y, path);
        if(stimulusType == StimulusType.VIDEO)
        { 
            obj.setPlayer(dimension,this.gameObject);
        }
        else
        {
            obj.setSprite(dimension, this.gameObject);
        }
        stimuli.Add(obj);
        


    }

    private void CreatePlaylist()
    {
        Debug.Log("Current Stimuli List");
        for(int i=0; i<stimuli.Count;i++)
        {
            Debug.Log("Stimulus: " + i + " " + stimuli[i].stimulusType.ToString() + " " + stimuli[i].dimension.ToString() + " " + stimuli[i].resolution_x + "x" + stimuli[i].resolution_y + " " + stimuli[i].path);
        }
        Debug.Log("Ready to start....");
    }

    IEnumerator WaitForUserInput()
    {
        bool done = false;
        while (!done) // essentially a "while true", but with a bool to break out naturally
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                done = true; // breaks the loop
                Debug.Log("SPACE BAR PRESSED");
                yield break;
            }
            yield return null; // wait until next frame, then continue execution from here (loop continues)

        }

        // now this function returns
    }

    IEnumerator playStimuliList()
    {

        if (videoIndex-1 >= 0 && stimuli[videoIndex-1].stimulusType == StimulusType.VIDEO)
        {
            player_logWriter.WriteLine(getCurrentTime() + "\t" + "Stop\t" +  stimuli[videoIndex-1].path + "\t \n");

        }
        yield return StartCoroutine(WaitForUserInput());

        if (videoIndex >= stimuli.Count)
        {
            SetEnvironmentBlack();
            yield break;
        }

        if (stimuli[videoIndex].stimulusType == StimulusType.IMAGE)
        {
            RenderSettings.skybox = skyboxBlack;
            DynamicGI.UpdateEnvironment();
            SetEnvironmentBlack();
            GameObject NewObj = new GameObject(); //Create the GameObjects

            Image NewImage = NewObj.AddComponent<Image>(); //Add the Image Component script
            NewImage.sprite = stimuli[videoIndex].image; //Set the Sprite of the Image Component on the new GameObject


            if (imageCanvas.transform.childCount > 0)
            {
                Destroy((imageCanvas.transform.GetChild(0).gameObject));
            }

            NewObj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);


            // size of the image is changed by chaing z axis
            NewObj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 200);
            NewObj.GetComponent<RectTransform>().SetParent(imageCanvas.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
                                                                                   //stimuli[]
            player_logWriter.WriteLine(getCurrentTime() + "\t" + "Start\t" + stimuli[videoIndex].path + "\t \n");
            NewObj.SetActive(true);

            yield return new WaitForSeconds(2);

            videoIndex++;


        }
        else
        {
            if (imageCanvas.transform.childCount > 0)
            {
                Destroy((imageCanvas.transform.GetChild(0).gameObject));
            }


            //Make sure that the NEXT VideoPlayer index is valid
            if (videoIndex >= stimuli.Count)
            { 
                SetEnvironmentBlack();
                yield break;
            }


            //Prepare video
            stimuli[videoIndex].vp.Prepare();

            //Wait until this video is prepared
            while (!stimuli[videoIndex].vp.isPrepared)
            {
                Debug.Log("Preparing Index: " + videoIndex);
                yield return null;
            }

            Debug.LogWarning("Done Preparing current Video Index: " + videoIndex);

            //Play first video
            stimuli[videoIndex].vp.Play();
           if(videoIndex - 1 >= 0 && stimuli[videoIndex - 1].stimulusType == StimulusType.IMAGE)
            {
                player_logWriter.WriteLine(getCurrentTime() + "\t" + "Stop\t" + stimuli[videoIndex-1].path + "\t\n");

            }
            player_logWriter.WriteLine(getCurrentTime() + "\t" + "Start\t" + stimuli[videoIndex].path + "\t\n");

            // Depending on stimulus, change RenderSkybox to 3D or 2D Material
            if (stimuli[videoIndex].dimension == Dimension.THREE_D)
            {
                RenderSettings.skybox = skyboxThreeDmat;
                DynamicGI.UpdateEnvironment();

            }
            else
            {
                RenderSettings.skybox = skyboxTwoDmat;
                DynamicGI.UpdateEnvironment();
            }

            //Wait while the current video is playing
            int nextIndex = (videoIndex + 1);
            while (stimuli[videoIndex].vp.isPlaying)
            {
                Debug.Log("Playing time: " + stimuli[videoIndex].vp.time + " INDEX: " + videoIndex);

                //Make sure that the NEXT VideoPlayer index is valid. Otherwise Exit since this is the end
                if (nextIndex >= stimuli.Count)
                {
                    Debug.LogWarning("End of All Videos: ");
                    //yield break;
                    finished = true;
                }
                else
                { 

                    //Prepare the NEXT video
                    Debug.LogWarning("Ready to Prepare NEXT Video Index: " + nextIndex);
                    if (nextIndex < stimuli.Count && stimuli[nextIndex].stimulusType == StimulusType.VIDEO)
                    {
                        stimuli[nextIndex].vp.Prepare();
                    }
                }
                yield return null;

            }



            if (finished)
            {

                player_logWriter.WriteLine(getCurrentTime() + "\t" + "Stop\t" + stimuli[videoIndex].path + "\t \n");

                SetEnvironmentBlack();
                GameObject NewObj = new GameObject(); //Create the GameObjects

                Image NewImage = NewObj.AddComponent<Image>(); //Add the Image Component script

                //Sprite sprite;
                Texture2D spriteTexture = Resources.Load<Texture2D>("ende");
                NewImage.sprite = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), 100.0f, 0, SpriteMeshType.Tight);


                if (imageCanvas.transform.childCount > 0)
                {
                    Destroy((imageCanvas.transform.GetChild(0).gameObject));
                }

                NewObj.GetComponent<RectTransform>().localScale = new Vector3(1, 1, 1);


                // size of the image is changed by chaing z axis
                NewObj.GetComponent<RectTransform>().localPosition = new Vector3(0, 0, 200);
                NewObj.GetComponent<RectTransform>().SetParent(imageCanvas.transform); //Assign the newly created Image GameObject as a Child of the Parent Panel.
                NewObj.SetActive(true);

                yield break;
            }




            Debug.Log("Done Playing current Video Index: " + videoIndex);

            //Wait until NEXT video is prepared
            while (!stimuli[videoIndex].vp.isPrepared)
            {
                Debug.Log("Preparing NEXT Video Index: " + nextIndex);
                yield return null;
            }

            Debug.LogWarning("Done Preparing NEXT Video Index: " + nextIndex);

            //Increment Video index
            videoIndex++;

             SetEnvironmentBlack();

        }
        //Play next prepared video. Pass false to it so that some codes are not executed at-all
        StartCoroutine(playStimuliList());

    }

    private IEnumerator waitForKeyPress(KeyCode key)
    {
        bool done = false;
        while (!done) // essentially a "while true", but with a bool to break out naturally
        {
            if (Input.GetKeyDown(key))
            {
                done = true; // breaks the loop
                yield return null; // wait until next frame, then continue execution from here (loop continues)

            }
        }

        // now this function returns
    }

    public void SetEnvironmentBlack()
    {
        RenderSettings.skybox = skyboxBlack;
        DynamicGI.UpdateEnvironment();
    }

    /*
    IEnumerator playVideo(bool firstRun = true)
    {
        
        //Make sure that the NEXT VideoPlayer index is valid
        if (videoIndex >= videoPlayerList.Count)
            yield break;

        //Prepare video
        videoPlayerList[videoIndex].Prepare();

        //Wait until this video is prepared
        while (!videoPlayerList[videoIndex].isPrepared)
        {
            Debug.Log("Preparing Index: " + videoIndex);
            yield return null;
        }
        Debug.LogWarning("Done Preparing current Video Index: " + videoIndex);

        //Play first video
        videoPlayerList[videoIndex].Play();

        //Wait while the current video is playing
        int nextIndex = (videoIndex + 1);
        while (videoPlayerList[videoIndex].isPlaying)
        {
            Debug.Log("Playing time: " + videoPlayerList[videoIndex].time + " INDEX: " + videoIndex);

            //Make sure that the NEXT VideoPlayer index is valid. Otherwise Exit since this is the end
            if (nextIndex >= videoPlayerList.Count)
            {
                Debug.LogWarning("End of All Videos: " + videoIndex);
                yield break;
            }

            //Prepare the NEXT video
            Debug.LogWarning("Ready to Prepare NEXT Video Index: " + nextIndex);
            videoPlayerList[nextIndex].Prepare();

            yield return null;
        }
        Debug.Log("Done Playing current Video Index: " + videoIndex);

        //Wait until NEXT video is prepared
        while (!videoPlayerList[nextIndex].isPrepared)
        {
            Debug.Log("Preparing NEXT Video Index: " + nextIndex);
            yield return null;
        }

        Debug.LogWarning("Done Preparing NEXT Video Index: " + videoIndex);

        //Increment Video index
        videoIndex++;
        videoIndex++;

        //Play next prepared video. Pass false to it so that some codes are not executed at-all
        StartCoroutine(playVideo(false));
        
    }
    */
}
