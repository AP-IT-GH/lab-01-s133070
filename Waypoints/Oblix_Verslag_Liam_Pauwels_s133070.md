# Analyse van hyperparameter-aanpassingen bij ML-Agent Oblix (rapport)

| S-nummer | s133070 |

## Inleiding

In dit verslag wordt er geanalyseerd hoe de ML-agent 'Oblix' reageert op verschillende aanpassingen in observaties, episode-lengte en PPO-hyperparameters. De agent wordt meerdere keren opnieuw getraind met kleine wijzigingen, zodat kan worden geanalyseerd welke configuratie de beste en meest stabiele resultaten oplevert voor de volledige taak juist uit te voeren.

Het doel van de agent is:
1. Een steen ophalen.
2. Deze naar de afleverlocatie brengen.
3. Dit proces herhalen tot alle stenen gebracht zijn naar een afleverlocatie.

Tijdens het testen wordt telkens een nieuwe training gestart vanaf nul. De resultaten worden geëvalueerd op basis van de TensorBoard grafiek **Environment / Cumulative Reward**.

## Methoden

De agent wordt meerdere keren opnieuw getraind met verschillende configuraties. Bij elke training worden volgende zaken aangepast en geëvalueerd:

- Observaties
- Episode-lengte (Max Step)
- Rewardstructuur
- PPO hyperparameters (beta, learning rate, batch size) <-- deze heb ik veranderd om te kijken wat    het kan doen met de zelfde script of het beter kan leren.
- Reproduceerbaarheid van resultaten (kijken of het geen 'lucky' resultaat was door de zelfde config en settings te gebruiken en een nieuwe training te starten).

Alle trainingen worden uitgevoerd tot 100k, 250k of 300k stappen. De grafieken worden telkens vergeleken. Dit hing af van de resultaten, als ik pas tegen 100k stappen een positieve verandering zag dan liet ik de agent langer doortrainen.

---

## Basisconfiguratie (Stap 9)

De baseline configuratie bevat:
- Observaties:
  - positie agent
  - boolean of steen wordt gedragen
- Continuous actions: 2
- Straf per frame: -0.0002f
- PPO standaard configuratie
- Max Step: 3000

Deze configuratie wordt gebruikt als referentiepunt voor verdere experimenten.

## Resultaten

![Oblix_stap9](screenshots/oblix_stap9.png)

Bij deze configuratie leert de agent het gedrag gedeeltelijk. Er zijn positieve rewards zichtbaar, maar het gedrag blijft instabiel. De agent ontdekt soms de juiste strategie, maar behoudt deze niet consequent.

---

## Toevoegen van transform.forward observatie (Stap 10)

Er wordt een extra observatie toegevoegd:
- richting waarin de agent kijkt, dit ging misschien helpen naar wat de agent juist moet toe gaan.

Totale observaties verhogen hierdoor.

## Resultaten

![Oblix_stap10](screenshots/Oblix_stap10.png)

De resultaten tonen dat het toevoegen van deze observatie geen verbetering oplevert. De grafiek blijft instabiel en de agent leert niet sneller dan bij de vorige testen.

Hieruit kan geconcludeerd worden dat extra observaties niet noodzakelijk leiden tot betere prestaties. Dus hebben we deze nadien weer weg gehaald.

---

## Toevoegen van velocity observatie (Stap 12)

Er wordt een extra observatie toegevoegd:
- rigidbody velocity (hiermee kunnen volgen hoe de agent beweegt).

## Resultaten

![Oblix_stap12](screenshots/Oblix_stap12.png)

De agent leert iets stabieler, maar de prestaties verbeteren niet significant. De gemiddelde reward blijft beperkt en er ontstaan geen duidelijke doorbraken. Ook krijgen we vaak de melding dat er geen volledige episode is geëindigd.

---

## Terug naar baseline observaties (Stap 13)

De extra observaties worden verwijderd en de configuratie wordt opnieuw getest.

## Resultaten

![Oblix_stap13](screenshots/Oblix_stap13.png)

De prestaties blijven vergelijkbaar met de oorspronkelijke baseline. Dit bevestigt dat extra observaties niet noodzakelijk een voordeel bieden in deze omgeving. Waardoor we dus andere variabele moeten gaan aanpassen.

---

## Aanpassen van PPO hyperparameters (Stap 14)

De volgende configuratie wordt getest:

- batch_size: 128
- buffer_size: 4096
- learning_rate: 1.0e-4
- beta: 1.0e-3
- gamma: 0.99

