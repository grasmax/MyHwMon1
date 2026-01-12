Eine winui3/c#-Anwendung, gebaut mit Visual Studio 2026 Community.
Die Anwendung wurde für einen Ryzen R7 7800X3D entwickelt und nur auf dieser CPU und einer Intel I7 7500U getestet.

Sie kann genutzt werden, um CPU-Daten (Temperatur, Auslastung,..., für den I7 nur die Auslastung) auszulesen, anzuzeigen und in eine MariaDB-Datenbanktabelle zu speichern.

Die Anwendung liest Daten mit https://www.nuget.org/packages/OpenHardwareMonitorLib / https://github.com/HardwareMonitor/OpenHardwareMonitor.
Achtung! Bitte die Sicherheitshinweise für diese Bibliothek beachten!

https://www.nuget.org/packages/MySqlConnector wird zum Schreiben in die MariaDB-Datenbank benutzt. Es wird vorausgesetzt, dass MariaDB installiert und eine Datenbank angelegt ist.
Vor Inbetriebnahme bitte 
* das create-Statement in mariadb.cs benutzen, um eine Tabelle in der MariaDB anzulegen und 
* die Verbindungsdaten in string connString = "" eintragen.
