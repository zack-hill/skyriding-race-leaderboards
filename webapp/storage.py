from dataclasses import dataclass
import sqlitecloud

CONNECTION_STRING = "sqlitecloud://cd1rspeeik.sqlite.cloud:8860?apikey=rHyR3deilinIX4nmvGMl7JwawKasBAmJsyba63ORLN8"


@dataclass
class CourseInfo:
    course_id: str
    race_name: str
    course_type: str


@dataclass
class CourseTime:
    course_id: str
    user_id: str
    time_ms: int
    character_name: str


class Storage:
    def __init__(self) -> None:
        self._connection = sqlitecloud.connect(CONNECTION_STRING)
        self._connection.execute("USE DATABASE skyriding_data.db")
        cursor = self._connection.cursor()
        cursor.execute(
            """
            CREATE TABLE IF NOT EXISTS course_time(
                user_id STR,
                course_id STR,
                time_ms INT,
                character_name STR
            )
            """
        )
        cursor.execute(
            """
            CREATE TABLE IF NOT EXISTS course_info(
                course_id STR,
                race_name STR,
                course_type STR
            )
            """
        )

    # TODO: Use with to handle this
    def commit(self) -> None:
        self._connection.commit()

    def get_course_times(self, course_id: str) -> list[CourseTime]:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT user_id, time_ms, character_name
            FROM course_time 
            WHERE course_id = ?
            ORDER BY time_ms ASC
            """,
            (course_id,),
        )
        course_times: list[CourseTime] = []
        for user_id, time_ms, character_name in result.fetchall():
            course_times.append(CourseTime(course_id, user_id, time_ms, character_name))
        return course_times

    def get_all_course_info(self) -> dict[str, CourseInfo]:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT course_id, race_name, course_type
            FROM course_info 
            """
        )
        rows = result.fetchall()
        course_info_dict = {}
        for course_id, race_name, course_type in rows:
            course_info_dict[str(course_id)] = CourseInfo(
                str(course_id), race_name, course_type
            )
        return course_info_dict

    def get_all_course_records(self) -> list[CourseTime]:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT user_id, course_id, MIN(time_ms), character_name
            FROM course_time 
            GROUP BY course_id
            """
        )
        course_times: list[CourseTime] = []
        for user_id, course_id, time_ms, character_name in result.fetchall():
            course_times.append(
                CourseTime(str(course_id), user_id, time_ms, character_name)
            )
        return course_times

    def get_time(self, user_id: str, course_id: str) -> int | None:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT time_ms 
            FROM course_time 
            WHERE 
                user_id = ? AND
                course_id = ?
            """,
            (user_id, course_id),
        )
        time_ms = result.fetchone()
        if time_ms is None:
            return None
        return int(time_ms[0])

    # TODO: Batch these calls
    def update_time(
        self, user_id: str, course_id: str, time_ms: int, character_name: str
    ) -> None:
        current_time = self.get_time(user_id, course_id)
        cursor = self._connection.cursor()
        if current_time is None:
            cursor.execute(
                """
                INSERT INTO course_time VALUES(?, ?, ?, ?)
                """,
                (user_id, course_id, time_ms, character_name),
            )
            return
        if current_time < time_ms:
            return
        cursor.execute(
            """
            UPDATE course_time
            SET 
                time_ms = ?,
                character_name = ?
            WHERE
                user_id = ? AND
                course_id = ?
            """,
            (time_ms, character_name, user_id, course_id),
        )
