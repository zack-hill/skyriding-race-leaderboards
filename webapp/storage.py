from dataclasses import dataclass
import math
import sqlitecloud

CONNECTION_STRING = "sqlitecloud://cd1rspeeik.sqlite.cloud:8860?apikey=rHyR3deilinIX4nmvGMl7JwawKasBAmJsyba63ORLN8"


@dataclass
class CourseInfo:
    course_id: str
    race_name: str
    course_type: str
    quest_id: int


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
                character_name STR,
                score FLOAT
            )
            """
        )
        cursor.execute(
            """
            CREATE TABLE IF NOT EXISTS course_info(
                course_id STR,
                race_name STR,
                course_type STR,
                quest_id INT
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

    def get_user_scores(self) -> list[tuple[str, float]]:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT user_id, SUM(score)
            FROM course_time 
            WHERE score IS NOT NULL
            GROUP BY user_id
            ORDER BY SUM(score) DESC
            """,
        )
        user_scores: list[tuple[str, float]] = [
            (user_id, score) for user_id, score in result.fetchall()
        ]
        return user_scores

    def get_all_course_info(self) -> dict[str, CourseInfo]:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT course_id, race_name, course_type, quest_id
            FROM course_info 
            """
        )
        rows = result.fetchall()
        course_info_dict = {}
        for course_id, race_name, course_type, quest_id in rows:
            course_info_dict[str(course_id)] = CourseInfo(
                str(course_id), race_name, course_type, quest_id
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
            (user_id, course_id),  # type: ignore
        )
        time_ms = result.fetchone()
        if time_ms is None:
            return None
        return int(time_ms[0])

    def get_users(self) -> list[str]:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT user_id
            FROM course_time 
            GROUP BY user_id
            """
        )
        return [user_id[0] for user_id in result.fetchall()]

    def get_active_courses(self) -> list[str]:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT course_id
            FROM course_time 
            GROUP BY course_id
            """
        )
        return [course_id[0] for course_id in result.fetchall()]

    def get_user_placement_map(self, course_id: str) -> dict[str, int]:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT user_id, time_ms, course_id
            FROM course_time 
            WHERE course_id = ?
            ORDER BY time_ms ASC
            """,
            (course_id,),
        )
        user_placements: dict[str, int] = {}
        for i, (user_id, _, _) in enumerate(result.fetchall()):
            user_placements[user_id] = i + 1
        return user_placements

    # TODO: Batch these calls
    def update_time(
        self, user_id: str, course_id: str, time_ms: int, character_name: str
    ) -> None:
        if (
            user_id is None
            or course_id is None
            or time_ms is None
            or character_name is None
        ):
            return
        current_time = self.get_time(user_id, course_id)
        cursor = self._connection.cursor()
        if current_time is None:
            cursor.execute(
                """
                INSERT INTO course_time VALUES(?, ?, ?, ?, ?)
                """,
                (user_id, course_id, time_ms, character_name, 0),  # type: ignore
            )
            self.refresh_course_scores(course_id)
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
            (time_ms, character_name, user_id, course_id),  # type: ignore
        )
        self.refresh_course_scores(course_id)

    def refresh_course_scores(self, course_id: str) -> None:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT user_id, time_ms
            FROM course_time 
            WHERE course_id = ?
            ORDER BY time_ms ASC
            """,
            (course_id,),
        )
        user_scores = [
            (self._get_user_score(i), user[0], course_id)
            for i, user in enumerate(result.fetchall())
        ]
        cursor.executemany(
            """
            UPDATE course_time
            SET 
                score = ?
            WHERE
                user_id = ? AND
                course_id = ?
            """,
            user_scores,  # type: ignore
        )

    @staticmethod
    def _get_user_score(index: int) -> float:
        return 100 / math.sqrt(index + 1)
