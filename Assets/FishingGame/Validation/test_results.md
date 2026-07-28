# Automatiska testresultat

Körmiljö: Unity 6000.4.5f1, Unity Test Framework 1.6.0.

## EditMode

- Resultat: **14/14 godkända**
- Tid: 0,089 sekunder
- XML: `/tmp/aris-fishing-editmode.xml`

Verifierat:

- unika stabila ID:n
- namn och sprites
- giltiga vikter och längdintervall
- alla rariteter kan väljas
- fast seed är reproducerbar
- serialisering och återställning
- okända fisk-ID:n
- tom eller korrupt data
- största längd
- fångstantal
- ogiltiga state-övergångar
- sex fiskeplatsers stegvisa raritetsgränser
- deterministiskt platsbaserat fiskurval
- balanserad slutnivå: cirka 2 % femstjärnigt

## PlayMode

- Resultat: **15/15 godkända**
- Tid: 30,967 sekunder
- XML: `/tmp/aris-fishing-playmode.xml`

Verifierat:

- öppning utan exceptions
- Idle/Kasta
- Casting/WaitingForBite
- tidigt tryck
- FishBiting/Dra upp
- ReelingIn/CatchReveal
- fiskbokssparning
- Fortsätt/Idle
- vänlig miss utan Game Over
- paus/återupptagning
- tillbaka till minispelsmeny
- global ljudinställning
- skydd mot dubbla fångstflöden
