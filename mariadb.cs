
// CREATE TABLE `t_monitor` (
	// `Zeitpunkt` DATETIME NULL DEFAULT NULL,
	// `Computername` VARCHAR(20) NULL DEFAULT NULL COLLATE 'utf8mb4_general_ci',
	// `Tctl` SMALLINT(6) NULL DEFAULT NULL,
	// `Tdie` SMALLINT(6) NULL DEFAULT NULL,
	// `Leistung` DOUBLE NULL DEFAULT NULL,
	// `Auslastung` DOUBLE NULL DEFAULT NULL,
	// `Aggr` DOUBLE NULL DEFAULT NULL,
	// `AggrEff` DOUBLE NULL DEFAULT NULL,
	// `Bemerkung` VARCHAR(100) NULL DEFAULT NULL COLLATE 'utf8mb4_general_ci'
// )
// COMMENT='Speichert Werte von MyHwMon1'
// COLLATE='utf8mb4_general_ci'
// ENGINE=InnoDB
// ;

// KI-generierter und erweiterter Code für Schreiben in MariaDB:


using System;
using MySqlConnector;

namespace MyHwMon1
{
    public partial class MainWindow // die Klasse MainWindow ist in MainWindow.xaml.cs definiert
    {
        private void WriteDb( double dAuslastung, double dLeistung, double dAggr, double dAggrEff, int iTctl, int iTdie)
        {
            // Verbindungsdaten definieren, dass Passwort darf natürlich nicht lesbar im Quelltext stehen:
            string connString = "Server=255.255.255.255;Port=3306;Database=mydatabase;User ID=user;Password=pwd;";

            using (var connection = new MySqlConnection(connString))
            {
                try
                {
                    connection.Open();// Verbindung öffnen

                    string insertSql = @"INSERT INTO t_monitor (Zeitpunkt, Computername, Tctl, Tdie, Auslastung, Leistung, Aggr, AggrEff, Bemerkung) 
                        VALUES (SYSDATE(), @PC, @Tctl, @Tdie, @Auslastung, @Leistung, @Aggr, @AggrEff, @Bemerkung)";

                    using (var insertCmd = new MySqlCommand(insertSql, connection))
                    {
                        // Parameter verwenden, um SQL-Injection zu verhindern
                        // --> die Werte werden nicht direkt ins insert-Statementgeschrieben,
                        // sondern einzeln "gebunden":

                        string sPc = Environment.MachineName;
                        insertCmd.Parameters.AddWithValue("@PC", sPc.Substring(0,Math.Min(20,sPc.Length)));
                        insertCmd.Parameters.AddWithValue("@Tctl", iTctl);
                        insertCmd.Parameters.AddWithValue("@Tdie", iTdie);
                        insertCmd.Parameters.AddWithValue("@Auslastung", dAuslastung);
                        insertCmd.Parameters.AddWithValue("@Leistung", dLeistung);
                        insertCmd.Parameters.AddWithValue("@Aggr", dAggr);
                        insertCmd.Parameters.AddWithValue("@AggrEff", dAggrEff);

                        insertCmd.Parameters.AddWithValue("@Bemerkung", "");

                        //Alternative zu SYSDATE():
                        //insertCmd.Parameters.AddWithValue("@datum", DateTime.Now);

                        int rowsAffected = insertCmd.ExecuteNonQuery();
                        Console.WriteLine($"{rowsAffected} Zeile(n) eingefügt.");
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Fehler: " + ex.Message);
                }
                // 2. Verbindung schließen (erfolgt automatisch durch das 'using'-Statement)
            }

        }
    }
}