# Datasets

Wikipedia besitzt eine Übersicht über viele Datasets für diverse Daten.
[Open Images](https://storage.googleapis.com/openimages/web/index.html) besitzt Millionen an Links zu Bildern in tausenden Kategorien. Diese können mit der AWS SDK heruntergeladen werden. Ein Pythonscript existiert bereits, welches die entsprechende Python-Implementierung der SDK (Boto3) nutzt. 
Die Kategorien, Beziehung zwischen den Kategorien, etc. liegen zusammen mit ImageIDs in [csv-Dateien](https://storage.googleapis.com/openimages/web/download.html#download_manually) vor (_mehrere Gigabyte groß....)_.

Die C#-Implementierung der AWS SDK kann über NUGET heruntergeladen werden.