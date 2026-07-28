# Arisspel

En färgstark samling spel och sagor för små barn, byggd med Unity 6.

## Innehåll

- Monstertrucks med lopp, progression och garage.
- Pussel.
- Memory.
- Fiske.
- Sagorna Lilla Lumi och Ari och lillasystern.
- Föräldradashboard för att slå av eller på varje kategori.
- Lokal profil och lokal progression.

## Monstertrucks

- Håll in den stora gaspedalen för att köra.
- Tävla mot den snälla datorstyrda trucken.
- Samla glittrande mynt.
- Kör över kullar och igenom regnbågsloopen.
- Automatisk hjälp rätar upp trucken och räddar den om den välter.
- Resultatskärm med 1–3 pluppar och sparad progression.
- Garage med valbara karosser, hjul, dekaler och tillbehör som låses upp
  genom bättre tävlingsresultat.
- Anpassat för liggande skärm på iPhone och iPad.

## Öppna projektet

Öppna repositoryts rotmapp i Unity `6000.4.5f1` eller en kompatibel Unity 6-version.
Startscenen är `Assets/Scenes/MonsterTruckRace.unity`.

Tryck på Play. På datorn kan mellanslag, högerpil eller `D` användas som gas.

## Automatiskt provlopp

En desktop-build kan startas med `-arisAutoDrive`. Då hoppas startmenyn över,
ett komplett lopp körs automatiskt och resultatet skrivs som `ARIS_AUTOTEST`
i spelarloggen. Processen avslutas med kod `0` när spelaren når målet först.

## Grafik

Monstertruckdelarna bygger på prototypmaterialet under `monstertruck/`.
Den färgstarka spelbakgrunden är AI-genererad projektgrafik och ligger under
`Assets/Resources/Art/Environment/`.

## Lokala byggen

Linux development-build:

```bash
/path/to/Unity -batchmode -nographics -quit \
  -projectPath . \
  -executeMethod ArisMonsterTrucks.Editor.ProjectBuilder.BuildLinuxTest \
  -logFile -
```

Android development-APK:

```bash
/path/to/Unity -batchmode -nographics -quit \
  -projectPath . \
  -executeMethod ArisMonsterTrucks.Editor.ProjectBuilder.BuildAndroidDevelopment \
  -logFile -
```

APK:n skapas som `Builds/Android/Arisspel.apk`.

## Codemagic

`codemagic.yaml` innehåller workflowen `unity-android-development`.
Codemagic behöver variabelgruppen `unity_credentials` med hemligheterna:

- `UNITY_EMAIL`
- `UNITY_SERIAL`
- `UNITY_PASSWORD`

Unity kräver Plus- eller Pro-licens för molnbyggen. Workflowen kör EditMode- och
PlayMode-tester, bygger en development-APK och återlämnar licensen i
publiceringsfasen.
