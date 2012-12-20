Updater
=======

I have deleted old repo since I rebuilt the application.

Updater is a simple updater fo w Windows based programs. It allows easy update propagation and has very small requirements (only .NET framework required).

WEBSERVER
=========
To use Updater you have to create update directory on your webserver, containinf .fupd files and app directories.
Directory structure should be as follows.
<UpdateDirecotry>
---App1.fupd
---App2.fupd
---<App1>
------File1.exe
---<App2>
------File3.exe
------File4.dll

.fupd is an update information file. It has following syntax 
VERSION;FILE1~FILE2~FILEX

So, let's take App2 as an example, App2.fupd should look like that
1440;File3.exe~File4.dll

LAUNCHING THE UPDATER
=====================

To use the updater you have launch it from app direcotry and provide it with several command line switches.
-v <version>        - Current application version

-rs <path>          - Restart path (this app will be launched after update)

-an <name>          - Short application name, used for finding update on web server.

-al <longname>          - Long application name, for display purposes

-s <address>        - Update server URL


All switches are required. If Updater finds newer version on server than <version> provider it will download files designated in <name>.fupd file and update old ones.

