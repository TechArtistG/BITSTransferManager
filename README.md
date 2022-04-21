# BITS Transfer Manager


BITS Transfer Manager is a .Net App + supporting Powershell script to ease the use of BITS file transfers.  BITS is a powerfull tool that supports asynchronous file transfers and is very resilient to transient network errors.  It can even recover from a full restart of windows in the case of power outages etc.  However there's no easy way to create and create and manage BITS jobs, until now!
 
 ### Features
 - Autocomplete asynchronous BITS jobs
 - Pause, Resume and Delete jobs
 - Shell integration to create new jobs by "Pasting" files
 - Handles File paths that contain escape characters
 - Handles full recursive folder transfers


[BITS Information](https://en.wikipedia.org/wiki/Background_Intelligent_Transfer_Service)

## BITS Transfer Manager App
This app can be used to to view and manage all the current BITS jobs that exist on your local computer.  
### Usage
- Expand jobs to show the included files:

 ![ScreenShot](./Images/screenshot_main.png)


- Pause, Resume or Delete jobs by right-clicking on a job title:

 ![ScreenShot](./Images/screenshot_contextmenu.png)

- The app will stay open in the tray when closed to ensure asynchronous jobs are completed.  To close the app right-click on it's tray icon and select exit.
- Drag-and-drop files/folders onto the app window to create new jobs.

## BITS File Paste Script and Shell Integration
This is a Poweshell script that when called will look at the contents of the Windows clipboard looking for file drop objects.  If it finds them and along with a given distination path will create a new BITS job.  As long as the script is left open it will update the job progress and autocomplete.  If the script window is closed the job will continue to copy but will not auto complete unless the BITS Transfer Manager app is running.

![ScreenShot](./Images/screenshot_explorer.png)

![ScreenShot](./Images/screenshot_powershell.png)
