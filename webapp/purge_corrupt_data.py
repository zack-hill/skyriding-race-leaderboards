import sqlitecloud

CONNECTION_STRING = "sqlitecloud://cd1rspeeik.sqlite.cloud:8860?apikey=rHyR3deilinIX4nmvGMl7JwawKasBAmJsyba63ORLN8"
connection = sqlitecloud.connect(CONNECTION_STRING)
connection.execute("USE DATABASE skyriding_data.db")

cursor = connection.cursor()
result = cursor.execute(
    """
    SELECT CAST(user_id AS varchar)
    FROM course_time
    GROUP BY user_id
    """,
)
print(result.fetchall())
result = cursor.execute(
    """
    DELETE 
    FROM course_time
    WHERE user_id IS NULL
    """,
    ("None",),  # type: ignore
)

print(result.rowcount)
