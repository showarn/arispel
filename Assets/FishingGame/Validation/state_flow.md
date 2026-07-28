# Fiskespelets state flow

`FishingStateMachine` är enda sanningskälla. UI:t anropar bara
`FishingGameController.PressPrimaryButton()`.

```text
Idle
  └─ tryck KASTA → Casting
       └─ kast landar → WaitingForBite
            ├─ tidigt tryck → stannar i WaitingForBite
            └─ konfigurerad väntan → FishBiting
                 ├─ tryck DRA UPP → ReelingIn
                 │    └─ fisken landar → CatchReveal
                 │         └─ BRA JOBBAT → ReturningToIdle → Idle
                 └─ generöst nappfönster löper ut
                      → ReturningToIdle → Idle
```

Alla aktiva tillstånd kan pausas till `Paused`. `Resume()` återgår endast till
det tillstånd som var aktivt före pausen. Ogiltiga övergångar ignoreras.

Tider:

- Kast: 0,95 sekunder.
- Väntan på napp: konfigurerbart intervall 1,5–4 sekunder.
- Nappfönster: 3 sekunder.
- Ingen timer visas för barnet.
