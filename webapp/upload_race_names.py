import sqlitecloud
import sys

CONNECTION_STRING = "sqlitecloud://cd1rspeeik.sqlite.cloud:8860?apikey=rHyR3deilinIX4nmvGMl7JwawKasBAmJsyba63ORLN8"
connection = sqlitecloud.connect(CONNECTION_STRING)
connection.execute("USE DATABASE skyriding_data.db")

file_name = "CurrencyTypes.11.0.5.56572 - CurrencyTypes.11.0.5.56572.csv"

with open(file_name, mode="r") as f:
    all_race_info = f.readlines()

cursor = connection.cursor()
cursor.execute("DELETE FROM race_info")
for race_info in all_race_info[1:]:
    race_name, race_id, name_lang, race_type = race_info.split(",")
    cursor.execute(
        """
        INSERT INTO race_info VALUES(?, ?, ?)
        """,
        (race_id, race_name, race_type),
    )
