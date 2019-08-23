# RelayServer Interceptor Entwicklung

Der RelayServer und die On-Premise Connectoren können mit sogenannten Interceptors erweitert werden.

## RelayServer

Derzeit stehen zwei Arten von Erweiterungspunkten zur Verfügung:

1. Der RequestInterceptor kann einen eingehenden Request ablehnen, manipulieren oder sofort beantworten, bevor er an den entsprechenden OnPremise Connector weitergeleitet wird.
1. Der ResponseInterceptor kann eine Response die vom OnPremise Connector erhalten wurde manipulieren oder ersetzen, bevor sie vom RelayServer an den Client weitergereicht wird.

### Interceptor Assembly erstellen

Um Interceptoren für den RelayServer bereit zu stellen reicht es aus, ein Assembly zu erstellen welches mindestens eine Klasse enthält, die mindestens eine der bereitgestellen Interceptor-Schnittstellen implementiert.

Hierzu wird ein .NET 4 Bibliotheksprojekt erstellt, das eine Referenz auf das Assembly `Thinktecture.Relay` hat. In diesem Assembly stehen die Interceptor-Schnittstellen zur Verfügung.

### Implementierung der Interceptors

Im Folgenden werden die möglichen Erweiterungspunkte aufgelistet und erklärt. Es reicht aus, mindestens eine der aufgelisteten Schnittstellen zu implementieren.

_Wichtig:_ Es ist _nicht_ erlaubt, eine Schnittstellen-Implementierung in mehr als einer Klasse bereitzustellen, es sei denn, die Klasse wird selber über ein eigenes Modul im DI-Container (siehe unten) registriert.

Für diesen Fall ist es allerdings _nicht_ erlaubt, mehr als ein DI-Modul bereitzustellen.

#### Laden der Interceptoren

Der RelayServer schaut zuerst nach einem DI-Modul (siehe unten). Wird genau ein DI-Modul gefunden, wird dieses registriert. Hiermit ist es möglich, auch eigene weitere Abhängigkeiten im Dependency Injection Container zu registrieren und diese in den eigenen Interceptoren zu verwenden.

Sollte kein DI-Modul gefunden werden, wird der RelayServer versuchen im Interceptor-Assembly die Klassen zu ermitteln, die mindestens eine der Interceptor-Schnittstellen implementieren. Wird genau eine pro Schnittstelle gefunden, wird diese Klasse automatisch in der DI registriert.
In diesem Fall kann eine Interceptor-Klasse lediglich die standardmäßig zur Verfügung stehenden Abhängigkeiten nutzen, wie z.b. den `Serilog.ILogger`.


#### Modifizieren des Requests

Um einen eingehenden Request zu modifizieren oder, noch vor dem Relay-Vorgang, unmittelbar zu beantworten, kann eine Klasse bereitgestellt werden die das Interface `IOnPremiseRequestInterceptor` implementiert.

Das Interface gibt die Methode `HttpResponseMessage OnRequestReceived(IInterceptedRequest request)` vor.

- Um den Request nicht zu verändern reicht es aus `null` zurück zu geben.
- Wird eine `HttpResponseMessage` zurück gegeben, so wird diese Response *unmittelbar* an den Client gesendet. Wenn nicht explizit das Property `AlwaysSendToOnPremiseConnector` auf dem Request auf `true` gesetzt wurde, wird das Weiterleiten des Requests an den OnPremiseConnector übersprungen.
- Um den Request zu modifizieren können die entsprechenden Werte direkt auf dem übergebenen `IInterceptedRequest` verändert werden.

Folgende Werte sind veränderbar:
  - `HttpMethod`: Hiermit kann die HTTP-Method (sog. Verb) verändert werden.
  - `Url`: Hiermit kann die URL der Anfrage modifiziert werden.
  - `HttpHeaders`: Hier können HTTP Header entfernt, neue hinzugefügt oder existierende geändert werden.
  - `Content`: Hier kann der Inhalt der Anfrage gelesen oder modifiziert werden.  
   *Achtung:* Wird auf dieses Property zugegriffen, so wird im Speicher eine Kopie des gesamten Inhaltes erstellt, da der Stream der Anfrage nicht mehrfach gelesen werden kann. Dies kann den Speicherverbrauch spürbar erhöhen.
  - `AlwaysSendToOnPremiseConnector`: Hiermit kann festgelegt werden, dass der Request immer zum OnPremiseConnector gesendet wird, auch wenn er durch das Zurückgeben einer `HttpResponseMessage` sofort beantwortet wird.
  - `Expiration`: Hier kann die Lebenszeit des Requests in der RabbitMQ geändert werden.
  - `AcknowledgmentMode`: Hier kann festgelegt werden, ob der Request vom OnPremiseConnector Acknowledged wird (Default), automatisch nach dem Lesen aus der RabbitMQ gelöscht wird (Auto), oder auf ein manuelles Acknowledge vom Ziel wartet (Manual). Details siehe unten im Abschnitt Acknowledgment.

