Umgesetzt werden soll eine C#-(Konsolen)anwendung zur Kategorisierung von Bildern, die vom User übergeben werden.
Das Modelltraining erfolgt im Entwicklungsprozess und wird mit ausgeliefert, kann aber auch vom User erneut durchgeführt werden.

Für das Training wird die AutoML-Funktionalität in der ML.NET-Erweiterung genutzt. Diese wählt automatisch den besten Algorithmus für das Training aus. 

Insgesamt kann der User unterschiedliche Funktionalitäten nutzen:
* Vorgegebene Bilder kategorisieren und die Ergebnisse in einer HTML-Datei ausgeben lassen
* das Modell mittels AutoMl und einer bereits eingebunden Online-Datenbank neu trainieren
![Activity_and_Usecase](https://www.plantuml.com/plantuml/png/JOun2i9054JxVueXVGfp00bQ2pOUuBixneNDBlpzmKAykr51xGRcFMR6vcg-p5aoBmwli3HT7J4PWwTQH8DNGtTgj0xQi0PvTLgcJ1ojJgRpZBi0IsFGHdIxb2CDPzveplBnzfj_v50ZSuQRff9eNsYGXwmJwFljM_dbGKQMk5vz0W00 "Activity_and_Usecase")

***
Dies soll über folgenden Ablauf realisiert werden: Der Nutzer kann nach der Prüfung einer Config-Datei im "Hauptmenü" auswählen, ob er Bilder kategorisieren möchte oder das Modell neu trainiert werden soll. Er wird durch die einzelnen Programmschritte geführt. Geplant ist, den Kontrollfluss dem Nutzer durch Tastatureingaben zu ermöglichen. Optional wird der Kontrollfluss über ein einfaches graphisches Benutzer-Interface realisiert. 

![Activity_and_Usecase-1](https://www.plantuml.com/plantuml/png/RP5BJiCm48RtFiKegnQf706LHwKY6jXa1Uh2gIVE26SI-O4gpiCnkil5i9rGGE255_Fu_szcrivBwqDdX5XSB66rkeBYjkU6j98U2GOxdXhaGa1yERkLmNV8uwaGzmqKDsGKMdYDrRgtv0WjClGfLiRV6F1M4kTm0Rv55adcPUhRGt6NV8P6AdWFy6GLnquarbWdsspvy4Cyi8QEF9IxoyTDDHTS1wTbZK0CswhShRBcd8kymnSm6eSm-LTdglst0O8BXuW8WHNKwFpuuKcVEIPcOeL7KC4UsgdwLnRQnEZH6gcYqAAJH-gaGNYMhO6ttchCeotdhZzBojvkG8hIblyPOiEAcXIE1nD2h95LlDyd "Activity_and_Usecase-1")

Zur Umsetzung werden einige Klassen deklariert.
1. Sollen Bilder kategorisiert werden, werden Details in einzelnen Instanzen der `CategorizedIamge`-Klasse  abgelegt, die anschließend bewertet werden.
2. Wird das Modell neu trainiert, wird für jede Kategorie eine Instanz von `Dataset` erzeugt, welche eine Kategorie sowie die einzelnen Image-IDs der Onlinedatenbank beinhaltet. Mittels statischer Methoden aus der AWSSDK werden die Bilder heruntergeladen, in Anlehung an das fertige Python-Script. Diese werden dann mittels der vom Visual-Studio-Modell-Builder erzeugten Klasse `ModelBuilder` genutzt, um ein neues Modell zu trainiern.

`ModelInput`, `ModelOutput`, `ConsumeModel` und `ModelBuilder` werden von Visual Studio Modell-Builder erstellt.
![Klassendiagramm](https://www.plantuml.com/plantuml/png/bLBBRi8m4BpdAwoS4f4SUaK8YOAgHY8KjVp0Dju6guwDOgSlnB_Ns0bkZ3WqbyWxixCpQrSSb28hqA1G7oDYliaMzBvOgIbHkcMYziKrl28FwYLy5HeRK-su78gMEERPqWOQ79KTziO4TTg9GcYh_Ojb3CnqkcALPb90-prpKEP-SzTVepP-R_IkcTAp4RrFcwJZOn0x8zUH_z-H1Vj9erDKkPtBJy4bq184rDZskpyg3PfL9q9xUC7v0-Ee4Zl8Xd7Ay-nWflcSJEWR4kT-q-vjNZUUejvLAIdo-uvnEbYw65w2nbgsx0TfqHZBnyXTAjA5VnFsdVZJUVJeqlCyceH2IUhdF-xAzKvAhepw9SYsbHKgrTE7DOGoSojUmZi-e-guLZRtTfILV4X6WxI5E7JkwGV0KwS7v0-qb49r3Lg6nq8p9Dx49Cz9V1VxJBy0 "Klassendiagramm")