## Resultaten

![Oblix_stap14](screenshots/Oblix_stap14.png)

De agent leert iets stabieler, maar de prestaties blijven wisselend. Er worden positieve rewards gehaald, maar geen consistente verbetering. Ook na het behalen van een positieve rewards gaat het drastisch dalende resultaten weergeven.

---

## Verhogen negatieve reward (Stap 15)

De straf per frame wordt verhoogd naar:
- -0.001

## Resultaten

![Oblix_stap15](screenshots/Oblix_stap15.png)

De prestaties verslechteren. De agent behaalt alleen negatieve rewards en leert moeilijker. Dit bevestigt dat een te grote negatieve reward het leerproces verstoort.

---

## Episode lengte verhogen (Stap 17)

De Max Step wordt verhoogd van:
- 3000 → 8000

## Resultaten

![Oblix_stap17](screenshots/Oblix_stap17.png)

De agent haalt maar 1 consistente resultaat maar deze is negatief.

---

## Eerste succesvolle configuratie (Stap 18)

Met:
- Max Step = 8000
- learning_rate = 1.0e-4
- beta = 1.0e-3

## Resultaten

![Oblix_stap18](screenshots/Oblix_stap18.png)

De agent behaalt meerdere positieve rewards. Dit is de eerste configuratie waarbij duidelijke doorbraken zichtbaar zijn. Het gedrag blijft echter nog instabiel. Dus moeten er nog veranderingen gebeuren.

---

## Verlagen beta (Stap 20)

Beta wordt aangepast:
- 1.0e-3 → 5.0e-4

## Resultaten

![Oblix_stap20](screenshots/Oblix_stap20.png)

De agent behaalt opnieuw positieve rewards. Het gedrag wordt iets stabieler, maar de prestaties blijven te veel schommelen.

---

## Verlagen learning rate (Stap 21)

learning_rate wordt aangepast:
- 1.0e-4 → 5.0e-5

## Resultaten

![Oblix_stap21](screenshots/Oblix_stap21.png)

De training wordt stabieler maar ook trager. De agent leert minder efficiënt en de eindresultaten verbeteren niet.

---

## Finale configuratie (Stap 22)

Configuratie:
- learning_rate: 1.0e-4
- beta: 5.0e-4
- batch_size: 128
- buffer_size: 4096
- Max Step: 8000

## Resultaten

![Oblix_stap22](screenshots/Oblix_stap22.png)

De agent behaalt meerdere hoge rewards en vertoont een duidelijke stijgende trend. De prestaties blijven stabiel in de tweede helft van de training zonder een grote zakken en je kan zien dat de strend linear stijgt. Om na te gaan of het niet 'lucky' was gaan we deze config en setting opnieuw proberen in een nieuwe training.

---

## Reproduceerbaarheid (Stap 23)

Dezelfde configuratie wordt opnieuw getest.

## Resultaten

![Oblix_stap23](screenshots/Oblix_stap23.png)

De resultaten bevestigen de vorige training. De agent behaalt opnieuw meerdere hoge rewards en eindigt positief. Dit toont aan dat de configuratie reproduceerbaar is ookal was er een korte grote dip op 242k stappen.

Dit zijn 22 en 23 bij elkaar.

![Oblix_stap22 en 23](screenshots/Oblix_stap22_23.png)

---

## Conclusie

Uit de resultaten kan geconcludeerd worden dat vooral de episode-lengte en PPO hyperparameters een grote invloed hebben op het leerproces van de agent.

Het verhogen van de Max Step naar 8000 gaf de agent voldoende tijd om de taak te voltooien. Daarnaast zorgde het verlagen van beta voor minder exploratie en een stabielere policy.

De beste resultaten werden behaald met volgende configuratie:

- learning_rate: 1.0e-4  
- beta: 5.0e-4  
- batch_size: 128  
- buffer_size: 4096  
- Max Step: 8000  

Deze configuratie toonde dus herhalende resultaten, waardoor deze configuratie + setting het beste waren voor dit project.


## Samenvatting gefaalde trainingen

Dit zijn alle gefaalde training samen op 1 grafiek.

![Oblix negatieve trends](screenshots/Oblix_negatief.png)

## Verrasende resultaten

De eerste drie testen verliepen heel positief, maar erna werd de agent verward in zijn taken en kreeg dan een zware daling.

![Oblix rare trends](screenshots/Oblix_raar.png)

## Laatste conclusie

De laatste twee/drie runs waren het meest belovend met een lineare stijging van de trend.