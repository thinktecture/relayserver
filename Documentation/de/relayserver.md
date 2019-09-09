# [Was ist der Thinktecture RelayServer?](1-was-ist-der-thinktecture-relayserver.md)
# [Architektur des RelayServers](2-architektur.md)
# [Installation des RelayServers](3-installation.md)
# [RelayServer Management Web](4-relayserver-management-web.md)
# [RelayServer Interceptor Entwicklung](5-relayserver-interceptor-entwicklung.md)
# [Entwicklungs-Setup](6-entwicklungssetup.md)

# Release Notes

## Version 2.3.0

* Allgemeine Verbesserungen

  * Der RelayServer warnt nun wenn die `SharedSecret` Einstellung fehlt und kann, wenn nicht im Multi-Server Betrieb eingesetzt, einen zufälligen Startwert verwenden.
  * Das EF-Model wurde um genauere Informationen und Indices erweitert.

## Version 2.2.1

* Fehlerbehebungen

  * Der OnPremiseConnector Demo-Service konnte ein Framwork-Assembly unter bestimmten Voraussetzungen nicht korrekt laden.
  * Der OnPremise-Connector wird seinen `HttpClient` mit dem er Antworten an den RelayServer sendet nun erneuern, falls dort Fehler auftreten.

## Version 2.2.0

* RelayServer Windows Docker Container-Unterstützung

  * Es ist nun möglich alle Konfigurationswerte inklusive dem `RelayContext` ConnectionString mittels Umgebunsvariablen zu übersteuern. Der Name der Umgebungsvariablen muss mit dem Präfix `RelayServer__` beginnen und dann mit dem Namen der Einstellung in der Anwendungskonfigurationsdatei enden.

* Customizing

  * Es ist jetzt möglich den `IRequestLogger` im RelayServer mit einer eigenen Implementation zu ersetzen.
  * Es ist nun möglich den `ITraceFileWriter` und `ITraceFileReader` im RelayServer mit einer eigenen Implementation zu ersetzen.

## Version 2.1.2

* Fehlerbehebungen

  * Der automatische Disconnect trennte aktive Verbindungen vor dem Ablaufen der `LinkSlidingConnectionLifetime`.

## Version 2.1.1

* Fehlerbehebungen

  * Der automatische Disconnect stand in individuellen OnPremiseConnector-Implementationen nicht korrekt zur Verfügung.

## Version 2.1.0

* Server-Seitige Link-Konfiguration

  * Link-Spezifische Settings können auf dem RelayServer konfigurieren werden.

* Automatischer Disconnect von On-Premise Connectoren

  * Wenn gewünscht, kann ein On-Premise Connector von sich aus die Verbindung wieder beenden, wenn er eine maximale Verbindungszeit erreicht hat und/oder eine gewisse Zeit nicht verwendet wurde.

* Allgemeine Verbesserungen

  * Die Zeitspanne in der sich On-Premise Connectoren nach einem Verbindungsverlust neu verbinden können ist nun konfigurierbar. Damit kann vermieden werden, dass z.B. bei einem Neustart des Servers alle On-Premise Connectoren innerhalb des gleichen kurzen Zeitfensters erneut verbinden wollen und damit versehentlich eine DDoS-Erkennung auslösen.
  * Interceptoren haben nun Zugriff auf die lokale Uri, die vom Client angefragt wurde, z.B. um Forwarded-Header zu setzen.
  * On-Premise Connector können HTTP-Redirects eines On-Premise Targets selber folgt oder diese relayen.
  * Es kann nun ein eigener `IPasswordComplexityValidator` implementiert und über ein eigenes Autofac-Modul aus einem eigenen CustomCodeAssembly registriert werden.

* Fehlerbehebungen

  * Wenn eine weiterzuleitende Anfrage einen Query-Parameter namens 'path' enthielt, führte das zu unerwartetem Verhalten.
  * Die konfigurierbare Filterung des Inhaltes von OnPremise-seitigen Fehler-Antworten wurde korrigiert.
  * Eine genauere Fehlermeldung wird angezeigt wenn die Konfigurationsdatei des RelayServers fehlt.

## Version 2.0.0

* Multi-Server Betrieb

  * Mehrere RelayServer können zur Lastverteilung parallel betrieben werden. Die Server müssen hierzu Zugriff auf einen gemeinsamen Netzwerk-Ordner haben, in dem zu übertragende Daten zwischen den Servern ausgetauscht werden.

* Verbesserte Verbindungsstabilität mit neueren OnPremiseConnectoren

  * Der RelayServer wird einen OnPremiseConnector mit Version 2.x oder neuer nun regelmässig mit einem Heartbeat anfragen. Bleibt dieser Heartbeat aus, so wird der OnPremiseConnector versuchen die Verbindung zum RelayServer neu aufzubauen.

* Eigenen Code ausführen

  * Es ist nun möglich, im RelayServer sowohl eigene WebAPI Controller einzubinden als auch eingehende Requests sowie ausgehende Responses zu verändern. Für Details siehe [RelayServer Interceptor Entwicklung](5-relayserver-interceptor-entwicklung.md).

    Folgende Möglichkeiten stehen zur Verfügung:

    * Requests

      * Auslesen und Verändern der HTTP-Methode
      * Auslesen und Verändern der URL
      * Auslesen und Verändern der HTTP-Header
      * Sofortiges Ablehnen oder Beantworten der Anfrage
      * Verändern der TTL des Requests in der Message Queue
      * Umstellen des Acknowledge-Modes auf automatische oder manuelle Acknowledgements

    * Responses

      * Auslesen und Verändern des Status Codes
      * Auslesen und Verändern der HTTP-Header
      * Vollständiges Ersetzen der Response

* Verbessertes Logging

  * Das Logging wurde von NLog auf Serilog umgestellt und die Logausgaben mit strukturierten Informationen angereichert. Es können eigene Serilog-Sinks und Enricher eingefügt werden. Zur Konfiguration siehe [Serilog AppSettings Konfiguration](https://github.com/serilog/serilog/wiki/AppSettings).

* Optimierungen

  * Der Speicherverbrauch des RelayServers und der On-Premise Connectoren wurde reduziert. Zudem wurden viele Performance-Optimierungen vorgenommen, um das System effizienter zu machen.

* Verbesserungen der Sicherheit

  * Alle Funktionen des RelayServers (Relaying, On-Premise Verbindungen, ManagementWeb) können einzeln deaktiviert oder für rein lokalen oder globalen Zugriff aktiviert werden.
  * Alle Dashboard- und Info-Endpunkte erfordern eine Authorisierung.
  * Fehlermeldungen enthalten keine Stack-Traces mehr.
  * Passwortänderungen für Management-Web Benutzer erfordert jetzt die Eingabe des bisherigen Passworts.
  * Regeln für bessere Passwortsicherheit eingeführt.
  * Zu viele Fehlversuche beim Login sperren einen User temporär aus.
  * Der "Strict Transport Security" Header wird gesetzt.
  * Der "X-Frame-Options" Header wird gesetzt.
  * Der "X-XSS-Protection" Header wird gesetzt.
  * Der Relaying-Endpunkt kann auf rein authentifizierte Requests eingeschränkt werden.

* Allgemeine Verbesserungen

  * Info- und Management-Endpunkte setzen nun die korrekten Cache-Header.
  * Update verwendeter Bibliotheken auf die jeweils neueste Version.

* Fehlerbehebungen

  * Einige Funktionen im ManagementWeb, wie z.B. das Bearbeiten von Nutzern, funktionieren jetzt zuverlässiger.

## Version 1.0.4

Initiales Release.
