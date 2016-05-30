# Weight Lifting Bar Detection using Kinect
Detect a weight-lifting bar using Kinect for Windows version 2.

![Kinect Weight Lifting Bar Detection - Vangos Pterneas](http://pterneas.com/wp-content/uploads/2016/05/kinect-weight-lifting-vangos.jpg)

## Video
[Watch on YouTube](https://youtu.be/R0LygrFHoEE)

## Prerequisites
* [Kinect for XBOX v2 sensor](http://amzn.to/1AvdswC) with an [adapter](http://amzn.to/1wPJG55) (or [Kinect for Windows v2 sensor](http://amzn.to/1DQtBSV))
* [Kinect for Windows v2 SDK](http://www.microsoft.com/en-us/download/details.aspx?id=44561)
* Windows 8.1 or higher
* Visual Studio 2013 or higher
* A dedicated USB 3 port

## Tutorial
Read the tutorial on [Vangos Pterneas Blog](http://pterneas.com/2016/05/17/weight-lifting-bar-kinect/)

## How to Use

    // 1) Initialization
    var barDetectionEngine = new BarDetectionEngine(
                                 sensor.CoordinateMapper,
                                 colorWidth,
                                 colorHeight,
                                 depthWidth,
                                 depthHeight);
    
    barDetectionEngine.BarDetected += BarDetectionEngine_BarDetected;
    
    // 2) Update
    barDetectionEngine.Update(depthData, bodyIndexData, body);
    
    // 3) Event handling
    private void BarDetectionEngine_BarDetected(object sender, BarDetectionResult e)
    {
        if (e != null)
        {
            var center = e.Trail;
            var height = e.BarHeight;
            var length = e.barLength;
            var left = e.Minimum;
            var right = e.maximum;
        }
    }

## Contributors
* [Vangos Pterneas](http://pterneas.com) from [LightBuzz](http://lightbuzz.com)

## License
You are free to use these libraries in personal and commercial projects by attributing the original creator of the project. [View full License](https://github.com/LightBuzz/kinect-weight-lifting-bar/blob/master/LICENSE).
