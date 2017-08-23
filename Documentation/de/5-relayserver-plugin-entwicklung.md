# RelayServer Plugin Entwicklung

Der RelayServer kann mit Plugins erweitert werden.

Derzeit stehen zwei Arten von Erweiterungspunkten zur Verfügung:

1. kann ein eingehenden Request abgelehnt, manipuliert oder sofort beantwortet werden, bevor er an den entsprechenden OnPremise Connector weitergeleitet wird
1. kann eine Response die vom OnPremise Connector erhalten wurde manipuliert oder ersetzt werden, bevor sie vom RelayServer an den client weitergereicht wird.

## Plugin Assembly erstellen

Um Plugins für den RelayServer bereit zu stellen reicht es aus, ein Assembly zu erstellen welches mindestens eine Klasse enthält, die mindestens eine der bereitgestellen Plugin-Schnittstellen implementiert.

Hierzu wird ein .NET 4 Bibliotheksprojekt erstellt, das eine Referenz auf das Assembly `Thinktecture.Relay` hat. In diesem Assembly stehen die Plugin-Schnittstellen zur Verfügung.

## Implementierung der Plugins

Im Folgenden werden die möglichen Erweiterungspunkte aufgelistet und erklärt. Es reicht aus, mindestens eine der aufgelisteten Schnittstellen zu implementieren.

_Wichtig:_ Es ist _nicht_ erlaubt, eine Schnittstellen-Implementierung in mehr als einer Klasse bereitzustellen, es sei denn, die Klasse wird selber über ein eigenes Modul im DI-Container (siehe unten) registriert.

Für diesen Fall ist es allerdings _nicht_ erlaubt, mehr als ein DI-Modul bereitzustellen.

### Laden der Plugins

Der RelayServer schaut zuerst nach einem DI-Modul (siehe unten). Wird genau ein DI-Modul gefunden, wird dieses registriert. Hiermit ist es möglich, auch eigene weitere Abhängigkeiten im Dependency Injection Container zu registrieren und diese in den eigenen Plugins zu verwenden.

Sollte kein DI-Modul gefunden werden, wird der RelayServer versuchen im Plugin-Assembly die Klassen zu ermitteln, die mindestens eine der Plugin-Schnittstellen implementieren. Wird genau eine pro Schnittstelle gefunden, wir diese Klasse in der DI registriert.

In diesem Fall kann eine Plugin-Klasse lediglich die standardmäßig zur Verfügung stehenden Abhängigkeiten nutzen, wie z.b. den `Nlog.ILogger`.

### Modifizieren des Requests

Alle Schnittstellen zur Modifikation des Requests funktionieren gleich. Es gibt für jedes der zu manipulierenden Elemente auf dem Request jeweils eine Methode, die für jeden Request aufgerufen wird.

`Handle{RequestProperty}(IOnPremiseConnectorRequest request, out HttpResponseMessage immidiateResponse)`

- Um den Request nicht zu verändern reicht es aus `null` zurück zu geben.
- Wird ein Wert zurück gegeben, so wird dieser den Wert auf dem original Request ersetzen.
- Wird im `out`-Parameter eine `HttpResonseMessage` zurück gegeben, so wird diese Reponse sofort an den Client gesendet und das weiterleiten des Requests übersprungen.
- Die Plugins erhalten in Folge jeweils den bereits vom vorherigen Plugin modifizierten Request.

Folgende Schnittstellen stehen bereit:

#### IRequestMethodManipulator

Zuerst kann die Http Methode (auch: 'Verb') modifiziert werden:

`string HandleMethod(IOnPremiseConnectorRequest request, out HttpResponseMessage response)`

Es wird ein `string` erwartet, der als neue Http Methode des Requests verwendet wird. Zum Beispiel kann die Methode von PATCH zu POST geändert werden.

#### IRequestUrlManipulator

Danach kann die URL des Requests verändert werden:

`string HandleUrl(IOnPremiseConnectorRequest request, out HttpResponseMessage response);`

Es wird ein `string` erwartet, der als neue Url des Requests verwendet wird. Beispielsweise können hier Query-Parameter verändert oder hinzugefügt werden.

#### IRequestHeaderManipulator

Im dritten Schritt können die Http header modifiziert werden:

`IDictionary<string, string> HandleHeaders(IOnPremiseConnectorRequest request, out HttpResponseMessage response);`

Es wird ein `IDictionary<string, string>` erwartet, das die neuen Header des Requests enthält. *Wichtig*: Die Header des Requests werden durch die Rückgabe dieser Methode ersetzt und nicht ergänzt. Es ist daher zur einfachen Ergänzung notwendig, die original-Header des Requests mit zu übernehmen.

