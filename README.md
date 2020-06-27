# SeriesRenamer
Consistently renames video files of a series and adds the episode title.
 
 ![Screenshot](/DemoPics/01.png "Screenshot")
 
## English? German?
This software was made for German use only. It relies on the German Wikipedia, using German keywords. Thus, it is not applicable for other languages. However, the software can apply English titles by setting the launch argument `-l en`.


## About
*SeriesRenamer* is a console application which renames your downloaded series consistently.
Season and episode of your file is exatracted using the filename, and then, the corresponsing series title is downloaded from Wikipedia.
Finally, a given rename scheme is applied.

## Download
[Series Renamer V1](https://github.com/Tom852/SeriesRenamer/releases/download/v1/publish.rar)

## How to use
Just launch `SeriesRenamer.exe`.

## Launch Args
You can start the program without any arguments. It will then ask you for all required information. Some variables, especially the Wikipedia URL are even deduced automagically. However, to save time, one can provide launch arguments:

* -f Path on the filesystem, containing the video files of your series.
* -n Name of the series
* -u German Wikipedia URL with the episode list
* -l Desired language, German (de) or English (en)

Example: `-f "F:\New Files\The Blacklist" -n "The Blacklist" -u "https://de.wikipedia.org/wiki/The_Blacklist/Episodenliste" -l "de"`

If the language argument is not provided, German will be assumed to be the desired language without any inquiry.

## badWords.txt
In the program's folder a file `badWords.txt` is located. Many downloaded files contain parts like *720p*. To prevent them from being identified as season 7, episode 20, such terms can be excluded within the file *badWords.txt*.

## Bugs, Features
Feel free to request any bugs and feature requests.
