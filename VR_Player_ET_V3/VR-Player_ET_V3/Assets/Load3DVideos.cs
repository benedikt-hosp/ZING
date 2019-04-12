using UnityEngine;
using UnityEngine.Video;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System;
using SimpleFileBrowser;
using UnityEngine.UI;
using UnityEditor;

public enum StimulusType
{
    VIDEO,
    IMAGE
}

public enum Dimension
{
    TWO_D,
    THREE_D,
    QUAD
}




public class StimulusDescriptionObject
{
    public RenderTexture rt_material;
    //public RenderTexture rt_skybox;
    public Texture omniImage;
    public Texture texture;
    public Sprite image;
    public float initialCameraX;
    public float initialCameraY;
    public float initialCameraZ;
    public float initialCameraW;

    public VideoPlayer vp;
    public  string path;
    public StimulusType stimulusType;
    public Dimension dimension;
    public bool omniDirectional = false;
    public bool quad = false;
    public int resolution_x, resolution_y;


    public StimulusDescriptionObject(StimulusType stimulusType, Dimension dimension, int resolution_x, int resolution_y, string path, bool omniDirectional, float camX, float camY, float camZ, float camW)
    {
        
        this.stimulusType = stimulusType;           // Image || Video
        this.dimension = dimension;                 // 2D || 3D
        this.resolution_x = resolution_x;           // width 
        this.resolution_y = resolution_y;           // height
        this.path = path;                           // path to file
        this.omniDirectional = omniDirectional;     // 360 degree || normal
        this.initialCameraX = camX;
        this.initialCameraY = camY;
        this.initialCameraZ = camZ;
        this.initialCameraW = camW;

    }
   
    internal void setPlayer(Dimension dimension, GameObject gameObject)
    {
        if(dimension == Dimension.THREE_D && stimulusType == StimulusType.VIDEO)
        { 
             // 3D
            this.vp = gameObject.AddComponent<VideoPlayer>();
            rt_material = Resources.Load<RenderTexture>("RenderTextures/rt_4K3D");

            vp.renderMode = VideoRenderMode.RenderTexture;
            vp.targetTexture = rt_material;

            //Disable Play on Awake for both Video and Audio
            vp.playOnAwake = false;

            //We want to play from video clip not from url
            vp.source = VideoSource.Url;

            //Set Audio Output to AudioSource
            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;

            //Assign the Audio from Video to AudioSource to be played
            vp.EnableAudioTrack(0, true);
            //videoPlayer.SetTargetAudioSource(0, audioSource);

            //Set video Clip To Play 
            vp.url = this.path;
        }

        if(dimension == Dimension.TWO_D && stimulusType == StimulusType.VIDEO)
        { 
            // 2D
            this.vp = gameObject.AddComponent<VideoPlayer>();
            rt_material = Resources.Load<RenderTexture>("RenderTextures/rt_4K2D");

            vp.renderMode = VideoRenderMode.RenderTexture;
            vp.targetTexture = rt_material;

            //Disable Play on Awake for both Video and Audio
            vp.playOnAwake = false;

            //We want to play from video clip not from url
            vp.source = VideoSource.Url;

            //Set Audio Output to AudioSource
            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;

            //Assign the Audio from Video to AudioSource to be played
            vp.EnableAudioTrack(0, true);
            //videoPlayer.SetTargetAudioSource(0, audioSource);

            //Set video Clip To Play 
            vp.url = this.path;
        }

        if (dimension == Dimension.QUAD && stimulusType == StimulusType.VIDEO)
        {
            // 2D
            this.vp = gameObject.AddComponent<VideoPlayer>();
            rt_material = Resources.Load<RenderTexture>("RenderTextures/rt_quad");

            vp.renderMode = VideoRenderMode.RenderTexture;
            vp.targetTexture = rt_material;

            //Disable Play on Awake for both Video and Audio
            vp.playOnAwake = false;

            //We want to play from video clip not from url
            vp.source = VideoSource.Url;

            //Set Audio Output to AudioSource
            vp.audioOutputMode = VideoAudioOutputMode.AudioSource;

            //Assign the Audio from Video to AudioSource to be played
            vp.EnableAudioTrack(0, true);
            //videoPlayer.SetTargetAudioSource(0, audioSource);

            //Set video Clip To Play 
            vp.url = this.path;
        }
    }

    

