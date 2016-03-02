# Komponenten des RelayServers

## Überblick
![2-architektur-ueberblick.png](./assets/2-architektur-ueberblick.png)


## RelayServer aus Sicht des Clients

1. Client kommuniziert mit dem RelayServer über API-Aufrufe durch Web Requests. Diese sind die anwendungsspezifischen APIs der Ziel-On-Premises Applikation.
1. Client findet den RelayServer durch eine fest konfigurierte URL und eine DNS-Abfrage (z.B. https://relay.company.example).
1. Client baut bei Bedarf eine Verbindung zum RelayServer auf und schließt die Verbindung nach Erhalt der Antwort wieder.
1. Es ist keine dauerhafte Verbindung zwischen Client und RelayServer notwendig (Request/Response Pattern).
1. Es werden keine besonderen Protokolle genutzt; ausschließlich HTTPS.
1. Es sind keine speziellen Libraries für den Client notwendig, d.h. alle erdenklichen Client-Plattformen werden unterstützt.

## RelayServer aus Sicht der On-Premises Applikation

1. Der On-Premises Connector baut beim Start eine Verbindung zum konfigurierten RelayServer auf (z.B. [https://relay.company.example](https://relay.company.example)).
1. Die dafür notwendige Verbindung ist eine ausschließlich ausgehende Verbindung, so dass in der Regel keine Änderungen an Firewalls, Routern oder NAT-Systemen notwendig sind.
1. Beim Verbindungsaufbau muss sich der On-Premises Connector beim RelayServer authentifizieren.
1. Nach erfolgter Authentifizierung gilt der On-Premises Connector als verbunden jedoch nicht automatisch auch als aktiv und damit für Clients erreichbar. Diese Entscheidung trifft der RelayServer anhand seiner Konfiguration. Diese Funktionalität ist die Grundlage für ein verlässliches Provisioning.
1. Der On-Premises Connector hält über verschiedene Ansätze die etablierte Verbindung dauerhaft offen und wartet auf mögliche Anfragen von Clients, die ihm der RelayServer weiterleitet.
1. Eingehende Anfragen von Clients werden vom On-Premises Connector entgegengenommen und der dahinter liegenden On-Premises Applikation als Web Request weitergeleitet. Aus Sicht der On-Premises Applikation besteht kein Unterschied zu einem „normalen" Client-Request.
1. Die Antwort auf die Client-Anfrage liefert die On-Premises Applikation daher an den On-Premises Connector zurück, der die Antwort wiederum über den RelayServer an den Client weiterreicht.

# Technische Details

## Verbindungsaufbau vom On-Premises Connector zum RelayServer

Beim Start des On-Premises Connectors wird eine Verbindung zum in der Konfigurationsdatei hinterlegten RelayServer aufgebaut. Da es sich dabei um eine ausgehende HTTPS-Verbindung bzw. Secure Websocket-Verbindung handelt, sind in Firewalls in den allermeisten Fällen daher keine besonderen oder kritischen Regeln für den On-Premises Connector zu hinterlegen.

Bei erfolgreicher Verbindung zum RelayServer muss sich der On-Premises Connector zunächst mit einem Key authentifizieren. Dieser Key wird out-of-band bspw. über sichere EMail kommuniziert. Nach erfolgreicher Authentifizierung wird zwischen RelayServer und On-Premises Connector eine Real-Time-Verbindung aufgebaut. Ist der On-Premises Connector laut Konfiguration in der Datenbank für die Kommunikation mit Clients freigeschaltet, so vermerkt der RelayServer in seiner Datenbank den Status „aktiv" für den verbundenen On-Premises Connector. Nachrichten von Clients für diesen On-Premises Connector werden ab jetzt weitergeleitet.

Für die Real-Time-Verbindung kommt SignalR als Verbindungstechnologie zum Einsatz. Sie sorgt auch dafür, dass die etablierte Verbindung zwischen RelayServer und On-Premises Connector dauerhaft geöffnet bleibt. Bei einer Unterbrechung der Verbindung wird durch die Funktionalität von SignalR darüber hinaus automatisch regelmäßig ein Verbindungsneuaufbau versucht, bis wieder eine stabile Verbindung besteht.

## Request Lifecycle
![2-request-lifecycle.png](./assets/2-request-lifecycle.png)

1. Der Client sendet einen API-Request via HTTPS zum RelayServer, der dort vom Request Handler empfangen wird
1. Der Request Handler erzeugt eine Nachricht, welche in einer zur Nachricht und damit zum Mandanten passenden Message Queue abgelegt wird. Gleichzeitig wird ein Thread erzeugt, der auf die zur Nachricht passende Antwort der On-Premises Applikation gehört.
1. Es wird ein dedizierter Real-Time Socket für die Nachricht erzeugt, der die Nachricht aus der Message Queue entgegen nimmt.
1. Wenn die Nachricht sehr groß ist bzw. größere Anhänge enthält, dann wird dieser Teil der Nachricht aus Gründen der Skalierbarkeit in einem temporären Speicher abgelegt.
1. Der Real-Time Socket sendet die Nachricht über die bestehende Websocket-Verbindung an den On-Premises Connector. Die Nachricht enthält dabei den kompletten http-Request des Clients.
1. Wenn es sich um eine Nachricht handelt, die sehr groß ist oder Anhänge enthält, so sendet der RelayServer dem On-Premises Connector die Nachricht mit der zusätzlichen Anweisung, über einen separaten Request den Body der Nachricht oder die Anhänge vom RelayServer herunterzuladen. Der On-Premises Connector kontaktiert dann zum Herunterladen des Anhangs den Request Handler des RelayServers.
1. Der Request Handler lädt den gewünschten Anhang aus dem temporären Speicher.
1. Die Daten aus dem temporären Speicher werden als GET-Operation vom On-Premises Connector empfangen. Nach dem erfolgreichen Download setzt der On-Premises Connector beide Bestandteile wieder zur ursprünglichen Nachricht zusammen.
1. Der On-Premises Connector sendet die empfangene (und ggf. zusammengefügte) Nachricht als http-Request an die eigentliche On-Premises Applikation.
1. Nachdem die On-Premises Applikation die Nachricht beantwortet hat, sendet der On-Premises Connector das Ergebnis an den Request Handler im RelayServer.
1. Der Request Handler des RelayServer legt die Antwort in seine Message Queue.
1. Da es durch Schritt 2 einen passenden Thread gibt, der auf die Antwort wartet, kann dieser Thread die Antwort entgegennehmen und als fertige Antwort dem Request Handler übergeben. Der Request Handler sendet die Antwort als letzten Schritt an den wartenden Client und beendet dadurch den wartenden Client Request erfolgreich.
