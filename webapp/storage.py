from dataclasses import dataclass
import sqlite3


@dataclass
class Race:
    race_id: str
    name: str


@dataclass
class User:
    user_id: str
    name: str


@dataclass
class RaceTime:
    race_id: str
    user_id: str
    time_ms: int
    character_name: str


class Storage:
    def __init__(self) -> None:
        self._connection = sqlite3.connect("skyriding_data.db")
        cursor = self._connection.cursor()
        cursor.execute(
            """
            CREATE TABLE IF NOT EXISTS race_time(
                user_id STR,
                race_id STR,
                time_ms INT,
                character_name STR
            )
            """
        )
        cursor.execute(
            """
            CREATE TABLE IF NOT EXISTS user(
                user_id STR,
                name STR
            )
            """
        )
        cursor.execute(
            """
            CREATE TABLE IF NOT EXISTS race(
                race_id STR,
                name STR
            )
            """
        )

    # TODO: Use with to handle this
    def commit(self) -> None:
        self._connection.commit()

    def add_user(self, user_id: str, name: str) -> None:
        cursor = self._connection.cursor()
        cursor.execute(
            """
            INSERT INTO user VALUES(?, ?)
            """,
            (user_id, name),
        )

    def get_user(self, user_id: str) -> User | None:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT * 
            FROM user
            WHERE user_id = ?
            """,
            user_id,
        )
        user_row = result.fetchone()
        if user_row is None:
            return None
        return User(*user_row)

    def add_race(self, race_id: str, name: str) -> None: ...

    # TODO: Allow getting windows of data
    def get_race_times(self, race_id: str) -> None:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT user_id, time_ms, character_name
            FROM race_time 
            WHERE race_id = ?
            ORDER BY time_ms ASC
            """,
            (race_id,),
        )
        race_times: list[RaceTime] = []
        for user_id, time_ms, character_name in result.fetchall():
            race_times.append(RaceTime(race_id, user_id, time_ms, character_name))
        return race_times

    def get_all_races(self) -> list[Race]:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT user_id, race_id, MIN(time_ms), character_name
            FROM race_time 
            GROUP BY race_id
            """
        )
        race_times: list[RaceTime] = []
        for user_id, race_id, time_ms, character_name in result.fetchall():
            race_times.append(RaceTime(race_id, user_id, time_ms, character_name))
        return race_times

    def get_time(self, user_id: str, race_id: str) -> int | None:
        cursor = self._connection.cursor()
        result = cursor.execute(
            """
            SELECT time_ms 
            FROM race_time 
            WHERE 
                user_id = ? AND
                race_id = ?
            """,
            (user_id, race_id),
        )
        time_ms = result.fetchone()
        if time_ms is None:
            return None
        return int(time_ms[0])

    # TODO: Batch these calls
    def update_time(
        self, user_id: str, race_id: str, time_ms: int, character_name: str
    ) -> None:
        current_time = self.get_time(user_id, race_id)
        cursor = self._connection.cursor()
        if current_time is None:
            cursor.execute(
                """
                INSERT INTO race_time VALUES(?, ?, ?, ?)
                """,
                (user_id, race_id, time_ms, character_name),
            )
            return
        if current_time < time_ms:
            return
        cursor.execute(
            """
            UPDATE race_time
            SET 
                time_ms = ?,
                character_name = ?
            WHERE
                user_id = ? AND
                race_id = ?
            """,
            (time_ms, character_name, user_id, race_id),
        )