    internal void setOmniImage()
    {
        //Sprite sprite;
        Texture2D spriteTexture = LoadTexture(this.path);
        this.omniImage = spriteTexture;
    }


    internal void setSprite()
    {
        //Sprite sprite;
        Texture2D spriteTexture = LoadTexture(this.path);
        this.texture = spriteTexture;
        image = Sprite.Create(spriteTexture, new Rect(0, 0, spriteTexture.width, spriteTexture.height), new Vector2(0, 0), 100.0f, 0, SpriteMeshType.Tight);
    }


    public Texture2D LoadTexture(string FilePath)
    {

        // Load a PNG or JPG file from disk to a Texture2D
        // Returns null if load fails

        Texture2D Tex2D;
        byte[] FileData;

        if (File.Exists(FilePath))
        {
            FileData = File.ReadAllBytes(FilePath);
            Tex2D = new Texture2D(2, 2, TextureFormat.RGB24, false);            // Create new "empty" texture
            if (Tex2D.LoadImage(FileData))          // Load the imagedata into the texture (size is set automatically)
                return Tex2D;                       // If data = readable -> return texture
        }
        else
        {
            Debug.Log("Image not found");
        }
        return null;                     // Return null if load failed
    }

}


//[InitializeOnLoad]
public class Load3DVideos : MonoBehaviour
{

    // png images
    public GameObject imageCanvas; //Parent Panel you want the new Images to be children of
    public GameObject imageCube;
    CanvasScaler cvsl;



    public GameObject rotationObj;

    bool calibrated = false;

    GameObject NewObj;
    Image NewImage;

    // Videos

    GameObject et_cam;
    GameObject vr_cam;


    //private List<VideoPlayer> videoPlayerList;
    private int videoIndex = 0;
    public Material skyboxThreeDmat;
    public Material skyboxTwoDmat;
    public Material skyboxBlack;
    public Material skyboxQuad;

    bool finished = false;
    bool start = true;
    string experimentPath;
    private long systemStartTime;
    EyeTracker et;
    StreamWriter et_logWriter;
    StreamWriter player_logWriter;
    StreamWriter et_gazeWriter;

    static string cmdInfo = "";


    FileInfo experimentFile;

    List<StimulusDescriptionObject> stimuli;

    void Start()
    {
        DynamicGI.synchronousMode = true;

        Resources.UnloadUnusedAssets();
        setupGUIElements();

        this.rotationObj = GameObject.FindWithTag("RotationObject");

        this.et_cam = GameObject.FindWithTag("ET-Camera");
        this.vr_cam = GameObject.FindWithTag("EyeCamera");


        skyboxThreeDmat = Resources.Load<Material>("Skyboxes/skybox_4K3D");
        skyboxTwoDmat = Resources.Load<Material>("Skyboxes/skybox_4K2D");
        skyboxBlack = Resources.Load<Material>("Skyboxes/skybox_black");
        skyboxQuad = Resources.Load<Material>("Skyboxes/skybox_quad");

        stimuli = new List<StimulusDescriptionObject>();
        experimentPath = createExperimentFolder();

        createWriters();
        //createEyeTrackerObject();
        //startCalibration();
        //startAsyncCalibrationCall();

        ReadCSVFile(experimentPath);
        CreatePlaylist();

    }


    private void startCalibration()
    {
        et.startCalibration();

    }


    public void startAsyncCalibrationCall()
    {
       
        while(!et.isCalibrationDone())
        {
            Debug.LogWarning("Waiting for calibration result");
        }
        calibrated = true;
    }


