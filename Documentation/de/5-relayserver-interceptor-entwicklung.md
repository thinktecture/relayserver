# RelayServer Interceptor Entwicklung

Der RelayServer kann mit sogenannten Interceptors erweitert werden.

Derzeit stehen zwei Arten von Erweiterungspunkten zur Verfügung:

1. Der RequestInterceptor kann einen eingehenden Request ablehnen, manipulieren oder sofort beantworten, bevor er an den entsprechenden OnPremise Connector weitergeleitet wird.
1. Der ResponseInterceptor kann eine Response die vom OnPremise Connector erhalten wurde manipulieren oder ersetzen, bevor sie vom RelayServer an den Client weitergereicht wird.

## Interceptor Assembly erstellen

Um Interceptoren für den RelayServer bereit zu stellen reicht es aus, ein Assembly zu erstellen welches mindestens eine Klasse enthält, die mindestens eine der bereitgestellen Interceptor-Schnittstellen implementiert.

Hierzu wird ein .NET 4 Bibliotheksprojekt erstellt, das eine Referenz auf das Assembly `Thinktecture.Relay` hat. In diesem Assembly stehen die Interceptor-Schnittstellen zur Verfügung.

## Implementierung der Interceptors

Im Folgenden werden die möglichen Erweiterungspunkte aufgelistet und erklärt. Es reicht aus, mindestens eine der aufgelisteten Schnittstellen zu implementieren.

_Wichtig:_ Es ist _nicht_ erlaubt, eine Schnittstellen-Implementierung in mehr als einer Klasse bereitzustellen, es sei denn, die Klasse wird selber über ein eigenes Modul im DI-Container (siehe unten) registriert.

Für diesen Fall ist es allerdings _nicht_ erlaubt, mehr als ein DI-Modul bereitzustellen.

### Laden der Interceptoren

Der RelayServer schaut zuerst nach einem DI-Modul (siehe unten). Wird genau ein DI-Modul gefunden, wird dieses registriert. Hiermit ist es möglich, auch eigene weitere Abhängigkeiten im Dependency Injection Container zu registrieren und diese in den eigenen Interceptoren zu verwenden.

Sollte kein DI-Modul gefunden werden, wird der RelayServer versuchen im Interceptor-Assembly die Klassen zu ermitteln, die mindestens eine der Interceptor-Schnittstellen implementieren. Wird genau eine pro Schnittstelle gefunden, wird diese Klasse automatisch in der DI registriert.
In diesem Fall kann eine Interceptor-Klasse lediglich die standardmäßig zur Verfügung stehenden Abhängigkeiten nutzen, wie z.b. den `Serilog.ILogger`.


### Modifizieren des Requests

Um einen eingehenden Request zu modifizieren oder, noch vor dem Relay-Vorgang, unmittelbar zu beantworten, kann eine Klasse bereitgestellt werden die das Interface `IOnPremiseRequestInterceptor` implementiert.

Das Interface gibt die Methode `HttpResponseMessage OnRequestReceived(IInterceptedRequest request)` vor.

- Um den Request nicht zu verändern reicht es aus `null` zurück zu geben.
- Wird eine `HttpResponseMessage` zurück gegeben, so wird diese Response *unmittelbar* an den Client gesendet. Wenn nicht explizit das Property `AlwaysSendToOnPremiseConnector` auf dem Request auf `true` gesetzt wurde, wird das Weiterleiten des Requests an den OnPremiseConnector übersprungen.
- Um den Request zu modifizieren können die entsprechenden Werte direkt auf dem übergebenen `IInterceptedRequest` verändert werden.

Folgende Werte sind veränderbar:
  - `HttpMethod`: Hiermit kann die HTTP-Method (sog. Verb) verändert werden.
  - `Url`: Hiermit kann die URL des Requests modifiziert werden.
  - `HttpHeaders`: Hier können HTTP Header entfernt, neue hinzugefügt oder existierende geändert werden.

Wird keine `HttpResonseMessage` zurück gegeben, so wird der modifizierte Request über einen OnPremiseConnector an das eigentliche Ziel weitergeleitet.

### Modifizieren der Response

Eine Response, die über einen OnPremise Connector vom On-Premise Dienst zurück übertragen wurde, kann vor dem Zurücksenden an den Client modifiziert werden. Hierzu ist eine Klasse zu erstellen die das Interface `IOnPremiseResponseInterceptor` implementiert.

Dieses Interface gibt zwei zu implementierende Methoden vor:

  * `HttpResponseMessage OnResponseFailed(IReadOnlyInterceptedRequest request)`: Wird aufgerufen, wenn vom On-Premise Dienst *keine* Antwort empfangen wurde. Für diesen Fall kann hier eine Antwort generiert werden.
  * `HttpResponseMessage OnResponseReceived(IReadOnlyInterceptedRequest request, IInterceptedResponse response)`: Wird aufgerufen, wenn eine Antwort empfangen wurde. Diese kann hier verändert oder durch eine eigene Antwort ersetzt werden.

Sollte keine Response vom OnPremise Connector vorliegen, so wird die erste Version aufgerufen.

- Wird `null` zurück gegeben, so greift das Standardverhalten des Relay Servers.
- Wird eine `HttpResonseMessage` zurück gegeben, so wird diese Reponse *unmittelbar* an den Client gesendet.

Sollte eine Response vom OnPremise Connector vorliegen, so wird die zweite Überladung aufgerufen.

- Wird `null` zurück gegeben, so wird die `IInterceptedResponse` an den Client weiter geleitet.
- Wird eine `HttpResonseMessage` zurück gegeben, so wird diese Reponse *unmittelbar* an den Client gesendet und die `IInterceptedResponse` verworfen.
- Um die Response zu modifizieren können die entsprechenden Werte direkt auf der übergebenen `IInterceptedResponse` verändert werden.

Folgende Werte sind veränderbar:
  - `StatusCode`: Hiermit kann der Status-Code der Antwort verändert werden.
  - `HttpHeaders`: Hier können HTTP Header entfernt, neue hinzugefügt oder existierende geändert werden.

### Optional: Registrierung über ein AutofacModule

Um die Interceptors und optional auch eigene Abhängigkeiten in der DI des RelayServers zu registrieren, kann optional ein eigenes AutoFac Modul bereitgestellt werden. Dies erlaubt mehr Kontrolle über das Management der Abhängigkeiten.

```
using Autofac;
using Thinktecture.Relay.Server.Interceptors;

/// <summary>
/// A relay server interceptor assembly can provide a single AutoFac
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

## Konfiguration der Interceptoren

In der `App.config` des RelayServers reicht es aus, den Konfigurationswert `InterceptorAssembly` mit einem Pfad zu belegen, der auf das Assembly mit den Interceptoren zeigt. Der Pfad kann entweder Absolut oder relativ angegeben werden.
