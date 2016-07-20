using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Datenbankverbindung
            string myConnectionString = "SERVER=192.168.10.181;" +
                                        "PORT=3306;" +
                                        "DATABASE=namen;" +
                                        "UID=user;" +
                                        "PASSWORD=user;";

            MySqlConnection connection = new MySqlConnection(myConnectionString);
            MySqlCommand command = connection.CreateCommand();

            // Initialisierung
            int anzahl = 1;
            int anzahlerfolg = 0;
            Random Rnd = new Random();
            int counter = 1;
            int safetycounter = 0;

            List<string> nameganzlist = new List<string>();
            List<string> nameganzlisttemp = new List<string>();
            List<string> vornamenlist = new List<string>();
            List<string> nachnamenlist = new List<string>();

            //Testloop
            while (anzahl != 0)
            { 
            
                //Aufforderung Anzahl Namen
                Console.WriteLine("Bitte Anzahl zu generierenden Namen eingeben:");

                //Prüfung ob eine Zahl eingegeben wurde
                if (int.TryParse(Console.ReadLine(), out anzahl))
                {
                    
                    connection.Open();

                    while (counter <= anzahl && safetycounter < 100)
                    {
                        //Initialisierung


                        command.CommandText = "SELECT COUNT(*) FROM namen.vornamen";
                        long anzvornamen = (long)command.ExecuteScalar() + 1;
                        int vornamenrnd = Rnd.Next(1, (int)anzvornamen); 

                        command.CommandText = "SELECT COUNT(*) FROM namen.nachnamen";
                        long anznachnamen = (long)command.ExecuteScalar() + 1;
                        int nachnamenrnd = Rnd.Next(1, (int)anznachnamen);

                        string nachnamen = "";
                        string vornamen = "";
                        string nameganz = "";

                        // Zufällig Nachnamen auswählen aus Nachnamen Tabelle und in Variable nachnamen speichern
                        command.CommandText = "SELECT lastname FROM nachnamen WHERE id LIKE '" + nachnamenrnd + "'";
                        nachnamen = (String)command.ExecuteScalar();

                        // Zufällig Voramen auswählen aus Vornamen Tabelle und in Variable vornamen speichern
                        command.CommandText = "SELECT firstname FROM vornamen WHERE id LIKE '" + vornamenrnd + "'";
                        vornamen = (String) command.ExecuteScalar();

                        // Vorname und Nachname in Variable nameganz kombinieren
                        nameganz = vornamen + nachnamen;

                        // Die Kombination der Liste nameganzlisttemp hinzufügen
                        nameganzlisttemp.Add(nameganz);

                        // Prüfen ob die Kombination bereits vorhanden ist
                        if (nameganzlisttemp.Distinct().Count() != nameganzlisttemp.Count())
                        {
                            // Abbruch: Falls die Kombination schon vorhanden ist mache nichts und generiere einen neuen Namen

                            // nameganzlisttemp duplikat entfernen
                            nameganzlisttemp.Remove(nameganzlisttemp.Last());
                            safetycounter++;
                        }
                        else
                        {
                            // Erfolg: Falls der eintrag noch nicht vorhanden ist wird der vorname und nachname in die jeweilige Liste gespeichert 
                            //         und der Counter um 1 erhöht
                            vornamenlist.Add(vornamen);
                            nachnamenlist.Add(nachnamen);
                            counter++;
                        }

                    }

                    // Tabelle Ergebnis löschen falls sie bereits existiert
                    command.CommandText = "DROP TABLE IF EXISTS namen.ergebnis";
                    command.ExecuteNonQuery();
                    
                    // Tabelle Ergebnis erstellen
                    command.CommandText = "CREATE TABLE `namen`.`ergebnis` (`id` INT NOT NULL AUTO_INCREMENT,`nachnamen` VARCHAR(100) NULL,`vornamen` VARCHAR(100) NULL,PRIMARY KEY (`id`));";
                    command.ExecuteNonQuery();

                    // Nachdem alle Zufalls Namen generiert wurden werden diese nun abgearbeitet und in die Tabelle ergebnis geschrieben
                    for (int i = 0; i < vornamenlist.Count; i++)
                    {
                        command.CommandText = "INSERT ergebnis (vornamen, nachnamen) VALUES ('" + vornamenlist[i] + "','" + nachnamenlist[i] + "')";
                        command.ExecuteNonQuery();
                    }

                    // While Schleife verlasssen und Programm abschliessen
                    anzahlerfolg = anzahl;
                    anzahl = 0;

                    connection.Close();
                }
                else
                {
                    //Error ausgeben, dass keine Zahl eingegeben wurde
                    Console.WriteLine("Bitte eine Zahl eingeben!!");
                    anzahl = 1;
                }
            }

            if (safetycounter >= 100)
            {
                Console.WriteLine("Leider konnten nicht genügend unterschiedliche Namen generiert werden.");
                Console.WriteLine("Das Programm wurde sicherheitshalber abgebrochen.");
                Console.WriteLine("Bitte fügen Sie zusätzliche Einträge in die Beispielnamen Tabellen ein.");
                Console.ReadLine();
            }
            else
            {
                // Abschliessende Ausgabe
                Console.WriteLine("Es wurden " + anzahlerfolg + " Datensätze zur Tabelle Ergebnis hinzugefügt.");
                Console.WriteLine("Besten Dank für die Nutzung unseres Service.");
                Console.ReadLine();
            }

        }
    }
}
