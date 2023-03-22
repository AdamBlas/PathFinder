using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotTaker : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
	    if (Input.GetKeyDown(KeyCode.Space))
	    {
	    	string filename = "Screenshots/" + System.DateTime.Now.ToString("yyyy-MM-dd-hh_mm-ss") + ".png";
	    	if (System.IO.Directory.Exists("Screenshots/") == false)
	    		System.IO.Directory.CreateDirectory("Screenshots/");
	    	ScreenCapture.CaptureScreenshot(filename);
	    	Debug.Log("Screenshot saved at " + filename);
	    }
    }
}