Wird keine `HttpResonseMessage` zurück gegeben, so wird der modifizierte Request über einen OnPremiseConnector an das eigentliche Ziel weitergeleitet.

#### Modifizieren der Response

Eine Response, die über einen OnPremise Connector vom On-Premise Dienst zurück übertragen wurde, kann vor dem Zurücksenden an den Client modifiziert werden. Hierzu ist eine Klasse zu erstellen die das Interface `IOnPremiseResponseInterceptor` implementiert.

Dieses Interface gibt zwei zu implementierende Methoden vor:

  * `HttpResponseMessage OnResponseFailed(IReadOnlyInterceptedRequest request)`: Wird aufgerufen, wenn vom On-Premise Dienst *keine* Antwort empfangen wurde. Für diesen Fall kann hier eine Antwort generiert werden.
  * `HttpResponseMessage OnResponseReceived(IReadOnlyInterceptedRequest request, IInterceptedResponse response)`: Wird aufgerufen, wenn eine Antwort empfangen wurde. Diese kann hier verändert oder durch eine eigene Antwort ersetzt werden.

Sollte keine Response vom OnPremise Connector vorliegen, so wird die erste Version aufgerufen.

- Wird `null` zurück gegeben, so greift das Standardverhalten des RelayServers.
- Wird eine `HttpResonseMessage` zurück gegeben, so wird diese Reponse *unmittelbar* an den Client gesendet.

Sollte eine Response vom OnPremise Connector vorliegen, so wird die zweite Überladung aufgerufen.

- Wird `null` zurück gegeben, so wird die `IInterceptedResponse` an den Client weiter geleitet.
- Wird eine `HttpResonseMessage` zurück gegeben, so wird diese Reponse *unmittelbar* an den Client gesendet und die `IInterceptedResponse` verworfen.
- Um die Response zu modifizieren können die entsprechenden Werte direkt auf der übergebenen `IInterceptedResponse` verändert werden.

Folgende Werte sind veränderbar:
  - `StatusCode`: Hiermit kann der Status-Code der Antwort verändert werden.
  - `HttpHeaders`: Hier können HTTP Header entfernt, neue hinzugefügt oder existierende geändert werden.
  - `Content`: Hier kann der Inhalt der Antwort gelesen oder modifiziert werden.  
   *Achtung:* Wird auf dieses Property zugegriffen, so wird im Speicher eine Kopie des gesamten Inhaltes erstellt, da der Stream der Antwort nicht mehrfach gelesen werden kann. Dies kann den Speicherverbrauch spürbar erhöhen.

#### Optional: Registrierung über ein AutofacModule

Um die Interceptors und optional auch eigene Abhängigkeiten in der DI des RelayServers zu registrieren, kann optional ein eigenes AutoFac Modul bereitgestellt werden. Dies erlaubt mehr Kontrolle über das Management der Abhängigkeiten.

```
using Autofac;
using Thinktecture.Relay.Server.Interceptors;

/// <summary>
/// A RelayServer interceptor assembly can provide a single AutoFac
/// Module that will register all interceptor types that are implemented
/// and should be used.
/// </summary>
public class InterceptorModule : Module
{
	/// <summary>
	/// Override the Load method of the AutoFac module to
	/// register the interceptor types.
	/// </summary>
	/// <param name="builder"></param>
	protected override void Load(ContainerBuilder builder)
	{
		// Each interceptor that should be added needs to be registered
		// with the container builder as its Interface type
		builder.RegisterType<RequestDemoInterceptor>()
			.As<IOnPremiseRequestInterceptor>();
		builder.RegisterType<ResponseDemoInterceptor>()
			.As<IOnPremiseResponseInterceptor>();

		base.Load(builder);
	}
}
```

