import sqlitecloud

CONNECTION_STRING = "sqlitecloud://cd1rspeeik.sqlite.cloud:8860?apikey=rHyR3deilinIX4nmvGMl7JwawKasBAmJsyba63ORLN8"
connection = sqlitecloud.connect(CONNECTION_STRING)
connection.execute("USE DATABASE skyriding_data.db")

file_name = "Skyriding Race Leaderboards Race Data - RaceData.csv"

with open(file_name, mode="r") as f:
    all_course_info = f.readlines()

cursor = connection.cursor()
cursor.execute("DELETE FROM course_info")
for course_info in all_course_info[1:]:
    race_name, course_id, name_lang, course_type = course_info.split(",")
    cursor.execute(
        """
        INSERT INTO course_info VALUES(?, ?, ?)
        """,
        (course_id, race_name, course_type),
    )
