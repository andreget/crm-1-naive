# NaiveCrm

En liten webapp for kundebehandling (CRM). Den er startpunktet for programmeringskurset og er med vilje enkel. Du skal forbedre den steg for steg i de kommende modulene.

## Hva den gjør

- **Kunder:** liste med søk og statusfilter, detaljside, opprett, endre og slett.
- **Aktiviteter:** registrer møter, samtaler, e-poster og oppgaver for en kunde, merk dem som ferdige, og se alle åpne aktiviteter i én oversikt. Forfalte aktiviteter er markert.

## Krav

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- Visual Studio, eller en annen editor sammen med kommandolinjeverktøyet `dotnet`

Sjekk at installasjonen fungerer:

```
dotnet --version
```

## Lag din egen kopi

1. Klikk **Use this template** øverst på denne siden, og deretter **Create a new repository**.
2. Klone det nye repoet ditt:

```
git clone <url-to-your-repository>
```

## Kjør appen

**Visual Studio:** åpne `NaiveCrm.csproj` og trykk `F5`. Nettleseren åpner <http://localhost:5080>.

**Kommandolinjen:** i mappen som inneholder `NaiveCrm.csproj`:

```
dotnet run
```

Åpne deretter <http://localhost:5080>.

> **Start alltid appen fra prosjektmappen.** Maler og datafiler leses med relative stier, så sidene feiler hvis appen startes fra en annen mappe.

## Prosjektstruktur

```
Program.cs          Endepunkter og hjelpefunksjoner
Models/             Customer og Activity
Services/           CustomerService og ActivityService (lagrer data i JSON-filer)
Templates/          HTML-maler med {{placeholders}}
wwwroot/            CSS og JavaScript
customers.json      Eksempelkunder (appen endrer denne filen)
```

## Data

- `customers.json` inneholder fem eksempelkunder. Appen skriver til denne filen når du oppretter, endrer eller sletter kunder, så den vises som endret i Git. Slik gjenoppretter du eksempeldataene:

```
git checkout customers.json
```

- `activities.json` opprettes første gang du legger til en aktivitet. Den følges ikke av Git. Slett filen for å fjerne alle aktiviteter.

## Feilsøking

| Problem | Hva du bør sjekke |
|---|---|
| Feil eller blank side rett etter oppstart | Appen er startet fra feil mappe. Start den fra mappen med `NaiveCrm.csproj`. |
| Port 5080 er opptatt | Endre `applicationUrl` i `Properties/launchSettings.json`. |
| Dataene ser rare ut etter eksperimentering | Gjenopprett `customers.json` og slett `activities.json`. |