Hiermit können Beispielsweise bestimmte Header ausgefiltert bzw. unterdrückt oder zusätzliche Header hinzugefügt werden.

#### IRequestBodyManipulator

Zuletzt kann der Body des Requests verändert werden.

`byte[] HandleBody(IOnPremiseConnectorRequest request, out HttpResponseMessage response);`

Es wird ein `byte[]` erwartet, das den neuen Inhalt des Requests enthält.

Hiermit könnte man Beispielsweise, falls z.B. ein Request durch den MethodeManipulator von einem GET zu einem POST verändert wurde, ein Body hinzugefügt werden, der gesendet werden soll.

### Modifizieren der Response

Alle Schnittstellen zur Modifikation der Response funktionieren gleich. Es gibt für jedes der zu manipulierenden Elemente auf der Response jeweils eine Methode, die für jede erhaltene Response aufgerufen wird.

`Handle{ResponseProperty}(IOnPremiseTargetResponse response, IOnPremiseConnectorRequest request, out HttpResponseMessage immidiateResponse);`

**Achtung:** Der Parameter `IOnPremiseTargetResponse response` kann `null` sein, falls kein On Premise Connector erreichbar ist oder ein sonstiger Fehler (wie ein Timeout) aufgetreten ist.

Falls der `response` Paramter `null` ist, wird auch der Rückgabewert der Methode verworfen, da auf einer nicht existierenden Response kein Wert verändert werden kann. Soll dennoch eine Antwort gesendet werden, so ist die der Parameter `imidiateResponse` zu nutzen.

- Um die Response nicht zu verändern reicht es aus `null` zurück zu geben.
- Wird ein Wert zurück gegeben, so wird dieser den Wert auf der originalen Response ersetzen.
- Wird im `out`-Parameter eine `HttpResonseMessage` zurück gegeben, so wird diese Reponse sofort an den Client gesendet und die vom On Premise Connector erhaltene Response verworfen.
- Die Plugins erhalten in Folge jeweils die bereits vom vorherigen Plugin modifizierte Response.

Folgende Schnittstellen stehen bereit:

#### IResponseStatusCodeManipulator

Zuerst kann der Statuscode einer Response verändert werden:

`HttpStatusCode? HandleStatusCode(IOnPremiseTargetResponse response, IOnPremiseConnectorRequest request, out HttpResponseMessage immidiateResponse);`

Es wird ein `HttpStatusCode?` erwartet, das den neuen Statuscode der Response enthält.

Hiermit kann z.B., wenn der original-Request ein POST war, ein Http 302 oder 303 in ein Http 307 umgewandelt werden, wenn an die neue Adresse auch gePOSTed werden soll.

#### IResponseHeaderManipulator

Im zweiten Schritt können die Header der Response verändert werden:

`IDictionary<string, string> HandleHeaders(IOnPremiseTargetResponse response, IOnPremiseConnectorRequest request, out HttpResponseMessage immidiateResponse);`

Es wird ein `IDictionary<string, string>` erwartet, das die neuen Header der Response enthält. *Wichtig*: Die Header der Response werden durch die Rückgabe dieser Methode ersetzt und nicht ergänzt. Es ist daher zur einfachen Ergänzung notwendig, die original-Header der Response mit zu übernehmen.

Hiermit können Beispielsweise bestimmte Header ausgefiltert bzw. unterdrückt oder zusätzliche Header hinzugefügt werden.

#### IResponseBodyManipulator

Zuletzt kann der Body der Response modifiziert werden:

`byte[] HandleBody(IOnPremiseTargetResponse response, IOnPremiseConnectorRequest request, out HttpResponseMessage immidiateResponse);`

Es wird ein `byte[]` erwartet, das den neuen Inhalt der Response enthält.

Hiermit könnte man Beispielsweise, falls z.B. ein Request durch den MethodeManipulator von einem GET zu einem POST verändert wurde, ein Body hinzugefügt werden, der gesendet werden soll.

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
		builder.RegisterType<RequestHeaderManipulatorDemoPlugin>()
			.As<IRequestHeaderManipulator>();
		builder.RegisterType<RequestMethodManipulatorDemoPlugin>()
			.As<IRequestMethodManipulator>();
		builder.RegisterType<ResponseBodyManipulatorDemoPlugin>()
			.As<IResponseBodyManipulator>();

		base.Load(builder);
	}
}
```

## Konfiguration des Plugins

In der `App.config` des RelayServers reicht es aus, den Konfigurationswert `PluginAssembly` mit einem Pfad zu belegen, der zu dem Plugin Assembly führt. Der Pfad kann entweder Absolut oder relativ angegeben werden.
