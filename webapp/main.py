from dataclasses import dataclass
from flask import Flask, request, render_template, redirect
from flask_bootstrap import Bootstrap5
from storage import Storage, CourseInfo

app = Flask(__name__)
bootstrap = Bootstrap5(app)

storage = Storage()
all_course_info = storage.get_all_course_info()

APP_DOWNLOAD_LINK = "https://www.dropbox.com/scl/fi/9b7s6mlg1z1jeacn00lje/SRL-Companion-App-Setup.exe?rlkey=09tyuov77v648xiygcskiqrrs&st=j51ww3ld&dl=1"
ADDON_DOWNLOAD_LINK = "https://www.dropbox.com/scl/fi/gixytd2boxdr9xs3d8mrt/Addon.zip?rlkey=mff2qthz7i4gljy02n6x5qnim&st=olx4h34i&dl=1"


@dataclass
class CourseRecord:
    user_id: str
    course_id: str
    time_disp: str
    character_name: str
    race_name: str
    course_type: str


@app.route("/")
def index():
    storage = Storage()
    course_times = storage.get_all_course_records()
    course_records: list[CourseRecord] = []
    for course_time in course_times:
        course_record = CourseRecord(
            user_id=course_time.user_id,
            course_id=course_time.course_id,
            time_disp=format_time(course_time.time_ms),
            character_name=course_time.character_name,
            race_name=course_time.course_id,
            course_type="",
        )
        course_info = all_course_info.get(course_time.course_id)
        if course_info is not None:
            course_record.race_name = course_info.race_name
            course_record.course_type = course_info.course_type
        course_records.append(course_record)
    course_records.sort(key=lambda x: x.race_name)
    return render_template("index.html", course_records=course_records)


@app.route("/course_leaderboard", methods=["GET"])
def course_leaderboard():
    course_id = request.args.get("course_id")
    storage = Storage()
    course_times = storage.get_course_times(course_id)
    for course_time in course_times:
        course_time.time_disp = format_time(course_time.time_ms)
    course_info = all_course_info.get(course_id)
    if course_info is None:
        course_info = CourseInfo(course_id, course_id, "")
    return render_template(
        "leaderboard.html",
        course_info=course_info,
        course_times=course_times,
    )


@app.route("/download", methods=["GET"])
def download():
    file_type = request.args.get("file")
    location = None
    if file_type == "app":
        location = APP_DOWNLOAD_LINK
    if file_type == "addon":
        location = ADDON_DOWNLOAD_LINK
    if location == None:
        return "Unknown download type", 404
    return redirect(location)


@app.route("/upload", methods=["POST"])
def upload_data():
    storage = Storage()
    result = request.get_json()
    print(result)
    battle_tag = result["battleTag"]
    for char_race_data in result["characterRaceData"]:
        character_name = char_race_data["characterName"]
        for course_time in char_race_data["courseTimes"]:
            course_id = course_time["courseId"]
            time_ms = course_time["timeMs"]
            storage.update_time(battle_tag, course_id, int(time_ms), character_name)
    storage.commit()
    return "", 200


def format_time(time_ms) -> str:
    time_s = time_ms / 1000
    return f"{round(time_s, 3):.3f}s"