    private void createWriters()
    {
        string resultFolder = Path.Combine(Application.dataPath, "..");
        resultFolder = Path.Combine(resultFolder, "Results");
        if (!Directory.Exists(resultFolder))
        {
            Directory.CreateDirectory(resultFolder);
        }
        Debug.Log("Saving into Resultsfolder: " + resultFolder);

        string player_logFilePath = Path.Combine(resultFolder, "player-log.txt");
        player_logWriter = new StreamWriter(player_logFilePath);
        Debug.Log("Results/player-log.txt created.");

        player_logWriter.WriteLine("timestamp\t" +
            "event\t" +
            "name\t\n");

        string et_gazeFilePath = Path.Combine(resultFolder, "et-gaze.txt");
        et_gazeWriter = new StreamWriter(et_gazeFilePath);
        Debug.Log("Results/et-gaze created.");



        string et_logFilePath = Path.Combine(resultFolder, "et-log.txt");
        et_logWriter = new StreamWriter(et_logFilePath);
        Debug.Log("Results/et-log.txt created.");



        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        long cur_time = (long)(System.DateTime.UtcNow - epochStart).TotalMilliseconds;
        systemStartTime = cur_time;

        Debug.Log("System start(ms): " + systemStartTime);
    }


    private void createEyeTrackerObject()
    {
        et = new EyeTracker(et_logWriter, et_gazeWriter);
        if (et.exists())
        {
            Debug.LogWarning("SMI set up");
        }
        else
        {
            Debug.LogWarning("SMI failed to set up");
        }
    }

    private string createExperimentFolder()
    {
        string path = Path.Combine(Application.dataPath, "..");
        path = Path.Combine(path, "Experiment");
        Directory.CreateDirectory(path);

        Debug.Log(path);


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

        /*
         if (et.exists() && calibrated)
             et.writeGaze();
             */
        if (Input.GetKey("escape"))
        {

            //player_logWriter.Write(getCurrentTime() + "\tSystem shutdown.\t\n");
            //et_logWriter.WriteLine(et.getSMITimeStamp() + "\t" + "SMI Eyetracker Stopped");

            //et.TestQuitApplication();
            //player_logWriter.Close();
            //et_gazeWriter.Close();
            //et_logWriter.Close();
            Application.Quit();
        }


    }

    private long getCurrentTime()
    {
        System.DateTime epochStart = new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc);
        long cur_time = (long)(System.DateTime.UtcNow - epochStart).TotalMilliseconds;

