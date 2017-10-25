# RelayServer Plugin Entwicklung

Der RelayServer kann mit Plugins erweitert werden.

Derzeit stehen zwei Arten von Erweiterungspunkten zur Verfügung:

1. Der RequestInterceptor kann ein eingehenden Request abgelehnt, manipuliert oder sofort beantwortet werden, bevor er an den entsprechenden OnPremise Connector weitergeleitet wird.
1. Der ResponseInterceptor kann eine Response die vom OnPremise Connector erhalten wurde manipuliert oder ersetzt werden, bevor sie vom RelayServer an den Client weitergereicht wird.

## Plugin Assembly erstellen

Um Plugins für den RelayServer bereit zu stellen reicht es aus, ein Assembly zu erstellen welches mindestens eine Klasse enthält, die mindestens eine der bereitgestellen Plugin-Schnittstellen implementiert.

Hierzu wird ein .NET 4 Bibliotheksprojekt erstellt, das eine Referenz auf das Assembly `Thinktecture.Relay` hat. In diesem Assembly stehen die Plugin-Schnittstellen zur Verfügung.

## Implementierung der Plugins

Im Folgenden werden die möglichen Erweiterungspunkte aufgelistet und erklärt. Es reicht aus, mindestens eine der aufgelisteten Schnittstellen zu implementieren.

_Wichtig:_ Es ist _nicht_ erlaubt, eine Schnittstellen-Implementierung in mehr als einer Klasse bereitzustellen, es sei denn, die Klasse wird selber über ein eigenes Modul im DI-Container (siehe unten) registriert.

Für diesen Fall ist es allerdings _nicht_ erlaubt, mehr als ein DI-Modul bereitzustellen.

### Laden der Plugins

Der RelayServer schaut zuerst nach einem DI-Modul (siehe unten). Wird genau ein DI-Modul gefunden, wird dieses registriert. Hiermit ist es möglich, auch eigene weitere Abhängigkeiten im Dependency Injection Container zu registrieren und diese in den eigenen Plugins zu verwenden.

Sollte kein DI-Modul gefunden werden, wird der RelayServer versuchen im Plugin-Assembly die Klassen zu ermitteln, die mindestens eine der Plugin-Schnittstellen implementieren. Wird genau eine pro Schnittstelle gefunden, wird diese Klasse automatisch in der DI registriert.
In diesem Fall kann eine Plugin-Klasse lediglich die standardmäßig zur Verfügung stehenden Abhängigkeiten nutzen, wie z.b. den `Nlog.ILogger`.


### Modifizieren des Requests

Um einen eingehenden Request zu modifizieren oder, noch vor dem Relay-Vorgang, unmittelbar zu beantworten, kann eine Klasse bereitgestellt werden die das Interface `IOnPremiseRequestInterceptor` implementiert.

Das Interface gibt die Methode `HttpResponseMessage OnRequestReceived(IInterceptedRequest request)` vor.

- Um den Request nicht zu verändern reicht es aus `null` zurück zu geben.
- Wird eine `HttpResonseMessage` zurück gegeben, so wird diese Reponse *unmittelbar* an den Client gesendet und das weiterleiten des Requests übersprungen.
- Um den Request zu modifizieren können die entsprechenden Werte direkt auf dem übergebenen `IInterceptedRequest` verändert werden.

Folgende Werte sind veränderbar:
  - `HttpMethod`: Hiermit kann die HTTP-Method (sog. Verb) verändert werden.
  - `Url`: Hiermit kann die URL des Requests modifiziert werden.
  - `HttpHeaders`: Hier können HTTP Header entfernt, neue hinzugefügt oder existierende geändert werden.
  - `Body`: Hier kann der Body des Requests verändert werden.

Wird keine `HttpResonseMessage` zurück gegeben, so wird der modifizierte Request über einen OnPremiseConnector an ddas eigentliche Ziel weitergeleitet.

### Modifizieren der Response

Eine Response, die über einen OnPremise Connector vom On-Premise Dienst zurück übertragen wurde, kann vor dem Zurücksenden an den Client modifiziert werden. Hierzu ist eine Klasse zu erstellen die das Interface `IOnPremiseResponseInterceptor` implementiert.

Dieses Interface gibt zwei zu implementierende Methoden vor:

  * `HttpResponseMessage OnResponseReceived(IReadOnlyInterceptedRequest request)`: Wird aufgerufen, wenn vom On-Premise Dienst *keine* Antwort empfangen wurde. Für diesen Fall kann hier eine Antwort generiert werden.
  * `HttpResponseMessage OnResponseReceived(IReadOnlyInterceptedRequest request, IInterceptedResponse response)`: Wird aufgerufen, wenn eine Antwort empfangen wurde. Diese kann hier verändert oder durch eine eigene Antwort ersetzt werden.

Sollte keine Response vom OnPremise Connector vorliegen, so wird die erste Version aufgerufen.

- Wird `null` zurück gegeben greift das Standardverhalten des Relay Servers.
- Wird eine `HttpResonseMessage` zurück gegeben, so wird diese Reponse *unmittelbar* an den Client gesendet.

Sollte eine Response vom OnPremise Connector vorliegen, so wird die zweite Überladung aufgerufen.

- Wird `null` zurück gegeben, so wird die `IInterceptedResponse` an den Client weiter geleitet.
- Wird eine `HttpResonseMessage` zurück gegeben, so wird diese Reponse *unmittelbar* an den Client gesendet und die `IInterceptedResponse` verworfen.
- Um die Response zu modifizieren können die entsprechenden Werte direkt auf der übergebenen `IInterceptedResponse` verändert werden.

Folgende Werte sind veränderbar:
  - `StatusCode`: Hiermit kann der Status code der Antwort verändert werden.
  - `HttpHeaders`: Hier können HTTP Header entfernt, neue hinzugefügt oder existierende geändert werden.
  - `Body`: Hier kann der Body der Response modifiziert werden.

### Optional: Registrierung über ein AutofacModule

Um die Plugins und optional auch eigene Abhängigkeiten in der DI des RelayServers zu registrieren kann optional ein eigenes AutoFac Modul bereitgestellt werden. Dies erlaubt mehr Kontrolle über das Management der Abhängigkeiten.

```
using Autofac;
using Thinktecture.Relay.Server.Plugins;

/// <summary>
/// A relay server plugin assembly can provide a single AutoFac
/// Module that will register all plugin types that are implemented
/// and should be used.
/// </summary>
public class PluginModule : Module
{
	/// <summary>
	/// Override the Load method of the AutoFac module to
	/// register the plugin types.
	/// </summary>
	/// <param name="builder"></param>
	protected override void Load(ContainerBuilder builder)
	{
		// Each plugin that should be added needs to be registered
		// with the container builder as its Interface type
		builder.RegisterType<RequestInterceptorDemoPlugin>()
			.As<IOnPremiseRequestInterceptor>();
		builder.RegisterType<ResponseInterceptorDemoPlugin>()
			.As<IOnPremiseResponseInterceptor>();

		base.Load(builder);
	}
}
```

## Konfiguration des Plugins

In der `App.config` des RelayServers reicht es aus, den Konfigurationswert `PluginAssembly` mit einem Pfad zu belegen, der zu dem Plugin Assembly führt. Der Pfad kann entweder Absolut oder relativ angegeben werden.
