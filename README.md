# SeriesRenamer
Benennt die Videofiles einer Serie einheitlich und fügt den Episodennamen hinzu.
 
 ![Screenshot](/DemoPics/01.png "Screenshot")
 
## About
Der SeriesRenamer ist eine Konsolenapplikation, die deine Serien einheitlich benennt.
Insbesondere wird der Episodentitel von jeder Episode ermittelt und dem Dateinamen hinzugefügt.

## Download
[Series Renamer V1](https://github.com/Tom852/SeriesRenamer/releases/download/v1/publish.rar)

## Launch Args
Das Programm kann einfach gestartet werden. Alles, was es wissen muss, wird per Konsole nachgefragt. Es wird insbeondere die Wiki URL automatisch ermittelt, sofern dies möglich ist. Um aber effizienter zu arbeiten, könnt ihr die folgenden Arbeitsvariablen auch per Argumente setzen:

* -f Pfad auf dem Dateisystem
* -n Name der Serie
* -u Wiki URL der Episodenliste
* -l Gewünschte Sprache, deutsch oder original

Example: `-f "F:\New Files\The Blacklist" -n "The Blacklist" -u "https://de.wikipedia.org/wiki/The_Blacklist/Episodenliste" -l "de"`

Falls ihr eure Serie englisch benennen wollt, ist dies explizit per Konsolenargument zu setzen. Das Programm fragt nicht explizit nach der Sprache.

## badWords.txt
Viele Downloads enthalten Terme wie *720p*. Damit dies nicht als Season 7, Episode 20 erkannt wird, können solche Terme im File *badWords.txt* angegeben werden.

## Bugs, Features
Bugs und Feature Requests dürft ihr mir gerne mitteilen ;)


## English Version?
This software was made for German use only. It relies on German Wikis with German identifiers and such, is thus not applicable for other languages. However, it can apply English titles, e.g. if a series does not contain German titles.
