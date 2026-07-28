# Korrigerat monstertruck-paket v2

Den första exporten hade för snäva beskärningar. Den här versionen är
omgjord och kvalitetskontrollerad.

## Korrigeringar

- Alla delar har minst 18 px transparent säkerhetsmarginal.
- Inga objekt ligger mot PNG-filens ytterkant.
- Hjulen är kompletta och deras presentationsskuggor är borttagna.
- De grå och blå stötdämparna exporteras som olika delar.
- 256×256-varianter av hjulen har exakt centrerad pivot.
- En monterad kontrollbild ingår.
- En atlas och JSON-koordinater ingår som komplement.

## Unity 6

1. Kopiera `Assets/MonsterTruck` till Unity-projektets `Assets`.
2. Låt Unity importera och kompilera filerna.
3. Kör:
   `Tools > Monster Truck > Create Corrected Prototype Prefab`
4. Prefaben skapas i:
   `Assets/MonsterTruck/Prefabs/MonsterTruckPrototypeV2.prefab`
5. Lägg ett markobjekt med `Collider2D` i scenen.

## Separata filer eller sprite sheet?

Använd de separata PNG-filerna under utvecklingen. De är enklast att byta
ut i bilbyggaren. Atlasen under `Assets/MonsterTruck/Atlas` kan användas
senare för att minska antalet texturer i den färdiga mobilversionen.

## Prototypstatus

Grafiken är nu komplett beskuren och kan användas för en fungerande
prototyp. Det är fortfarande AI-genererad konceptgrafik. För en slutlig
kommersiell version bör samtliga delar ritas om som ett konsekvent,
pixelperfekt originalpaket.