### Konfiguration der Interceptoren

In der `App.config` des RelayServers reicht es aus, den Konfigurationswert `InterceptorAssembly` mit einem Pfad zu belegen, der auf das Assembly mit den Interceptoren zeigt. Der Pfad kann entweder Absolut oder relativ angegeben werden.

## Acknowledgment

RelayServer setzt darauf, das Requests vom On-Premise Connector bestätigt werden, bevor die Nachricht auch in der Message Queue als abgearbeitet markiert (Acknowledged) wird. In der Standardeinstellung wird der On-Premise Connector wird eine Nachricht acknowledgen, sobald er diese Empfangen hat. Danach wird der Request dann an das On-Premise Target weitergeleitet.

Ein Request-Interceptor kann den `AcknowledgmentMode` eines Requests auf `Auto` oder `Manual` umstellen.  
Bei `Auto` wird der Request beim Auslesen automatisch acknowledged. Es wird also nicht garantiert, dass dieser auch beim On-Premise Connector ankommt.  
Bei `Manual` muss der Request manuell Acknowledged werden. Dies kann z.B. durch das On-Premise Target geschehen, um zu bestätigen dass der Request auch in der Tat beim Zielsystem angekommen ist.

Um einen Request manuell zu bestätigen muss ein HTTP GET Request an den `/request/acknowledge` Endpunkt auf dem RelayServer gesendet werden, welcher im Query-String die Parameter `aid` mit der AcknowledgeId, `oid` mit der OriginId und optional noch `cid` mit der ConnectionId übermittelt. Diese Parameter werden bei einem Web-Target vom On-Premise Connector in den HTTP-Headern `X-TTRELAY-ACKNOWLEDGE-ORIGIN-ID`, `X-TTRELAY-ACKNOWLEDGE-ID` sowie `X-TTRELAY-CONNECTION-ID` an die Ziel-Api übergeben. Der Acknowledge-Request muss zudem einen für den RelayServer gültigen Bearer-Token im Authorization Header bereitstellen.  
Für In-Process Targets kann alternativ die Methode `AcknowledgeRequestAsync` auf dem `RelayServerConnector` aufgerufen werden, die den authentifizierten Request senden wird.

## On-Premise Connector

Auch hier stehen zwei Arten von Interceptoren zur Verfügung:

1. Der RequestInterceptor kann einen eingehenden Request manipulieren bevor er an das On-Premise Target weitergeleitet wird.
1. Der ResponseInterceptor kann eine Response die vom On-Premise Target empfangen und manipulieren, bevor sie zurück zum RelayServer gereicht wird.

### Implementation

Für die On-Premise Interceptoren stehen die zu implementierenden Interfaces im Assembly `Thinktecture.Relay.OnPremiseConnector` zur Verfügung.

Die Interfaces befinden sich im Namespace `Thinktecture.Relay.OnPremiseConnector.Interceptor` und sind:
* `IOnPremiseRequestInterceptor`
* `IOnPremiseResponseInterceptor`

Die Interceptoren werden für jeden Request neu über den IoC Container erzeugt und können auch andere Abhängigkeiten über die DI erhalten.

#### Hinweise

* Wenn der Stream eines Requests oder einer Response verändert werden soll, sind die Features des Streams zu beachten (`CanSeek`, `CanRead`, `CanWrite` etc.). Im Zweifel sollte besser eine Kopie des Content-Streams erzeugt und dem Request bzw. der Response neu zugewiesen werden, damit dieser zur Weiterleitung erneut gelesen werden kann.
* Ändert sich die Größe eines Requests oder einer Response, sollte auch ein ggf. vorhander `Content-Length` Header angepasst werden.

### Registrieren der Interceptoren

Um die On-Premise Interceptoren zu registrieren, müssen diese in den IoC Container (`Microsoft.Extensions.DependencyInjection`) eingetragen werden, und der `IServiceProvider` der diese bereitstellt dem `RelayServerConnector` im Contructor übergeben werden. Ein Beispiel hierfür steht im Projekt `Thinktecture.Relay.OnPremiseConnector.InterceptorSample` bereit.

Es ist nicht möglich, den Beispiel-`OnPremiseConnectorService` mit Interceptoren zu erweitern.
