# alejof-thumbnailer

Azure Functions App to generate "smart" thumbnails for uploaded images.

# Functions

There are two functions in this Azure Functions app, one that generates "smart" thumbnails for images, using the  [Computer Vision API][vision-api], an another one that scales down images if they are too big (because the computer vision api fails with big images).

The functions are triggered by queue messages containing the full path of an image blob in Blob Storage.

[vision-api]:https://azure.microsoft.com/en-us/services/cognitive-services/computer-vision/
