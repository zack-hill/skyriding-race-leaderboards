import json
import re
import urllib.request

import sqlitecloud

from storage import Storage

CONNECTION_STRING = "sqlitecloud://cd1rspeeik.sqlite.cloud:8860?apikey=rHyR3deilinIX4nmvGMl7JwawKasBAmJsyba63ORLN8"
connection = sqlitecloud.connect(CONNECTION_STRING)
connection.execute("USE DATABASE skyriding_data.db")

storage = Storage()
all_course_info = storage.get_all_course_info()

cursor = connection.cursor()
cursor.execute("ALTER TABLE course_info ADD quest_id int;")

for _, course_info in all_course_info.items():
    search = course_info.race_name
    if course_info.course_type.lower() != "standard":
        search = f"{search} - {course_info.course_type.lower()}"
    url = f"https://www.wowhead.com/quests/name:{search}"
    url = url.replace(" ", "+")
    page = urllib.request.urlopen(url).read().decode()
    result = re.search(r"new Listview\((.+)\)", page)
    if result is None:
        continue
    result = re.search(r"data:(\[.+\])", result.group(1))
    if result is None:
        continue
    data = json.loads(result.group(1))
    id = int(data[0]["id"])
    name = data[0]["name"]
    cursor.execute(
        """
        UPDATE course_info
        SET quest_id = ?
        WHERE race_name = ? AND
        course_type = ?
       """,
        (id, course_info.race_name, course_info.course_type),  # type: ignore
    )
    print(f"{search} | {name} | {id}")
