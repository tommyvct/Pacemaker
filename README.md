# Pacemaker
 
A stealth C# program to re-enable malfunctioned device and change screen backlit PWM frequency using Intel driver interface.

## Usage

On first run, the program will create a config file `conf.txt` under `%localappdata%/Pacemaker`, and open the default text editor for editing.

The file contains examples of usage:
```
####### PWM frequency Configuration #########
# change 1200 to desired frequency
#PWM 1200
#######        Device Restart       #########
# format: {GUID} instanceID
# Example: {4d36e972-e325-11ce-bfc1-08002be10318} PCI\VEN_168C&DEV_0042&SUBSYS_403517AA&REV_30\4&1D6086F3&0&00E0
```

Both the GUID and instance ID can be found in Device Manger. 
Double click on the suspect device, select Details tab:
In the Property dropdown menu, use `Class GUID` and `Device instance path` as instance ID.
Both GUID and instance ID must be put on the same line, and GUID first.

Under normal circumstances the program will be completely stealth, no console, no dialogues, no toast.

If something went wrong, for example, the GUID and instance ID given in `conf.txt` is incorrect, the program will pop up a dialogue or a toast about it.


### More about `conf.txt`

1. Lines start with `#`, and lines with more or less than 2 strings will be ignored.

2. The `conf.txt` file can be deleted, by passing `NUKE` as parameter:

    ```
    Pacemaker.exe NUKE
    ```
3. It can also use a temporary `conf.txt`:

    ```
    Pacemaker.exe ~\Desktop\temp_conf.txt
    ```


## Build

Targeted to .NET Framework 4.7.2, with `x64` architecture.

Use Visual Studio 2019 or command line build tools to build:
```
msbuild.exe Pacemaker.sln
```

### About `igfxDHLib.dll`

It can be found somewhere under `%systemroot%/System32/DriverStore/FileRepository/`. If not found there is a copy of it in the project.

The latest driver is based on [DCH Driver Framework](https://www.intel.ca/content/www/ca/en/support/articles/000031275/graphics.html), and `igfxDHLib.dll` is no longer a part of new driver. 

The currect solution still works, but there is no guarantee that it will work on later version of Intel Graphics Driver. 

## Credit
https://stackoverflow.com/a/1610140

https://stackoverflow.com/a/34956412

https://www.codeproject.com/Articles/30031/Query-hardware-device-status-in-C

https://github.com/anatoliis/PWMHelper

And the [Microsoft Docs](https://docs.microsoft.com/)! Though some of the documentation lacks practical example.
