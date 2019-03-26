# [Was ist der Thinktecture RelayServer?](1-was-ist-der-thinktecture-relayserver.md)
# [Architektur des RelayServers](2-architektur.md)
# [Installation des RelayServers](3-installation.md)
# [RelayServer Management Web](4-relayserver-management-web.md)
# [RelayServer Interceptor Entwicklung](5-relayserver-interceptor-entwicklung.md)
# [Entwicklungs-Setup](6-entwicklungssetup.md)

# Release Notes

## Version 2.1.1

* Fehlerbehebungen

  * Der automatische Disconnect stand in individuellen OPC-Implementationen nicht korrekt zur Verfügung.

## Version 2.1.0

* Server-Seitige Link-Konfiguration

  * Es ist nun möglich, Link-Spezifische Settings auf dem RelayServer zu konfigurieren.

* Automatischer Disconnect von On-Premise Connectoren

  * Wenn gewünscht, kann ein On-Premise Connector von sich aus die Verbindung wieder abbrechen, wenn er eine maximale Verbindungszeit erreicht und/oder eine gewisse Zeit nicht verwendet wird.

* Allgemeine Verbesserungen

  * Die Zeitspanne in der sich On-Premise Connectoren nach einem Verbindungsverlust neu verbinden können ist nun konfigurierbar. Damit kann vermieden werden, dass z.B. bei einem Neustart des Servers alle On-Premise Connectoren innerhalb des gleichen kurzen Zeitfensters erneut verbinden wollen und damit versehentlich eine DDoS-Erkennung auslösen.
  * Interceptoren haben nun Zugriff auf die lokale Uri, die vom Client angefragt wurde, z.B. um Forwarded-Header zu setzen.
  * Es ist nun möglich, zu konfigurieren ob der On-Premise Connector redirects eines On-Premise Targets selber folgt oder diese relayed.
  * Es kann nun ein eigener `IPasswordComplexityValidator` implementiert und über ein eigenes Autofac-Modul aus einem eigenen CustomCodeAssembly registriert werden.

* Fehlerbehebungen

  * Wenn eine weiterzuleitende Anfrage einen Query-Parameter namens 'path' enthielt, führte das zu unerwartetem Verhalten
  * Die konfigurierbare Filterung des Inhaltes von OnPremise-Seitigen Fehler-Antworten wurde korrigiert
  * Eine genauere Fehlermeldung wird angezeigt, wenn die Konfigurationsdatei des RelayServers fehlt

## Version 2.0.0

* Multi-Server Betrieb

  * Es ist nun möglich, mehrere RelayServer zur Lastverteilung parallel zu betreiben. Die Server müssen hierzu Zugriff auf einen gemeinsamen Netzwerk-Ordner haben, in dem zu übertragende Daten zwischen den Servern ausgetauscht werden.

* Verbesserte Verbindungsstabilität mit neueren OnPremise Connectoren

  * Der RelayServer wird einen OnPremiseConnector mit Version 2.x oder neuer nun regelmässig mit einem Heartbeat anfragen. Bleibt dieser Heartbeat aus, so wird der OnPremiseConnector versuchen die Verbindung zum RelayServer neu aufzubauen.

* Möglichkeit, eigenen Code ausführen zu lassen

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

  * Das Logging wurde von NLog auf Serilog umgestellt, und die Logausgaben mit strukturierten Informationen angereichert. Es können eigene Serilog-Sinks und Enricher eingefügt werden. Zur Konfiguration siehe [Serilog AppSettings Konfiguration](https://github.com/serilog/serilog/wiki/AppSettings).

* Optimierungen

  * Der Speicherverbrauch des RelayServers und der On-Premise Connectoren wurde reduziert. Zudem wurden viele Performance-Optimierungen vorgenommen, um das System effizienter zu machen.

* Verbesserungen der Sicherheit

  * Es ist nun möglich, alle Funktionen des RelayServers (Relaying, On-Premise Verbindungen, ManagementWeb) einzeln zu deaktivieren oder für rein lokalen oder globalen Zugriff zu aktivieren.
  * Alle Dashboard & Info-Endpunkte erfordern nun Authorisierung
  * Fehlermeldungen enthalten keine Stacktraces mehr
  * Passwortänderungen für Management-Web Benutzer erfordert jetzt die Eingabe des alten Passworts
  * Regeln für bessere Passwortsicherheit eingeführt
  * Zu viele Fehlversuche beim Login sperren einen User temporär aus
  * Strict Transport Security Header werden nun gesetzt
  * X-Frame-Options Header werden nun gesetzt
  * X-XSS-Protection Header werden nun gesetzt
  * Es ist nun möglich, den Relaying-Endpunkt auf Authentifizierte Requests einzuschränken

* Allgemeine Verbesserungen

  * Info- und Management Endpunkte setzten nun korrekte Cache-Header
  * Update verwendeter Bibliotheken auf die jeweils neueste Version

* Fehlerbehebungen

  * Einige Funktionen im ManagementWeb wie z.B. das Bearbeiten von Nutzern funktionieren jetzt zuverlässiger

## Version 1.0.4

Initial release.
