![Logo of the project](https://raw.githubusercontent.com/AkseliKoponen/DevAcademy/main/Logo.png)

# City Bike Data 2021
> An exercise for Solita DevAcademy by Akseli Koponen

A Blazor web app for displaying HSL City Bike Data from the summer of 2021.
Created with Visual Studio using Blazor Template.

## Installing / Getting started

Go to https://devacademyakselikoponen.azurewebsites.net/ to view the app in Azure.

The app may run slowly on cloud but you can also open the project in visual studio by following these steps.
>1. Download the database from https://drive.google.com/file/d/1qTyV0VRHPn6THy8NyXUy_2p3g7iRxaSH/view?usp=sharing
>2. Move the database to the project folder (DevAcademy/Blazor/)


## Features

* Filter and search through the bike trips and stations.
* View additional information about specific stations
* Add new trips and stations to the database
* Tooltips
* Automatic tools to get distance and duration when adding new trips.

## Notes
biketrips.db SQLite database was created by combining the .csv files into one and then importing them as a table to the database.
The original .csv files contained duplicates of EVERY trip. The duplicates were deleted from the database file.

##Links
You may contact me at akseli.koponen42@gmail.com. I am willing to create more code in exchange for food or even money.

## Contributing and Developing

This software will not be developed further. If it pleases you, go ahead and copy the repository and go crazy.

## Licensing

This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org/>