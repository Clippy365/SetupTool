[![Screenshot](https://i.postimg.cc/x1ys0cqS/Unbenannt.png)](https://postimg.cc/K1j7fcn9)

This tool is used to set up your Windows 10 installation. By clicking the checkboxes on the left hand side, you can choose which software it will install. You can also edit this list and add new packages as long as they are in the [chocolatey repository](https://chocolatey.org/packages). In the background it will use the chocolatey package manager for installing software. You can also change some important Windows settings with it by checking the checkboxes on the right hand side. It can for example disable windows telemetry, peer-to-peer updates or remove useless pre-installed apps like candy crush or groove music.

## Build
After you "git cloned" it or just downloaded the zip file (which is more common since you won't have git installed on a fresh Windows 10), all you have to do is unpack the zip file and run the Build.cmd script. It will translate the source code and create the "SetupTool.exe" file (Windows 10 ships with a C# compiler). To move this tool to another location, you also need to copy Newtonsoft.Json.dll, applicationList.json and the RegFiles folder. To run it, you need admin privileges.

## Example application list
By default there is an example applicationList.json with some common software packages listed. You can edit it to fit your needs, or just delete it and create your own one from within the SetupTool GUI.


## Donation
If you like this software tool, you can consider a donation via PayPal. Here's the link:

[![](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://www.paypal.com/donate?hosted_button_id=JLM9EX9K6E7YN)
