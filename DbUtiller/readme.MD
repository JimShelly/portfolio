# DbUtiller
This application was the solution I came up with to prolong the life of an Access application. 

**Problem**
The client came to me about an MS Access application they have had for over 20 years.  Over that time, the size of the file has grown quite large and have had increased corruption.  The client has been going in every morning and running a Compact/Repair to keep the application running. 

**Solution 1:**

Our first approach was to convert the Access databases to SQL Server and then create linked tables in the application.

This came with many challenges because of the version of Access they were using, how some of the code was written, and communication and datatype errors that were showing up.  

The estimate in time and money were higher than what the client wanted. 

**Solution 2** 

I proposed a C# application that used DAO to communicate with the Access database to stay as native to MS Access as I could.  
I chose C# for the following reasons: 
1.  I wanted a modern language for future maintainability
2.  I wanted this to be a console app so it could be scheduled to run or run manually. 
3.  The client is moving away from Access and didn't want to add further functionality to the Access app itself.  
4.  Developer time was less writing the code in C# than it would have been incorporating it into the existing MS Access application. 

**Final Approach**
I decided on a console application to provide the client with a light-weight solution that they could schedule as a cron job.  

I also included other features like Compact/Repair, Backups, as well as Purging from the main database, archiving the purged records in a duplicate database, and Pruning, where they can delete orphaned, unnecessary records.

All in an attempt to stabilize the Access application. 

**Structure**
- Program (Starting point of all console apps) 
  - Application (Main class that encompasses all features)
  - AccessApp - Class to represent the Access Application
  - AccessDatabase - Class to represent the database(s)
  - Options - Command Line argument management
  - appsettings.json - embedded resource of the database and actions configurations. 
  - AppSettings - class to wrap the appsettings.json data
  - Utils - common static functions and methods

  