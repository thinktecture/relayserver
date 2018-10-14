# Thinktecture RelayServer

[English documentation](./Documentation/en/relayserver.md).

Der Thinktecture RelayServer (nachfolgend kurz RelayServer genannt) ermöglicht als Open-Source-Software eine bidirektionale, sichere Kommunikation von Clients, mobilen Endgeräten und Applikationen mit On-Premises-Applikationen hinter Routern und Firewalls über das HTTPS-Protokoll bei vollständiger zentraler Kontrolle und Auditierung der zulässigen Teilnehmer und der erlaubten Daten.

Ein für Client-Anwendungen und On-Premises-Applikationen gleichermaßen erreichbarer Server bildet einen sicheren Austauschpunkt für Nachrichten und Daten. Da der Server unter der vollen Kontrolle des Betreibers steht und sowohl mit Clients als auch On-Premises-Applikationen durchgängig verschlüsselt kommuniziert, ist die Sicherheit der übertragenen Daten jederzeit gewährleistet.

Die Positionierung des RelayServers im öffentlichen Internet sorgt weiterhin dafür, dass sowohl Clients als auch On-Premises-Applikationen nur ausgehende Verbindungen zur Kommunikation aufbauen müssen. Damit sind Firewalls, Router und NAT-Systeme in der Regel ebensowenig ein Problem, wie häufig wechselnde IP-Adressen von einfachen DSL-Anschlüssen oder Mobilfunkverbindungen.

![2-architektur-ueberblick.png](./Documentation/de/assets/2-architektur-ueberblick.png)

# Vorteile des RelayServers

- Open-Source-Software
- Vollständige Datenhoheit liegt beim Betreiber des RelayServers
- Als reine Softwarelösung schnell implementierbar
- Für die Client-Software sind keine Libraries notwendig
- Die Lösung ist mandantenfähig, so dass über einen RelayServer Daten für mehrere verschiedene On-Premises-Applikationen und deren Clients fließen können
- Firewalls, NAT und Proxys stellen in der Regel kein Problem dar
- Der Transportkanal ist via HTTPS-Verbindung verschlüsselt
- Die On-Premises-Applikation benötigt als Minimalanforderung eine offene ausgehende HTTPS-Verbindung
- Keine spezielle Hardware notwendig
- Keine Firewall-Änderungen notwendig
- Keine Datenhaltung in einer DMZ notwendig

# Architektur, Installation und Management

Eine vollständige Dokumentation zum RelayServer inklusive aller Details zu seiner Architektur, einer ausführlichen Installationsanleitung und einer Einführung in das RelayServer Management Web befindet sich unter [thinktecture/relayserver/Documentation/de](./Documentation/de/relayserver.md).

# Sponsoren
Das Ziel dieser Sponsorenliste ist es, Unternehmen hervorzuheben, die sich technisch und/oder finanziell an diesem Open-Source-Projekt beteiligen, weil es ihnen hilft, bei ihren eigenen Projekten Zeit und Geld zu sparen.

## Sponsoren aus Deutschland
[<img width="120px" src="./Documentation/de/assets/logo_sponsor_kwp.svg" />](https://www.kwpsoftware.de)

## Sponsoren aus der Schweiz
[<img width="120px" src="./Documentation/de/assets/logo_sponsor_cmi.svg" />](https://www.cmiag.ch/)