        return (cur_time - systemStartTime);
    }

    private void setupGUIElements()
    {
        // find Canvas for Images
        imageCanvas = GameObject.FindGameObjectWithTag("ImageCanvas");


        imageCube = GameObject.FindGameObjectWithTag("imageCube");

        imageCube.GetComponent<MeshRenderer>().enabled = false;
        imageCube.AddComponent<MeshRenderer>();
    }

    private void OpenExperiment2()
    {
        //fb = SimpleFileBrowser.FileBrowser;
        //fb = FileBrowser;
        //fileBrowser = FileBrowser.Instance;


        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters
        //FileBrowser.SetFilters(true, new FileBrowser.Filter("Text Files", ".txt"));


        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        //fb.SetDefaultFilter(".txt");

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        //        fb.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // It is sufficient to add a quick link just once
        // Name: Users
        // Path: C:\Users
        // Icon: default (folder icon)
        //FileBrowser.AddQuickLink("Users", "C:\\Users", null);

        // Show a save file dialog 
        // onSuccess event: not registered (which means this dialog is pretty useless)
        // onCancel event: not registered
        // Save file/folder: file, Initial path: "C:\", Title: "Save As", submit button text: "Save"
        // FileBrowser.ShowSaveDialog( null, null, false, "C:\\", "Save As", "Save" );

        // Show a select folder dialog 
        // onSuccess event: print the selected folder's path
        // onCancel event: print "Canceled"
        // Load file/folder: folder, Initial path: default (Documents), Title: "Select Folder", submit button text: "Select"
        // FileBrowser.ShowLoadDialog( (path) => { Debug.Log( "Selected: " + path ); }, 
        //                                () => { Debug.Log( "Canceled" ); }, 
        //                                true, null, "Select Folder", "Select" );

        // Coroutine example
        // StartCoroutine(ShowLoadDialogCoroutine());
    }

    private void OpenExperiment()
    {
        // Set filters (optional)
        // It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
        // if all the dialogs will be using the same filters

        FileBrowser.SetFilters(true, new FileBrowser.Filter("Text Files", ".txt"));


        // Set default filter that is selected when the dialog is shown (optional)
        // Returns true if the default filter is set successfully
        // In this case, set Images filter as the default filter
        // FileBrowser.SetDefaultFilter(".txt");

        // Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
        // Note that when you use this function, .lnk and .tmp extensions will no longer be
        // excluded unless you explicitly add them as parameters to the function
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

        // Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
        // It is sufficient to add a quick link just once
        // Name: Users
        // Path: C:\Users
        // Icon: default (folder icon)
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);

    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(false, null, "Load File", "Load");

        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
        Debug.Log(FileBrowser.Success + " " + FileBrowser.Result);

        string filePath = FileBrowser.Result;


        ReadCSVFile(filePath);
        CreatePlaylist();
        // StartCoroutine(playVideo());

    }


    IEnumerator ShowLoadDialogCoroutine2()
    {
        // Show a load file dialog and wait for a response from user
        // Load file/folder: file, Initial path: default (Documents), Title: "Load File", submit button text: "Load"
        yield return FileBrowser.WaitForLoadDialog(false, null, "Load File", "Load");

        // Dialog is closed
        // Print whether a file is chosen (FileBrowser.Success)
        // and the path to the selected file (FileBrowser.Result) (null, if FileBrowser.Success is false)
        Debug.Log(FileBrowser.Success + " " + FileBrowser.Result);

        string filePath = FileBrowser.Result;


        ReadCSVFile(filePath);
        CreatePlaylist();
        // StartCoroutine(playVideo());

    }

    private void ReadCSVFile(string filePath)
    {

        
        string[] arguments = Environment.GetCommandLineArgs();
        foreach (string arg in arguments)
        {
            cmdInfo += arg.ToString();
        }

        UnityEngine.Debug.LogError("CMD: " + cmdInfo);

        string str1 = "not found";
        string[] cmdInfoList = cmdInfo.Split(char.Parse("#"));
        if (cmdInfoList.Length > 1)
        {
            str1 = cmdInfoList[1];
        }

        // DEBUGGING
        // str1 = "G:/Git_repositories/VR-Player/VRExperimentDesigner/tempExperiment.vrexperiment";
        str1 = "G:/Git_repositories/VR-Player/VRExperimentDesigner/VRExperimentDesigner/VR-Player/VR-Player_ET_V3_Data/Resources/tempExperiment.vrexperiment";

        // release
        //str1 = Application.dataPath + "/Resources/tempExperiment.vrexperiment";




        if (File.Exists(str1))
        {
            string experimentFileContent= File.ReadAllText(str1);


            //UnityEngine.Debug.LogError(File.ReadAllText(str1));
        
            filePath = str1;


            // Handle any problems that might arise when reading the text

                string[] experimentFileContentChunks = experimentFileContent.Split('\n');
                    // While there's lines left in the text file, do this:
                    foreach(string line in experimentFileContentChunks)
                    {
                        if (line != null)
                        {
                            // Do whatever you need to do with the text line, it's a string now
                            // In this example, I split it into arguments based on ';'
                            // deliniators, then send that array to DoStuff()
                            Debug.LogError("Splitting line: " + line);
                            string[] entries = line.Split(';');
                            if (entries.Length > 0)
                            {

                                if(!String.IsNullOrEmpty(line))
                                    CreatePlayableObject(entries);

                            }
                        }
                    }
          
          

        }

    }

    void CreatePlayableObject(string[] entries)
    {


        // Structure: 0 File type;
        //            1 dimension;
        //            2 resolution;
        //            3 startx camera,
        //            4 starty camera;
        //            5 filename;
        //            6 file path

        // Image;2D;ja;3840x1920;-1;-1;Video 1 Bild.png;D:/Windows_Backup/USER/Documents/360Player/Assets/stimuli/360bilder/Video 1 Bild.png;
        StimulusType stimulusType;                  //  0 Image or Video
        Dimension dimension = Dimension.TWO_D;      //  1 dimension 
        bool omniDirectional;                       //  2 ja = 3D, nein = 2D
        int resolution_x, resolution_y;             //  3 resolution
        float initCameraX;                            //  4 cameraX
        float initCameraY;                            //  5 cameraY
        float initCameraZ;                            //  6 cameraZ
        float initCameraW;                            //  7 cameraW
        string filename;                            //  8 file name
        string path;                                //  9 File path


        for(int i =0; i<entries.Length;i++)
        {
            Debug.LogError("item " + i + " = " + entries[i]);

        }

        //Type type, Dimension dimension, int resolution_x, int resolution_y, string path
        // TODO when we want to have different resolutions possible
        // 2. Resolution => split 
        //string[] axis = entries[3].Split('x');

       
        string[] axis = entries[3].Split('x');
        resolution_x = int.Parse(axis[0]);
        resolution_y = int.Parse(axis[1]);
      



        // 0. Type: Video | Image
        if (entries[0].Equals("Video"))
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
                    if (resolution_x == 3840)
                    {
                        dimension = Dimension.THREE_D;
                    }

                    if (resolution_x == 2304)
                    {
                        dimension = Dimension.QUAD;
                    }

                }

            }

        }
        else
        {
            stimulusType = StimulusType.IMAGE;
            // 1. Dimension
            if (entries[1].Equals("NULL"))
            {
                return;
            }
            else
            {
                if (entries[1].Equals("3D"))
                {
                    if (resolution_x == 3840)
                        dimension = Dimension.THREE_D;

                    if (resolution_x == 2304)
                        dimension = Dimension.QUAD;
                }
                else
                {
                    dimension = Dimension.TWO_D;
                }
            }
        }

      

      
        // 3. Omni directional image/video?
        if (entries[2].Equals("ja"))
        {
            omniDirectional = true;
        }
        else
        {
            omniDirectional = false;
        }

        // 4. camera X init position
        if (entries[4].Equals("-1"))
        {
            initCameraX = 0;
        }
        else
        {
            initCameraX = float.Parse(entries[4]);

        }

        // 5. camera y init position
        if (entries[5].Equals("-1"))
        {
            initCameraY = 0;
        }
        else
        {
            initCameraY = float.Parse(entries[5]);

        }


        // 6. camera y init position
        if (entries[6].Equals("-1"))
        {
            initCameraZ = 0;
        }
        else
        {
            initCameraZ = float.Parse(entries[6]);

        }

        // 7. camera y init position
        if (entries[7].Equals("-1"))
        {
            initCameraW = 0;
        }
        else
        {
            initCameraW = float.Parse(entries[7]);

        }


        // 6. file name
        filename = entries[8];

        // 7. Path to files
        path = entries[9];
        Debug.LogWarning(path);

        StimulusDescriptionObject obj = new StimulusDescriptionObject(stimulusType, dimension, resolution_x, resolution_y, path, omniDirectional,initCameraX,initCameraY,initCameraZ,initCameraW);
        if (stimulusType == StimulusType.VIDEO)
        {
            obj.setPlayer(dimension, this.gameObject);
        }
        else
        {
            if (omniDirectional)
            {
                obj.setOmniImage();
            }
            else
            {
                obj.setSprite();
            }
        }
        stimuli.Add(obj);

    }

    private void CreatePlaylist()
    {
        Debug.Log("DEBUG: Current Stimuli List");
        for (int i = 0; i < stimuli.Count; i++)
        {
            Debug.Log("Stimulus: " + i + " " + stimuli[i].stimulusType.ToString() + " " + stimuli[i].dimension.ToString() + " " + stimuli[i].omniDirectional.ToString() + " " + stimuli[i].resolution_x + "x" + stimuli[i].resolution_y + " " + stimuli[i].path);
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
                Debug.LogError("SPACE BAR PRESSED");
                yield break;
            }
            yield return null; // wait until next frame, then continue execution from here (loop continues)

        }

        // now this function returns
    }


    IEnumerator playStimuliList()
    {

        if (videoIndex - 1 >= 0 && stimuli[videoIndex - 1].stimulusType == StimulusType.VIDEO)
        {
            player_logWriter.WriteLine(getCurrentTime() + "\t" + "Stop\t" + stimuli[videoIndex - 1].path + "\t \n");

        }
        yield return StartCoroutine(WaitForUserInput());


        // last stimulus was shown
        // show black skybox with "ENDE" image
        if (videoIndex >= stimuli.Count)
        {
            ShowFinalImage();
            yield break;
        }

        // showing image
        if (stimuli[videoIndex].stimulusType == StimulusType.IMAGE)
        {

            if (stimuli[videoIndex].omniDirectional)
            {
                // 360 image
                // 360 3D image
                if (stimuli[videoIndex].dimension == Dimension.THREE_D)
                {
                    Debug.LogWarning("Setting skybox material to omniImage 3D");
                    skyboxThreeDmat.mainTexture = stimuli[videoIndex].omniImage;
                    RenderSettings.skybox = skyboxThreeDmat;
                    DynamicGI.UpdateEnvironment();


                }

                // 360 image
                // 360 2D image
                if (stimuli[videoIndex].dimension == Dimension.TWO_D)
                {
                    Debug.LogWarning("Setting skybox material to omniImage 2D");
                    skyboxTwoDmat.mainTexture = stimuli[videoIndex].omniImage;
                    RenderSettings.skybox = skyboxTwoDmat;
                    DynamicGI.UpdateEnvironment();
                    //videoIndex++;
                }

                // 360 image
                // 360 3D Quad image
                if (stimuli[videoIndex].dimension == Dimension.QUAD)
                {
                    Debug.LogWarning("Setting skybox material to omniImage Quad");
                    skyboxQuad.mainTexture = stimuli[videoIndex].omniImage;
                    RenderSettings.skybox = skyboxQuad;
                    DynamicGI.UpdateEnvironment();
                    //videoIndex++;
                }

                yield return new WaitForSeconds(2);
                videoIndex++;
            }
            else
            {

                // NORMAL IMAGE
                RenderSettings.skybox = skyboxBlack;
                DynamicGI.UpdateEnvironment();
                SetEnvironmentBlack();



                Debug.LogWarning("width: " + stimuli[videoIndex].image.textureRect.width + " height: " + stimuli[videoIndex].image.textureRect.height);

                if (stimuli[videoIndex].texture.width >= 1400 && stimuli[videoIndex].texture.height >= 500)
                {
                    imageCube.transform.localScale = new Vector3(4.5f, 2.5f, 0.133f);

                }
                else
                {
                    //imageCube.transform.localToWorldMatrix ??
                    imageCube.transform.localScale = new Vector3(1.5f, 1.5f, 0.133f);
                }

                imageCube.GetComponent<MeshRenderer>().enabled = true;
                imageCube.GetComponent<MeshRenderer>().material.mainTexture = stimuli[videoIndex].texture;

                yield return new WaitForSeconds(2);

                videoIndex++;
            }

        }
        else
        {

            imageCube.GetComponent<MeshRenderer>().enabled = false;

            //Make sure that the NEXT VideoPlayer index is valid
            if (videoIndex >= stimuli.Count)
            {
                finished = true;
                //    yield break;
            }


            //Prepare video
            if(!stimuli[videoIndex].vp.isPrepared)
            { 
                stimuli[videoIndex].vp.Prepare();
            }

            //Wait until this video is prepared
            while (!stimuli[videoIndex].vp.isPrepared)
            {
                Debug.Log("Preparing Index: " + videoIndex);
                yield return null;
            }

            Debug.LogWarning("Done Preparing current Video Index: " + videoIndex);

            
            // LOG
            if (videoIndex - 1 >= 0 && stimuli[videoIndex - 1].stimulusType == StimulusType.IMAGE)
            {
                player_logWriter.WriteLine(getCurrentTime() + "\t" + "Stop\t" + stimuli[videoIndex - 1].path + "\t\n");

            }

            player_logWriter.WriteLine(getCurrentTime() + "\t" + "Start\t" + stimuli[videoIndex].path + "\t\n");


            // Depending on stimulus, change RenderSkybox to 3D or 2D Material
            if (stimuli[videoIndex].dimension == Dimension.THREE_D)
            {
                skyboxThreeDmat.mainTexture = Resources.Load<RenderTexture>("RenderTextures/rt_4K3D");
                RenderSettings.skybox = skyboxThreeDmat;
                DynamicGI.UpdateEnvironment();

            }

            if (stimuli[videoIndex].dimension == Dimension.TWO_D)
            {

                skyboxTwoDmat.mainTexture = Resources.Load<RenderTexture>("RenderTextures/rt_4K2D");

                RenderSettings.skybox = skyboxTwoDmat;
                DynamicGI.UpdateEnvironment();
            }

            if (stimuli[videoIndex].dimension == Dimension.QUAD)
            {
                skyboxQuad.mainTexture = Resources.Load<RenderTexture>("RenderTextures/rt_quad");
                RenderSettings.skybox = skyboxQuad;
                DynamicGI.UpdateEnvironment();
            }


            Quaternion newQuaternion = new Quaternion();
            newQuaternion.Set(stimuli[videoIndex].initialCameraX, stimuli[videoIndex].initialCameraY, stimuli[videoIndex].initialCameraZ, stimuli[videoIndex].initialCameraW);

            Camera.main.transform.rotation = newQuaternion;
            et_cam.transform.rotation = newQuaternion;
            vr_cam.transform.rotation = newQuaternion;

             //Play first video
             stimuli[videoIndex].vp.Play();

            //Wait while the current video is playing
            int nextIndex = (videoIndex + 1);
            while (stimuli[videoIndex].vp.isPlaying)
            {
                Debug.Log("Playing time: " + stimuli[videoIndex].vp.time + " INDEX: " + videoIndex);

                //Make sure that the NEXT VideoPlayer index is valid. Otherwise Exit since this is the end
                if (nextIndex >= stimuli.Count)
                {
                    Debug.LogWarning("End of All Videos: " + videoIndex);
                    //yield break;
                    finished = true;
                }

                //Prepare the NEXT video
                Debug.LogWarning("Ready to Prepare NEXT Video Index: " + nextIndex);
                if (nextIndex < stimuli.Count && stimuli[nextIndex].stimulusType == StimulusType.VIDEO)
                {
                    stimuli[nextIndex].vp.Prepare();
                }
                yield return null;

            }



            if (finished)
            {
                ShowFinalImage();
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

        if (finished)
        {
            ShowFinalImage();
            yield break;
        }

        //Play next prepared video. Pass false to it so that some codes are not executed at-all
        StartCoroutine(playStimuliList());

    }


    //Adds CanvasScaler component to the Canvas GameObject 
    void addCanvasScaler(int width, int height)
    {

        cvsl.referenceResolution = new Vector2(width, height);

    }

    IEnumerator waitForKeyPress(KeyCode key)
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

    void SetEnvironmentBlack()
    {
        RenderSettings.skybox = skyboxBlack;
        DynamicGI.UpdateEnvironment();
    }

    void ShowFinalImage()
    {

        Quaternion newQuaternion = new Quaternion();
        newQuaternion.Set(0,0,0,0);

        Camera.main.transform.rotation = newQuaternion;
        et_cam.transform.rotation = newQuaternion;
        vr_cam.transform.rotation = newQuaternion;

        SetEnvironmentBlack();

        //imageCube.transform.localToWorldMatrix ??
        imageCube.transform.localScale = new Vector3(1.5f, 1.5f, 0.133f);
        //Sprite sprite;
        Texture2D spriteTexture = Resources.Load<Texture2D>("Images/ende");

        imageCube.GetComponent<MeshRenderer>().enabled = true;
        imageCube.GetComponent<MeshRenderer>().material.mainTexture = spriteTexture;

    }
}

   