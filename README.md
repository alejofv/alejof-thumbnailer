# alejof-thumbnailer
Azure function to generate thumbnails for uploaded images.

_Note: this app is intended to be used along the [alejof-notes-api](https://github.com/alexphi/alejof-notes-api), since it expects a certain path pattern for the uploaded blobs._

# Functions

There are actually two functions in this Azure Functions app, one that generates "smart" thumbnails for images, using the  [Computer Vision API][vision-api], an another one that scales down images if they are too big (because the computer vision api fails with big images).

The functions are triggered by queue messages containing the full path of an image blob in Blob Storage.

[vision-api]:https://azure.microsoft.com/en-us/services/cognitive-services/computer-vision/
